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

            string p1_value = planSetup.ApprovalStatus.ToString();
            AutoCheckStatus p1_status = CheckResult(planSetup.ApprovalStatus == PlanSetupApprovalStatus.PlanningApproved);
            checklistItems.Add(new ChecklistItem("P1. Planen är planning approved", "Kontrollera att planen är Planning Approved i Aria.", p1_value, p1_status));

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

            string p4_value = string.Empty;
            AutoCheckStatus p4_status = AutoCheckStatus.FAIL;
            DataTable dataTableUseGated = AriaInterface.Query("select distinct ExternalFieldCommon.MotionCompTechnique from Radiation,ExternalFieldCommon where Radiation.RadiationSer=ExternalFieldCommon.RadiationSer and Radiation.PlanSetupSer=" + planSetupSer.ToString());

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
            checklistItems.Add(new ChecklistItem("P4. Use Gated är korrekt", "Kontrollera att rutan Use Gated under Plan properties svarar mot ordination (läkare ordinerar gating under kommentarer under behandlingsordination i behandlingskortet)", p4_value, p4_status));

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
            
            string p8_value_detailed = string.Empty;
            string p8_value = String.Empty;
            bool isSplit = false;
            AutoCheckStatus p8_status = AutoCheckStatus.UNKNOWN;
            if (checklistType == ChecklistType.EclipseVMAT)
                isSplit = GetIsSplitVMAT(planSetup);
            foreach (Beam beam in planSetup.Beams)
            {
                if (!beam.IsSetupField)
                {
                    double openMU;
                    double wedgedMU;
                    GetMU(beam, out openMU, out wedgedMU);

                    p8_value_detailed += beam.Id + ":\r\n";
                    if (checklistType == ChecklistType.EclipseVMAT)
                    {
                        p8_value_detailed += "  Open: " + openMU.ToString("0.0") + " MU\r\n  " + beam.MetersetPerGy.ToString("0.0") + " MU/Gy\r\n";
                        if (beam.MetersetPerGy > 300 && !isSplit)
                            p8_value += (p8_value.Length == 0 ? string.Empty : ", ") + beam.Id + ": För många MU/Gy";
                        else if (beam.MetersetPerGy > 550 && isSplit)
                            p8_value += (p8_value.Length == 0 ? string.Empty : ", ") + beam.Id + ": För många MU/Gy";
                    }
                    else
                    {
                        p8_value_detailed += "  Open: " + openMU.ToString("0.0") + ", Wedged: " + wedgedMU.ToString("0.0") + "\r\n";  
                    }
                    p8_value_detailed += "  Energi: " + beam.EnergyModeDisplayName + "\r\n\r\n";

                    if (openMU < 10 && openMU != 0 || wedgedMU < 30 && wedgedMU != 0)
                        p8_value += (p8_value.Length == 0 ? string.Empty : ", ") + beam.Id + ": För få MU";
                    if (treatmentUnitManufacturer == TreatmentUnitManufacturer.Elekta && openMU + wedgedMU > 999)
                        p8_value += (p8_value.Length == 0 ? string.Empty : ", ") + beam.Id + ": För många MU";
                }
            }
            if (p8_value.Length > 0 )
            {
                if (checklistType == ChecklistType.EclipseVMAT)
                    p8_status = AutoCheckStatus.WARNING;
                else
                    p8_status = AutoCheckStatus.FAIL;
                p8_value = reorderBeamParam(p8_value, ",");
            }

            p8_value_detailed = reorderBeamParam(p8_value_detailed, "\r\n\r\n");
            checklistItems.Add(new ChecklistItem("P8. Fälten ser rimliga ut vad gäller form, energi, MU och korrektion av artefakter", "Kontrollera att fälten ser rimliga ut vad gäller form, energi, MU och korrektion av artefakter\r\n  • Riktlinje för RapidArc är max 300 MU/Gy om bländarna är utanför target under hela varvet (sett ur BEV). Vid delvis skärmat target är denna gräns max 550 MU/Gy.\r\n  • Öppna fält ska ha ≥10 MU och fält med fast kil (Elekta) ska ha ≥30 kilade MU.\r\n  •  För Elekta gäller dessutom att totala antalet MU per fält (öppet + kilet) ej får överstiga 999 MU.", p8_value, p8_value_detailed, p8_status));

            if (treatmentUnitManufacturer == TreatmentUnitManufacturer.Elekta)
            {
                AutoCheckStatus p9_status = AutoCheckStatus.UNKNOWN;
                string p9_value = ElektaMLCCheck(planSetup);
                p9_status = CheckResult(String.Compare(p9_value, "MLC positioner OK.", true) == 0);
                checklistItems.Add(new ChecklistItem("P9. MLC:n är indragen till X-bländare, och ett/två blad är öppna utanför Y-bländare", "Kontrollera att MLC:n är indragen till X-bländare eller innanför, och att ett helt bladpar är öppet utanför Y-bländare på resp. sida om Y1 resp. Y2 har decimal 0,7, 0,8 eller 0,9.", p9_value, p9_status));
            }

            string p10_value = "Metod: " + planSetup.PlanNormalizationMethod + ", target: " + planSetup.TargetVolumeID + ", prescribed percentage: " + planSetup.PrescribedPercentage + ", värde: " + planSetup.PlanNormalizationValue.ToString("0.0");
            AutoCheckStatus p10_status = AutoCheckStatus.MANUAL;
            double normLimitVMAT = 3.0;
            if (checklistType == ChecklistType.EclipseVMAT && Math.Abs(planSetup.PlanNormalizationValue - 100) > normLimitVMAT)
            {
                p10_status = AutoCheckStatus.FAIL;
            }
            checklistItems.Add(new ChecklistItem("P10. Normering är korrekt", "Kontrollera att planen är normerad på korrekt vis \r\n  • Normalt till targetvolymens medeldos i Eclipse (om särskilt skäl föreligger kan en punktnormering användas) respektive punktdos i MasterPlan. \r\n  • För stereotaktiska lungor i Eclipse normeras dosen till isocenter och ordineras till 75%-isodosen.\r\n  • För VMAT får normeringsvärdet skall normeringsvärdet vara i intervallet [0.970, 1.030].", p10_value, p10_status));

            string p11_value = string.Empty;            
            string p12_value = string.Empty;
            string p12_value_detailed = string.Empty;
            AutoCheckStatus p12_status = AutoCheckStatus.WARNING;
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
            int p12_numberOfPass = 0;
            for (int refPointNr = 0; refPointNr < referencePoints.Count; refPointNr++)
            {
                p12_value_detailed += (p12_value_detailed.Length == 0 ? string.Empty : "\r\n\r\n") + referencePoints[refPointNr].Id + ":\r\n";

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
                p12_value_detailed += "  Dosbidrag från aktuell plan: " + (fractionation == null ? double.NaN : referencePointDose[refPointNr] * (double)fractionation.NumberOfFractions).ToString("0.000") + " Gy, " + referencePointDose[refPointNr].ToString("0.000") + " Gy/fr " + " (Total dose limit: " + totalDoseLimit.ToString("0.000") + " Gy, daily limit: " + dailyDoseLimit.ToString("0.000") + " Gy, session limit: " + sessionDoseLimit.ToString("0.000") + " Gy)\r\n";                   
                p12_value_detailed += "  Totalt dosbidrag från samtliga godkändaplaner: " + referencePointTotalDose[refPointNr].ToString("0.000") + " Gy " + " (Total limit: " + totalDoseLimit.ToString("0.000") + " Gy)";
                
                if (activeReferencePoints.Contains(refPointNr)) // Reference point is present in the active plan
                {
                    p11_value += (p11_value.Length == 0 ? string.Empty : ", ") + referencePoints[refPointNr].Id + ": " + referencePointDose[refPointNr].ToString("0.000") + " Gy";
                    p12_value += (p12_value.Length == 0 ? string.Empty : ", ") + referencePoints[refPointNr].Id + ": (T:" + totalDoseLimit.ToString("0.000") + "/D:" + dailyDoseLimit.ToString("0.000") + "/S:" + sessionDoseLimit.ToString("0.000") + " Gy)";
                    
                    if (Math.Round(referencePointDose[refPointNr], 3) <= Math.Round(dailyDoseLimit, 3) &&
                        Math.Round(referencePointDose[refPointNr], 3) <= Math.Round(sessionDoseLimit, 3) &&
                        Math.Round(referencePointTotalDose[refPointNr], 3) == Math.Round(totalDoseLimit, 3))
                    {
                        p12_numberOfPass++;
                    }
                }
            }
            if (activeReferencePoints.Count > 0 && p12_numberOfPass == activeReferencePoints.Count)
                p12_status = AutoCheckStatus.UNKNOWN;
            checklistItems.Add(new ChecklistItem("P11. Referenspunkternas dosbidrag är korrekta", "Kontrollera att dosbidrag till referenspunkter (dos) är korrekta:\r\n  • Varje plan ska ha en punkt (primary reference point) som summerar upp till ordinerad dos för det största PTV som planen primärt behandlar.\r\n  • Om flera planer bidrar med dos till samma targetvolymer eller om en plan bidrar med dos till flera targetvolymer ska det finnas referenspunkter utan lokalisation i alla planer som summerar dosen till dessa volymer.\r\n  • Referenspunkterna ska inte ha dosbidrag från tidigare behandlingar.", p11_value, AutoCheckStatus.MANUAL));
            checklistItems.Add(new ChecklistItem("P12. Referenspunkternas gränser är korrekta", "Kontrollera att referenspunkternas gränser (dos) är korrekta", p12_value, p12_value_detailed, p12_status));

            if (checklistType == ChecklistType.Eclipse || checklistType == ChecklistType.EclipseGating)
                checklistItems.Add(new ChecklistItem("P13. Skarven är flyttad korrekt för skarvplan", "Skarvplaner: Skarven är flyttad korrekt och fälten är i övrigt likadana\r\n  • Bröstbehandlingar med kollimator i 0° för både huvudfält i fossa- och tang.-fält flyttas endast om eventuellt PTV_66 ligger i skarven.", string.Empty, AutoCheckStatus.MANUAL));


            AutoCheckStatus p14_status = AutoCheckStatus.UNKNOWN;
            string p14_value = string.Empty;
            int p14_numberOfPass = 0;
            List<string> machineId = new List<string>();
            foreach (Beam beam in planSetup.Beams)
            {
                machineId.Add(beam.TreatmentUnit.Id);
                if (String.Equals(beam.TreatmentUnit.ToString(), planSetup.Beams.First().TreatmentUnit.ToString()))
                    p14_numberOfPass += 1;
                p14_value += (p14_value.Length == 0 ? string.Empty : ", ") + beam.Id + ": " + beam.TreatmentUnit.Id;
            }
            if (p14_numberOfPass == numberOfBeams)
                p14_status = AutoCheckStatus.PASS;
            else
                p14_status = AutoCheckStatus.FAIL;
            p14_value = reorderBeamParam(p14_value, ",");
            checklistItems.Add(new ChecklistItem("P14. Konsekvent maskinval", "Kontrollera att samtliga fält är planerade till en och samma behandlingsapparat.", p14_value, p14_status));

            AutoCheckStatus p15_status = AutoCheckStatus.UNKNOWN;
            string p15_value = string.Empty;
            string p15_value_detailed = "Följande bokningar är aktiva:\r\n";
            string treatMachineIdCommon = machineId.ToArray().GroupBy(v => v)
                .OrderByDescending(g => g.Count())
                .First()
                .Key;
            p15_value = "Planerat: " + treatMachineIdCommon + (machineId.Distinct().Count() == 1 ? " (enhetligt)" : " (tvetydigt)");
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
                    p15_value_detailed += (string)row["ScheduledStartTime"].ToString() + ": " + (string)row["MachineId"] + "\r\n";
                }
                bookedMachineIdCommon = bookedMachineId.ToArray().GroupBy(v => v)
                    .OrderByDescending(g => g.Count())
                    .First()
                    .Key;
                p15_value += ", Bokat: " + bookedMachineIdCommon + (bookedMachineId.Distinct().Count() == 1 ? " (enhetligt)" : " (tvetydigt)");
                if (String.Equals(treatMachineIdCommon, bookedMachineIdCommon, StringComparison.OrdinalIgnoreCase) && bookedMachineId.Distinct().Count() == 1 && machineId.Distinct().Count() == 1)
                    p15_status = AutoCheckStatus.PASS;
                else if (String.Equals(treatMachineIdCommon, bookedMachineIdCommon, StringComparison.OrdinalIgnoreCase))
                    p15_status = AutoCheckStatus.MANUAL;
                else
                    p15_status = AutoCheckStatus.FAIL;
            }
            else
            {
                p15_value += ", Bokat: -";
                p15_status = AutoCheckStatus.WARNING;
            }

            checklistItems.Add(new ChecklistItem("P15. Kontrollera konsekvens mellan planerad och bokad behandlingsapparat.", "Kontrollera att patienten är bokad till den behandlingsapparat som planen är planerad för.", p15_value, p15_value_detailed, p15_status));
        }
    }
}