using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace Checklist
{
    public partial class Checklist
    {
        public void X()
        {
            checklistItems.Add(new ChecklistItem("X. Slutförande"));
            
            //checklistItems.Add(new ChecklistItem("X1. Skriv in antal MU, diodvärden och följande eventuella diodkorrektioner i behandlingskortet", "Skriv in antal MU, diodvärden och följande eventuella diodkorrektioner i behandlingskortet (se ”In vivo-dosimetri”-dokumentet för detaljer):\r\n  • Kil (Mimatordioder på Elekta)\r\n  • Kort SSD (IBA-dioder)\r\n  • Långt SSD (Mimatordioder)", string.Empty, AutoCheckStatus.MANUAL));

            string x2_value = string.Empty;
            AutoCheckStatus x2_status = AutoCheckStatus.MANUAL;
            if (image != null && image.Series != null)
            {
                x2_value = image.Series.ImagingDeviceId;
                if (string.Compare(image.Series.ImagingDeviceId, "CT_A") == 0 ||
                   string.Compare(image.Series.ImagingDeviceId, "CT_B") == 0 ||
                   string.Compare(image.Series.ImagingDeviceId, "CT_C") == 0)
                {
                    if (planSetup.Beams.Count() > 0)
                    {
                        double userY = -image.UserOrigin.y * 0.1;
                        double isoY = -(planSetup.Beams.First().IsocenterPosition.y - image.UserOrigin.y) * 0.1;
                        if (planSetup.TreatmentOrientation.ToString().IndexOf("Prone") != -1) // Change sign if Orientation is Prone.
                        { 
                            isoY *= -1;
                            userY *= -1;
                        }
                        double shiftY = 7.1;
                        double sumY = -isoY - userY + shiftY;
                        x2_value += ", Beräknad britshöjd: " + (-userY).ToString("0.0") + (-isoY >= 0 ? "+" : string.Empty) + (-isoY).ToString("0.0") + "+" + shiftY.ToString("0.0") + " = " + sumY.ToString("0.0") + " cm";
                        if (sumY < -30)
                            x2_status = AutoCheckStatus.WARNING;
                    }
                }
                else if (string.Compare(image.Series.ImagingDeviceId, "PET/CT 01") == 0)
                {
                    x2_value += ", Mät position manuellt";
                }
                else if (string.Compare(image.Series.ImagingDeviceId, "PET/CT 02") == 0)
                {
                    x2_value += ", Mät position manuellt";
                }
                else if (string.Compare(image.Series.ImagingDeviceId, "PET/CT 03") == 0)
                {
                    x2_value += ", Mät position manuellt";
                }
                else if (syntheticCT)
                {
                    x2_value += ", Mät position manuellt";
                }
            }
            checklistItems.Add(new ChecklistItem("X2. Förväntad britshöjd räknas ut och läggs in i Aria", "Räkna ut förväntad britshöjd och lägg in i Aria (på alla fält, inklusive setup-fält) i modulen Treatment Preparation i rutan för Couch Vrt:\r\n• Eclipse: -DICOM offset Z - isocenter Z + offset cm\r\n• Offset är 7,1 cm för CT_A, CT_B, CT_C\r\n• Observera att vid för prone byter DICOM-koordinaten tecken\r\n• Observera risk för kollision mellan gantry och bord vid Vrt < -30 cm\r\nVid SSD-teknik:\r\n  • Ska tjockleken på eventuell vacuumpåse bestämmas genom mätning i CT-bilderna och antecknas under Setup note\r\n  • Räkna ut förflyttning från fältet närmast 0° till övriga fält (Isocenterkoordinat för ursprungsfältet minus övriga fälts isocenterkoordinater) och anteckna detta på sida 2 i behandlingsprotokollet. Exempel: Relativ förflyttning från fält 1 till fält 2: ∆Vrt=25,0 cm.\r\n  • Skriv följande under Setup note: ”FHA-beh. Ring fysiker vid start.”", x2_value, x2_status));
            //checklistItems.Add(new ChecklistItem("X2. Förväntad britshöjd räknas ut och läggs in i Aria", "Räkna ut förväntad britshöjd och lägg in i Aria (på alla fält, inklusive setup-fält) i modulen Treatment Preparation i rutan för Couch Vrt:\r\n• Eclipse: -DICOM offset Z - isocenter Z + offset cm\r\nMasterPlan: -TPRP coordinate Z - isocenter Z + offset cm\r\n• Offset är 7,1 cm för CT_A, CT_B, CT_C och -17,5 för PET/CT 01 (kan dock variera beroende på britshöjd vid PET-undersökningen)\r\n• Observera risk för kollision mellan gantry och bord vid Vrt < -30 cm\r\nVid SSD-teknik:\r\n  • Ska tjockleken på eventuell vacuumpåse bestämmas genom mätning i CT-bilderna och antecknas under Setup note\r\n  • Räkna ut förflyttning från fältet närmast 0° till övriga fält (Isocenterkoordinat för ursprungsfältet minus övriga fälts isocenterkoordinater) och anteckna detta på sida 2 i behandlingsprotokollet. Exempel: Relativ förflyttning från fält 1 till fält 2: ∆Vrt=25,0 cm.\r\n  • Skriv följande under Setup note: ”FHA-beh. Ring fysiker vid start.”", x2_value, x2_status));
            // Add elinores corda computation here

            if (checklistType == ChecklistType.EclipseVMAT && GetVMATCoplanar(planSetup) == false)
            {
                // check that setup note for beam "Uppl*gg" contains the string
                AutoCheckStatus x3_status = AutoCheckStatus.FAIL;
                string defSetup = "OBS! Icke-coplanar behandling (britsrotation). Använd NC-plattan som förlängning av britsen.";
                DataTable setupNote = AriaInterface.Query("select SetupNote from Radiation where PlanSetupSer=" + planSetupSer.ToString() + " and UPPER(RadiationId) like 'UPPL%GG'");
                foreach (DataRow row in setupNote.Rows)
                    {
                        if (row["SetupNote"].ToString().IndexOf(defSetup) >= 0)
                        x3_status = AutoCheckStatus.PASS;
                    }
                if (x3_status == AutoCheckStatus.FAIL)
                {
                    Clipboard.SetText(defSetup + "\r\n");
                    checklistItems.Add(new ChecklistItem("X3. Notera icke coplanar VMAT under Setup note", "Planen i fråga är en icke coplanar VMAT behandling. Säkerställ att en notering om detta finns under planens Setup note. Den exakta formuleringen ska vara: \r\n" + defSetup, string.Empty, defSetup, x3_status));
                }
                else
                    checklistItems.Add(new ChecklistItem("X3. Notera icke coplanar VMAT under Setup note", "Planen i fråga är en icke coplanar VMAT behandling. Säkerställ att en notering om detta finns under planens Setup note. Den exakta formuleringen ska vara: \r\n" + defSetup, string.Empty, x3_status));
            }

            if (checklistType == ChecklistType.Eclipse || checklistType == ChecklistType.EclipseGating)
                checklistItems.Add(new ChecklistItem("X4. Genomför oberoende MU-kontroll", "Genomför obeorende MU-kontroll via RVP", "", AutoCheckStatus.MANUAL));

            if (checklistType == ChecklistType.EclipseVMAT)
            {
                AutoCheckStatus x5_status = AutoCheckStatus.MANUAL; 
                string x5_value = string.Empty;
                string x5_details = string.Empty;
                //lägg till kontroll att QC - plan finns och ger annars varning. WORK in progress
                //List<PlanSetup> allplans = patient.Courses.SelectMany(p => p.PlanSetups).ToList();
                //List<PlanSetup> verpallplans = allplans.Where(p => p.PlanIntent == "VERIFICATION").ToList();
                //List<PlanSetup> corrverplans = new List<PlanSetup>();

                //var verpallplansFilter = verpallplans.Where(x => x.VerifiedPlan != null).ToList();
                //var testuid = verpallplansFilter.Select(x => x.VerifiedPlan.UID).ToList();
                //    //foreach (PlanSetup p in verpallplansFilter)
                //    //{
                   
                   
                        
                    //    string curuid = p.VerifiedPlan.UID; 
                    //    if (curuid == planSetup.UID)
                    //        corrverplans.Add(p); 
                  
                    //}
                
                //int noQCplans = verpallplans.Count();
                //List<PlanSetup> corallplans = corrverplans.Where(p => p.Id.ToLower().Contains("qc") && p.Id.ToLower().Contains(planSetup.Id.ToLower().Substring(0, 3)) && p.Id.ToLower().Contains("d") && p.Id.ToLower().Contains("4")).ToList();

                //// Note if
                //try
                //{
                //    if (noQCplans > 0 && allplans.Count() == 0)

                //    {
                //        x5_status = AutoCheckStatus.WARNING;
                //        x5_value = "Det finns " + noQCplans + " QC-plan(er) kopplade till planen, men är inkorrekt namngivna.";
                //        x5_details = "Korrekt namngivning av QC-planer är: QC PX_X d4 eller QC PX_X delta4";
                //    }
                //    if (allplans.Count() > 1)
                //    {
                //        x5_status = AutoCheckStatus.MANUAL;
                //        string ids = string.Empty;
                //        foreach (PlanSetup p in allplans)
                //            ids = ids + (ids.Length == 0 ? p.Id : ", " + p.Id);
                //        x5_value = "QC-planer finns och är korrekt namngivna: " + ids;

                //    }
                //}
                //catch (Exception exception)
                //{
                //    x5_value = "Problem med att hitta QC-planer..." + exception.Message;
                //}

                checklistItems.Add(new ChecklistItem("X5. QC Course sätts till Completed.", "Sätt status på QC coursen till Completed.", x5_value, x5_details, x5_status));
            }
            if (checklistType == ChecklistType.EclipseGating && image.Comment.IndexOf("DIBH") != -1 || checklistType == ChecklistType.EclipseGating && image.Comment.IndexOf("BH") != -1)
            {
                double[] deltaCouch = new double[3];
                Beam beam = planSetup.Beams.Where(b => b.IsSetupField == false).FirstOrDefault();
                deltaCouch[0] = -beam.IsocenterPosition.x / 10.0;
                deltaCouch[1] = -beam.IsocenterPosition.z / 10.0;
                DataTable CouchPos = AriaInterface.Query("select distinct Slice.CouchVrt from Slice inner join Series on Series.SeriesSer=Slice.SeriesSer where Series.SeriesUID='" + image.Series.UID + "'");
                double couchVrt = (double)CouchPos.Rows[0]["CouchVrt"];
                deltaCouch[2] = beam.IsocenterPosition.y / 10.0 - couchVrt;
                checklistItems.Add(new ChecklistItem("X6. Fyll i värden för Delta Couch.", "Fyll i beräknade Delta Couch-värden för planens alla fält.", String.Format("Vrt: {0:N2} cm, Lng: {1:N2} cm, Lat: {2:N2} cm", deltaCouch[2], deltaCouch[1], deltaCouch[0]), String.Format("Delta Couch shift (cm):\r\nVrt:\t{0:N2}\r\nLng:\t{1:N2}\r\nLat:\t{2:N2}", deltaCouch[2], deltaCouch[1], deltaCouch[0]), AutoCheckStatus.MANUAL));
                
                
                //checklistItems.Add(new ChecklistItem("X7. Importera underlag till Catalyst.", "Importera plan och strukturset till Catalyst i enlighet med gällande metodbeskrivning.", String.Empty, AutoCheckStatus.MANUAL));

            }

            checklistItems.Add(new ChecklistItem("X7. Treatment Approved", "Gör planen Treatment Approved. Planen får endast göras Treatment Approved efter att ovanstående kontroller är utförda och Oberoende MU-kontroll eller QC-mätning är godkänd.", string.Empty, AutoCheckStatus.MANUAL));

            checklistItems.Add(new ChecklistItem("X8. Task sätts till Done", "Tryck Done när alla kontroller är klara\r\n  • Ändra Qty till det antal planer som har kontrollerats\r\n  • Om planen har kontrollmätts tycker man Done först när planen både är kontrollerad och kontrollmätt", string.Empty, AutoCheckStatus.MANUAL));

            //checklistItems.Add(new ChecklistItem("X5. Signera i rutan Fysiker kontroll", "Genomgången checklista med accepterat resultat bekräftas med signatur i behandlingskortet i rutan Fysiker kontroll.", string.Empty, AutoCheckStatus.MANUAL));
        }
    }
}
