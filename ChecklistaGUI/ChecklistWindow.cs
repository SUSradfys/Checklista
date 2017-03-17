using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Checklist
{
    public partial class ChecklistWindow : Form
    {
        private DatabaseManager databaseManager = new DatabaseManager(Settings.RESULT_SERVER, Settings.RESULT_USERNAME, Settings.RESULT_PASSWORD);
        private long checklistSer;
        private int numberOfItems = 0;
        private ToolTip toolTip = new ToolTip();
        private Point mouseLastPos = new Point(-1, -1);
        private bool statusSelected = false;
        private bool acceptedUsingSpaceKey;

        public ChecklistWindow(long checklistSer)
        {
            this.checklistSer = checklistSer;

            InitializeComponent();

            // Visa fönstret på den andra skärmen (maximerat) om flera skärmar är kopplade till datorn
            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen != Screen.PrimaryScreen)
                {
                    this.StartPosition = FormStartPosition.Manual;
                    this.Location = screen.WorkingArea.Location;
                    this.WindowState = FormWindowState.Maximized;
                    break;
                }
            }

            string userId;
            string status = databaseManager.GetStatus(checklistSer, out userId);
            labelChecklistSer.Text = "Löpnummer: " + checklistSer.ToString() + "\r\nAnvändarnamn: " + userId + (String.Compare(status, "UNAPPROVED") == 0 ? string.Empty : "\r\n" + status);

            UpdateChecklist();
            if (string.Compare(status, "REJECTED") == 0 || string.Compare(status, "APPROVED") == 0)
            {
                buttonAccept.Enabled = false;
                buttonReject.Enabled = false;
                statusSelected = true;
            }
            else
                SelectNextUncheckedItem();            
        }

        public void UpdateChecklist()
        {
            listViewChecklist.Items.Clear();
            listViewChecklist.BeginUpdate();

            try
            {
                DataTable dataTable = databaseManager.GetResults(checklistSer);
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    if (string.Compare((string)dataRow["DetailedInfo"], "DELIMINATOR") == 0)
                    {
                        ListViewItem listViewItem = listViewChecklist.Items.Add("X");
                        listViewItem.Tag=dataRow["CheckListItemSer"];
                        listViewItem.SubItems.Add((string)dataRow["ShortInfo"]);
                        listViewItem.SubItems[1].Font = new System.Drawing.Font(listViewItem.Font, FontStyle.Bold);
                    }
                    else
                    {                        
                        ListViewItem listViewItem = listViewChecklist.Items.Add("");
                        listViewItem.Tag = dataRow["CheckListItemSer"];
                        string autoCheckStatus = ((string)dataRow["AutoCheckStatus"]).Trim();
                        bool checkStatus = (bool)dataRow["CheckStatus"];
                        listViewItem.Checked = checkStatus;
                        if (string.Compare(autoCheckStatus, "PASS") == 0)
                            listViewItem.ForeColor = Color.DarkGreen;
                        else if (string.Compare(autoCheckStatus, "FAIL") == 0)
                            listViewItem.ForeColor = Color.Red;
                        else if (string.Compare(autoCheckStatus, "WARNING") == 0)
                            listViewItem.ForeColor = Color.Blue;
                        bool rareCheck = (bool)dataRow["rareCheck"];
                        listViewItem.SubItems.Add((string)dataRow["ShortInfo"]);
                        listViewItem.SubItems[1].Tag = (string)dataRow["DetailedInfo"];
                        listViewItem.SubItems.Add(dataRow["ShortResult"] == DBNull.Value ? string.Empty : (string)dataRow["ShortResult"]);
                        if (dataRow["DetailedResult"] != DBNull.Value)
                        {
                            listViewItem.SubItems.Add("...          ");
                            listViewItem.SubItems[3].Tag = (string)dataRow["DetailedResult"];
                        }
                        numberOfItems++;
                        /*if (image != null)
                        {
                            listViewItem.SubItems.Add("...          ");
                            listViewItem.SubItems[3].Tag = image;
                        }*/
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            listViewChecklist.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.ColumnContent);
            listViewChecklist.AutoResizeColumn(2, ColumnHeaderAutoResizeStyle.ColumnContent);

            listViewChecklist.EndUpdate();            
        }

        public void SelectNextUncheckedItem()
        {
            int selectedIndex = (listViewChecklist.SelectedIndices.Count == 0 ? -1 : listViewChecklist.SelectedIndices[0]);
            foreach (ListViewItem listViewItem in listViewChecklist.Items)
            {
                if (listViewItem.Index >= selectedIndex && string.Compare(listViewItem.Text, "X") != 0 && listViewItem.Checked == false)
                {
                    listViewItem.Selected = true;
                    listViewItem.Focused = true;
                    listViewItem.EnsureVisible();
                    selectedIndex = listViewItem.Index;
                    break;
                }
            }
        }        

        private void listViewChecklist_MouseMove(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo info = listViewChecklist.HitTest(e.X, e.Y);
            if (mouseLastPos != e.Location)
            {
                if (info.Item != null && info.SubItem != null && info.SubItem.Tag != null && string.Compare(info.SubItem.Text, "...          ") != 0)
                {
                    toolTip.ToolTipTitle = info.SubItem.Text;
                    toolTip.Show((string)info.SubItem.Tag, info.Item.ListView, e.X, e.Y, 20000);
                }
                else
                {
                    toolTip.SetToolTip(listViewChecklist, string.Empty);
                }
            }

            mouseLastPos = e.Location;
        }
        
        private void listViewChecklist_MouseClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo info = listViewChecklist.HitTest(e.X, e.Y);

            if (info.SubItem != null && string.Compare(info.SubItem.Text, "...          ") == 0)
                ShowDetails(info.Item);
        }
        
        private void ShowDetails(ListViewItem listViewItem)
        {
            if (listViewItem.SubItems.Count == 4 && string.Compare(listViewItem.SubItems[3].Text, "...          ") == 0)
            {
                if (listViewItem.SubItems[3].Tag.GetType() == typeof(string))
                {
                    DetailWindow detailWindow = new DetailWindow((string)listViewItem.SubItems[1].Text, (string)listViewItem.SubItems[1].Tag, (string)listViewItem.SubItems[3].Tag, listViewItem.Checked);
                    DialogResult dialogResult = detailWindow.ShowDialog();
                    if (dialogResult == DialogResult.Yes)
                        listViewItem.Checked = true;
                    else if (dialogResult == DialogResult.No)
                        listViewItem.Checked = false;
                }
                /*else if (listViewItem.SubItems[3].Tag.GetType() == typeof(VMS.TPS.Common.Model.API.Image))
                {
                    ImageWindow imageWindow = new ImageWindow(listViewItem.SubItems[1].Text, (VMS.TPS.Common.Model.API.Image)listViewItem.SubItems[3].Tag, listViewItem.Checked);
                    DialogResult dialogResult = imageWindow.ShowDialog();
                    if (dialogResult == DialogResult.Yes)
                        listViewItem.Checked = true;
                    else if (dialogResult == DialogResult.No)
                        listViewItem.Checked = false;
                }*/
            }
        }
        
        private void listViewChecklist_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void listViewChecklist_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if (string.Compare(e.SubItem.Text, "X") == 0)
                e.DrawDefault = false;
            else
                e.DrawDefault = true;
        }

        private void listViewChecklist_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            if (string.Compare(e.Item.Text, "X") == 0)
                e.DrawDefault = false;
            else
                e.DrawDefault = true;
        }

        private void listViewChecklist_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (listViewChecklist.SelectedItems.Count == 1)
                    ShowDetails(listViewChecklist.SelectedItems[0]);
            }            
        }

        private void listViewChecklist_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
                SelectNextUncheckedItem();
        }

        private void listViewChecklist_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            int nrChecked = 0;
            foreach (ListViewItem listViewItem in listViewChecklist.Items)
            {
                if (listViewItem != null && string.Compare(listViewItem.Text, "X") != 0 && listViewItem.Checked)
                    nrChecked++;
            }
            if (nrChecked == numberOfItems)
            {
                buttonAccept.Enabled = true;
                buttonAccept.Focus();
                buttonAccept.Select();
            }
            else
                buttonAccept.Enabled = false;
        }

        private void ChecklistWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!statusSelected)
            {
                if (MessageBox.Show("Resultatet av kontrollen har ej angivits. Ingen information om resultatet av kontrollen kommer att sparas. Den automatiskt insamlade informationen sparas dock.\r\n\r\nÄr du säker på att du vill avsluta kontrollen?", "Checklista", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.No)
                    e.Cancel = true;                    
            }            
        }

        private void buttonAccept_Click(object sender, EventArgs e)
        {
            if (acceptedUsingSpaceKey)
                acceptedUsingSpaceKey = false;
            else if(SaveResults("APPROVED"))
            {
                statusSelected = true;
                this.Close();
            }
        }        

        private void buttonAccept_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
                acceptedUsingSpaceKey = true;
        }

        private void buttonReject_Click(object sender, EventArgs e)
        {
            if (SaveResults("REJECTED"))
            {
                statusSelected = true;
                this.Close();
            }
        }

        private bool SaveResults(string status)
        {
            Dictionary<long, bool> checkListItemStatuses = new Dictionary<long, bool>();
            foreach (ListViewItem listViewItem in listViewChecklist.Items)
                if (string.Compare(listViewItem.Text, "X") != 0)
                    checkListItemStatuses.Add((long)listViewItem.Tag, listViewItem.Checked);

            return databaseManager.SaveResult(new KeyValuePair<long, string>(checklistSer, status), checkListItemStatuses.ToArray());
        }
    }
}
