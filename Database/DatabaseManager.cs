using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Data;
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

        public DatabaseManager(string server, string userId, string password, bool logFull)
        {
            this.server = server;
            this.userId = userId;
            this.password = password;
            if (logFull)
                database = "Checklist";
            else
                database = "MinimalCheck";
        }

        public bool CreateDatabase()
        {
            Exception exception = null;
            if (SQLTools.CreateDatabase(server, userId, password, database, out exception))
            {
                using (StreamReader textStreamReader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Checklist.Database.dbo.Checklist.sql")))
                {
                    string command = textStreamReader.ReadToEnd();
                    if (!SQLTools.CreateTable(server, userId, password, database, command, out exception))
                    {
                        if (exception != null)
                            System.Windows.Forms.MessageBox.Show(exception.Message, "Error creating SQL-database", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        return false;
                    }
                }
                using (StreamReader textStreamReader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Checklist.Database.dbo.ChecklistItem.sql")))
                {
                    string command = textStreamReader.ReadToEnd();
                    if (!SQLTools.CreateTable(server, userId, password, database, command, out exception))
                    {
                        if (exception != null)
                            System.Windows.Forms.MessageBox.Show(exception.Message, "Error creating SQL-database", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        return false;
                    }
                }
                return true;
            }
            else
            {
                if (exception != null)
                    System.Windows.Forms.MessageBox.Show(exception.Message, "Error creating SQL-database", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }

        public long SaveResult(string userId, string patientId, string firstName, string lastName, long patientSer, string courseId, long courseSer, string planSetupId, long planSetupSer, ChecklistItem[] checklistItems)
        {
            long checklistSer = -1;
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(@"Server=" + server + "; database=" + database + "; User ID='" + this.userId + "'; Pwd='" + password + "'"))
                {
                    sqlConnection.Open();
                    using (SqlTransaction transaction = sqlConnection.BeginTransaction())
                    {
                        bool doRollback = false;                        
                        try
                        {
                            using (SqlCommand sqlCommand = new SqlCommand("INSERT INTO Checklist (Status,DateTime,UserId,PatientId,FirstName,LastName,PatientSer,CourseId,CourseSer,PlanSetupId,PlanSetupSer) VALUES (@Status,@DateTime,@UserId,@PatientId,@FirstName,@LastName,@PatientSer,@CourseId,@CourseSer,@PlanSetupId,@PlanSetupSer); SELECT Scope_Identity();", sqlConnection, transaction))
                            {
                                sqlCommand.Parameters.AddWithValue("@Status", "UNAPPROVED");
                                sqlCommand.Parameters.AddWithValue("@DateTime", DateTime.Now);
                                sqlCommand.Parameters.AddWithValue("@UserId", userId);
                                sqlCommand.Parameters.AddWithValue("@PatientId", patientId);
                                sqlCommand.Parameters.AddWithValue("@FirstName", firstName);
                                sqlCommand.Parameters.AddWithValue("@LastName", lastName);
                                sqlCommand.Parameters.AddWithValue("@PatientSer", patientSer);
                                sqlCommand.Parameters.AddWithValue("@CourseId", courseId);
                                sqlCommand.Parameters.AddWithValue("@CourseSer", courseSer);
                                sqlCommand.Parameters.AddWithValue("@PlanSetupId", planSetupId);
                                sqlCommand.Parameters.AddWithValue("@PlanSetupSer", planSetupSer);
                                checklistSer = Convert.ToInt64(sqlCommand.ExecuteScalar());
                            }
                            foreach (ChecklistItem checklistItem in checklistItems)
                            {
                                //Console.WriteLine("Checklist ser: " + checklistSer.ToString());
                                using (SqlCommand sqlCommand = new SqlCommand("INSERT INTO ChecklistItem (ChecklistSer,ShortInfo,DetailedInfo,ShortResult,DetailedResult,CheckStatus,AutoCheckStatus,BinaryData) VALUES (@ChecklistSer,@ShortInfo,@DetailedInfo,@ShortResult,@DetailedResult,@CheckStatus,@AutoCheckStatus,@BinaryData)", sqlConnection, transaction))
                                {
                                    if (string.Compare(checklistItem.ShortInfo, "DELIMINATOR") == 0)
                                    {
                                        sqlCommand.Parameters.AddWithValue("@ChecklistSer", checklistSer);
                                        sqlCommand.Parameters.AddWithValue("@ShortInfo", checklistItem.ShortInfo);
                                        sqlCommand.Parameters.AddWithValue("@DetailedInfo", checklistItem.DetailedInfo);
                                        sqlCommand.Parameters.AddWithValue("@ShortResult", DBNull.Value);
                                        sqlCommand.Parameters.AddWithValue("@DetailedResult", DBNull.Value);
                                        sqlCommand.Parameters.AddWithValue("@CheckStatus", false);
                                        sqlCommand.Parameters.AddWithValue("@AutoCheckStatus", checklistItem.AutoCheckStatus);
                                        sqlCommand.Parameters.Add(new SqlParameter("@BinaryData", SqlDbType.VarBinary, -1));
                                        sqlCommand.Parameters["@BinaryData"].Value = DBNull.Value;
                                    }
                                    else
                                    {
                                        sqlCommand.Parameters.AddWithValue("@ChecklistSer", checklistSer);
                                        sqlCommand.Parameters.AddWithValue("@ShortInfo", checklistItem.ShortInfo);
                                        sqlCommand.Parameters.AddWithValue("@DetailedInfo", checklistItem.DetailedInfo);
                                        if (checklistItem.ShortResult == null)
                                            sqlCommand.Parameters.AddWithValue("@ShortResult", DBNull.Value);
                                        else
                                            sqlCommand.Parameters.AddWithValue("@ShortResult", checklistItem.ShortResult);
                                        if (checklistItem.DetailedResult == null)
                                            sqlCommand.Parameters.AddWithValue("@DetailedResult", DBNull.Value);
                                        else
                                            sqlCommand.Parameters.AddWithValue("@DetailedResult", checklistItem.DetailedResult.Length > 8000 ? checklistItem.DetailedResult.Substring(0, 8000) : checklistItem.DetailedResult);
                                        sqlCommand.Parameters.AddWithValue("@CheckStatus", checklistItem.Status);
                                        sqlCommand.Parameters.AddWithValue("@AutoCheckStatus", checklistItem.AutoCheckStatus);
                                        sqlCommand.Parameters.Add(new SqlParameter("@BinaryData", SqlDbType.VarBinary, -1));
                                        sqlCommand.Parameters["@BinaryData"].Value = DBNull.Value;
                                    }
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
                            transaction.Commit();
                    }
                }
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show(exception.Message, "SQL Connection Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }

            return checklistSer;
        }

    

        /*public Exception SaveResult(AnalysisResult result)
        {
            Exception exception = null;
            string sqlCommandString = "INSERT INTO Results (Status, UserId, PatientId, FirstName, LastName, PatientSer, CourseId, CourseSer, PlanSetupId, PlanSetupSer, MachineId, FractionNumber, UserName1, UserName2, DateTime, ResultDateTime, RecordingTime, AverageBaselineShift, MaxBaselineShift, StdDevBaselineShift, AverageAmplitude, StdDevAmplitude, AverageAmplitudeImages, AverageAmplitudeDiffFromImg, MaxAmplitudeDiffFromImg, AverageAmplitudeRef, AverageAmplitudeDiffFromRef, MaxAmplitudeDiffFromRef, LowerWindowLevel, UpperWindowLevel, Comment) VALUES (@status, @userId, @patientId, @firstName, @lastName, @patientSer, @courseId, @courseSer, @planSetupId, @planSetupSer, @machineId, @fractionNumber, @userName1, @userName2, @dateTime, @resultDateTime, @recordingTime, @averageBaselineShift, @maxBaselineShift, @stdDevBaselineShift, @averageAmplitude, @stdDevAmplitude, @averageAmplitudeImages, @averageAmplitudeDiffFromImg, @maxAmplitudeDiffFromImg, @averageAmplitudeRef, @averageAmplitudeDiffFromRef, @maxAmplitudeDiffFromRef, @lowerWindowLevel, @upperWindowLevel, @comment)";

            SqlConnection sqlConnection = OpenDatabase();
            try
            {
                SqlCommand sqlCommand = new SqlCommand(sqlCommandString, sqlConnection);

                sqlCommand.Parameters.AddWithValue("@status", result.Status);
                sqlCommand.Parameters.AddWithValue("@userId", result.UserId);
                sqlCommand.Parameters.AddWithValue("@patientId", result.PatientId);
                sqlCommand.Parameters.AddWithValue("@firstName", result.FirstName);
                sqlCommand.Parameters.AddWithValue("@lastName", result.LastName);
                sqlCommand.Parameters.AddWithValue("@patientSer", result.PatientSer);
                sqlCommand.Parameters.AddWithValue("@courseId", result.CourseId);
                sqlCommand.Parameters.AddWithValue("@courseSer", result.CourseSer);
                sqlCommand.Parameters.AddWithValue("@planSetupId", result.PlanSetupId);
                sqlCommand.Parameters.AddWithValue("@planSetupSer", result.PlanSetupSer);

                sqlCommand.Parameters.AddWithValue("@machineId", result.MachineId);
                sqlCommand.Parameters.AddWithValue("@fractionNumber", result.FractionNumber);
                sqlCommand.Parameters.AddWithValue("@userName1", result.UserName1);
                sqlCommand.Parameters.AddWithValue("@userName2", result.UserName2);
                sqlCommand.Parameters.AddWithValue("@dateTime", result.DateTime);

                sqlCommand.Parameters.AddWithValue("@resultDateTime", result.ResultDateTime);
                sqlCommand.Parameters.AddWithValue("@recordingTime", double.IsNaN(result.RecordingTime) ? double.MinValue : result.RecordingTime);
                sqlCommand.Parameters.AddWithValue("@averageBaselineShift", double.IsNaN(result.AverageBaselineShift) ? double.MinValue : result.AverageBaselineShift);
                sqlCommand.Parameters.AddWithValue("@maxBaselineShift", double.IsNaN(result.MaxBaselineShift) ? double.MinValue : result.MaxBaselineShift);
                sqlCommand.Parameters.AddWithValue("@stdDevBaselineShift", double.IsNaN(result.StddevBaselineShift) ? double.MinValue : result.StddevBaselineShift);
                sqlCommand.Parameters.AddWithValue("@averageAmplitude", double.IsNaN(result.AverageAmplitude) ? double.MinValue : result.AverageAmplitude);
                sqlCommand.Parameters.AddWithValue("@stdDevAmplitude", double.IsNaN(result.StddevAmplitude) ? double.MinValue : result.StddevAmplitude);
                sqlCommand.Parameters.AddWithValue("@averageAmplitudeImages", double.IsNaN(result.AverageAmplitudeImages) ? double.MinValue : result.AverageAmplitudeImages);
                sqlCommand.Parameters.AddWithValue("@averageAmplitudeDiffFromImg", double.IsNaN(result.AverageAmplitudeDiffFromImg) ? double.MinValue : result.AverageAmplitudeDiffFromImg);
                sqlCommand.Parameters.AddWithValue("@maxAmplitudeDiffFromImg", double.IsNaN(result.MaxAmplitudeDiffFromImg) ? double.MinValue : result.MaxAmplitudeDiffFromImg);
                sqlCommand.Parameters.AddWithValue("@averageAmplitudeRef", double.IsNaN(result.AverageAmplitudeRef) ? double.MinValue : result.AverageAmplitudeRef);
                sqlCommand.Parameters.AddWithValue("@averageAmplitudeDiffFromRef", double.IsNaN(result.AverageAmplitudeDiffFromRef) ? double.MinValue : result.AverageAmplitudeDiffFromRef);
                sqlCommand.Parameters.AddWithValue("@maxAmplitudeDiffFromRef", double.IsNaN(result.MaxAmplitudeDiffFromRef) ? double.MinValue : result.MaxAmplitudeDiffFromRef);
                sqlCommand.Parameters.AddWithValue("@lowerWindowLevel", double.IsNaN(result.LowerWindowLevel) ? double.MinValue : result.LowerWindowLevel);
                sqlCommand.Parameters.AddWithValue("@upperWindowLevel", double.IsNaN(result.UpperWindowLevel) ? double.MinValue : result.UpperWindowLevel);
                sqlCommand.Parameters.AddWithValue("@comment", string.Empty);

                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
                sqlCommand = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return e;
            }
            finally
            {
                sqlConnection.Close();
            }

            return exception;
        }

        public DataTable LoadResult()
        {
            try
            {
                SqlConnection sqlConnection = OpenDatabase();

                try
                {
                    SqlDataAdapter dataAdapter;

                    dataAdapter = new SqlDataAdapter("SELECT * FROM Results", sqlConnection);

                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);

                    return dataTable;
                }
                catch
                {
                }

                sqlConnection.Close();
            }
            catch
            {
            }

            return null;
        }

        public string[] GetFileNames(bool onlyNew)
        {
            try
            {
                SqlConnection sqlConnection = OpenDatabase();

                try
                {
                    SqlDataAdapter dataAdapter;

                    if (onlyNew)
                        dataAdapter = new SqlDataAdapter("SELECT FileName FROM RPMFiles WHERE New='true'", sqlConnection);
                    else
                        dataAdapter = new SqlDataAdapter("SELECT FileName FROM RPMFiles", sqlConnection);

                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);

                    string[] files = new string[dataTable.Rows.Count];
                    for (int i = 0; i < files.Length; i++)
                        files[i] = dataTable.Rows[i][0].ToString();

                    return files;
                }
                catch
                {
                }

                sqlConnection.Close();
            }
            catch
            {
            }

            return new string[0];
        }

        public Exception AddFileToDatabase(string path, bool newFile)
        {
            Exception exception = null;
            string sqlCommandString = "INSERT INTO RPMFiles (FileName, New) VALUES (@fileName, @new)";

            SqlConnection sqlConnection = OpenDatabase();
            try
            {
                string fileName = Path.GetFileNameWithoutExtension(path);
                SqlCommand sqlCommand = new SqlCommand(sqlCommandString, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@fileName", fileName);
                sqlCommand.Parameters.AddWithValue("@new", newFile);
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
                sqlCommand = null;
            }
            catch (SqlException e)
            {
                if (e.Number != 2627 && e.Number != 2601)
                    return e;
            }
            catch (Exception e)
            {
                return e;
            }
            finally
            {
                sqlConnection.Close();
            }

            return exception;
        }*/
    }
}
