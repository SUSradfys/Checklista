using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Checklist.Checklist
{
    class OldChecks
    {
        OldChecks()
        {
            string u3_value = string.Empty;
            foreach (Beam beam in planSetup.Beams)
            {
                if (!beam.IsSetupField)
                {
                    if (u3_value != string.Empty)
                        u3_value += ", ";
                    u3_value += beam.Id + ": " + Math.Round(beam.Meterset.Value, 1).ToString() + " " + beam.Meterset.Unit.ToString();
                }
            }
            checklistItems.Add(new ChecklistItem("U3. Jämför MU mellan behandlingsprotokoll och Aria", "Kontrollera att MU stämmer överens mellan Aria/Eclipse och behandlingsprotokoll", u3_value, AutoCheckStatus.MANUAL));

            if (checklistType != ChecklistType.EclipseVMAT && checklistType != ChecklistType.MasterPlanIMRT)
                checklistItems.Add(new ChecklistItem("U4. Diodvärden finns dokumenterade i protokollet", "Kontrollera att diodvärden finns dokumenterade i protokollet för konventionella planer.\r\nEclipse: Centralaxeln är lämplig som diodpunkt, i annat fall ska manuellt inskrivna diodvärden finnas i protokollet", string.Empty, AutoCheckStatus.MANUAL));

            string g1_value = (image == null ? "-" : image.Comment);
            AutoCheckStatus g1_status = CheckResult(string.Compare(g1_value, "RT Thorax med gating  3.0  I30s") == 0);
            checklistItems.Add(new ChecklistItem("G1. CT-studie är korrekt m.a.p. gatingordination", "Kontrollera att det är ritat i korrekt CT-studie m.a.p. gatingordination (anges av läkare under kommentarer under behandlingsordination i behandlingskortet) och Image comment i protokollet", g1_value, g1_status));

            string s11_value = string.Empty;
            AutoCheckStatus s11_status = AutoCheckStatus.PASS;
            foreach (Beam beam in planSetup.Beams)
            {
                string refImageId = string.Empty;
                DataTable dataTableIDUPosVrt = AriaInterface.Query("select Image.ImageId from Radiation,Image where Image.ImageSer=Radiation.RefImageSer and Radiation.PlanSetupSer=" + planSetupSer.ToString() + " and Radiation.RadiationId='" + beam.Id + "'");
                if (dataTableIDUPosVrt.Rows.Count == 1)
                    refImageId = (string)dataTableIDUPosVrt.Rows[0][0];
                else
                    s11_status = AutoCheckStatus.FAIL;
                s11_value += (s11_value.Length == 0 ? string.Empty : ", ") + beam.Id + ": " + (refImageId.Length == 0 ? "-" : refImageId);
            }
            checklistItems.Add(new ChecklistItem("S3. Referensbild/DRR kopplad till alla fält", "Kontrollera att alla fält har en DRR kopplad samt har referensbild kopplad (gul ram runt bildikonen)", s11_value, s11_status));

        }
    }
}
