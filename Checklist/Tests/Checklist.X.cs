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
                        IEnumerable<Beam> txBeams = planSetup.Beams.Where(x => x.IsSetupField == false); 
                        //Summarises all ssds for treatment beams, if this is > x*1000 the bool will be true. 
                        bool fhaTechnique = txBeams.Select(x => Math.Round(x.SSD)).ToList().Sum() >= (double)txBeams.Count() * 1000;
                        IEnumerable<VVector> uniqueIsos = new List<VVector>(); 
                        if (fhaTechnique)
                           uniqueIsos = GetAllIsocenters(planSetup);
                        if (uniqueIsos.Count() > 1 && uniqueIsos.Count() <= 2) //This part will give 2 different couch positions based if there are 2 isocenters. 
                        {
                            bool warningToggle = false;
                            double firstIsoYPos = double.NaN;
                            foreach (Beam beam in planSetup.Beams)
                            {
                                
                                if (!beam.IsSetupField && !(firstIsoYPos==-(beam.IsocenterPosition.y - image.UserOrigin.y) * 0.1))
                                {
                                    double isoY = -(beam.IsocenterPosition.y - image.UserOrigin.y) * 0.1;
                                    
                                    if (planSetup.TreatmentOrientation.ToString().IndexOf("Prone") != -1) // Change sign if Orientation is Prone.
                                    {
                                        isoY *= -1;
                                        userY *= -1;
                                    }
                                    double shiftY = 7.1;
                                    double sumY = -isoY - userY + shiftY;
                                    x2_value += ", Beräknad britshöjd " +(Double.IsNaN(firstIsoYPos) ? "Iso 1: ": "Iso 2: ") + (-userY).ToString("0.0") + (-isoY >= 0 ? "+" : string.Empty) + (-isoY).ToString("0.0") + "+" + shiftY.ToString("0.0") + " = " + sumY.ToString("0.0") + " cm";
                                    if (sumY < -30 && !warningToggle)
                                        x2_status = AutoCheckStatus.WARNING;
                                    if (Double.IsNaN(firstIsoYPos))
                                        firstIsoYPos = isoY;
                                }
                            }
                        }
                        else if (uniqueIsos.Count() > 2) // if more that 2 isos it will promp the user to measure manually. 
                        {
                            x2_value += ", Mer än tre iso mät positioner manuellt";
                        }
                        else
                        {
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

            if (checklistType == ChecklistType.EclipseVMAT || checklistType == ChecklistType.EclipseConformal)
            {
                AutoCheckStatus x5_status = AutoCheckStatus.MANUAL; 
                string x5_value = string.Empty;
                string x5_details = string.Empty;
                //List<string> reqStrings = new List<string>() {"qc","d","4"}; 
                //lägg till kontroll att QC - plan finns och ger annars varning. WORK in progress
                DataTable qcPlans = AriaInterface.Query("select PlanSetup.PlanSetupId from (select ClinRTPlanSer = RTPlan.RTPlanSer from RTPlan where RTPlan.PlanSetupSer = '" + planSetupSer.ToString() +"') as ClinRTPlan, PlanSetup inner join RTPlan on PlanSetup.PlanSetupSer = RTPlan.PlanSetupSer inner join PlanRelationship on PlanRelationship.RTPlanSer = RTPlan.RTPlanSer where PlanRelationship.RelationshipType = 'VERIFIED_PLAN' and PlanRelationship.RelatedRTPlanSer = ClinRTPlanSer order by PlanSetup.PlanSetupId");
                
                if (qcPlans.Rows.Count > 0)
                {
                    foreach (DataRow row in qcPlans.Rows)
                    {
                        x5_value += (x5_value.Length == 0 ? "Verifikationsplaner finns: " : ", ") + (string)row["PlanSetupId"];
                    }
                    if (!(x5_value.IndexOf("QC") > 1 && (x5_value.IndexOf("d") > 1 || x5_value.IndexOf("D") > 1) && x5_value.IndexOf("4") > 1))
                    {
                        x5_value += " OBS: Inkorrekt namn på verifikationsplan, automatisk export ej möjlig";
                        x5_status = AutoCheckStatus.WARNING; 
                    }
                }
                else // SÄtter varning om det inte finns nån QC-plan
                {
                    x5_status = AutoCheckStatus.FAIL;
                    x5_value = "Det finns ingen QC-plan kopplad till den kliniska planen. Det skall föreligga QC-plan för Delta4."; 
                }
                
                checklistItems.Add(new ChecklistItem("X5. QC-planer/QC course sätts till Completed.", "Sätt status på QC coursen till Completed. \nKontroll av befintiliga QC-planer görs. \nNamngivning enligt: QC PX_X Delta4.", x5_value, x5_status));
            }
            if (checklistType == ChecklistType.EclipseGating && image.Comment.IndexOf("DIBH") != -1 || checklistType == ChecklistType.EclipseGating && image.Comment.IndexOf("BH") != -1)
            {
                double[] deltaCouch = new double[3];
                IEnumerable<VVector> AllIsosPos = GetAllIsocenters(planSetup);
                if (AllIsosPos.Count() == 1)
                {
                    Beam beam = planSetup.Beams.Where(b => b.IsSetupField == false).FirstOrDefault();
                    deltaCouch[0] = -beam.IsocenterPosition.x / 10.0;
                    deltaCouch[1] = -beam.IsocenterPosition.z / 10.0;
                    DataTable CouchPos = AriaInterface.Query("select distinct Slice.CouchVrt from Slice inner join Series on Series.SeriesSer=Slice.SeriesSer where Series.SeriesUID='" + image.Series.UID + "'");
                    double couchVrt = (double)CouchPos.Rows[0]["CouchVrt"];
                    deltaCouch[2] = beam.IsocenterPosition.y / 10.0 - couchVrt;
                    checklistItems.Add(new ChecklistItem("X6. Fyll i värden för Delta Couch.", "Fyll i beräknade Delta Couch-värden för planens alla fält.", String.Format("Vrt: {0:N2} cm, Lng: {1:N2} cm, Lat: {2:N2} cm", deltaCouch[2], deltaCouch[1], deltaCouch[0]), String.Format("Delta Couch shift (cm):\r\nVrt:\t{0:N2}\r\nLng:\t{1:N2}\r\nLat:\t{2:N2}", deltaCouch[2], deltaCouch[1], deltaCouch[0]), AutoCheckStatus.MANUAL));
                }
                else
                {
                    string IsoInfo = string.Empty;
                    DataTable CouchPos = AriaInterface.Query("select distinct Slice.CouchVrt from Slice inner join Series on Series.SeriesSer=Slice.SeriesSer where Series.SeriesUID='" + image.Series.UID + "'");
                    double couchVrt = (double)CouchPos.Rows[0]["CouchVrt"];
                    int count = 1; 
                    foreach (VVector iso in AllIsosPos)
                    {
                        deltaCouch[0] = -iso.x / 10.0;
                        deltaCouch[1] = -iso.z / 10.0;
                        deltaCouch[2] = iso.y / 10 - couchVrt;
                        IsoInfo += "Iso "+ count.ToString()  + String.Format(": Vrt: {0:N2} cm, Lng: {1:N2} cm, Lat: {2:N2} cm", deltaCouch[2], deltaCouch[1], deltaCouch[0]) + " \r\n\r\n";
                        count += 1;
                    }
                    checklistItems.Add(new ChecklistItem("X6. Fyll i värden för Delta Couch.", "Fyll i beräknade Delta Couch-värden för planens alla fält.", "Planen ifråga har " + (count-1).ToString() + " isocenter och därmed skall multipla delta couch fyllas i se Detaljer ----->", "Delta Couch shift (cm):\r\n" + IsoInfo, AutoCheckStatus.MANUAL));


                }

                //checklistItems.Add(new ChecklistItem("X7. Importera underlag till Catalyst.", "Importera plan och strukturset till Catalyst i enlighet med gällande metodbeskrivning.", String.Empty, AutoCheckStatus.MANUAL));

            }

            checklistItems.Add(new ChecklistItem("X7. Treatment Approved", "Gör planen Treatment Approved. Planen får endast göras Treatment Approved efter att ovanstående kontroller är utförda och Oberoende MU-kontroll eller QC-mätning är godkänd.", string.Empty, AutoCheckStatus.MANUAL));

            checklistItems.Add(new ChecklistItem("X8. Task sätts till Done", "Tryck Done när alla kontroller är klara\r\n  • Ändra Qty till det antal planer som har kontrollerats\r\n  • Om planen har kontrollmätts tycker man Done först när planen både är kontrollerad och kontrollmätt", string.Empty, AutoCheckStatus.MANUAL));

            //checklistItems.Add(new ChecklistItem("X5. Signera i rutan Fysiker kontroll", "Genomgången checklista med accepterat resultat bekräftas med signatur i behandlingskortet i rutan Fysiker kontroll.", string.Empty, AutoCheckStatus.MANUAL));
        }
    }
}
