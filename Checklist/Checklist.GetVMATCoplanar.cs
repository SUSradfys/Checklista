using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMS.TPS.Common.Model.API;

namespace Checklist
{
    public partial class Checklist
    {
        private bool GetVMATCoplanar(PlanSetup planSetup)
        {
            bool coplanar = true;
            foreach (Beam beam in planSetup.Beams)
            {
                if (!beam.IsSetupField && beam.ControlPoints[0].PatientSupportAngle != 0)
                    return false;

            }
            
            return coplanar;
        }
    }
}