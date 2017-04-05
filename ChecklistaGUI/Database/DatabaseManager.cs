using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Data.SqlClient;

namespace Checklist
{
    public class DatabaseManager
    {
        private readonly string database = "Checklist";
        //private string database;
        private string server;
        private string userId;
        private string password;

        public DatabaseManager(string server, string userId, string password)
        {
            this.server = server;
            this.userId = userId;
            this.password = password;
        }

        public string GetStatus(long checklistSer, out string userId)
        {
            string status = string.Empty;
            userId = string.Empty;
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(@"Server=tcp:" + server + ", 1433; database=" + database + "; User ID='" + this.userId + "'; Pwd='" + password + "'" + "; Connection Timeout=300"))
                //using (SqlConnection sqlConnection = new SqlConnection(@"Server=" + server.Replace("\\\\", "\\") + "; database=" + database + "; User ID='" + this.userId + "'; Pwd='" + password + "'; Connection Timeout=300"))
                {
                    sqlConnection.Open();

                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter("SELECT Status,UserId FROM Checklist where ChecklistSer=" + checklistSer.ToString(), sqlConnection))
                    {
                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);
                        if (dataTable.Rows.Count == 1)
                        {
                            status = ((string)dataTable.Rows[0][0]).Trim();
                            userId = ((string)dataTable.Rows[0][1]).Trim();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show(exception.Message, "SQL Query Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            return status;
        }

        public DataTable GetResults(long checklistSer)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(@"Server=tcp:" + server + ", 1433; database=" + database + "; User ID='" + this.userId + "'; Pwd='" + password + "'" + "; Connection Timeout=300"))
                //using (SqlConnection sqlConnection = new SqlConnection(@"Server=" + server + "; database=" + database + "; User ID='" + this.userId + "'; Pwd='" + password + "'"))
                {
                    sqlConnection.Open();

                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter("SELECT * FROM ChecklistItem where ChecklistSer=" + checklistSer.ToString() + " order by ChecklistItemSer", sqlConnection))
                    {
                        dataAdapter.Fill(dataTable);
                    }
                }
            }
            catch(Exception exception)
            {
                System.Windows.Forms.MessageBox.Show(exception.Message, "SQL Query Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }

            return dataTable;
        }

        public bool SaveResult(KeyValuePair<long,string> checklistStatus, KeyValuePair<long,bool>[] checklistItemStatuses)
        {
            bool saveCompleted = false;

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(@"Server=tcp:" + server + ", 1433; database=" + database + "; User ID='" + this.userId + "'; Pwd='" + password + "'" + "; Connection Timeout=300"))
                //using (SqlConnection sqlConnection = new SqlConnection(@"Server=" + server + "; database=" + database + "; User ID='" + this.userId + "'; Pwd='" + password + "'"))
                {
                    sqlConnection.Open();
                    using (SqlTransaction transaction = sqlConnection.BeginTransaction())
                    {
                        bool doRollback = false;
                        try
                        {
                            using (SqlCommand sqlCommand = new SqlCommand("UPDATE Checklist SET Status=@Status WHERE ChecklistSer=@ChecklistSer", sqlConnection, transaction))
                            {
                                sqlCommand.Parameters.AddWithValue("@ChecklistSer", checklistStatus.Key);
                                sqlCommand.Parameters.AddWithValue("@Status", checklistStatus.Value);
                                sqlCommand.ExecuteNonQuery();
                            }
                            foreach (KeyValuePair<long, bool> checklistItem in checklistItemStatuses)
                            {
                                using (SqlCommand sqlCommand = new SqlCommand("UPDATE ChecklistItem SET CheckStatus=@CheckStatus WHERE ChecklistItemSer=@ChecklistItemSer", sqlConnection, transaction))
                                {
                                    sqlCommand.Parameters.AddWithValue("@ChecklistItemSer", checklistItem.Key);
                                    sqlCommand.Parameters.AddWithValue("@CheckStatus", checklistItem.Value);
                                    sqlCommand.ExecuteNonQuery();
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            doRollback = true;
                            System.Windows.Forms.MessageBox.Show(exception.Message, "SQL Transaction Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        }
                        if (doRollback)
                            transaction.Rollback();
                        else
                        {
                            transaction.Commit();
                            saveCompleted = true;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show(exception.Message, "SQL Connection Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }

            return saveCompleted;
        }
    }
}
