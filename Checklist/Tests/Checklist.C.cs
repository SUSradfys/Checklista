using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMS.TPS.Common.Model.API;

namespace Checklist
{
    public partial class Checklist
    {
        public void C() //Conformal Arc
        {
            if (checklistType == ChecklistType.EclipseConformal)
            {
                checklistItems.Add(new ChecklistItem("C. Conformal Arc"));

                string c1_value = string.Empty;
                
                AutoCheckStatus c1_status = AutoCheckStatus.FAIL;
                int c1_numberOfWarnings = 0;
                int c1_numberOfPass = 0;
                List<double> collAngles = new List<double>();
                foreach (Beam beam in planSetup.Beams)
                {
                    if (!beam.IsSetupField)
                    {
                        double collimatorAngle = beam.ControlPoints[0].CollimatorAngle;
                        collAngles.Add(collimatorAngle);
                        if (treatmentUnitManufacturer == TreatmentUnitManufacturer.Varian)
                        { 
                            if (collimatorAngle >= 0 && collimatorAngle <= 5)
                                c1_numberOfPass++;
                            else if (collimatorAngle > 5 && collimatorAngle < 355)
                                c1_numberOfWarnings++;
                        }
                        

                        c1_value += (c1_value.Length == 0 ? string.Empty : ", ") + beam.Id + ": " + collimatorAngle.ToString("0.0") + "°";
                    }
                }
                if (c1_numberOfPass == numberOfTreatmentBeams)
                    c1_status = AutoCheckStatus.PASS;
                else if (c1_numberOfPass + c1_numberOfWarnings == numberOfTreatmentBeams)
                    c1_status = AutoCheckStatus.WARNING;
                if (collAngles.Count > 1 && collAngles.Distinct().ToList().Count < 2)
                    c1_status = AutoCheckStatus.FAIL;

                checklistItems.Add(new ChecklistItem("C1. Kollimatorvinkeln är lämplig", "Kontrollera att kollimatorvinkeln är lämplig\r\n  • Varian Conformal Arc: vanligtvis 0° till 5°, men passar detta ej PTV är andra vinklar ok", c1_value, c1_status));
                 
                string c2_value = string.Empty;
                AutoCheckStatus c2_status = AutoCheckStatus.WARNING;
                int c2_numberOfPass = 0;
                foreach (Beam beam in planSetup.Beams)
                {
                    if (!beam.IsSetupField)
                    {
                        double jawLim = 2.5; 
                        double X1 = 0.1 * beam.ControlPoints[0].JawPositions.X1;
                        double X2 = 0.1 * beam.ControlPoints[0].JawPositions.X2;
                        double Y1 = 0.1 * beam.ControlPoints[0].JawPositions.Y1;
                        double Y2 = 0.1 * beam.ControlPoints[0].JawPositions.Y2;
                        if ((Math.Abs(X1) <= jawLim && Math.Abs(X2) <= jawLim && Math.Abs(Y1) <= jawLim && Math.Abs(Y2) <= jawLim) && (beam.EnergyModeDisplayName == "6X-FFF" || beam.EnergyModeDisplayName == "10X-FFF" ))
                            c2_numberOfPass++;
                        else if ((Math.Abs(X1) > jawLim || Math.Abs(X2) > jawLim || Math.Abs(Y1) > jawLim || Math.Abs(Y2) > jawLim) && (beam.EnergyModeDisplayName == "6X" || beam.EnergyModeDisplayName == "10X"))
                            c2_numberOfPass++;
                        c2_value += (c2_value.Length == 0 ? string.Empty : ", ") + beam.Id + " " + beam.EnergyModeDisplayName + ": X1: " + X1.ToString("0.0") + " cm, X2: " + X2.ToString("0.0") + " cm, Y1: "+ Y1.ToString("0.0") + " cm, Y2: " + Y2.ToString("0.0") + " cm";
                    }
                }
                if (c2_numberOfPass == numberOfTreatmentBeams)
                    c2_status = AutoCheckStatus.PASS;
                checklistItems.Add(new ChecklistItem("C2. Fältbredden är rimlig ", "Kontrollera att fältet har en rimlig fältbredd och har korrekt behandlingsteknik FF eller FFF (riktvärde <= 5 cm, vid större fältbredd ska FF användas).", c2_value, c2_status));

                string c3_value = string.Empty;
                AutoCheckStatus c3_status = AutoCheckStatus.WARNING;
                //int c3_numberOfPass = 0;
                double totArcLength = 0;
                foreach (Beam beam in planSetup.Beams)
                {
                    
                    if (!beam.IsSetupField)
                    {

                        //double arclength = beam.ControlPoints[0].GantryAngle - beam.ControlPoints[beam.ControlPoints.Count()].GantryAngle; 

                        totArcLength += beam.ArcLength; 
                       

                        c3_value += (c3_value.Length == 0 ? string.Empty : ", ") + beam.Id + ": " + beam.ArcLength.ToString("0.0") + "°";
                    }

                }
                c3_value += ". Total längd: " + totArcLength.ToString("0.0"); 
                if (totArcLength > 180)
                    c3_status = AutoCheckStatus.PASS;


                checklistItems.Add(new ChecklistItem("C3. Arclängd är rimlig", "Kontrollera att totala längden på arcs är mer än 180°)", c3_value, c3_status));
                // No Jawtracking for Conformal arcs
                //var v5_values = CheckJawTracking(planSetup);

                string c4_value = string.Empty;
                AutoCheckStatus c4_status = AutoCheckStatus.MANUAL; 
                                        
                checklistItems.Add(new ChecklistItem("C4. Leveransmönstret är rimligt", "Kontrollera visuellt att leveransmönstret är rimligt (att MLCn går längs Z_PTV-konturen)", c4_value, c4_status));

                    

                
            }
        }
    }
}
