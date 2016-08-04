using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Checklist
{
    public static class AriaInterface
    {
        private static SqlConnection connection = null;

        public static void Connect()
        {
            connection = new SqlConnection("Data Source='" + Settings.ARIA_SERVER + "';UID='" + Settings.ARIA_USERNAME + "';PWD='" + Settings.ARIA_PASSWORD + "';Database='" + Settings.ARIA_DATABASE +"';");
            connection.Open();
        }

        public static void Disconnect()
        {
            connection.Close();
        }

        public static DataTable Query(string queryString)
        {
            DataTable dataTable = new DataTable();
            try
            {
                SqlDataAdapter adapter = new SqlDataAdapter(queryString, connection) { MissingMappingAction = MissingMappingAction.Passthrough, MissingSchemaAction = MissingSchemaAction.Add };
                adapter.Fill(dataTable);
                adapter.Dispose();
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show(exception.Message, "SQL Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }

            return dataTable;
        }

        public static void GetPlanSetupSer(string patientId, string courseId, string planSetupId, out long patientSer, out long courseSer, out long planSetupSer)
        {
            patientSer = -1;
            courseSer = -1;
            planSetupSer = -1;

            DataTable dataTableSerials = Query("select Patient.PatientSer,Course.CourseSer,PlanSetup.PlanSetupSer from Patient,Course,PlanSetup where PatientId='" + patientId + "' and CourseId='" + courseId + "' and PlanSetupId='" + planSetupId + "' and Course.PatientSer=Patient.PatientSer and PlanSetup.CourseSer=Course.CourseSer");
            if (dataTableSerials.Rows.Count == 1)
            {
                patientSer = (long)dataTableSerials.Rows[0][0];
                courseSer = (long)dataTableSerials.Rows[0][1];
                planSetupSer = (long)dataTableSerials.Rows[0][2];
            }
        }
    }
}
