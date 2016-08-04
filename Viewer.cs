using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Checklist
{
    public static class Viewer
    {
        public static void Start(long checklistSer)
        {
            try
            {
                Process myProcess = new Process();

                myProcess.StartInfo.UseShellExecute = false;
                myProcess.StartInfo.FileName = @"\\mtdb001\va_DATA$\Filedata\ProgramData\Vision\PublishedScripts\Checklista.exe";
                //myProcess.StartInfo.FileName = @"Checklista.exe";
                myProcess.StartInfo.CreateNoWindow = true;
                myProcess.StartInfo.Arguments = checklistSer.ToString();
                myProcess.Start();
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show(exception.Message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }
}
