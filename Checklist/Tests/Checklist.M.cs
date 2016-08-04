using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMS.TPS.Common.Model.API;

namespace Checklist
{
    public partial class Checklist
    {
        public void M()
        {
            if (checklistType == ChecklistType.MasterPlan || checklistType == ChecklistType.MasterPlanIMRT)
            {
                checklistItems.Add(new ChecklistItem("M. MasterPlan"));

                string s4_value = string.Empty;
                if (checklistType != ChecklistType.MasterPlan && checklistType != ChecklistType.MasterPlanIMRT)
                    foreach (PlanSetup planSetupInCourse in course.PlanSetups)
                        foreach (Beam beam in planSetup.Beams)
                            if (beam.IsSetupField)
                            {
                                s4_value += (s4_value.Length == 0 ? "Setupfält i plan: " : ", ") + planSetupInCourse.Id;
                                break;
                            }
                checklistItems.Add(new ChecklistItem("M1. Setupfälten ligger i en separat plan", "Kontrollera att eventuella setupfält ligger i en separat plan. ", s4_value, AutoCheckStatus.MANUAL));

                checklistItems.Add(new ChecklistItem("M2. Jämför MU mellan protokoll och Aria", "Kontrollera att MU stämmer överens mellan protokoll och Aria.", string.Empty, AutoCheckStatus.MANUAL));

                if (checklistType != ChecklistType.MasterPlanIMRT)
                {
                    checklistItems.Add(new ChecklistItem("M3. Britskorrektion av MU", "Kontrollera MU är korrigerat för brits (står i Field ID). Avvikelse på max 1MU accepteras på grund av ev. avrundning", string.Empty, AutoCheckStatus.MANUAL));

                    checklistItems.Add(new ChecklistItem("M4. Normeringspunkt i planen satt till \"dose specification point\"", "Kontrollera att normeringspunkt i planen är satt till \"dose specification point\"", string.Empty, AutoCheckStatus.MANUAL));
                }
            }
        }
    }
}
