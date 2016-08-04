using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMS.TPS.Common.Model.API;

namespace Checklist
{
    public partial class Checklist
    {
        private bool GetIsSplitVMAT(PlanSetup planSetup)
        {
            bool isSplit = false;
            if (numberOfTreatmentBeams < 2)
                return isSplit;
            else
            {
                double X1;
                double X2;
                foreach (Beam beam in planSetup.Beams)
                {
                    X1 = beam.ControlPoints[0].JawPositions.X1;
                    X2 = beam.ControlPoints[0].JawPositions.X2;
                    // if difference between absolute jaw openings (of X1 and X2) > 1/3 of field width: then it is split.
                    if (!beam.IsSetupField && Math.Abs(Math.Abs(beam.ControlPoints[0].JawPositions.X1) - Math.Abs(beam.ControlPoints[0].JawPositions.X2)) > 1.0 / 3.0 * (beam.ControlPoints[0].JawPositions.X2 - beam.ControlPoints[0].JawPositions.X1))
                    {
                        return true;
                    }
                }
                
            }
            return isSplit;
        }
    }
}
