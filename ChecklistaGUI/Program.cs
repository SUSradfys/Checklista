using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Checklist
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                long checklistSer;
                if (long.TryParse(args[0], out checklistSer))
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new ChecklistWindow(checklistSer));
                }
                else
                    MessageBox.Show("Parameter not valid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
                MessageBox.Show("Wrong number of parameters provided", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
