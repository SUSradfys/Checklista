using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using VMS.TPS.Common.Model.API;

namespace Checklist
{
    public partial class Checklist
    {
        public bool CheckPrescription(string planSetupSer)
        {
            bool valid = true;
            DataTable prescription = AriaInterface.Query("select PrescriptionSer from PlanSetup where PlanSetupSer=" + planSetupSer.ToString() + " and PrescriptionSer is not null");
            if (prescription.Rows.Count == 0)
                valid = false;

            return valid;
        }
    }
}