using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Checklist
{
    public partial class Checklist
    {
        public void S()
        {
            checklistItems.Add(new ChecklistItem("S. Setup"));
            if (treatmentUnitManufacturer == TreatmentUnitManufacturer.Varian)
            {
                string s1_value = string.Empty;
                AutoCheckStatus s1_status = AutoCheckStatus.FAIL;
                int s1_numberOfPass = 0;
                int s1_numberOfWarnings = 0;
                foreach (Beam beam in planSetup.Beams)
                {
                    double iduPosVrt = double.NaN;
                    DataTable dataTableIDUPosVrt = AriaInterface.Query("select IDUPosVrt from Radiation,ExternalFieldCommon where ExternalFieldCommon.RadiationSer=Radiation.RadiationSer and Radiation.PlanSetupSer=" + planSetupSer.ToString() + " and Radiation.RadiationId='" + beam.Id + "'");
                    if (dataTableIDUPosVrt.Rows.Count == 1 && dataTableIDUPosVrt.Rows[0][0] != DBNull.Value && !Operators.LikeString(beam.Id.ToLower(),"Uppl*gg".ToLower(), CompareMethod.Text))
                    {
                        iduPosVrt = (double)dataTableIDUPosVrt.Rows[0][0];
                        if (iduPosVrt == -50)
                            s1_numberOfPass++;
                        else
                            s1_numberOfWarnings++;
                    }
                    else if (Operators.LikeString(beam.Id.ToLower(), "Uppl*gg".ToLower(), CompareMethod.Text)) // Field with Id Upplägg may have any ImageVrt Position, even undefined.
                        s1_numberOfPass++;
                    s1_value += (s1_value.Length == 0 ? string.Empty : ", ") + beam.Id + ": " + (double.IsNaN(iduPosVrt) ? "-" : ((double)iduPosVrt).ToString("0.0") + " cm");
                }
                if (s1_numberOfPass == numberOfBeams)
                    s1_status = AutoCheckStatus.PASS;
                else if (s1_numberOfPass + s1_numberOfWarnings == numberOfBeams)
                    s1_status = AutoCheckStatus.WARNING;
                s1_value = reorderBeamParam(s1_value, ",");
                checklistItems.Add(new ChecklistItem("S1. Bildplattans vertikala position är -50 cm om inte särskilda skäl föreligger", "Kontrollera att bildplattans vertikala position är -50 cm om inte särskilda skäl föreligger", s1_value, s1_status));
            }

            if (treatmentUnitManufacturer == TreatmentUnitManufacturer.Varian)
            {
                string s2_value = string.Empty;
                AutoCheckStatus s2_status = AutoCheckStatus.FAIL;
                int s2_numberOfPass = 0;
                List<double> s2_gantryAngles = new List<double>();
                List<bool> s2_gantryAnglesExtended = new List<bool>();
                foreach (Beam beam in planSetup.Beams)
                    if (!beam.IsSetupField)
                    {
                        DataTable dataTableExtended = AriaInterface.Query("select GantryRtnExt from ExternalField,Radiation where GantryRtnExt='EN' and ExternalField.RadiationSer=Radiation.RadiationSer and Radiation.PlanSetupSer=" + planSetupSer.ToString() + " and Radiation.RadiationId='" + beam.Id + "'");
                        bool extended = (dataTableExtended.Rows.Count == 1);
                        s2_gantryAnglesExtended.Add(extended);
                        s2_gantryAngles.Add(beam.ControlPoints[0].GantryAngle);
                        s2_value += (s2_value.Length == 0 ? "Sida: " + treatmentSide.ToString() + ", " : ", ") + beam.Id + ": " + beam.ControlPoints[0].GantryAngle.ToString("0.0") + (extended ? "E" : string.Empty);
                    }
                if (checklistType == ChecklistType.EclipseVMAT)
                {
                    foreach (bool extended in s2_gantryAnglesExtended)
                        if (!extended)
                            s2_numberOfPass++;
                }
                else if (treatmentSide == TreatmentSide.PlusX)
                {
                    bool fieldsOnOppositeSide = false;
                    foreach (double angle in s2_gantryAngles)
                        if (angle > 185 && angle <= 290)
                            fieldsOnOppositeSide = true;
                    for (int beamNr = 0; beamNr < s2_gantryAngles.Count; beamNr++)
                    {
                        if (fieldsOnOppositeSide)
                        {
                            if (s2_gantryAnglesExtended[beamNr] == false)
                                s2_numberOfPass++;
                        }
                        else
                        {
                            if (s2_gantryAngles[beamNr] > 180 && s2_gantryAngles[beamNr] <= 185)
                            {
                                if (s2_gantryAnglesExtended[beamNr] == true)
                                    s2_numberOfPass++;
                            }
                            else
                            {
                                if (s2_gantryAnglesExtended[beamNr] == false)
                                    s2_numberOfPass++;
                            }
                        }
                    }
                }
                else
                {
                    bool fieldsOnOppositeSide = false;
                    foreach (double angle in s2_gantryAngles)
                        if (angle >= 70 && angle < 175)
                            fieldsOnOppositeSide = true;
                    for (int beamNr = 0; beamNr < s2_gantryAngles.Count; beamNr++)
                    {
                        if (fieldsOnOppositeSide)
                        {
                            if (s2_gantryAnglesExtended[beamNr] == false)
                                s2_numberOfPass++;
                        }
                        else
                        {
                            if (s2_gantryAngles[beamNr] >= 175 && s2_gantryAngles[beamNr] <= 180)
                            {
                                if (s2_gantryAnglesExtended[beamNr] == true)
                                    s2_numberOfPass++;
                            }
                            else
                            {
                                if (s2_gantryAnglesExtended[beamNr] == false)
                                    s2_numberOfPass++;
                            }
                        }
                    }
                }
                if (s2_numberOfPass == numberOfTreatmentBeams)
                    s2_status = AutoCheckStatus.PASS;
                s2_value = reorderBeamParam(s2_value, ",");
                checklistItems.Add(new ChecklistItem("S2. Extended har valts korrekt på behandlingsfält", "Kontrollera att Extended har valts korrekt på behandlingsfält. Om det finns fält som går över långt på motstående sida (70° resp. 290°) ska inga fält ha Extended. I annat fall gäller:\r\n  • Vänstersidiga behandlingar: Fält med 180<gantryvinkel<=185 ska ha Extended\r\n  • Högersidiga behandlingar: Fält med 175<=gantryvinkel<=180 ska ha Extended\r\n  • Övriga fält ska ej ha Extended\r\n  • Notera att det omvända förhållandet mellan behandlingssida och fält som ska ha Extended gäller om patienten är orienterad Feet First (fötterna mot gantryt)", s2_value, s2_status));
            }

            string s3_value = string.Empty;
            AutoCheckStatus s3_status = AutoCheckStatus.UNKNOWN;
            List<double> s3_setupFieldAngles = new List<double>();
            List<string> s3_beamIds = new List<string>();
            foreach (Beam beam in planSetup.Beams)
                if (beam.IsSetupField && !Operators.LikeString(beam.Id.ToLower(), "Uppl*gg".ToLower(), CompareMethod.Text))
                {
                    s3_setupFieldAngles.Add(beam.ControlPoints[0].GantryAngle);
                    s3_beamIds.Add(beam.Id.ToLower());
                    s3_value += (s3_value.Length == 0 ? "Sida: " + treatmentSide.ToString() + ", " : ", ") + beam.Id + ": " + beam.ControlPoints[0].GantryAngle.ToString("0.0");
                }
            int s3_cbctIndex = s3_beamIds.IndexOf("cbct");
            if (treatmentUnitManufacturer == TreatmentUnitManufacturer.Varian)
            {
                if (s3_setupFieldAngles.Count == 0)
                    s3_value = "Setupfält saknas i planen";
                else if (s3_setupFieldAngles.Count == 1 && s3_cbctIndex == 0)
                    s3_status = AutoCheckStatus.PASS;
                else if (s3_setupFieldAngles.Count == 3)
                {
                    // Ta bort CBCT från fortsatta kontrollen
                    if (s3_cbctIndex != -1)
                    {
                        s3_beamIds.RemoveAt(s3_cbctIndex);
                        s3_setupFieldAngles.RemoveAt(s3_cbctIndex);
                    }
                }
                if (s3_setupFieldAngles.Count == 2)
                {
                    if (treatmentSide == TreatmentSide.PlusX) // Vänstersidig
                    {
                        if (s3_setupFieldAngles.IndexOf(0) != -1 && s3_setupFieldAngles.IndexOf(270) != -1)
                            s3_status = AutoCheckStatus.PASS;
                        else
                            s3_status = AutoCheckStatus.FAIL;
                    }
                    else // Högersidig
                    {
                        if (s3_setupFieldAngles.IndexOf(180) != -1 && s3_setupFieldAngles.IndexOf(270) != -1)
                            s3_status = AutoCheckStatus.PASS;
                        else
                            s3_status = AutoCheckStatus.FAIL;
                    }
                }
            }
            else
                s3_status = AutoCheckStatus.MANUAL; // If not Varian, then do manual check of setup angles.
            s3_value = reorderBeamParam(s3_value, ",");
            checklistItems.Add(new ChecklistItem("S3. Gantryvinklar för setupfälten är korrekta", "Kontrollera att setupfältens gantryvinklar är korrekta (patientgeometrin avgör vinklar)\r\n  • Standard: 270° respektive 0°\r\n•  • Högersidiga behandlingar: 180° respektive 270°", s3_value, s3_status));

            string s4_value;
            AutoCheckStatus s4_status = AutoCheckStatus.FAIL;
            int s4_numberOfPass = 0;
            bool s4_isocenterCouldNotBeDetermined = false;
            VVector s4_isocenterPosition = new VVector(double.NaN, double.NaN, double.NaN);
            foreach (Beam beam in planSetup.Beams)
            {
                double allowedDiff = 0.5;  // the allowed difference between isocenters in mm
                if (double.IsNaN(s4_isocenterPosition.x) && double.IsNaN(s4_isocenterPosition.y) && double.IsNaN(s4_isocenterPosition.z))
                {
                     s4_isocenterPosition = beam.IsocenterPosition;
                     if (double.IsNaN(beam.IsocenterPosition.x) == false && double.IsNaN(beam.IsocenterPosition.y) == false && double.IsNaN(beam.IsocenterPosition.z) == false)
                         s4_numberOfPass++;
                     else
                         s4_isocenterCouldNotBeDetermined = true;
                 }
                
                 //else if (Math.Round(s4_isocenterPosition.x, 1) == Math.Round(beam.IsocenterPosition.x, 1) && Math.Round(s4_isocenterPosition.y, 1) == Math.Round(beam.IsocenterPosition.y, 1) && Math.Round(s4_isocenterPosition.z, 1) == Math.Round(beam.IsocenterPosition.z, 1))
                 else if (Math.Abs(s4_isocenterPosition.x - beam.IsocenterPosition.x) <= allowedDiff && Math.Abs(s4_isocenterPosition.y - beam.IsocenterPosition.y) <= allowedDiff && Math.Abs(s4_isocenterPosition.z - beam.IsocenterPosition.z) <= allowedDiff)
                    s4_numberOfPass++;

            }
                        
            if (s4_numberOfPass == numberOfBeams)//numberOfTreatmentBeams)
            {
                s4_value = "Samma isocenter";
                s4_status = AutoCheckStatus.PASS;
            }
            else if (s4_isocenterCouldNotBeDetermined)
            {
                s4_value = "Isocenterposition kunde ej bestämmas";
                s4_status = AutoCheckStatus.WARNING;
            }
            else
            {
                s4_value = "Olika isocenter mellan fälten";
                s4_status = AutoCheckStatus.WARNING;
            }

            checklistItems.Add(new ChecklistItem("S4. Alla fält har samma isocenter vid isocentrisk teknik", "Kontrollera att samtliga fälts (inklusive setup-fält) Isocenter sammanfaller.", s4_value, s4_status));

            string s5_value = string.Empty;
            foreach (Beam beam in planSetup.Beams)
            {
                if (Operators.LikeString(beam.Id.ToLower(), "Uppl*gg".ToLower(), CompareMethod.Text) == false)
                {
                    DataTable DRRSetting = AriaInterface.Query("select distinct SliceRT.AcqNote from Radiation, PlanSetup, SliceRT, Slice, Image where  PlanSetup.PlanSetupSer =" + planSetupSer.ToString() + " and Radiation.RadiationId ='" + beam.Id + "' and PlanSetup.PlanSetupSer = Radiation.PlanSetupSer and SliceRT.RadiationSer = Radiation.RadiationSer and SliceRT.SliceSer = Slice.SliceSer and SliceRT.AcqNote like 'MultiWindow%'");
                    if (DRRSetting.Rows.Count == 1 && DRRSetting.Rows[0][0] != DBNull.Value)
                        // split file path
                        s5_value += (s5_value.Length == 0 ? "" : ", ") + beam.Id + ": " + QueryContents.FindInFiles((string)DRRSetting.Rows[0][0]);//(string)DRRSetting.Rows[0][0];
                }
            }
            s5_value = reorderBeamParam(s5_value, ",");
            checklistItems.Add(new ChecklistItem("S5. Kvalitén på DRR samt strukturer för verifikationsbilder", "Kontrollera att: \r\nKvalitén på DRR för alla fält som ska ha en DRR är acceptabel.\r\nStrukturer för verifikatiobilder existerar. För assymetriska mam gäller:\r\n  • Body genererad\r\n  • Lunga genererad\r\n  • Klavikelfäste resp sternum manuellt ritad", s5_value, AutoCheckStatus.MANUAL));

            /*
            DataTable site = AriaInterface.Query("select Prescription.Site from Prescription, PlanSetup where PlanSetup.PlanSetupSer='" + planSetupSer.ToString() + "' and Prescription.PrescriptionSer=PlanSetup.PrescriptionSer");
            if (site.Rows[0]["Site"].ToString() == "Thorax" && planSetup.Id.IndexOf("lg") != -1 || site.Rows[0]["Site"].ToString().IndexOf("Mam") == 0 && planSetup.Id.IndexOf("lg") != -1)
            {
                string s6_value = string.Empty;
                string s6_value_detail = string.Empty;
                AutoCheckStatus s6_status;
                int s6_numberOfPass = 0;
                foreach (Beam beam in planSetup.Beams)
                {
                    if (beam.IsSetupField)
                    {
                        DataTable ContourLayers = AriaInterface.Query("select UserCurve=sum(case when CurveLabel='User' then 1 else 0 end), LungStruct=sum(case when GraphicAnnotationId like 'Lung%' and CurveLabel='Proj Structures' then 1 else 0 end), BodyStruct=sum(case when GraphicAnnotationId = 'BODY' and CurveLabel='Proj Structures' then 1 else 0 end) from Radiation, GraphicAnnotation, GraphicAnnotationType where Radiation.PlanSetupSer='" + planSetupSer.ToString() + "' and Radiation.RadiationId='" + beam.Id + "' and GraphicAnnotation.ImageSer=Radiation.RefImageSer and GraphicAnnotationType.GraphicAnnotationTypeSer=GraphicAnnotation.GraphicAnnotationTypeSer");
                        if ((int)ContourLayers.Rows[0]["UserCurve"] < 3 || (int)ContourLayers.Rows[0]["LungStruct"] < 1 || (int)ContourLayers.Rows[0]["BodyStruct"] < 1)
                        {
                            s6_value += (s6_value.Length == 0 ? "" : ", ") + beam.Id;
                            s6_value_detail += (s6_value_detail.Length == 0 ? "" : "\r\n") + beam.Id + ":\r\n  • Lunga genererad: " + Convert.ToBoolean((int)ContourLayers.Rows[0]["LungStruct"]).ToString() + "\r\n  • Body genererad: " + Convert.ToBoolean((int)ContourLayers.Rows[0]["BodyStruct"]).ToString() + "\r\n  • Manuellt ritat: " + Convert.ToBoolean((int)ContourLayers.Rows[0]["UserCurve"]).ToString();
                        }
                        else
                            s6_numberOfPass++;
                    }
                    else
                        s6_numberOfPass++;
                }
                if (s6_numberOfPass < planSetup.Beams.Count())
                    s6_status = AutoCheckStatus.FAIL;
                else
                    s6_status = AutoCheckStatus.MANUAL;

                checklistItems.Add(new ChecklistItem("S6. Strukturer för verifikationsbilder existerar", "Kontrollera att strukturer för verifikatiobilder existerar. För assymetriska mam:\r\n  • Body genererad\r\n  • Lunga genererad\r\n  • Klavikelfäste resp sternum manuellt ritad", s6_value, s6_value_detail, s6_status));
            }
            */


            string s7_value = string.Empty;
            AutoCheckStatus s7_status = AutoCheckStatus.MANUAL;
            List<string> s7_tolerances = new List<string>();
            foreach (Beam beam in planSetup.Beams)
            {
                string toleranceId = string.Empty;
                DataTable toleranceIdTable = AriaInterface.Query("select Tolerance.ToleranceId from Radiation,ExternalFieldCommon,Tolerance where ExternalFieldCommon.RadiationSer=Radiation.RadiationSer and ExternalFieldCommon.ToleranceSer=Tolerance.ToleranceSer and Radiation.PlanSetupSer=" + planSetupSer.ToString() + " and Radiation.RadiationId='" + beam.Id + "'");
                if (toleranceIdTable.Rows.Count == 1 && toleranceIdTable.Rows[0][0] != DBNull.Value)
                    s7_tolerances.Add((string)toleranceIdTable.Rows[0][0]);
                else
                    s7_tolerances.Add("Saknas");
            }
            string[] uniqueTolerances = s7_tolerances.Distinct().ToArray();
            if (uniqueTolerances.Length == 1 && String.Equals(s7_tolerances[0], "Saknas", StringComparison.OrdinalIgnoreCase) == false)
                s7_value = s7_tolerances[0];
            else if (uniqueTolerances.Length == 1 && String.Equals(s7_tolerances[0], "Saknas", StringComparison.OrdinalIgnoreCase) == false)
            {
                s7_value = s7_tolerances[0];
                s7_status = AutoCheckStatus.FAIL;
            }
            else
            {
                foreach (string tolerance in uniqueTolerances)
                    s7_value += (s7_value.Length == 0 ? string.Empty : ", ") + tolerance;
                s7_status = AutoCheckStatus.FAIL;
            }
            checklistItems.Add(new ChecklistItem("S7. Toleranstabell har valts korrekt", "Kontrollera att korrekt toleranstabell har använts, baserat på maskintyp (Elekta/Varian), strålkvalité (elektroner/fotoner), nätmask (H&N fix), annan fixation som sitter fast i britsen (fast fix) eller icke fast fixation (utan fix).", s7_value, s7_status));

            if (treatmentUnitManufacturer == TreatmentUnitManufacturer.Varian)
            {
                string s8_value = string.Empty;
                string s8_value_detailed = string.Empty;
                AutoCheckStatus s8_status = AutoCheckStatus.MANUAL;
                List<string> s8_imageModalityBeam = new List<string>();
                List<int> s8_imageModalityBeamNr = new List<int>();
                List<string> s8_beamId = new List<string>();
                foreach (Beam beam in planSetup.Beams)
                {
                    if (beam.IsSetupField && !Operators.LikeString(beam.Id.ToLower(), "Uppl*gg".ToLower(), CompareMethod.Text))
                    {
                        List<string> protocolIds = new List<string>();

                        s8_value_detailed += (s8_value_detailed.Length == 0 ? string.Empty : "\r\n\r\n") + beam.Id + ": ";
                        s8_value = string.Empty;
                        DataTable dataTableImageSessions = AriaInterface.Query("select Session.SessionNum,SessionProcedure.SessionProcedureTemplateId from SessionProcedurePart,SessionProcedure,Session,Radiation where SessionProcedurePart.RadiationSer=Radiation.RadiationSer and SessionProcedure.SessionProcedureSer=SessionProcedurePart.SessionProcedureSer and Session.SessionSer=SessionProcedure.SessionSer and Radiation.PlanSetupSer=" + planSetupSer.ToString() + " and Radiation.RadiationId='" + beam.Id + "' order by SessionNum");
                        foreach (DataRow dataRow in dataTableImageSessions.Rows)
                        {
                            int sessionNr = (int)dataRow[0];
                            string procedureTemplateId = (dataRow[1] == DBNull.Value ? "-" : (string)dataRow[1]);
                            s8_value_detailed += "\r\n  Session " + sessionNr.ToString() + ": " + procedureTemplateId;
                            protocolIds.Add(procedureTemplateId);
                        }

                        s8_beamId.Add(beam.Id);
                        string[] uniqueprotocolIds = protocolIds.Distinct().ToArray();
                        if (uniqueprotocolIds.Length == 0)
                        {
                            s8_imageModalityBeam.Add("Ingen");
                            s8_status = AutoCheckStatus.FAIL;
                        }
                        else if (uniqueprotocolIds.Length == 1)
                        {
                            s8_imageModalityBeam.Add(uniqueprotocolIds[0]);
                        }
                        else
                        {
                            s8_imageModalityBeam.Add("Flera");
                            s8_status = AutoCheckStatus.FAIL;
                        }
                        if (fractionation != null && fractionation.NumberOfFractions != protocolIds.Count)
                            s8_status = AutoCheckStatus.FAIL;
                        s8_imageModalityBeamNr.Add(protocolIds.Count);
                    }
                }
                for (int beamNr = 0; beamNr < s8_imageModalityBeam.Count; beamNr++)
                    s8_value += (s8_value.Length == 0 ? string.Empty : ", ") + s8_beamId[beamNr] + ": " + s8_imageModalityBeam[beamNr] + " (" + s8_imageModalityBeamNr[beamNr].ToString() + " fr)";
                checklistItems.Add(new ChecklistItem("S8. Bildtagningsmodalitet är korrekt", "Kontrollera att bildtagning är korrekt\r\nVarian:\r\n  • Korrekt bildtagningsmodalitet är inlagd, samt att bildtagning är aktiverad för alla sessioner på setup-fälten\r\n  • Se bilaga 4 i dokumentet ”Verifikationsbilder”\r\nElekta:\r\n  • Inga setup-fält på tangentiella bröstbehandlingar\r\n  • Inga setup-fält på L09, L07 och L05 (XVI används i första hand för icke-laterala behandlingar)\r\n  • På L03 tas bilder med behandlingsfält om de finns i 0/180° och 90/270°, annars ska det finnas setup-fält", s8_value, s8_value_detailed, s8_status));
            }

            string s9_value = "Saknas";
            AutoCheckStatus s9_status = AutoCheckStatus.FAIL;
            foreach (Beam beam in planSetup.Beams)
            {
                if (beam.IsSetupField && Operators.LikeString(beam.Id.ToLower(), "Uppl*gg".ToLower(), CompareMethod.Text))
                {
                    s9_status = AutoCheckStatus.PASS;
                    s9_value = "Existerar";
                    break;
                }
            }
            checklistItems.Add(new ChecklistItem("S9. Uppläggsfält existerar i planen", "Kontrollera att det finns ett fält med Id Upplägg i behandlingsplanen", s9_value, s9_status));

            checklistItems.Add(new ChecklistItem("S10. Uppläggningen är rimlig", "Kontrollera att uppläggningen är rimlig för den givna geometrin \r\n  • Exempelvis att behandlingar av extremt kaudala target inte är orienterade 'Head First'", "", AutoCheckStatus.MANUAL));
        }
    }
}
