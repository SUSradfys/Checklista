using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Checklist
{
    public partial class DetailWindow : Form
    {
        public DetailWindow(string task, string taskDetailed, string value, bool itemChecked)
        {
            InitializeComponent();

            textBoxTask.Text = taskDetailed;
            textBoxValue.Text = value;

            checkBox.Text = task;
            checkBox.Checked = itemChecked;

            textBoxTask.Select(0, 0);
            textBoxValue.Select(0, 0);
            textBoxValue.Select();
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (this.IsHandleCreated)
            {
                this.DialogResult = (checkBox.Checked ? DialogResult.Yes : DialogResult.No);
                this.Close();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
