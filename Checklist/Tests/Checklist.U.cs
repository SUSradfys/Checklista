using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using VMS.TPS.Common.Model.API;

namespace Checklist
{
    public partial class Checklist
    {
        public void U() // The name U is kept for historical reasons. Might change to R in future versions.
        {
            checklistItems.Add(new ChecklistItem("R. Remiss/Ordination"));

            string r1_imageid = string.Empty;
            if (planSetup.StructureSet != null && planSetup.StructureSet.Image != null)
                r1_imageid = planSetup.StructureSet.Image.Id;
            string r1_value = "Personnummer: " + patient.Id + ", Course: " + course.Id + ", Plan: " + planSetup.Id + ", CT: " + (image == null ? "-" : image.Id);
            checklistItems.Add(new ChecklistItem("R1. Jämför id (course, plan, CT-set, patient) mellan protokoll, behandlingskort och Aria", "Kontrollera att Course, plannamn, CT-set och patientens personnummer stämmer överens mellan protokoll behandlingskort och Aria.", r1_value, AutoCheckStatus.MANUAL));

            AutoCheckStatus r2_status = AutoCheckStatus.FAIL;
            string r2_value = string.Empty;
            string prescriptionVolume = string.Empty;
            long prescriptionAnatomySer = long.MinValue;
            bool guessedVolume = false;
            bool multiplePrescriptionLevels = false;
            string planningVolume = string.Empty;
            DataTable planning = AriaInterface.Query("select distinct PlanSetupSer, PrimaryPTVSer, PatientVolumeSer, StructureId from PlanSetup, Structure where PlanSetup.PlanSetupSer = " + planSetupSer.ToString() + "  and PlanSetup.PrimaryPTVSer = Structure.PatientVolumeSer");
            if (planning.Rows.Count == 1 && planning.Rows[0][3] != DBNull.Value)
                    planningVolume = (string)planning.Rows[0][3];
            DataTable prescription = AriaInterface.Query("select distinct PlanSetupSer, PlanSetup.PrescriptionSer, PrescriptionAnatomy.PrescriptionSer, PrescriptionAnatomy.PrescriptionAnatomySer, PrescriptionAnatomyItem.PrescriptionAnatomySer, ItemType, ItemValue, Prescription.Status, Prescription.PrescriptionSer, Prescription.PrescriptionName  from PlanSetup, Prescription, PrescriptionAnatomy, PrescriptionAnatomyItem where PlanSetup.PlanSetupSer = " + planSetupSer.ToString() + " and PlanSetup.PrescriptionSer = PrescriptionAnatomy.PrescriptionSer and PrescriptionAnatomy.PrescriptionAnatomySer = PrescriptionAnatomyItem.PrescriptionAnatomySer and PrescriptionAnatomyItem.ItemType = 'VOLUME ID' and PlanSetup.PrescriptionSer = Prescription.PrescriptionSer");
            if (prescription.Rows.Count > 0 && prescription.Rows[0][6] != DBNull.Value)
            {
                //string volumeName = string.Empty;
                string prescriptionStatus = (string)prescription.Rows[0][7];
                string prescriptionName = (string)prescription.Rows[0][9];

                if (prescription.Rows.Count == 1)
                {
                    prescriptionVolume = (string)prescription.Rows[0][6];
                    prescriptionAnatomySer = (long)prescription.Rows[0][3];
                    
                }
                else
                {
                    multiplePrescriptionLevels = true;
                    foreach (DataRow row in prescription.Rows)
                    {
                        string volumeName = (string)row[6];
                        if (volumeName.IndexOf(planningVolume) == 0 && planningVolume.Length > 1)
                        {
                            prescriptionVolume = (string)row[6];
                            prescriptionAnatomySer = (long)row[3];
                            guessedVolume = true;
                            break;
                        }
                    }
                }

                r2_status = CheckResult(string.Compare(prescriptionStatus, "Approved") == 0);
                r2_value = prescriptionName + ": " + prescriptionStatus;
            }
            else if (prescription.Rows.Count == 0)
                r2_value = "Ordination saknas";
            checklistItems.Add(new ChecklistItem("R2. Kontrollera status på kopplad ordination.", "Kontrollera att det finns en ordination kopplad till planen samt att dess status är satt till 'Approved'.", r2_value, r2_status));

            if (r2_status == AutoCheckStatus.PASS)
            {
                string r3_value;
                AutoCheckStatus r3_status = AutoCheckStatus.MANUAL;
                if (multiplePrescriptionLevels == true && guessedVolume == false)
                {
                    r3_value = "Multipla ordinationsvolymer existerar. Ingen matchar den planerade volymen. ";
                    r3_status = AutoCheckStatus.WARNING;
                }
                else
                {
                    r3_value = "Ordinerad volym: " + prescriptionVolume + ", ";
                    if (prescriptionVolume.IndexOf(planningVolume) == 0 && planningVolume.Length > 1) // maybe this is too nice. Perhaps it should be a String.Compare. Possibly with a string split for prescritonVolume (using :)
                        r3_status = AutoCheckStatus.PASS;
                    else
                        r3_status = AutoCheckStatus.WARNING;
                }
                r3_value += "Planerad volym: " + (planningVolume == string.Empty ? "-" : planningVolume);
                checklistItems.Add(new ChecklistItem("R3. Kontrollera att ordinerad volym stämmer överens med planerad volym.", "Kontrollera att volymen som planens primära referenspunkt tillhör motsvarar den volym som det är ordinerat till.", r3_value, r3_status));

                AutoCheckStatus r4_status = AutoCheckStatus.UNKNOWN;
                string r4_value = string.Empty;
                string r4_value_detailed = string.Empty;
                if (r3_status == AutoCheckStatus.PASS || prescription.Rows.Count == 1)
                {
                    int numberOfFractions = int.MinValue;
                    double dosePerFraction = double.NaN;
                    double totalDose = double.NaN;
                    DataTable prescriptionItem = AriaInterface.Query("select NumberOfFractions, ItemType, ItemValue, PrescriptionAnatomyItem.PrescriptionAnatomySer, PrescriptionAnatomy.PrescriptionAnatomySer, PrescriptionAnatomy.PrescriptionSer, Prescription.PrescriptionSer  from Prescription, PrescriptionAnatomy, PrescriptionAnatomyItem where PrescriptionAnatomy.PrescriptionAnatomySer = " + prescriptionAnatomySer.ToString() + " and PrescriptionAnatomy.PrescriptionAnatomySer = PrescriptionAnatomyItem.PrescriptionAnatomySer and PrescriptionAnatomy.PrescriptionSer = Prescription.PrescriptionSer");
                    if (prescriptionItem.Rows.Count > 0)
                    {
                        numberOfFractions = (int)prescriptionItem.Rows[0]["NumberOfFractions"];
                        foreach (DataRow row in prescriptionItem.Rows)
                        {
                            if (String.Equals((string)row["ItemType"], "Total dose", StringComparison.OrdinalIgnoreCase))
                                double.TryParse((string)row["ItemValue"], out totalDose);
                            if (String.Equals((string)row["ItemType"], "Dose per fraction", StringComparison.OrdinalIgnoreCase))
                                double.TryParse((string)row["ItemValue"], out dosePerFraction);
                        }
                        r4_value = "Ordination: " + dosePerFraction.ToString("0.000") + " Gy * " + numberOfFractions.ToString() + " = " + totalDose.ToString("0.000") + " Gy";
                        r4_value_detailed = "Ordination: \r\n  • Fraktionsdos: " + dosePerFraction.ToString("0.000") + " Gy \r\n  • Antal fraktioner: " + numberOfFractions.ToString() + "\r\n  • Totaldos: " + totalDose.ToString("0.000") + " Gy\r\n";
                    }
                    if (fractionation != null)
                    {
                        r4_value += (r4_value == null ? "Ordination: - , " : ", ") + "Planerat: " + fractionation.PrescribedDosePerFraction.ToString() + " * " + fractionation.NumberOfFractions.ToString() + " = " + planSetup.TotalPrescribedDose.ToString();
                        r4_status = CheckResult(numberOfFractions == fractionation.NumberOfFractions && dosePerFraction == fractionation.PrescribedDosePerFraction.Dose && totalDose == planSetup.TotalPrescribedDose.Dose);
                    }
                }
                else
                {
                    r4_status = AutoCheckStatus.MANUAL;
                    if (fractionation != null)
                        r4_value += (r4_value == string.Empty ? "Ordination: Tvetydigt, " : ", ") + "Planerat: " + fractionation.PrescribedDosePerFraction.ToString() + " * " + fractionation.NumberOfFractions.ToString() + " = " + planSetup.TotalPrescribedDose.ToString();
                }
                if (fractionation == null)
                    r4_status = AutoCheckStatus.FAIL;
                else
                    r4_value_detailed += (r4_value_detailed == string.Empty ? "" : "\r\n") + "Planerat: \r\n  • Fraktionsdos: " + fractionation.PrescribedDosePerFraction.ToString() + "\r\n  • Antal fraktioner: " + fractionation.NumberOfFractions.ToString() + "\r\n  • Totaldos: " + planSetup.TotalPrescribedDose.ToString();
                checklistItems.Add(new ChecklistItem("R4. Kontrollera att ordination stämmer med vad som planerats.", "Kontrollera att ordination överensstämmer med plan vad gäller \r\n  • Fraktionsdos\r\n  • Antal fraktioner\r\n  • Totaldos", r4_value, r4_value_detailed, r4_status));
            }
            
        }
    }
}
