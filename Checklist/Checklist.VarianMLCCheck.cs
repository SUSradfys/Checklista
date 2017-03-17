using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMS.TPS.Common.Model.API;

namespace Checklist
{
    public partial class Checklist
    {
        private decimal spacing;
        private Dictionary<decimal, int> leaf; // = new Dictionary<decimal, int>();
        private string VarianMLCCheck(Beam beam)
        {
            string checkResult = String.Empty;
            // invalid for setup fields or fields with no MLC
            if (beam.IsSetupField || beam.ControlPoints[0].LeafPositions.Length == 0)
                return string.Empty;

            // check which MLC leafs are in field
            int nLeafs = beam.ControlPoints[0].LeafPositions.Length / 2;

            decimal top = -200m;
            spacing = 5;
            decimal position = top;
            int correspondingLeaf = -1;
            leaf = new Dictionary<decimal, int>();
            for (int i = 0; i < 2 * nLeafs; i++)
            {

                if (position <= -100 || position > 100)
                {
                    if (i % 2 == 0)
                        correspondingLeaf++;
                }
                else
                    correspondingLeaf++;
                leaf.Add(position, correspondingLeaf);

                position += spacing; // update position
                if (position > -top)
                    break;
            }

            int firstLeaf = getLeafNr(beam.ControlPoints[0].JawPositions.Y1, true);
            int lastLeaf = getLeafNr(beam.ControlPoints[0].JawPositions.Y2, false);

            double tolerance = 1.0; // allow a tolerance of 1 mm
            double min_X1 = beam.ControlPoints[0].LeafPositions[0, firstLeaf];
            double max_X2 = beam.ControlPoints[0].LeafPositions[1, firstLeaf];
            // check that max(MLC opening) + tolerace >= collimator
            for (int i = firstLeaf; i <= lastLeaf; i++)
            {
                min_X1 = Math.Min(min_X1, beam.ControlPoints[0].LeafPositions[0, i]);
                max_X2 = Math.Max(max_X2, beam.ControlPoints[0].LeafPositions[1, i]);
            }

            if (min_X1 + tolerance < beam.ControlPoints[0].JawPositions.X1 || max_X2 - tolerance > beam.ControlPoints[0].JawPositions.X2)
                checkResult += ": MLC utanför kollimator. ";
            if (min_X1 - tolerance > beam.ControlPoints[0].JawPositions.X1 || max_X2 + tolerance < beam.ControlPoints[0].JawPositions.X2)
                checkResult += ": MLC inte i linje med kollimator. ";

            // Check that MLC leaf pairs are not open outside of collimator
            for (int i = 0; i < firstLeaf; i++)
            {
                if (beam.ControlPoints[0].LeafPositions[0, i] != beam.ControlPoints[0].LeafPositions[1, i])
                {
                    checkResult += "MLC öppen utanför fältet. ";
                    break;
                }
            }
            for (int i = lastLeaf + 1; i < nLeafs; i++)
            {
                if (beam.ControlPoints[0].LeafPositions[0, i] != beam.ControlPoints[0].LeafPositions[1, i])
                {
                    checkResult += "MLC öppen utanför fältet. ";
                    break;
                }
            }
            return checkResult;
        }

        private int getLeafNr(double jawPosition, bool ceil)
        {
            if (ceil)
                jawPosition = Math.Floor(jawPosition / (double)spacing) * (int)spacing;
            else
            {
                if (jawPosition % (double)spacing == 0)
                    jawPosition -= 1;
                jawPosition = Math.Floor(jawPosition / (double)spacing) * (int)spacing;
            }
            return leaf[(decimal)jawPosition];
        }

    }
}