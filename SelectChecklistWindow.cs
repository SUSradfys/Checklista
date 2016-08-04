using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace Checklist
{
    public partial class SelectChecklistWindow : Form
    {
        private ChecklistType selectedChecklistType;

        public SelectChecklistWindow()
        {
            this.DialogResult = DialogResult.Cancel;

            InitializeComponent();

            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (string name in assembly.GetManifestResourceNames())
                Console.WriteLine(name);

            ChecklistType[] checklistTypes=EnumExtensions.EnumToList<ChecklistType>().ToArray();
            ChecklistType[] validChecklistTypes = checklistTypes;
            validChecklistTypes = validChecklistTypes.Where(s => s.GetDescription().IndexOf("MasterPlan") == -1).ToArray();

            tableLayoutPanel.ColumnCount = validChecklistTypes.Count();            
            foreach (ChecklistType validChecklistType in validChecklistTypes)
            {
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

                Button button = new Button() { Text = validChecklistType.GetDescription(), Dock = DockStyle.Fill, Tag = validChecklistType, ForeColor = Color.White, BackColor = Color.Black, TextAlign = ContentAlignment.TopCenter, Font = new Font(this.Font.FontFamily, 12, FontStyle.Bold) };
                button.Click += new EventHandler(button_Click);
                button.FlatStyle = FlatStyle.Flat;
                button.Cursor = Cursors.Hand;

                using (System.IO.Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Checklist.Images." + validChecklistType.ToString() + ".png"))
                {
                    if (imageStream != null)
                        button.Image = Image.FromStream(imageStream);
                    //else
                    //    button.Image = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("Checklist.Images.QuestionMark.png"));
                }

                button.ImageAlign = ContentAlignment.BottomCenter;
                button.TextImageRelation = TextImageRelation.TextAboveImage;
                button.BackgroundImageLayout = ImageLayout.Zoom;
                tableLayoutPanel.Controls.Add(button);
            }            
        }

        private void button_Click(object sender, EventArgs e)
        {
            selectedChecklistType = (ChecklistType)((Button)sender).Tag;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /*private void comboBoxChecklista_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }*/

        public ChecklistType ChecklistType { get { return selectedChecklistType; } }

        private void pictureBoxClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

    }
}
