using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMS.TPS.Common.Model.API;
using System.Windows.Forms;

namespace Checklist
{
    public partial class Checklist
    {
        private List<ChecklistItem> GetSlimList(List<ChecklistItem> checklistItems, List<string> keepers)
        {
            List<ChecklistItem> kept = checklistItems.Where(x => keepers.Contains(x.AutoCheckStatus)).ToList();
            /*
            List<ChecklistItem> kept = new List<ChecklistItem>();
            foreach (ChecklistItem checklistItem in checklistItems)
            {
                if (String.Compare(checklistItem.AutoCheckStatus, "NONE") == 0 || String.Compare(checklistItem.AutoCheckStatus, "WARNING") == 0 || String.Compare(checklistItem.AutoCheckStatus, "FAIL") == 0)
                {
                    kept.Add(checklistItem);
                }
            }
            */
            return kept;


        }
    }
}