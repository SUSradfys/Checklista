﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using VMS.TPS.Common.Model.API;

namespace Checklist
{
    public partial class Checklist
    {
        public void B()
        {
            checklistItems.Add(new ChecklistItem("B. Bolus"));

            string b1_value = string.Empty;
            AutoCheckStatus b1_status = AutoCheckStatus.UNKNOWN;
            if (planSetup.StructureSet != null)
            {
                bool tpsBolusExist = false;
                bool tpsBolusLinkedToField = false; 
                foreach (Structure structure in planSetup.StructureSet.Structures)
                {
                    if (string.Compare(structure.DicomType, "BOLUS") == 0)
                    {
                        tpsBolusExist = true;
                        
                    }
                }
                foreach (Beam b in planSetup.Beams)
                {
                    if (b.Boluses.Count() > 0)
                        tpsBolusLinkedToField = true;

                }
                if (!tpsBolusExist)
                {
                    b1_value = "Ej ansatt";
                    b1_status = AutoCheckStatus.PASS;
                }
                else if (tpsBolusExist && !tpsBolusLinkedToField)
                {
                    b1_value = "Bolus har ansatts i TPS, men ej kopplats till minst ett fält. Verifera.";
                    b1_status = AutoCheckStatus.WARNING;
                }
                else if (tpsBolusExist && tpsBolusLinkedToField)
                    b1_value = "Bolus har ansatts i TPS, och kopplats till minst ett fält. Verifiera.";
            }
            else
                b1_value = "StructureSet saknas";
            checklistItems.Add(new ChecklistItem("B1. I dosplaneringssystemet ansatt bolus är med i beräkningen", "Kontrollera att bolus som ansatts i dosplaneringssystemet är med i beräkningen (kopplat till resp. fält)", b1_value, b1_status));

            AutoCheckStatus b2_status = AutoCheckStatus.MANUAL;
            string b2_value = string.Empty;
            // Check against prescription
            if (prescSer > 0)
            {
                DataTable bolus = AriaInterface.Query("select PlanSetupSer, PlanSetup.PrescriptionSer, Prescription.PrescriptionSer, BolusFrequency, BolusThickness from PlanSetup, Prescription where PlanSetup.PrescriptionSer = Prescription.PrescriptionSer and PlanSetup.PlanSetupSer = " + planSetupSer.ToString());
                if (bolus.Rows.Count > 0)
                {
                    b2_value += (bolus.Rows[0][3] == DBNull.Value ? string.Empty : (string)bolus.Rows[0][3]);
                    b2_value += (b2_value.Length == 0 ? string.Empty : ", ") + (bolus.Rows[0][4] == DBNull.Value ? string.Empty : (string)bolus.Rows[0][4]);
                }

                if (String.IsNullOrWhiteSpace(b2_value))
                    b2_value = "Information saknas";
            }
            else
            {
                b2_status = AutoCheckStatus.WARNING;
                b2_value = "Ordination saknas, kontroll ej möjlig.";
            }
            checklistItems.Add(new ChecklistItem("B2. Ordinationen innehåller information om bolus", "Kontrollera att bolus finns angivet i ordinationen (aktuell tjocklek och bolustyp).", b2_value, b2_status));
        }
    }
}
