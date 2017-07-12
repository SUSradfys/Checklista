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
        public long CheckPrescription(string planSetupSer)
        {
            // Rather than checking if a prescription exists this method now returns the prescritption serial.
            long prescSer;
            DataTable prescription = AriaInterface.Query("select PrescriptionSer from PlanSetup where PlanSetupSer=" + planSetupSer.ToString() + " and PrescriptionSer is not null");
            if (prescription.Rows.Count > 0)
                prescSer = (long)prescription.Rows[0]["PrescriptionSer"];
            else
                prescSer = -1;

            return prescSer;
        }
    }
}