namespace Checklist
{
    partial class SelectPlanWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.listBoxPlans = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.listBoxCourses = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonClose = new System.Windows.Forms.Button();
            this.textBoxPatient = new System.Windows.Forms.TextBox();
            this.buttonOpen = new System.Windows.Forms.Button();
            this.textBoxPatientId = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // listBoxPlans
            // 
            this.listBoxPlans.FormattingEnabled = true;
            this.listBoxPlans.Location = new System.Drawing.Point(483, 64);
            this.listBoxPlans.Name = "listBoxPlans";
            this.listBoxPlans.Size = new System.Drawing.Size(156, 199);
            this.listBoxPlans.TabIndex = 17;
            this.listBoxPlans.SelectedIndexChanged += new System.EventHandler(this.listBoxPlans_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(480, 48);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(36, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "Plans:";
            // 
            // listBoxCourses
            // 
            this.listBoxCourses.FormattingEnabled = true;
            this.listBoxCourses.Location = new System.Drawing.Point(321, 64);
            this.listBoxCourses.Name = "listBoxCourses";
            this.listBoxCourses.Size = new System.Drawing.Size(156, 199);
            this.listBoxCourses.TabIndex = 14;
            this.listBoxCourses.SelectedIndexChanged += new System.EventHandler(this.listBoxCourses_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(318, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Courses:";
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(321, 12);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 12;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // textBoxPatient
            // 
            this.textBoxPatient.Location = new System.Drawing.Point(12, 64);
            this.textBoxPatient.Multiline = true;
            this.textBoxPatient.Name = "textBoxPatient";
            this.textBoxPatient.ReadOnly = true;
            this.textBoxPatient.Size = new System.Drawing.Size(303, 201);
            this.textBoxPatient.TabIndex = 11;
            // 
            // buttonOpen
            // 
            this.buttonOpen.Location = new System.Drawing.Point(240, 12);
            this.buttonOpen.Name = "buttonOpen";
            this.buttonOpen.Size = new System.Drawing.Size(75, 23);
            this.buttonOpen.TabIndex = 10;
            this.buttonOpen.Text = "Open";
            this.buttonOpen.UseVisualStyleBackColor = true;
            this.buttonOpen.Click += new System.EventHandler(this.buttonOpen_Click);
            // 
            // textBoxPatientId
            // 
            this.textBoxPatientId.Location = new System.Drawing.Point(12, 12);
            this.textBoxPatientId.Name = "textBoxPatientId";
            this.textBoxPatientId.Size = new System.Drawing.Size(222, 20);
            this.textBoxPatientId.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "Patient:";
            // 
            // SelectPlanWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(651, 276);
            this.Controls.Add(this.listBoxPlans);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.listBoxCourses);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.textBoxPatient);
            this.Controls.Add(this.buttonOpen);
            this.Controls.Add(this.textBoxPatientId);
            this.Controls.Add(this.label2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "SelectPlanWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Select patient";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxPlans;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListBox listBoxCourses;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.TextBox textBoxPatient;
        private System.Windows.Forms.Button buttonOpen;
        private System.Windows.Forms.TextBox textBoxPatientId;
        private System.Windows.Forms.Label label2;
    }
}

