using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;
using VMS.TPS.Common.Model.API;
using Checklist;

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
                //if (string.Compare(context.CurrentUser.Id, "r143285") == 0 || string.Compare(context.CurrentUser.Id, "r105231") == 0 || string.Compare(context.CurrentUser.Id, "r150801") == 0 || string.Compare(context.CurrentUser.Id, "r105229") == 0 || string.Compare(context.CurrentUser.Id, "r157726") == 0 || string.Compare(context.CurrentUser.Id, "r177773") == 0 || string.Compare(context.CurrentUser.Id, "r170483") == 0)
                string profession = string.Empty;
                AriaInterface.Connect();
                DataTable user = AriaInterface.Query("Select StaffId, Profession from Staff where StaffId = '" + context.CurrentUser.Id.ToString() + "'");
                AriaInterface.Disconnect();
                if (user.Rows.Count == 1 && user.Rows[0]["Profession"] != DBNull.Value)
                    profession = (string)user.Rows[0]["Profession"];
                if (string.Compare(profession, "Fysiker") == 0 || string.Compare(profession, "dpl") == 0)
                {
                    bool logFull;
                    if (string.Compare(profession, "Fysiker") == 0)
                        logFull = true;
                    else
                        logFull = false;
                    Checklist.SelectChecklistWindow selectChecklistWindow = new Checklist.SelectChecklistWindow();
                    if (selectChecklistWindow.ShowDialog() == DialogResult.OK)
                    {
                        Checklist.Checklist checklist = new Checklist.Checklist(context.Patient, context.Course, context.PlanSetup, selectChecklistWindow.ChecklistType, context.CurrentUser.Id, logFull);
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
