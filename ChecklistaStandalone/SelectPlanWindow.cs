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
            
            //textBoxPatientId.Text = "test_FN";
            textBoxPatientId.Text = "QC_Checklista"; 
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
                    Checklist checklist = new Checklist((Patient)textBoxPatient.Tag, (Course)listBoxCourses.SelectedItem, (PlanSetup)listBoxPlans.SelectedItem, selectChecklistWindow.ChecklistType, "r150801", true);
                    checklist.Analyze();
                }
            }
        }
    }
}
