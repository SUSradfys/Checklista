using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using VMS.TPS.Common.Model.API;
using System.Windows.Forms;

namespace Checklist
{
    public partial class Checklist
    {
        public void U() // The name U is kept for historical reasons. Might change to R in future versions.
        {
            checklistItems.Add(new ChecklistItem("R. Strålanmälan/Ordination"));

            string r1_imageid = string.Empty;
            if (planSetup.StructureSet != null && planSetup.StructureSet.Image != null)
                r1_imageid = planSetup.StructureSet.Image.Id;
            string r1_value = "Personnummer: " + patient.Id + ", Course: " + course.Id + ", Plan: " + planSetup.Id + ", CT: " + (image == null ? "-" : image.Id);
            string r1_value_detail = string.Empty;
            DataTable remarks = AriaInterface.Query("select Image.ImageNotes from Image, Series where Series.SeriesUID = '" + image.Series.UID.ToString() + "' and Image.SeriesSer = Series.SeriesSer and Image.ImageType = 'Image' and Image.ImageId = '" + image.Id.ToString() + "'");
            if (remarks.Rows.Count == 1 && remarks.Rows[0][0] != DBNull.Value)
            {
                r1_value_detail = (string)remarks.Rows[0][0];
                string remark = (string)remarks.Rows[0][0];
                int count = remark.Select((c, i) => remark.Substring(i)).Count(sub => sub.StartsWith("User"));
                if (count > 1)
                    MessageBox.Show(remark, "Aktuella remarks");
            }
            checklistItems.Add(new ChecklistItem("R1. Jämför id (course, plan, CT-set, patient) mellan strålanmälan, protokoll och Aria", "Kontrollera att \r\n  • Patientens personnummer stämmer överens mellan strålanmälan, protokoll och Aria\r\n  • Course, plannamn och CT-set stämmer överens mellan protokoll och Aria.", r1_value, AutoCheckStatus.MANUAL));

            AutoCheckStatus r2_status = AutoCheckStatus.FAIL;
            string r2_value = string.Empty;
            string r2_value_detail = string.Empty;
            string prescriptionVolume = string.Empty;
            long prescriptionAnatomySer = long.MinValue;
            bool guessedVolume = false;
            bool multiplePrescriptionLevels = false;
            string planningVolume = string.Empty;
            DataTable prescription = new DataTable();

            // If/else based on wether prescription exists or not
            if (prescSer > 0)  // prescription exists
            {
                DataTable planning = AriaInterface.Query("select distinct PlanSetupSer, PrimaryPTVSer, PatientVolumeSer, StructureId from PlanSetup, Structure where PlanSetup.PlanSetupSer = " + planSetupSer.ToString() + "  and PlanSetup.PrimaryPTVSer = Structure.PatientVolumeSer");
                if (planning.Rows.Count == 1 && planning.Rows[0][3] != DBNull.Value)
                    planningVolume = (string)planning.Rows[0][3];
                prescription = AriaInterface.Query("select distinct PlanSetupSer, PlanSetup.PrescriptionSer, PrescriptionAnatomy.PrescriptionSer, PrescriptionAnatomy.PrescriptionAnatomySer, PrescriptionAnatomyItem.PrescriptionAnatomySer, ItemType, ItemValue, Prescription.Status, Prescription.PrescriptionSer, Prescription.PrescriptionName, Prescription.Notes from PlanSetup, Prescription, PrescriptionAnatomy, PrescriptionAnatomyItem where PlanSetup.PlanSetupSer = " + planSetupSer.ToString() + " and PlanSetup.PrescriptionSer = PrescriptionAnatomy.PrescriptionSer and PrescriptionAnatomy.PrescriptionAnatomySer = PrescriptionAnatomyItem.PrescriptionAnatomySer and PrescriptionAnatomyItem.ItemType = 'VOLUME ID' and PlanSetup.PrescriptionSer = Prescription.PrescriptionSer");
                if (prescription.Rows.Count > 0 && prescription.Rows[0][6] != DBNull.Value)
                {
                    //string volumeName = string.Empty;
                    string prescriptionStatus = (string)prescription.Rows[0][7];
                    string prescriptionName = (string)prescription.Rows[0][9];
                    if (prescription.Rows[0][10] != DBNull.Value)
                        r2_value_detail = (string)prescription.Rows[0][10];

                    if (prescription.Rows.Count == 1)
                    {
                        prescriptionVolume = (string)prescription.Rows[0][6];
                        prescriptionAnatomySer = (long)prescription.Rows[0][3];

                    }
                    /*
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
                    */

                    r2_status = CheckResult(string.Compare(prescriptionStatus, "Approved") == 0);
                    r2_value = prescriptionName + ": " + prescriptionStatus;
                }
                else if (prescription.Rows.Count == 0)
                    r2_value = "Ordination saknas";
            }
            else  // prescription does not exist
            {
                r2_value = "Ordination saknas/är inte kopplad";
                //r2_value_detail = "Kontrollen kan ej utföras korrekt utan kopplad ordination.";
                r2_status = AutoCheckStatus.WARNING;
            }
            if (String.IsNullOrEmpty(r2_value_detail))
                checklistItems.Add(new ChecklistItem("R2. Status på kopplad ordination.", "Kontrollera att planen är kopplad till en ordination med status 'Approved'.", r2_value, r2_status));
            else
                checklistItems.Add(new ChecklistItem("R2. Status på kopplad ordination.", "Kontrollera att planen är kopplad till en ordination med status 'Approved'.", r2_value, r2_value_detail, r2_status));

            string r3_value = String.Empty;
            /*
            AutoCheckStatus r3_status = AutoCheckStatus.MANUAL;
            if (multiplePrescriptionLevels == true && guessedVolume == false)
            {
                r3_value = "Multipla ordinationsvolymer existerar. Ingen matchar den planerade volymen. ";
                r3_status = AutoCheckStatus.MANUAL;
            }
            else if (multiplePrescriptionLevels == true && guessedVolume == true)
            {
                r3_value = "Multipla ordinationsvolymer existerar. Följande matchar den planerade volymen: " + prescriptionVolume + ", ";
                r3_status = AutoCheckStatus.MANUAL;
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
            */
            switch (prescription.Rows.Count)
            {
                case 0:
                    r3_value += "Ordination saknas. ";
                    break;
                case 1:
                    r3_value += "Ordinerad volym: ";
                    break;
                default:
                    r3_value += "Ordinerade volymer: ";
                    break;
            }
            if (prescription.Rows.Count > 1)
                r3_value = "Ordinerade volymer: ";
            else
                r3_value = "Ordinerad volym: ";
            foreach (DataRow row in prescription.Rows)
            {
                r3_value += (string)row[6] + ", ";
            }
            r3_value += "Planerad volym: " + (planningVolume == string.Empty ? "-" : planningVolume);
            r3_value = r3_value.Replace(", Planerad volym", "; Planerad volym");
            checklistItems.Add(new ChecklistItem("R3. Ordinerad volym stämmer överens med planerad volym.", "Kontrollera att volymen som planens primära referenspunkt tillhör motsvarar den volym som det är ordinerat till.", r3_value, AutoCheckStatus.MANUAL));

            AutoCheckStatus r4_status = AutoCheckStatus.UNKNOWN;
            string r4_value = string.Empty;
            string r4_value_detailed = string.Empty;

            List<int> numberOfFractions = new List<int>();
            List<double> dosePerFraction = new List<double>();
            List<double> totalDose = new List<double>();

            switch (prescription.Rows.Count)
            {
                case 0:
                    r4_status = AutoCheckStatus.WARNING;
                    r4_value = "Ordination: Saknas";
                    break;
                default:
                    foreach (DataRow row in prescription.Rows)
                    {
                        string volumeName = (string)row[6];
                        long ser = (long)row[3];
                        DataTable prescriptionItem = AriaInterface.Query("select NumberOfFractions, ItemType, ItemValue, PrescriptionAnatomyItem.PrescriptionAnatomySer, PrescriptionAnatomy.PrescriptionAnatomySer, PrescriptionAnatomy.PrescriptionSer, Prescription.PrescriptionSer  from Prescription, PrescriptionAnatomy, PrescriptionAnatomyItem where PrescriptionAnatomy.PrescriptionAnatomySer = " + ser.ToString() + " and PrescriptionAnatomy.PrescriptionAnatomySer = PrescriptionAnatomyItem.PrescriptionAnatomySer and PrescriptionAnatomy.PrescriptionSer = Prescription.PrescriptionSer");
                        double tdose = -1, dosepf = -1;
                        foreach (DataRow itemRow in prescriptionItem.Rows)
                        {
                            numberOfFractions.Add((int)prescriptionItem.Rows[0]["NumberOfFractions"]);
                            if (String.Equals((string)itemRow["ItemType"], "Total dose", StringComparison.OrdinalIgnoreCase))
                            {
                                double.TryParse((string)itemRow["ItemValue"], out tdose);
                                totalDose.Add(tdose);
                            }
                            if (String.Equals((string)itemRow["ItemType"], "Dose per fraction", StringComparison.OrdinalIgnoreCase))
                            {
                                double.TryParse((string)itemRow["ItemValue"], out dosepf);
                                dosePerFraction.Add(dosepf);
                            }
                        }
                        if (tdose > 0 && dosepf > 0)
                            r4_value_detailed += (r4_value_detailed == string.Empty ? "Ordination: \r\n" : "\r\n") + "  • Volym: " + volumeName + "\r\n  • Fraktionsdos: " + dosepf.ToString("0.000") + " Gy \r\n  • Antal fraktioner: " + numberOfFractions.LastOrDefault().ToString() + "\r\n  • Totaldos: " + tdose.ToString("0.000") + " Gy\r\n";
                    }

                    // Check if numberOfFractions, dosePerFraction are distinct
                    if (numberOfFractions.Distinct().ToList().Count > 1)
                    {
                        r4_status = AutoCheckStatus.FAIL;
                        r4_value = "Ordination: Inkonsekvent antal fraktioner";
                    }
                    else
                    {
                        if (dosePerFraction.Distinct().ToList().Count > 1)
                        {
                            r4_status = AutoCheckStatus.MANUAL;
                            r4_value = "Ordination: SIB * " + numberOfFractions[0].ToString();
                        }
                        if (numberOfFractions.Distinct().ToList().Count == 1 && dosePerFraction.Distinct().ToList().Count == 1)
                        {
                            r4_value = "Ordination: " + dosePerFraction[0].ToString("0.000") + "Gy * " + numberOfFractions[0].ToString() + " = " + totalDose[0].ToString("0.000") + " Gy";
                        }
                    }
                    break;
            }

            if (fractionation == null)
                r4_status = AutoCheckStatus.FAIL;
            else
            {
                if (r4_status == AutoCheckStatus.UNKNOWN)
                {
                    r4_status = CheckResult(numberOfFractions[0] == fractionation.NumberOfFractions && Math.Round(dosePerFraction[0], 3) == Math.Round(fractionation.PrescribedDosePerFraction.Dose, 3) && Math.Round(totalDose[0], 3) == Math.Round(planSetup.TotalPrescribedDose.Dose, 3));
                }
                // Even for a SIB we need to check the number of fractions
                if (r4_status == AutoCheckStatus.MANUAL)
                {
                    if (numberOfFractions[0] != fractionation.NumberOfFractions)
                        r4_status = AutoCheckStatus.FAIL;
                }
                r4_value += "; Planerat: " + fractionation.PrescribedDosePerFraction.ToString() + " * " + fractionation.NumberOfFractions.ToString() + " = " + planSetup.TotalPrescribedDose.ToString();
                r4_value_detailed += (r4_value_detailed == string.Empty ? "" : "\r\n") + "Planerat: \r\n  • Volym: " + planningVolume + "\r\n  • Fraktionsdos: " + fractionation.PrescribedDosePerFraction.ToString() + "\r\n  • Antal fraktioner: " + fractionation.NumberOfFractions.ToString() + "\r\n  • Totaldos: " + planSetup.TotalPrescribedDose.ToString();
            }
            checklistItems.Add(new ChecklistItem("R4. Planen är konsekvent med vad som ordinerats.", "Kontrollera att planen är konsekvent med vad som ordinerats gällande: \r\n  • Fraktionsdos\r\n  • Antal fraktioner\r\n  • Totaldos", r4_value, r4_value_detailed, r4_status));

        }
    }
}
