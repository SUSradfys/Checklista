using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.IO;

namespace Checklist
{
    public static class SQLTools
    {
        public static bool CreateDatabase(string serverName, string userId, string pwd, string databaseName, out Exception lastException)
        {
            string command;
            bool error = false;

            lastException = null;

            try
            {
                SqlConnection sqlConnection = new SqlConnection(@"Server=" + serverName + "; database=master; User ID='" + userId + "'; Pwd='" + pwd + "'");
                sqlConnection.Open();

                try
                {
                    command = "CREATE DATABASE " + databaseName;
                    SqlCommand sqlCommand = new SqlCommand(command, sqlConnection);
                    sqlCommand.ExecuteNonQuery();
                }
                catch (SqlException exception)
                {
                    if (exception.Number != 1801) //  1801 - Database exists
                    {
                        lastException = exception;
                        error = true;
                    }
                }
                catch (Exception exception)
                {
                    lastException = exception;
                    error = true;
                }

                sqlConnection.Close();
            }
            catch (Exception exception)
            {
                lastException = exception;
                error = true;
            }

            return !error;
        }

        public static bool CreateTable(string serverName, string userId, string pwd, string databaseName, string command, out Exception lastException)
        {
            bool error = false;

            lastException = null;

            try
            {
                SqlConnection sqlConnection = new SqlConnection(@"Server=" + serverName + "; database=" + databaseName + "; User ID='" + userId + "'; Pwd='" + pwd + "'");
                sqlConnection.Open();

                try
                {
                    SqlCommand sqlCommand = new SqlCommand(command, sqlConnection);
                    sqlCommand.ExecuteNonQuery();
                }
                catch (SqlException exception)
                {
                    if (exception.Number != 2714) // 2714 - Table exist
                    {
                        lastException = exception;
                        error = true;
                    }
                }
                catch (Exception exception)
                {
                    lastException = exception;
                    error = true;
                }

                sqlConnection.Close();
            }
            catch (Exception exception)
            {
                lastException = exception;
                error = true;
            }

            return !error;
        }
    }
}
