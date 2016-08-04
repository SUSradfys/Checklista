namespace Checklist
{
    partial class ChecklistWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChecklistWindow));
            this.listViewChecklist = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonAccept = new System.Windows.Forms.Button();
            this.buttonReject = new System.Windows.Forms.Button();
            this.labelChecklistSer = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // listViewChecklist
            // 
            this.listViewChecklist.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewChecklist.CheckBoxes = true;
            this.listViewChecklist.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.listViewChecklist.FullRowSelect = true;
            this.listViewChecklist.Location = new System.Drawing.Point(12, 12);
            this.listViewChecklist.MultiSelect = false;
            this.listViewChecklist.Name = "listViewChecklist";
            this.listViewChecklist.OwnerDraw = true;
            this.listViewChecklist.ShowItemToolTips = true;
            this.listViewChecklist.Size = new System.Drawing.Size(1252, 705);
            this.listViewChecklist.TabIndex = 10;
            this.listViewChecklist.UseCompatibleStateImageBehavior = false;
            this.listViewChecklist.View = System.Windows.Forms.View.Details;
            this.listViewChecklist.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.listViewChecklist_DrawColumnHeader);
            this.listViewChecklist.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.listViewChecklist_DrawItem);
            this.listViewChecklist.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.listViewChecklist_DrawSubItem);
            this.listViewChecklist.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listViewChecklist_ItemChecked);
            this.listViewChecklist.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listViewChecklist_KeyDown);
            this.listViewChecklist.KeyUp += new System.Windows.Forms.KeyEventHandler(this.listViewChecklist_KeyUp);
            this.listViewChecklist.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listViewChecklist_MouseClick);
            this.listViewChecklist.MouseMove += new System.Windows.Forms.MouseEventHandler(this.listViewChecklist_MouseMove);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "";
            this.columnHeader1.Width = 26;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Uppgift";
            this.columnHeader2.Width = 455;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Värde i Aria";
            this.columnHeader3.Width = 449;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Detaljer";
            // 
            // buttonAccept
            // 
            this.buttonAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAccept.BackColor = System.Drawing.Color.LightGreen;
            this.buttonAccept.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonAccept.Enabled = false;
            this.buttonAccept.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAccept.Location = new System.Drawing.Point(12, 723);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(144, 53);
            this.buttonAccept.TabIndex = 11;
            this.buttonAccept.Text = "Plan godkänd";
            this.buttonAccept.UseVisualStyleBackColor = false;
            this.buttonAccept.Click += new System.EventHandler(this.buttonAccept_Click);
            this.buttonAccept.KeyDown += new System.Windows.Forms.KeyEventHandler(this.buttonAccept_KeyDown);
            // 
            // buttonReject
            // 
            this.buttonReject.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonReject.BackColor = System.Drawing.Color.Pink;
            this.buttonReject.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonReject.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonReject.Location = new System.Drawing.Point(162, 723);
            this.buttonReject.Name = "buttonReject";
            this.buttonReject.Size = new System.Drawing.Size(144, 53);
            this.buttonReject.TabIndex = 12;
            this.buttonReject.Text = "Plan underkänd";
            this.buttonReject.UseVisualStyleBackColor = false;
            this.buttonReject.Click += new System.EventHandler(this.buttonReject_Click);
            // 
            // labelChecklistSer
            // 
            this.labelChecklistSer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelChecklistSer.Location = new System.Drawing.Point(791, 723);
            this.labelChecklistSer.Name = "labelChecklistSer";
            this.labelChecklistSer.Size = new System.Drawing.Size(473, 56);
            this.labelChecklistSer.TabIndex = 13;
            this.labelChecklistSer.Text = "Löpnummer: ";
            this.labelChecklistSer.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // ChecklistWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1276, 788);
            this.Controls.Add(this.labelChecklistSer);
            this.Controls.Add(this.buttonReject);
            this.Controls.Add(this.buttonAccept);
            this.Controls.Add(this.listViewChecklist);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ChecklistWindow";
            this.Text = "Checklista";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ChecklistWindow_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listViewChecklist;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.Button buttonReject;
        private System.Windows.Forms.Label labelChecklistSer;
    }
}