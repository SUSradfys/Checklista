using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Data;

namespace Checklist
{
    public partial class Checklist
    {
        public void I()
        {
            checklistItems.Add(new ChecklistItem("I. Bildmaterial"));

            string i1_value = string.Empty;
            AutoCheckStatus i1_status;
            if (image != null && image.Series != null)
            {
                if (string.Compare(image.Series.ImagingDeviceId, "CT_A") == 0 ||
                   string.Compare(image.Series.ImagingDeviceId, "CT_B") == 0 ||
                   string.Compare(image.Series.ImagingDeviceId, "CT_C") == 0 ||
                   string.Compare(image.Series.ImagingDeviceId, "PET/CT 01") == 0 ||
                   string.Compare(image.Series.ImagingDeviceId, "PET/CT 02") == 0 ||
                   string.Compare(image.Series.ImagingDeviceId, "PET/CT 03") == 0)
                {
                    if (image.Comment.IndexOf("RT") == 0)
                        i1_status = AutoCheckStatus.PASS;
                    else
                        i1_status = AutoCheckStatus.FAIL;
                    i1_value = image.Series.ImagingDeviceId + ", " + image.Comment;
                }
                // Specific checks for synthetic CTs
                else if (string.Compare(image.Series.ImagingDeviceId, "sCT_MR_A") == 0)
                {
                    // set synthetic CT
                    syntheticCT = true;
                    string trigger = "MR acquisition: ";
                    i1_status = AutoCheckStatus.MANUAL;
                    //string MRparameterCheckFile = patient.LastName + "_" + image.Series.Study.CreationDateTime.Value.ToString("yyyyMMdd") + "_" + image.Series.Study.CreationDateTime.Value.ToString("HHmmss") + ".txt";
                    string MRparameterCheckFile = patient.LastName + "_" + image.Comment.Substring(image.Comment.IndexOf(trigger) + trigger.Length).Trim() + ".txt";
                    string MRparameterCheckResult = ValidateMR(MRparameterCheckFile);
                    if (!String.Equals("MRI SYNTHETIC CT PARAMETERS OK", MRparameterCheckResult))
                        i1_status = AutoCheckStatus.FAIL;
                    i1_value = image.Series.ImagingDeviceId + ", " + MRparameterCheckResult + ", " + image.Comment;
                }

                else
                { 
                    i1_status = AutoCheckStatus.FAIL;
                    i1_value = image.Series.ImagingDeviceId + ", " + image.Comment;
                }
            }
            else
            {
                i1_status = AutoCheckStatus.FAIL;
                i1_value = "-";
            }
            if (checklistType == ChecklistType.MasterPlan || checklistType == ChecklistType.MasterPlanIMRT)
                i1_status = AutoCheckStatus.UNKNOWN;
            checklistItems.Add(new ChecklistItem("I1. CT-protokoll för radioterapi har använts", "Kontrollera att ett CT-protokoll för radioterapi har använts:\r\n• Eclipse: Se Image comment under Image properties (för serien) eller protokollet", i1_value, i1_status));

            if (syntheticCT == true)
            {
                string sCT_version = "v1.1.2";
                // The version is first in the ImageComment of each slice. Select the first slice in the Image (the first item is the volume, so skip that). Split the string on whitespace and take the first item
                string i2_value = image.Series.Images.Where(img => img.ZSize == 1).FirstOrDefault().Comment.ToString().Split(' ').FirstOrDefault().ToString();

                AutoCheckStatus i2_status = CheckResult(string.Compare(i2_value, sCT_version) == 0);
                checklistItems.Add(new ChecklistItem("I2. MRI-planner version", "Kontrollera att korrekt version av MRI-planner använts vid generering av sCT\r\n• " + sCT_version, i2_value, i2_status));

            }

            // Will now use information from Prescription rather than verifying against ChecklistType
            string i3_value = "CT-underlag: ";
            AutoCheckStatus i3_status = AutoCheckStatus.MANUAL;
            if (image != null)
                i3_value += image.Id + "; Ordination: ";
            else
            {
                i3_value += "-" + "; Ordination: ";
                i3_status = AutoCheckStatus.FAIL;
            }

            
            DataTable prescription = AriaInterface.Query("select Gating from Prescription, PlanSetup where PlanSetup.PrescriptionSer = Prescription.PrescriptionSer and PlanSetup.PlanSetupSer = '" + planSetupSer.ToString() + "'");
            switch (prescription.Rows.Count)
            {
                case 0:
                    if (i3_status != AutoCheckStatus.FAIL)
                        i3_status = AutoCheckStatus.WARNING;
                    i3_value = "Ordination saknas";
                    break;
                case 1:
                    if (prescription.Rows[0]["Gating"] != DBNull.Value)
                        i3_value += (string)prescription.Rows[0]["Gating"];
                    else
                        i3_value += "Friandning";
                    break;
                default:
                    i3_value += "Obestämbart";
                    break;
            }

            /*
            if (image != null)
            {
                if (checklistType == ChecklistType.EclipseGating)
                {
                    if (image.Comment.ToLower().IndexOf("gating") != -1 && image.Id.ToLower().IndexOf("gating") != -1 || image.Comment.ToLower().IndexOf("bh") != -1 && image.Id.ToLower().IndexOf("bh") != -1)
                        i3_status = AutoCheckStatus.PASS;
                    else
                        i3_status = AutoCheckStatus.FAIL;
                }
                else
                {
                    if (image.Comment.ToLower().IndexOf("gating") != -1 || image.Id.ToLower().IndexOf("gating") != -1 || image.Comment.ToLower().IndexOf("bh") != -1 || image.Id.ToLower().IndexOf("bh") != -1)
                        i3_status = AutoCheckStatus.FAIL;
                    else
                        i3_status = AutoCheckStatus.PASS;
                }
                i3_value = image.Comment + ", " + image.Id;
            }
            else
            {
                i3_status = AutoCheckStatus.FAIL;
                i3_value = "-";
            }
            if (checklistType == ChecklistType.MasterPlan || checklistType == ChecklistType.MasterPlanIMRT)
                i3_status = AutoCheckStatus.UNKNOWN;
            */
            checklistItems.Add(new ChecklistItem("I3. CT-studie är korrekt m.a.p. gatingordination", "Kontrollera att den korrekta CT-studien med avseende på gatingordination har använts.", i3_value, i3_status));

            string i4_value = string.Empty;
            AutoCheckStatus i4_status = AutoCheckStatus.UNKNOWN;
            if (structureSet != null)
            {
                int firstImagePlane = int.MaxValue;
                int lastImagePlane = int.MinValue;
                foreach (Structure structure in structureSet.Structures)
                {
                    if (string.Compare(structure.DicomType, "PTV") == 0)
                    {
                        for (int imagePlane = 0; imagePlane < image.ZSize; imagePlane++)
                        {
                            VVector[][] contour = structure.GetContoursOnImagePlane(imagePlane);
                            if (contour.Length > 0 && contour[0].Length > 0)
                            {
                                if (imagePlane < firstImagePlane)
                                    firstImagePlane = imagePlane;
                                if (imagePlane > lastImagePlane)
                                    lastImagePlane = imagePlane;
                            }
                        }
                    }
                }
                if (firstImagePlane != int.MaxValue && lastImagePlane != int.MinValue)
                {
                    double minusZ = 0.1 * firstImagePlane * image.ZRes;
                    double plusZ = 0.1 * (image.ZSize - lastImagePlane - 1) * image.ZRes;
                    if (minusZ >= 4 && plusZ >= 4)
                        i4_status = AutoCheckStatus.PASS;
                    else
                        i4_status = AutoCheckStatus.WARNING;
                    // DICOM -> IEC61217: z -> y
                    i4_value = "-y: " + minusZ.ToString("0.0") + " cm, +y: " + plusZ.ToString("0.0") + " cm";
                }
            }
            checklistItems.Add(new ChecklistItem("I4. Axiell utökning av beräkningsvolym gjorts då det behövs", "Kontrollera att axiell utökning av beräkningsvolym gjorts då det behövs (<4 cm mellan target och första/sista snittet i 3D-volymen) och även att det inte gjorts då det är oberättigat (t.ex. superior för skalle).", i4_value, i4_status));

            string i5_value= (image == null ? "-" : image.ImagingOrientation.ToString());
            checklistItems.Add(new ChecklistItem("I5. Patientriktning har angivits korrekt vid CT-undersökningen", "Kontrollera att patientriktning har angivits korrekt vid CT-undersökningen genom att jämföra orienteringsfigur mot CT-data.", i5_value, AutoCheckStatus.MANUAL));

            AutoCheckStatus i6_status = AutoCheckStatus.MANUAL;
            string i6_value = ("Planorientering: " + planSetup.TreatmentOrientation.ToString());
            i6_value += "; CT-orientering: " + i5_value;
            if (!String.Equals(planSetup.TreatmentOrientation.ToString(), image.ImagingOrientation.ToString()))
                i6_status = AutoCheckStatus.WARNING;
            checklistItems.Add(new ChecklistItem("I6. Orientering är konsekvent mellan CT-undersökning och behandlingsplan.", "Kontrollera att samma orientering valts för CT-undersökning och behandlingsplanen om inte särskilda skäl föreligger.", i6_value, i6_status));

            string i7_value = (checklistType==ChecklistType.EclipseGating ? "Obs Gating! Referenspunkt sätts utifrån icke-gatad CT" : string.Empty);
            checklistItems.Add(new ChecklistItem("I7. Referenspunkten (anatomisk) är korrekt placerad", "Kontrollera att referenspunkten (anatomisk) är korrekt placerad (User Origin i Eclipse). Observera att på patienter som ska ha gating sätts User Origin utifrån det icke gatade CT-underlaget.", i7_value, AutoCheckStatus.MANUAL));


        }
    }
}
