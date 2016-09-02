using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMS.TPS.Common.Model.API;

namespace Checklist
{
    public partial class Checklist
    {
        private TreatmentSide GetTreatmentSide(PlanSetup planSetup)
        {
            // Isocenter är ej relativt user origin

            double isoPosX = double.NaN;
            double userOriginX = double.NaN;

            if (planSetup.StructureSet != null && planSetup.StructureSet.Image != null)
                userOriginX = planSetup.StructureSet.Image.UserOrigin.x;

            foreach (Beam beam in planSetup.Beams)
            {
                if (double.IsNaN(isoPosX))
                    isoPosX = beam.IsocenterPosition.x;
                else if (Math.Round(isoPosX, 1) != Math.Round(beam.IsocenterPosition.x, 1))
                    return TreatmentSide.Unknown;
            }

            if (double.IsNaN(isoPosX))
                return TreatmentSide.Unknown;
            else if (isoPosX < -50 && planSetup.TreatmentOrientation.ToString().IndexOf("H") == 0 || isoPosX >= 50 && planSetup.TreatmentOrientation.ToString().IndexOf("F") == 0)
                return TreatmentSide.MinusX;
            else
                return TreatmentSide.PlusX;
        }
    }
}
