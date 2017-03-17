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
                   string.Compare(image.Series.ImagingDeviceId, "PET/CT 02") == 0)
                {
                    if (image.Comment.IndexOf("RT") == 0)
                        i1_status = AutoCheckStatus.PASS;
                    else
                        i1_status = AutoCheckStatus.FAIL;
                }
                else
                    i1_status = AutoCheckStatus.FAIL;
                i1_value = image.Series.ImagingDeviceId + ", " + image.Comment;
            }
            else
            {
                i1_status = AutoCheckStatus.FAIL;
                i1_value = "-";
            }
            if (checklistType == ChecklistType.MasterPlan || checklistType == ChecklistType.MasterPlanIMRT)
                i1_status = AutoCheckStatus.UNKNOWN;
            checklistItems.Add(new ChecklistItem("I1. CT-protokoll för radioterapi har använts", "Kontrollera att ett CT-protokoll för radioterapi har använts:\r\n• Eclipse: Se Image comment under Image properties (för serien) eller protokollet", i1_value, i1_status));

            // Will now use information from Prescription rather than verifying against ChecklistType
            string i2_value = "CT-underlag: ";
            AutoCheckStatus i2_status = AutoCheckStatus.MANUAL;
            if (image != null)
                i2_value += image.Id + "; Ordination: ";
            else
            {
                i2_value += "-" + "; Ordination: ";
                i2_status = AutoCheckStatus.FAIL;
            }
            DataTable prescription = AriaInterface.Query("select Gating from Prescription, PlanSetup where PlanSetup.PrescriptionSer = Prescription.PrescriptionSer and PlanSetup.PlanSetupSer = '" + planSetupSer.ToString() + "'");
            if (prescription.Rows.Count == 1)
            {
                if (prescription.Rows[0]["Gating"] != DBNull.Value)
                    i2_value += (string)prescription.Rows[0]["Gating"];
                else
                    i2_value += "Friandning";
            }
            else
                i2_value += "Obestämbart";

            /*
            if (image != null)
            {
                if (checklistType == ChecklistType.EclipseGating)
                {
                    if (image.Comment.ToLower().IndexOf("gating") != -1 && image.Id.ToLower().IndexOf("gating") != -1 || image.Comment.ToLower().IndexOf("bh") != -1 && image.Id.ToLower().IndexOf("bh") != -1)
                        i2_status = AutoCheckStatus.PASS;
                    else
                        i2_status = AutoCheckStatus.FAIL;
                }
                else
                {
                    if (image.Comment.ToLower().IndexOf("gating") != -1 || image.Id.ToLower().IndexOf("gating") != -1 || image.Comment.ToLower().IndexOf("bh") != -1 || image.Id.ToLower().IndexOf("bh") != -1)
                        i2_status = AutoCheckStatus.FAIL;
                    else
                        i2_status = AutoCheckStatus.PASS;
                }
                i2_value = image.Comment + ", " + image.Id;
            }
            else
            {
                i2_status = AutoCheckStatus.FAIL;
                i2_value = "-";
            }
            if (checklistType == ChecklistType.MasterPlan || checklistType == ChecklistType.MasterPlanIMRT)
                i2_status = AutoCheckStatus.UNKNOWN;
            */
            checklistItems.Add(new ChecklistItem("I2. CT-studie är korrekt m.a.p. gatingordination", "Kontrollera att den korrekta CT-studien med avseende på gatingordination har använts.", i2_value, i2_status));

            string i3_value = string.Empty;
            AutoCheckStatus i3_status = AutoCheckStatus.UNKNOWN;
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
                        i3_status = AutoCheckStatus.PASS;
                    else
                        i3_status = AutoCheckStatus.WARNING;
                    // DICOM -> IEC61217: z -> y
                    i3_value = "-y: " + minusZ.ToString("0.0") + " cm, +y: " + plusZ.ToString("0.0") + " cm";
                }
            }
            checklistItems.Add(new ChecklistItem("I3. Axiell utökning av beräkningsvolym gjorts då det behövs", "Kontrollera att axiell utökning av beräkningsvolym gjorts då det behövs (<4 cm mellan target och första/sista snittet i 3D-volymen) och även att det inte gjorts då det är oberättigat (t.ex. superior för skalle).", i3_value, i3_status));

            string i4_value= (image == null ? "-" : image.ImagingOrientation.ToString());
            checklistItems.Add(new ChecklistItem("I4. Patientriktning har angivits korrekt vid CT-undersökningen", "Kontrollera att patientriktning har angivits korrekt vid CT-undersökningen genom att jämföra orienteringsfigur mot CT-data.", i4_value, AutoCheckStatus.MANUAL));

            AutoCheckStatus i5_status = AutoCheckStatus.MANUAL;
            string i5_value = ("Planorientering: " + planSetup.TreatmentOrientation.ToString());
            i5_value += "; CT-orientering: " + i4_value;
            if (!String.Equals(planSetup.TreatmentOrientation.ToString(), image.ImagingOrientation.ToString()))
                i5_status = AutoCheckStatus.WARNING;
            checklistItems.Add(new ChecklistItem("I5. Orientering är konsekvent mellan CT-undersökning och behandlingsplan.", "Kontrollera att samma orientering valts för CT-undersökning och behandlingsplanen om inte särskilda skäl föreligger.", i5_value, i5_status));

            string i6_value = (checklistType==ChecklistType.EclipseGating ? "Obs Gating! Referenspunkt sätts utifrån icke-gatad CT" : string.Empty);
            checklistItems.Add(new ChecklistItem("I6. Referenspunkten (anatomisk) är korrekt placerad", "Kontrollera att referenspunkten (anatomisk) är korrekt placerad (User Origin i Eclipse). Observera att på patienter som ska ha gating sätts User Origin utifrån det icke gatade CT-underlaget.", i6_value, AutoCheckStatus.MANUAL));


        }
    }
}
