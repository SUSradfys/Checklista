using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMS.TPS.Common.Model.API;

namespace Checklist
{
    public partial class Checklist
    {
        private void GetMU(Beam beam, out double openMU, out double wedgedMU)
        {
            openMU = beam.Meterset.Value;
            wedgedMU = 0;

            if (beam.Wedges.Count() > 0)
            {
                Wedge wedge = beam.Wedges.ElementAt(0);

                if (beam.ControlPoints.Count == 4)
                {
                    wedgedMU = beam.ControlPoints[1].MetersetWeight * beam.Meterset.Value;
                    openMU = (1 - beam.ControlPoints[2].MetersetWeight) * beam.Meterset.Value;
                }
                else
                {
                    openMU = 0;
                    wedgedMU = beam.ControlPoints[1].MetersetWeight * beam.Meterset.Value;
                }
            }
        }
    }
}
