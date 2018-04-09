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
                List<double> collAngles = new List<double>();
                foreach (Beam beam in planSetup.Beams)
                {
                    if (!beam.IsSetupField)
                    {
                        double collimatorAngle = beam.ControlPoints[0].CollimatorAngle;
                        collAngles.Add(collimatorAngle);
                        if (treatmentUnitManufacturer == TreatmentUnitManufacturer.Varian)
                        { 
                            if (collimatorAngle == 5 || collimatorAngle == 355)
                                v1_numberOfPass++;
                            else if (collimatorAngle > 5 && collimatorAngle < 355)
                                v1_numberOfWarnings++;
                        }
                        else if (treatmentUnitManufacturer == TreatmentUnitManufacturer.Elekta)
                        { 
                            if (collimatorAngle == 30 || collimatorAngle == 330)
                                v1_numberOfPass++;
                        }

                        v1_value += (v1_value.Length == 0 ? string.Empty : ", ") + beam.Id + ": " + collimatorAngle.ToString("0.0") + "°";
                    }
                }
                if (v1_numberOfPass == numberOfTreatmentBeams)
                    v1_status = AutoCheckStatus.PASS;
                else if (v1_numberOfPass + v1_numberOfWarnings == numberOfTreatmentBeams)
                    v1_status = AutoCheckStatus.WARNING;
                if (collAngles.Count > 1 && collAngles.Distinct().ToList().Count < 2)
                    v1_status = AutoCheckStatus.FAIL;

                checklistItems.Add(new ChecklistItem("V1. Kollimatorvinkeln är lämplig", "Kontrollera att kollimatorvinkeln är lämplig\r\n  • Varian: vanligtvis 5° resp. 355°, men passar detta ej PTV är andra vinklar ok (dock ej vinklar mellan 355° och 5°)\r\n  • Elekta: 30° resp. 330°", v1_value, v1_status));
                                
                if (checklistType == ChecklistType.EclipseVMAT)//JSR && treatmentUnitManufacturer == TreatmentUnitManufacturer.Varian)
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
                    checklistItems.Add(new ChecklistItem("V2. Fältbredden är rimlig ", "Kontrollera att VMAT-fält har en rimlig fältbredd (riktvärde 15 cm, vid större target ska två arcs och delade fält övervägas).", v2_value, v2_status));

                    string v3_details = string.Empty;
                    string v3_value = string.Empty;
                    AutoCheckStatus v3_status = AutoCheckStatus.MANUAL;
                    

                    
                    checklistItems.Add(new ChecklistItem("V3. Optimeringsbolus är korrekt använt", "Kontrollera att optimeringsbolus har använts korrekt för ytliga target:	\r\n  Eclipse H&N (VMAT):\r\n    • Optimeringsbolus har använts vid optimeringen i de fall då PTV ligger mindre än 4 mm innanför ytterkonturen.\r\n    • BODY ska inkludera eventuellt optimeringsbolus\r\n  Eclipse Ani, Recti (VMAT):\r\n    • BODY ska inkludera eventuellt optimeringsbolus\r\n  Optimeringsbolus i Eclipse (VMAT):\r\n    • HU för optimeringsbolus är satt till 0 HU\r\n    • Optimeringsbolus är skapat genom 5 mm (H&N) eller 6 mm (Ani, Recti) expansion från det PTV-struktur optimeringen skett på. Boluset ska ej gå innanför patientens hudyta.", v3_value, v3_details, v3_status)); //JSR

                    checklistItems.Add(new ChecklistItem("V4. Robusthet", "Kontrollera planens robusthet m.a.p. ISO-center-förskjutning m.h.a. Uncertainty-planer. Planerna skapas av dosplaneraren.\r\n    • Skillnaderna i maxdos för uncertainty-planerna (±0,4 cm i x, y, resp. z) är <5% relativt originalplanen.\r\n    • CTV täckning är acceptabel.", string.Empty, AutoCheckStatus.MANUAL));

                    // Check for jawtracking: 
                    var v5_values = CheckJawTracking(planSetup);
                    
                    string v5_value = v5_values.Item1;
                    AutoCheckStatus v5_status = v5_values.Item2; 
                                        
                    checklistItems.Add(new ChecklistItem("V5. Leveransmönstret är rimligt", "Kontrollera att leveransmönstret är rimligt (att det inte är en stor andel extremt små öppningar, att riskorgan skärmas, att alla segment går på ett target samt jawtracking för kollimatorerna)", v5_value, v5_status));

                    
                }
            }
        }
    }
}
