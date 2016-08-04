using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;
using VMS.TPS.Common.Model.API;

namespace VMS.TPS
{
    public class Script
    {
        public Script()
        {
        }

        public void Execute(ScriptContext context)
        {
            if (context.PlanSetup != null)
            {
                if (string.Compare(context.CurrentUser.Id, "r143285") == 0 || string.Compare(context.CurrentUser.Id, "r105231") == 0 || string.Compare(context.CurrentUser.Id, "r150801") == 0 || string.Compare(context.CurrentUser.Id, "r105229") == 0 || string.Compare(context.CurrentUser.Id, "r157726") == 0 || string.Compare(context.CurrentUser.Id, "r177773") == 0 || string.Compare(context.CurrentUser.Id, "r170483") == 0)
                {
                    Checklist.SelectChecklistWindow selectChecklistWindow = new Checklist.SelectChecklistWindow();
                    if (selectChecklistWindow.ShowDialog() == DialogResult.OK)
                    {
                        Checklist.Checklist checklist = new Checklist.Checklist(context.Patient, context.Course, context.PlanSetup, selectChecklistWindow.ChecklistType, context.CurrentUser.Id);
                        checklist.Analyze();
                    }
                }
                else
                {
                    MessageBox.Show("Permission denied!", "Checklista", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    try
                    {

                        using (StreamWriter sw = File.AppendText(@"\\mtdb001\va_DATA$\Filedata\ProgramData\Vision\PublishedScripts\denied_login.txt"))
                        {
                            sw.WriteLine(DateTime.Now.ToString() + "\t" + context.CurrentUser.Id + "\t" + context.CurrentUser.Name);
                        }	
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                MessageBox.Show("Ingen plan vald!", "Checklista", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }            
        }

    }
}
