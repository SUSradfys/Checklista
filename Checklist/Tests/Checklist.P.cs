using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Globalization;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace Checklist
{
    public partial class Checklist
    {
        public void P()
        {
            checklistItems.Add(new ChecklistItem("P. Dosplan"));

            string p1_value = string.Empty;  
            string p1_value_detail = string.Empty;
            bool reviewed = false;
            AutoCheckStatus p1_status = AutoCheckStatus.MANUAL; //CheckResult(planSetup.ApprovalStatus == PlanSetupApprovalStatus.PlanningApproved);
            DataTable history = AriaInterface.Query("SELECT DISTINCT HistoricalStatus=Approval.Status, HistoricalStatusDate=Approval.StatusDate, HistoricalStatusUserId=Approval.StatusUserName, HistoricalStatusUserName=CONCAT((SELECT DISTINCT Staff.AliasName FROM Staff WHERE Staff.StaffId=Approval.StatusUserName), (SELECT DISTINCT Doctor.AliasName FROM Doctor WHERE Doctor.DoctorId=Approval.StatusUserName)) FROM Approval, PlanSetup, Staff WHERE PlanSetup.PlanSetupSer=Approval.TypeSer AND Approval.ApprovalType='PlanSetup' and PlanSetup.PlanSetupSer = '" + planSetupSer.ToString() + "' ORDER BY HistoricalStatusDate");
            if (history.Rows.Count > 0)
                p1_value_detail += "Historisk Status\tTid\t\t\tUserId\tUserName\r\n";
            foreach (DataRow row in history.Rows)
            {
                if (String.Equals((string)row["HistoricalStatus"], "Reviewed"))
                    reviewed = true;
                p1_value += (string)row["HistoricalStatus"] + ", ";
                p1_value_detail += (string)row["HistoricalStatus"] + "\t" + row["HistoricalStatusDate"].ToString() + "\t" + (string)row["HistoricalStatusUserId"];
                if (row["HistoricalStatusUserName"] == DBNull.Value)
                    p1_value_detail += "\r\n";
                else
                    p1_value_detail += "\t" + (string)row["HistoricalStatusUserName"] + "\r\n";
            }
            p1_value += planSetup.ApprovalStatus.ToString();
            if (String.Equals(planSetup.ApprovalStatus.ToString(), "PlanningApproved") == false)
                p1_status = AutoCheckStatus.FAIL;
            else if (reviewed == false)
                p1_status = AutoCheckStatus.FAIL;
            else
            {
                if (history.Rows.Count == 2 && String.Equals((string)history.Rows[1]["HistoricalStatus"], "Reviewed"))
                    p1_status = AutoCheckStatus.PASS;
            }
            if (String.IsNullOrEmpty(p1_value_detail) == true)
                checklistItems.Add(new ChecklistItem("P1. Planens status är korrekt", "Kontrollera att planen är Planning Approved i Aria samt att den tidigare haft status Reviewed", p1_value, p1_status));
            else
                checklistItems.Add(new ChecklistItem("P1. Planens status är korrekt", "Kontrollera att planen är Planning Approved i Aria samt att den tidigare haft status Reviewed", p1_value, p1_value_detail, p1_status));

            string p2_value = string.Empty;
            AutoCheckStatus p2_status = AutoCheckStatus.FAIL;
            if (treatmentUnitManufacturer == TreatmentUnitManufacturer.Multiple)
                p2_status = AutoCheckStatus.WARNING;
            int p2_numberOfPass = 0;
            foreach (Beam beam in planSetup.Beams)
            {
                bool fff = (beam.EnergyModeDisplayName.IndexOf("FFF") != -1);
                if (treatmentUnitManufacturer == TreatmentUnitManufacturer.Varian)
                {
                    if (fff == false && beam.DoseRate == 600)
                        p2_numberOfPass++;
                    else if (fff == true && beam.EnergyModeDisplayName.IndexOf("6") == 0 && beam.DoseRate == 1400)
                        p2_numberOfPass++;
                    else if (fff == true && beam.EnergyModeDisplayName.IndexOf("10") == 0 && beam.DoseRate == 2400)
                        p2_numberOfPass++;
                }
                else if (treatmentUnitManufacturer == TreatmentUnitManufacturer.Elekta)
                {
                    if (beam.EnergyModeDisplayName.IndexOf("4") == 0 && beam.DoseRate == 250)
                        p2_numberOfPass++;
                    else if (beam.EnergyModeDisplayName.IndexOf("6") == 0 && beam.DoseRate == 600)
                        p2_numberOfPass++;
                    else if (beam.EnergyModeDisplayName.IndexOf("10") == 0 && beam.DoseRate == 500)
                        p2_numberOfPass++;
                    /*{
                        if (beam.TreatmentUnit.Id.IndexOf("L05") == -1 && beam.DoseRate == 400)
                            p2_numberOfPass++;
                        else if (beam.TreatmentUnit.Id.IndexOf("L05") != -1 && beam.DoseRate == 500)
                            p2_numberOfPass++;
                    }*/
                }
                p2_value += (p2_value.Length == 0 ? string.Empty : ", ") + beam.Id + ": " + beam.DoseRate.ToString();
            }
            if (p2_numberOfPass == numberOfBeams)
                p2_status = AutoCheckStatus.PASS;
            p2_value = reorderBeamParam(p2_value, ",");
            checklistItems.Add(new ChecklistItem("P2. Dosraten är korrekt", "Kontrollera att dosraten (MU/min) är korrekt:\r\n  • Varian: 600 (ej FFF), 1400 (6 MV FFF), 2400 (10 MV FFF)\r\n  • Elekta: 250 (4 MV), 600 (6 MV), 500 (10 MV)", p2_value, p2_status));

            if (treatmentUnitManufacturer == TreatmentUnitManufacturer.Varian)
            {
                string p3_value = string.Empty;
                AutoCheckStatus p3_status = AutoCheckStatus.FAIL;
                int p3_numberOfPass = 0;
                foreach (Beam beam in planSetup.Beams)
                {
                    if (!beam.IsSetupField)
                    {
                        double treatmentTime = double.NaN;
                        DataTable treatmentTimeTable = AriaInterface.Query("select TreatmentTime from Radiation,ExternalFieldCommon where ExternalFieldCommon.RadiationSer=Radiation.RadiationSer and Radiation.PlanSetupSer=" + planSetupSer.ToString() + " and Radiation.RadiationId='" + beam.Id + "'");

                        if (treatmentTimeTable.Rows.Count == 1 && treatmentTimeTable.Rows[0][0] != DBNull.Value)
                        {
                            treatmentTime = (double)treatmentTimeTable.Rows[0][0];

                            double openMU;
                            double wedgedMU;
                            GetMU(beam, out openMU, out wedgedMU);

                            if (beam.EnergyModeDisplayName.IndexOf("FFF") != -1)
                            {
                                if (openMU < 600 && treatmentTime == 0.5)
                                    p3_numberOfPass++;
                            }
                            else if (checklistType == ChecklistType.EclipseVMAT)
                            {
                                if (openMU <= 400 && treatmentTime == 2)
                                    p3_numberOfPass++;
                                else if (openMU > 400 && treatmentTime == 3)
                                    p3_numberOfPass++;
                            }
                            else if (checklistType == ChecklistType.EclipseGating)
                            {
                                if (treatmentTime == 5)
                                    p3_numberOfPass++;
                            }
                            else
                            {
                                if (openMU <= 500 && wedgedMU == 0 && treatmentTime == 1 || wedgedMU > 0 && wedgedMU <= 300 && treatmentTime == 1)
                                    p3_numberOfPass++;
                                else if (openMU > 500 && wedgedMU == 0 && treatmentTime == 2 || wedgedMU > 300 && treatmentTime == 2)
                                    p3_numberOfPass++;
                            }
                        }

                        p3_value += (p3_value.Length == 0 ? string.Empty : ", ") + beam.Id + ": " + (double.IsNaN(treatmentTime) ? "-" : treatmentTime.ToString() + " min");
                    }
                }
                if (p3_numberOfPass == numberOfTreatmentBeams)
                    p3_status = AutoCheckStatus.PASS;
                p3_value = reorderBeamParam(p3_value, ",");

                checklistItems.Add(new ChecklistItem("P3. Beam on-tiderna är korrekta", "Kontrollera att fälten är tilldelade korrekta beam on-tider:\r\n  • 0.5 min för FFF fält med <600 MU\r\n  • 1 min för öppna fält med <=500 MU och kilfält med <=300 MU\r\n  • 2 min för öppna fält med >500 MU, kilfält med >300 MU, och RapidArc (<=400 MU/arc)\r\n  • 3 min för RA (>400 MU/arc)\r\n  • 5 min för gating", p3_value, p3_status));
            }

            // Will now use information from Prescription rather than verifying against ChecklistType
            string p4_value = string.Empty;
            AutoCheckStatus p4_status = AutoCheckStatus.FAIL;
            string prescribedGating = string.Empty;
            DataTable prescription = AriaInterface.Query("select Gating from Prescription, PlanSetup where PlanSetup.PrescriptionSer = Prescription.PrescriptionSer and PlanSetup.PlanSetupSer = '" + planSetupSer.ToString() + "'");
            if (prescription.Rows[0]["Gating"] != DBNull.Value)
                prescribedGating = (string)prescription.Rows[0]["Gating"];

            DataTable dataTableUseGated = AriaInterface.Query("select distinct ExternalFieldCommon.MotionCompTechnique from Radiation,ExternalFieldCommon where Radiation.RadiationSer=ExternalFieldCommon.RadiationSer and Radiation.PlanSetupSer=" + planSetupSer.ToString());
            if (dataTableUseGated.Rows.Count == 1 && dataTableUseGated.Rows[0][0] != DBNull.Value && string.Compare((string)dataTableUseGated.Rows[0][0], "GATING") == 0)
            {
                if (String.IsNullOrEmpty(prescribedGating) == false)
                    p4_status = AutoCheckStatus.PASS;
                else
                    p4_status = AutoCheckStatus.FAIL;
                p4_value = "Ikryssad";
            }
            else
            {
                if (String.IsNullOrEmpty(prescribedGating) == false)
                    p4_status = AutoCheckStatus.FAIL;
                else
                    p4_status = AutoCheckStatus.PASS;
                p4_value = "Ej ikryssad";
            }
            /*
            if (dataTableUseGated.Rows.Count == 1 && dataTableUseGated.Rows[0][0] != DBNull.Value && string.Compare((string)dataTableUseGated.Rows[0][0], "GATING") == 0)
            {
                if (checklistType == ChecklistType.EclipseGating)
                    p4_status = AutoCheckStatus.PASS;
                else
                    p4_status = AutoCheckStatus.FAIL;
                p4_value = "Ikryssad";
            }
            else
            {
                if (checklistType == ChecklistType.EclipseGating)
                    p4_status = AutoCheckStatus.FAIL;
                else
                    p4_status = AutoCheckStatus.PASS;
                p4_value = "Ej ikryssad";
            }
            */
            checklistItems.Add(new ChecklistItem("P4. Use Gated är korrekt", "Kontrollera att rutan Use Gated under Plan properties svarar mot ordination.", p4_value, p4_status));

            if (checklistType == ChecklistType.EclipseGating)
            {
                string p5_value = string.Empty;
                AutoCheckStatus p5_status = AutoCheckStatus.FAIL;
                if (image != null && image.Comment.ToLower().IndexOf("bh") == -1)
                {
                    int p5_numberOfPass = 0;
                    foreach (Beam beam in planSetup.Beams)
                    {
                        if (!beam.IsSetupField)
                        {
                            double openMU;
                            double wedgedMU;
                            GetMU(beam, out openMU, out wedgedMU);
                            if (wedgedMU == 0)
                                p5_numberOfPass++;
                        }
                    }
                    if (p5_numberOfPass == numberOfTreatmentBeams)
                    {
                        p5_status = AutoCheckStatus.PASS;
                    }
                    p5_value = "EIG";
                }
                else
                {
                    p5_status = AutoCheckStatus.PASS;
                    p5_value = "Breath hold";
                }
                checklistItems.Add(new ChecklistItem("P5. Kilade fält ej förekommande för EIG", "Kontrollera att det inte finns några kilade fält i EIG gating-plan.", p5_value, p5_status));
            }

            string p6_value = string.Empty;
            string p6_value_detailed = string.Empty;
            AutoCheckStatus p6_status = AutoCheckStatus.UNKNOWN;
            DataTable dataTableTreatmentSessions = AriaInterface.Query("select Session.SessionNum,SessionRTPlan.Status from Session,SessionRTPlan,RTPlan where Session.SessionSer=SessionRTPlan.SessionSer and RTPlan.RTPlanSer=SessionRTPlan.RTPlanSer and RTPlan.PlanSetupSer=" + planSetupSer.ToString() + " order by SessionNum");
            foreach (DataRow dataRow in dataTableTreatmentSessions.Rows)
            {
                int sessionNr = (int)dataRow[0];
                string status = (dataRow[1] == DBNull.Value ? "-" : (string)dataRow[1]);
                p6_value_detailed += (p6_value_detailed.Length == 0 ? string.Empty : "\r\n") + "Session " + sessionNr.ToString() + ": " + status;
            }
            p6_value = "# aktiva sessioner: " + dataTableTreatmentSessions.Rows.Count.ToString();
            if (fractionation != null && fractionation.NumberOfFractions > 0 && fractionation.NumberOfFractions == dataTableTreatmentSessions.Rows.Count)
                p6_status = AutoCheckStatus.PASS;
            else
                p6_status = AutoCheckStatus.FAIL;
            checklistItems.Add(new ChecklistItem("P6. Behandlingssessionerna är aktiva", "Kontrollera att alla behandlingssessioner är aktiva", p6_value, p6_value_detailed, p6_status));

            if (checklistType == ChecklistType.Eclipse || checklistType == ChecklistType.EclipseGating || checklistType == ChecklistType.MasterPlan)
            {
                string p7_value = string.Empty;
                AutoCheckStatus p7_status = AutoCheckStatus.FAIL;
                int p7_numberOfPass = 0;
                foreach (Beam beam in planSetup.Beams)
                {
                    if (!beam.IsSetupField)
                    {
                        double diode_value = double.NaN;
                        if (beam.Comment.Length > 0)
                        {
                            NumberFormatInfo numberFormatInfo = new NumberFormatInfo() { NumberDecimalSeparator = ".", NumberGroupSeparator = string.Empty, NegativeInfinitySymbol = "Inf", PositiveInfinitySymbol = "Inf", NaNSymbol = "NaN", NumberDecimalDigits = 0 };
                            string[] splitString = beam.Comment.ToLower().Split(new string[] { " ", "gy" }, StringSplitOptions.RemoveEmptyEntries);
                            if (splitString.Length == 0)
                                double.TryParse(beam.Comment.Replace(',', '.'), NumberStyles.Number | NumberStyles.AllowExponent, numberFormatInfo, out diode_value);
                            else
                                double.TryParse(splitString[0].Replace(',', '.'), NumberStyles.Number | NumberStyles.AllowExponent, numberFormatInfo, out diode_value);
                        }
                        double openMU;
                        double wedgedMU;
                        GetMU(beam, out openMU, out wedgedMU);
                        if (double.IsNaN(diode_value) == false)
                            p7_numberOfPass++;
                        else
                        {
                            if (treatmentUnitManufacturer == TreatmentUnitManufacturer.Varian && openMU + wedgedMU < 25)
                                p7_numberOfPass++;
                            else if (treatmentUnitManufacturer == TreatmentUnitManufacturer.Elekta && openMU < 25 && openMU + wedgedMU < 50)
                                p7_numberOfPass++;
                        }
                        p7_value += (p7_value.Length == 0 ? string.Empty : ", ") + beam.Id + ": " + (double.IsNaN(diode_value) ? "-" : diode_value.ToString() + " Gy");
                    }
                }
                if (p7_numberOfPass == numberOfTreatmentBeams)
                    p7_status = AutoCheckStatus.PASS;
                p7_value = reorderBeamParam(p7_value, ",");
                checklistItems.Add(new ChecklistItem("P7. Diodvärden finns införda under Comments för fälten", "Kontrollera att diodvärden finns införda för fält med >=25 MU alternativt >=50 MU (öppet+kil) för Elekta.", p7_value, p7_status));
            }

            checklistItems.Add(new ChecklistItem("P8. Dosfördelningen är rimlig", "Kontrollera att dosfördelningen är rimlig med avseende på targettäckning och omkringliggande riskorgan", "", AutoCheckStatus.MANUAL));

            string p9_value_detailed = string.Empty;
            string p9_value = String.Empty;
            bool isSplit = false;
            AutoCheckStatus p9_status = AutoCheckStatus.UNKNOWN;
            if (checklistType == ChecklistType.EclipseVMAT)
                isSplit = GetIsSplitVMAT(planSetup);
            foreach (Beam beam in planSetup.Beams)
            {
                if (!beam.IsSetupField)
                {
                    double openMU;
                    double wedgedMU;
                    GetMU(beam, out openMU, out wedgedMU);

                    p9_value_detailed += beam.Id + ":\r\n";
                    if (checklistType == ChecklistType.EclipseVMAT)
                    {
                        p9_value_detailed += "  Open: " + openMU.ToString("0.0") + " MU\r\n  " + beam.MetersetPerGy.ToString("0.0") + " MU/Gy\r\n";
                        if (beam.MetersetPerGy > 300 && !isSplit)
                            p9_value += (p9_value.Length == 0 ? string.Empty : ", ") + beam.Id + ": För många MU/Gy";
                        else if (beam.MetersetPerGy > 550 && isSplit)
                            p9_value += (p9_value.Length == 0 ? string.Empty : ", ") + beam.Id + ": För många MU/Gy";
                    }
                    else
                    {
                        p9_value_detailed += "  Open: " + openMU.ToString("0.0") + ", Wedged: " + wedgedMU.ToString("0.0") + "\r\n";  
                    }
                    p9_value_detailed += "  Energi: " + beam.EnergyModeDisplayName + "\r\n\r\n";

                    /*
                    if (openMU < 10 && openMU != 0 || wedgedMU < 30 && wedgedMU != 0)
                        p9_value += (p9_value.Length == 0 ? string.Empty : ", ") + beam.Id + ": För få MU";
                    if (treatmentUnitManufacturer == TreatmentUnitManufacturer.Elekta && openMU + wedgedMU > 999)
                        p9_value += (p9_value.Length == 0 ? string.Empty : ", ") + beam.Id + ": För många MU";
                    */
                    if (treatmentUnitManufacturer == TreatmentUnitManufacturer.Elekta)
                    {
                        if (openMU + wedgedMU > 999)
                            p9_value += (p9_value.Length == 0 ? string.Empty : ", ") + beam.Id + ": För många MU";
                        if (openMU < 10 && openMU != 0 || wedgedMU < 30 && wedgedMU != 0)
                            p9_value += (p9_value.Length == 0 ? string.Empty : ", ") + beam.Id + ": För få MU";
                    }
                    else if (treatmentUnitManufacturer == TreatmentUnitManufacturer.Varian)
                    {
                        if (openMU < 10 && wedgedMU == 0 || wedgedMU != 0 && openMU + wedgedMU < 20)
                            p9_value += (p9_value.Length == 0 ? string.Empty : ", ") + beam.Id + ": För få MU";
                    }
                }
            }
            if (p9_value.Length > 0 )
            {
                if (checklistType == ChecklistType.EclipseVMAT)
                    p9_status = AutoCheckStatus.WARNING;
                else
                    p9_status = AutoCheckStatus.FAIL;
                p9_value = reorderBeamParam(p9_value, ",");
            }

            p9_value_detailed = reorderBeamParam(p9_value_detailed, "\r\n\r\n");
            checklistItems.Add(new ChecklistItem("P9. Fälten ser rimliga ut vad gäller form, energi, MU och korrektion av artefakter", "Kontrollera att fälten ser rimliga ut vad gäller form, energi, MU och korrektion av artefakter\r\n  • Riktlinje för RapidArc är max 300 MU/Gy om bländarna är utanför target under hela varvet (sett ur BEV). Vid delvis skärmat target är denna gräns max 550 MU/Gy.\r\n  • Öppna fält ska ha ≥10 MU.\r\n  • Fält med dynamisk kil (Varian) ska ha minst 20 MU.\r\n  • Fält med fast kil (Elekta) ska ha ≥30 kilade MU.\r\n  •  För Elekta gäller dessutom att totala antalet MU per fält (öppet + kilat) ej får överstiga 999 MU.", p9_value, p9_value_detailed, p9_status));

            if (checklistType != ChecklistType.EclipseVMAT)
            { 
                if (treatmentUnitManufacturer == TreatmentUnitManufacturer.Elekta)
                {
                    AutoCheckStatus p10_status = AutoCheckStatus.UNKNOWN;
                    string p10_value = ElektaMLCCheck(planSetup);
                    p10_status = CheckResult(String.Compare(p10_value, "MLC positioner OK.", true) == 0);
                    checklistItems.Add(new ChecklistItem("P10. MLC:n är indragen till X-bländare, och ett/två blad är öppna utanför Y-bländare", "Kontrollera att MLC:n är indragen till X-bländare eller innanför, och att ett helt bladpar är öppet utanför Y-bländare på resp. sida om Y1 resp. Y2 har decimal 0,7, 0,8 eller 0,9.", p10_value, p10_status));
                }
            }

            string p11_value = "Metod: " + planSetup.PlanNormalizationMethod + ", target: " + planSetup.TargetVolumeID + ", prescribed percentage: " + (planSetup.PrescribedPercentage * 100.0).ToString("0.0") + ", värde: " + planSetup.PlanNormalizationValue.ToString("0.0");
            AutoCheckStatus p11_status = AutoCheckStatus.MANUAL;
            if (planSetup.PlanNormalizationMethod.Equals("No plan normalization", StringComparison.OrdinalIgnoreCase))
                p11_status = AutoCheckStatus.FAIL;
            double normLimitVMAT = 3.0;
            if (checklistType == ChecklistType.EclipseVMAT && Math.Abs(planSetup.PlanNormalizationValue - 100) > normLimitVMAT)
            {
                p11_status = AutoCheckStatus.FAIL;
            }
            checklistItems.Add(new ChecklistItem("P11. Normering är korrekt", "Kontrollera att planen är normerad på korrekt vis \r\n  • Normalt till targetvolymens medeldos (om särskilt skäl föreligger kan en punktnormering användas). \r\n  • För stereotaktiska lungor i Eclipse normeras dosen till isocenter och ordineras till 75%-isodosen.\r\n  • För VMAT ska Plan Normalization Value skall normeringsvärdet vara i intervallet [0.970, 1.030].", p11_value, p11_status));

            string p12_value = string.Empty;            
            string p13_value = string.Empty;
            string p13_value_detailed = string.Empty;
            AutoCheckStatus p13_status = AutoCheckStatus.WARNING;
            List<ReferencePoint> referencePoints = new List<ReferencePoint>();
            List<double> referencePointDose = new List<double>();
            List<double> referencePointTotalDose = new List<double>();
            List<int> activeReferencePoints=new List<int>();
            // Get dose to reference points in active course
            foreach (PlanSetup planSetupInCourse in course.PlanSetups)
            {
                if (planSetupInCourse.ApprovalStatus != PlanSetupApprovalStatus.UnApproved && planSetupInCourse.ApprovalStatus != PlanSetupApprovalStatus.Rejected)
                {
                    foreach (Beam beam in planSetupInCourse.Beams)
                    {
                        foreach (FieldReferencePoint fieldReferencePoint in beam.FieldReferencePoints)
                        {
                            int referencePointIndex = -1;
                            for (int refPointNr = 0; refPointNr < referencePoints.Count; refPointNr++)
                            {
                                if (string.Compare(fieldReferencePoint.ReferencePoint.Id, referencePoints[refPointNr].Id) == 0)
                                {
                                    referencePointIndex = refPointNr;
                                    break;
                                }
                            }
                            if (referencePointIndex == -1)
                            {
                                referencePointIndex = referencePoints.Count;
                                referencePoints.Add(fieldReferencePoint.ReferencePoint);
                                referencePointDose.Add(0.0);
                                referencePointTotalDose.Add(0.0);
                            }                            
                            referencePointTotalDose[referencePointIndex] += fieldReferencePoint.FieldDose.Dose * (planSetupInCourse.UniqueFractionation == null ? double.NaN : (double)planSetupInCourse.UniqueFractionation.NumberOfFractions);
                            if (planSetupInCourse == planSetup)
                            {
                                referencePointDose[referencePointIndex] += fieldReferencePoint.FieldDose.Dose;
                                if( activeReferencePoints.Contains(referencePointIndex) == false)
                                    activeReferencePoints.Add(referencePointIndex);
                            }
                        }
                    }
                }
            }
            int p13_numberOfPass = 0;
            for (int refPointNr = 0; refPointNr < referencePoints.Count; refPointNr++)
            {
                p13_value_detailed += (p13_value_detailed.Length == 0 ? string.Empty : "\r\n\r\n") + referencePoints[refPointNr].Id + ":\r\n";

                double totalDoseLimit = double.NaN;
                double dailyDoseLimit = double.NaN;
                double sessionDoseLimit = double.NaN;

                DataTable dataTableRefPointLimits = AriaInterface.Query("select distinct RefPoint.RefPointId,RefPoint.TotalDoseLimit,RefPoint.DailyDoseLimit,RefPoint.SessionDoseLimit from PlanSetup,Radiation,RadiationRefPoint,RefPoint where RadiationRefPoint.RefPointSer=RefPoint.RefPointSer and RadiationRefPoint.RadiationSer=Radiation.RadiationSer and PlanSetup.PlanSetupSer=Radiation.PlanSetupSer and PlanSetup.CourseSer=" + courseSer.ToString() + " and RefPoint.RefPointId='" + referencePoints[refPointNr].Id + "'");
                if (dataTableRefPointLimits.Rows.Count == 1)
                {
                    totalDoseLimit = (dataTableRefPointLimits.Rows[0][1] == DBNull.Value ? totalDoseLimit = double.NaN : totalDoseLimit = (double)dataTableRefPointLimits.Rows[0][1]);
                    dailyDoseLimit = (dataTableRefPointLimits.Rows[0][2] == DBNull.Value ? dailyDoseLimit = double.NaN : dailyDoseLimit = (double)dataTableRefPointLimits.Rows[0][2]);
                    sessionDoseLimit = (dataTableRefPointLimits.Rows[0][3] == DBNull.Value ? sessionDoseLimit = double.NaN : sessionDoseLimit = (double)dataTableRefPointLimits.Rows[0][3]);
                }
                p13_value_detailed += "  Dosbidrag från aktuell plan: " + (fractionation == null ? double.NaN : referencePointDose[refPointNr] * (double)fractionation.NumberOfFractions).ToString("0.000") + " Gy, " + referencePointDose[refPointNr].ToString("0.000") + " Gy/fr " + " (Total dose limit: " + totalDoseLimit.ToString("0.000") + " Gy, daily limit: " + dailyDoseLimit.ToString("0.000") + " Gy, session limit: " + sessionDoseLimit.ToString("0.000") + " Gy)\r\n";                   
                p13_value_detailed += "  Totalt dosbidrag från samtliga godkändaplaner: " + referencePointTotalDose[refPointNr].ToString("0.000") + " Gy " + " (Total limit: " + totalDoseLimit.ToString("0.000") + " Gy)";
                
                if (activeReferencePoints.Contains(refPointNr)) // Reference point is present in the active plan
                {
                    p12_value += (p12_value.Length == 0 ? string.Empty : ", ") + referencePoints[refPointNr].Id + ": " + referencePointDose[refPointNr].ToString("0.000") + " Gy";
                    p13_value += (p13_value.Length == 0 ? string.Empty : ", ") + referencePoints[refPointNr].Id + ": (T:" + totalDoseLimit.ToString("0.000") + "/D:" + dailyDoseLimit.ToString("0.000") + "/S:" + sessionDoseLimit.ToString("0.000") + " Gy)";
                    
                    if (Math.Round(referencePointDose[refPointNr], 3) <= Math.Round(dailyDoseLimit, 3) &&
                        Math.Round(referencePointDose[refPointNr], 3) <= Math.Round(sessionDoseLimit, 3) &&
                        Math.Round(referencePointTotalDose[refPointNr], 3) == Math.Round(totalDoseLimit, 3))
                    {
                        p13_numberOfPass++;
                    }
                }
            }
            if (activeReferencePoints.Count > 0 && p13_numberOfPass == activeReferencePoints.Count)
                p13_status = AutoCheckStatus.UNKNOWN;
            checklistItems.Add(new ChecklistItem("P12. Referenspunkternas dosbidrag är korrekta", "Kontrollera att dosbidrag till referenspunkter (dos) är korrekta:\r\n  • Varje plan ska ha en punkt (primary reference point) som summerar upp till ordinerad dos för det största PTV som planen primärt behandlar.\r\n  • Om flera planer bidrar med dos till samma targetvolymer eller om en plan bidrar med dos till flera targetvolymer ska det finnas referenspunkter utan lokalisation i alla planer som summerar dosen till dessa volymer.\r\n  • Referenspunkterna ska inte ha dosbidrag från tidigare behandlingar.", p12_value, AutoCheckStatus.MANUAL));
            checklistItems.Add(new ChecklistItem("P13. Referenspunkternas gränser är korrekta", "Kontrollera att referenspunkternas gränser (dos) är korrekta", p13_value, p13_value_detailed, p13_status));

            if (checklistType == ChecklistType.Eclipse || checklistType == ChecklistType.EclipseGating)
                checklistItems.Add(new ChecklistItem("P14. Skarven är flyttad korrekt för skarvplan", "Skarvplaner: Skarven är flyttad korrekt och fälten är i övrigt likadana\r\n  • Bröstbehandlingar med kollimator i 0° för både huvudfält i fossa- och tang.-fält flyttas endast om eventuellt PTV_66 ligger i skarven.", string.Empty, AutoCheckStatus.MANUAL));


            AutoCheckStatus p15_status = AutoCheckStatus.UNKNOWN;
            string p15_value = string.Empty;
            int p15_numberOfPass = 0;
            List<string> machineId = new List<string>();
            foreach (Beam beam in planSetup.Beams)
            {
                machineId.Add(beam.TreatmentUnit.Id);
                if (String.Equals(beam.TreatmentUnit.ToString(), planSetup.Beams.First().TreatmentUnit.ToString()))
                    p15_numberOfPass += 1;
                p15_value += (p15_value.Length == 0 ? string.Empty : ", ") + beam.Id + ": " + beam.TreatmentUnit.Id;
            }
            if (p15_numberOfPass == numberOfBeams)
                p15_status = AutoCheckStatus.PASS;
            else
                p15_status = AutoCheckStatus.FAIL;
            p15_value = reorderBeamParam(p15_value, ",");
            checklistItems.Add(new ChecklistItem("P15. Konsekvent maskinval", "Kontrollera att samtliga fält är planerade till en och samma behandlingsapparat.", p15_value, p15_status));

            AutoCheckStatus p16_status = AutoCheckStatus.UNKNOWN;
            string p16_value = string.Empty;
            string p16_value_detailed = "Följande bokningar är aktiva:\r\n";
            string treatMachineIdCommon = machineId.ToArray().GroupBy(v => v)
                .OrderByDescending(g => g.Count())
                .First()
                .Key;
            p16_value = "Planerat: " + treatMachineIdCommon + (machineId.Distinct().Count() == 1 ? " (enhetligt)" : " (tvetydigt)");
            DateTime date = DateTime.Now;
            List<string> bookedMachineId = new List<string>();
            string bookedMachineIdCommon = string.Empty;
            //DataTable bookings = AriaInterface.Query("select x.MachineId, x.ScheduledStartTime from (SELECT DISTINCT Patient.PatientSer, Patient.PatientId, ScheduledActivity.ActualEndDate,ScheduledActivity.ScheduledActivityCode, ScheduledActivity.ActivityInstanceSer,Machine.MachineId,ScheduledActivity.ScheduledStartTime, ScheduledActivity.ObjectStatus FROM ScheduledActivity,Attendee,Patient,Machine WHERE Patient.PatientId = " + patient.Id.ToString() + " AND ScheduledActivity.ObjectStatus='Active' AND ScheduledActivity.ScheduledActivityCode = 'Open' AND ScheduledActivity.PatientSer=Patient.PatientSer AND Attendee.ActivityInstanceSer=ScheduledActivity.ActivityInstanceSer AND Machine.ResourceSer=Attendee.ResourceSer and ScheduledActivity.ScheduledStartTime > '" + date.ToString("yyyy-MM-dd") + " 00:00:00') as x where PatientId = '" + patient.Id.ToString() + "' ORDER BY ScheduledStartTime");
            DataTable bookings = AriaInterface.Query("SELECT DISTINCT Patient.PatientSer, Patient.PatientId, ScheduledActivity.ActualEndDate,ScheduledActivity.ScheduledActivityCode, ScheduledActivity.ActivityInstanceSer,Machine.MachineId,ScheduledActivity.ScheduledStartTime, ScheduledActivity.ObjectStatus FROM ScheduledActivity,Attendee,Patient,Machine WHERE Patient.PatientId = '" + patient.Id.ToString() + "' AND ScheduledActivity.ObjectStatus='Active' AND ScheduledActivity.ScheduledActivityCode = 'Open' AND ScheduledActivity.PatientSer=Patient.PatientSer AND Attendee.ActivityInstanceSer=ScheduledActivity.ActivityInstanceSer AND Attendee.ObjectStatus='Active' AND Machine.MachineType = 'RadiationDevice' AND Machine.ResourceSer=Attendee.ResourceSer and ScheduledActivity.ScheduledStartTime > '" + date.ToString("yyyy-MM-dd") + " 00:00:00' ORDER BY ScheduledStartTime");
            if (bookings.Rows.Count > 0)
            {
                foreach (DataRow row in bookings.Rows)
                {
                    bookedMachineId.Add((string)row["MachineId"]);
                    p16_value_detailed += (string)row["ScheduledStartTime"].ToString() + ": " + (string)row["MachineId"] + "\r\n";
                }
                bookedMachineIdCommon = bookedMachineId.ToArray().GroupBy(v => v)
                    .OrderByDescending(g => g.Count())
                    .First()
                    .Key;
                p16_value += ", Bokat: " + bookedMachineIdCommon + (bookedMachineId.Distinct().Count() == 1 ? " (enhetligt)" : " (tvetydigt)");
                if (String.Equals(treatMachineIdCommon, bookedMachineIdCommon, StringComparison.OrdinalIgnoreCase) && bookedMachineId.Distinct().Count() == 1 && machineId.Distinct().Count() == 1)
                    p16_status = AutoCheckStatus.PASS;
                else if (String.Equals(treatMachineIdCommon, bookedMachineIdCommon, StringComparison.OrdinalIgnoreCase))
                    p16_status = AutoCheckStatus.MANUAL;
                else
                    p16_status = AutoCheckStatus.FAIL;
            }
            else
            {
                p16_value += ", Bokat: -";
                p16_status = AutoCheckStatus.WARNING;
            }

            checklistItems.Add(new ChecklistItem("P16. Konsekvens mellan planerad och bokad behandlingsapparat.", "Kontrollera att patienten är bokad till den behandlingsapparat som planen är planerad för.", p16_value, p16_value_detailed, p16_status));
        }
    }
}