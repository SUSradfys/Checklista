using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace Checklist
{
    public partial class SelectPlanWindow : Form
    {
        private VMS.TPS.Common.Model.API.Application application;

        public SelectPlanWindow(VMS.TPS.Common.Model.API.Application application)
        {
            this.application = application;
            this.Location = new Point(0, 0);

            InitializeComponent();

            //textBoxPatientId.Text = "197510070365"; // Eclipse, RA
            //textBoxPatientId.Text = "194309145813"; // Eclipse, RA
            //textBoxPatientId.Text = "194811192998"; // Eclipse, warnings, FFS, Elekta
            //textBoxPatientId.Text = "198610012091"; // OMP
            //textBoxPatientId.Text = "198707012723"; // Gating
            //textBoxPatientId.Text = "195311092265"; // Två planer
            //textBoxPatientId.Text = "193902103997"; // FFF
            //textBoxPatientId.Text = "test_FN";
            textBoxPatientId.Text = "QC_Checklista"; 
            //textBoxPatientId.Text = "194611033749"; //Ordination
            //textBoxPatientId.Text = "197610203528"; // Extended APPA
            //textBoxPatientId.Text = "196906133506"; // sida Elekta
            //textBoxPatientId.Text = "195707194493"; // ordination PTV/CTV
            //textBoxPatientId.Text = "195503169319"; // multiple prescription levels
            //textBoxPatientId.Text = "196703224318"; // no guessing prescription volume

            //textBoxPatientId.Text = "test_fn_FFFmam";

            //buttonOpen_Click(null, null);
            //listBoxCourses.SelectedIndex = 0;
            //listBoxPlans.SelectedIndex = 0;
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            try
            {
                Patient patient = application.OpenPatientById(textBoxPatientId.Text);

                listBoxCourses.Items.Clear();
                if (patient != null)
                {
                    textBoxPatient.Tag = patient;
                    textBoxPatient.Text = patient.ToString();
                    foreach (Course course in patient.Courses)
                        listBoxCourses.Items.Add(course);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            try
            {
                application.ClosePatient();
                textBoxPatient.Text = string.Empty;
                listBoxCourses.Items.Clear();
                listBoxPlans.Items.Clear();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void listBoxCourses_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBoxPlans.Items.Clear();
            if (listBoxCourses.SelectedIndex != -1)
            {
                Course course = (Course)listBoxCourses.SelectedItem;
                foreach (PlanSetup planSetup in course.PlanSetups)
                    listBoxPlans.Items.Add(planSetup);
            }
        }

        private void listBoxPlans_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxPlans.SelectedIndex != -1)
            {                
                SelectChecklistWindow selectChecklistWindow = new SelectChecklistWindow();
                if (selectChecklistWindow.ShowDialog() == DialogResult.OK)
                {
                    Checklist checklist = new Checklist((Patient)textBoxPatient.Tag, (Course)listBoxCourses.SelectedItem, (PlanSetup)listBoxPlans.SelectedItem, selectChecklistWindow.ChecklistType, "r143285");
                    checklist.Analyze();
                }
            }
        }
    }
}
