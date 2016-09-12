using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMS.TPS.Common.Model.API;

namespace Checklist
{
    public partial class Checklist
    {
        public void V()
        {
            if (checklistType == ChecklistType.EclipseVMAT || checklistType == ChecklistType.MasterPlanIMRT)
            {
                checklistItems.Add(new ChecklistItem("V. VMAT/IMRT"));

                string v1_value = string.Empty;
                AutoCheckStatus v1_status = AutoCheckStatus.FAIL;
                int v1_numberOfWarnings = 0;
                int v1_numberOfPass = 0;
                foreach (Beam beam in planSetup.Beams)
                {
                    if (!beam.IsSetupField)
                    {
                        double collimatorAngle = beam.ControlPoints[0].CollimatorAngle;
                        if (checklistType == ChecklistType.MasterPlanIMRT)
                        {
                            if (collimatorAngle == 2)
                                v1_numberOfPass++;
                            else if (collimatorAngle > 2 && collimatorAngle < 358)
                                v1_numberOfWarnings++;
                        }
                        else
                        {
                            if (collimatorAngle == 5 || collimatorAngle == 355)
                                v1_numberOfPass++;
                            else if (collimatorAngle > 5 && collimatorAngle < 355)
                                v1_numberOfWarnings++;
                        }
                        v1_value += (v1_value.Length == 0 ? string.Empty : ", ") + beam.Id + ": " + collimatorAngle.ToString("0.0") + "°";
                    }
                }
                if (v1_numberOfPass == numberOfTreatmentBeams)
                    v1_status = AutoCheckStatus.PASS;
                else if (v1_numberOfPass + v1_numberOfWarnings == numberOfTreatmentBeams)
                    v1_status = AutoCheckStatus.WARNING;
                checklistItems.Add(new ChecklistItem("V1. Kollimatorvinkeln är lämplig", "Kontrollera att kollimatorvinkeln är lämplig\r\n  • VMAT: vanligtvis 5° grader resp. 355°, men passar detta ej PTV är andra vinklar ok (dock ej vinklar mellan 355° och 5°)", v1_value, v1_status));
                                
                if (checklistType == ChecklistType.EclipseVMAT)
                {
                    string v2_value = string.Empty;
                    AutoCheckStatus v2_status = AutoCheckStatus.WARNING;
                    int v2_numberOfPass = 0;
                    foreach (Beam beam in planSetup.Beams)
                    {
                        if (!beam.IsSetupField)
                        {
                            double fieldWidth = 0.1 * (beam.ControlPoints[0].JawPositions.X2 - beam.ControlPoints[0].JawPositions.X1);
                            if (fieldWidth <= 15)
                                v2_numberOfPass++;
                            v2_value += (v2_value.Length == 0 ? string.Empty : ", ") + beam.Id + ": " + fieldWidth.ToString("0.0") + " cm";
                        }
                    }
                    if (v2_numberOfPass == numberOfTreatmentBeams)
                        v2_status = AutoCheckStatus.PASS;
                    checklistItems.Add(new ChecklistItem("V2. Fältbredden är rimlig ", "Kontrollera att VMAT-fält har en rimlig fältbredd (riktvärde 15 cm, vid större target rekommenderas två arcs och delade fält).", v2_value, v2_status));

                    string v3_details = string.Empty;
                    if (planSetup.OptimizationSetup != null)
                    {
                        /*foreach (OptimizationObjective optimizationObjective in planSetup.OptimizationSetup.Objectives)
                            if(optimizationObjective.GetType()==typeof(OptimizationPointObjective))
                            {
                                OptimizationPointObjective optimizationPointObjective = (OptimizationPointObjective)optimizationObjective;
                                v3_details += (v3_details.Length == 0 ? "Optimization objectives:\r\n  " : "\r\n  ") + optimizationPointObjective.StructureId + ": " +  optimizationPointObjective.Operator.ToString() + ", dose: " + optimizationPointObjective.Dose.Dose.ToString("0.000") + ", volume: " + optimizationPointObjective.Volume.ToString("0.0") + ", priority: " + optimizationPointObjective.Priority.ToString();
                            }*/
                        foreach (OptimizationParameter optimizationParameter in planSetup.OptimizationSetup.Parameters)
                        {
                            if (optimizationParameter.GetType() == typeof(OptimizationPointCloudParameter))
                            {
                                OptimizationPointCloudParameter optimizationPointCloudParameter = (OptimizationPointCloudParameter)optimizationParameter;
                                v3_details += (v3_details.Length == 0 ? string.Empty : "\r\n") + "Point cloud parameter: " + optimizationPointCloudParameter.Structure.Id + "=" + optimizationPointCloudParameter.Structure.DicomType.ToString();
                            }
                            else if (optimizationParameter.GetType() == typeof(OptimizationNormalTissueParameter))
                            {
                                OptimizationNormalTissueParameter optimizationNormalTissueParameter = (OptimizationNormalTissueParameter)optimizationParameter;
                                v3_details += (v3_details.Length == 0 ? string.Empty : "\r\n") + "Normal tissue parameter: priority=" + optimizationNormalTissueParameter.Priority.ToString();
                            }
                            else if (optimizationParameter.GetType() == typeof(OptimizationExcludeStructureParameter))
                            {
                                OptimizationExcludeStructureParameter optimizationExcludeStructureParameter = (OptimizationExcludeStructureParameter)optimizationParameter;
                                v3_details += (v3_details.Length == 0 ? string.Empty : "\r\n") + "Exclude structure parameter: " + optimizationExcludeStructureParameter.Structure.Id;
                            }
                        }
                    }
                    checklistItems.Add(new ChecklistItem("V3. Optimeringsbolus är korrekt använt", "Kontrollera att optimeringsbolus har använts korrekt för ytliga target:	\r\n  Eclipse H&N (VMAT):\r\n    • Optimerings-PTV har använts vid optimeringen i de fall då PTV har beskurits med hänsyn till ytterkonturen\r\n    • Skillnaderna i maxdos för uncertainty-planerna (±0,4 cm i x, y, resp. z) är <5% relativt orginalplanen. Planerna skapas av dosplaneraren.\r\n    • HELP_BODY inkluderar både patientens ytterkontur (BODY) och optimeringsbolus\r\n  Eclipse Ani, Recti (VMAT):\r\n    • BODY ska inkludera eventuellt optimeringsbolus\r\n  Optimeringsbolus i Eclipse (VMAT):\r\n    • HU för optimeringsbolus är satt till 0 HU\r\n    • Optimeringsbolus är skapat genom 5 mm (H&N) eller 6 mm (Ani, Recti) expansion från det PTV-struktur optimeringen skett på. Boluset ska ej gå innanför patientens hudyta.", string.Empty, v3_details, AutoCheckStatus.MANUAL));

                    checklistItems.Add(new ChecklistItem("V4. Leveransmönstret är rimligt", "Kontrollera att leveransmönstret är rimligt (att det inte är en stor andel extremt små öppningar och att riskorgan skärmas, samt att alla segment går på ett target)", string.Empty, AutoCheckStatus.MANUAL));
                }
            }
        }
    }
}
