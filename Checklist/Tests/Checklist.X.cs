using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMS.TPS.Common.Model.API;

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
                            isoY *= -1;
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
            }
            checklistItems.Add(new ChecklistItem("X2. Förväntad britshöjd räknas ut och läggs in i Aria", "Räkna ut förväntad britshöjd och lägg in i Aria (på alla fält, inklusive setup-fält) i modulen Treatment Preparation i rutan för Couch Vrt:\r\n• Eclipse: -DICOM offset Z - isocenter Z + offset cm\r\nMasterPlan: -TPRP coordinate Z - isocenter Z + offset cm\r\n• Offset är 7,1 cm för CT_A, CT_B, CT_C och -17,5 för PET/CT 01 (kan dock variera beroende på britshöjd vid PET-undersökningen)\r\n• Observera att vid för prone byter DICOM-koordinaten tecken\r\n• Observera risk för kollision mellan gantry och bord vid Vrt < -30 cm\r\nVid SSD-teknik:\r\n  • Ska tjockleken på eventuell vacuumpåse bestämmas genom mätning i CT-bilderna och antecknas under Setup note\r\n  • Räkna ut förflyttning från fältet närmast 0° till övriga fält (Isocenterkoordinat för ursprungsfältet minus övriga fälts isocenterkoordinater) och anteckna detta på sida 2 i behandlingsprotokollet. Exempel: Relativ förflyttning från fält 1 till fält 2: ∆Vrt=25,0 cm.\r\n  • Skriv följande under Setup note: ”FHA-beh. Ring fysiker vid start.”", x2_value, x2_status));
            // Add elinores corda computation here

            if (checklistType == ChecklistType.EclipseVMAT && GetVMATCoplanar(planSetup) == false)
            {
                checklistItems.Add(new ChecklistItem("X3. Notera icke coplanar VMAT under Setup note", "Planen i fråga är en icke coplanar VMAT behandling. Säkerställ att en notering om detta finns under planens Setup note", string.Empty, AutoCheckStatus.MANUAL));
            }

            checklistItems.Add(new ChecklistItem("X4. Treatment Approved", "Gör planen Treatment Approved. Planen får endast göras Treatment Approved efter att ovanstående kontroller är utförda och Oberoende MU-koll eller QC-mätning är signerad.", string.Empty, AutoCheckStatus.MANUAL));

            if (checklistType == ChecklistType.EclipseVMAT)
                checklistItems.Add(new ChecklistItem("X5. QC Course sätts till Completed.", "Sätt status på QC coursen till Completed.", "", AutoCheckStatus.MANUAL));

            checklistItems.Add(new ChecklistItem("X6. Task sätts till Done", "Tryck Done när alla kontroller är klara\r\n  • Ändra Qty till det antal planer som har kontrollerats\r\n  • Om planen har kontrollmätts tycker man Done först när planen både är kontrollerad och kontrollmätt", string.Empty, AutoCheckStatus.MANUAL));

            //checklistItems.Add(new ChecklistItem("X5. Signera i rutan Fysiker kontroll", "Genomgången checklista med accepterat resultat bekräftas med signatur i behandlingskortet i rutan Fysiker kontroll.", string.Empty, AutoCheckStatus.MANUAL));
        }
    }
}
