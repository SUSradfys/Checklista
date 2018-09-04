using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace Checklist
{
    public partial class Checklist
    {
        private IEnumerable<VVector> GetAllIsocenters(PlanSetup planSetup)
        {
            List<VVector> allIsoPos = new List<VVector>();
            //VVector firstIsoPos = new VVector(double.NaN, double.NaN, double.NaN);
            //VVector secondIsoPos = new VVector(double.NaN, double.NaN, double.NaN);
            //VVector thirdIsoPos = new VVector(double.NaN, double.NaN, double.NaN);
            // Maximum of three isos lookup, who creates a plan with > 3 isocenters? 
            // List<double> ssds = planSetup.Beams.Select(x => Math.Round(x.SSD)).ToList();
            foreach (Beam beam in planSetup.Beams)
            {
                if (!beam.IsSetupField) //Only include treatment isocenters
                    allIsoPos.Add(beam.IsocenterPosition); 
                //double allowedDiff = 1;  // the allowed difference between isocenters in mm
                //if (double.IsNaN(firstIsoPos.x) && double.IsNaN(firstIsoPos.y) && double.IsNaN(firstIsoPos.z))
                //{
                //    firstIsoPos = beam.IsocenterPosition;
                //    allIsoPos.Add(firstIsoPos);
                //    uniqueIsoPos.Add(firstIsoPos); 
                //}

                ////else if (Math.Round(s5_isocenterPosition.x, 1) == Math.Round(beam.IsocenterPosition.x, 1) && Math.Round(s5_isocenterPosition.y, 1) == Math.Round(beam.IsocenterPosition.y, 1) && Math.Round(s5_isocenterPosition.z, 1) == Math.Round(beam.IsocenterPosition.z, 1))
                //else if (Math.Abs(firstIsoPos.x - beam.IsocenterPosition.x) >= allowedDiff || Math.Abs(firstIsoPos.y - beam.IsocenterPosition.y) >= allowedDiff || Math.Abs(firstIsoPos.z - beam.IsocenterPosition.z) >= allowedDiff)
                //{
                //    if (double.IsNaN(secondIsoPos.x) && double.IsNaN(secondIsoPos.y) && double.IsNaN(secondIsoPos.z))
                //    {
                //        secondIsoPos = beam.IsocenterPosition;
                //        allIsoPos.Add(secondIsoPos);
                //        uniqueIsoPos.Add(secondIsoPos);

                //    }
                //    else if ((Math.Abs(firstIsoPos.x - beam.IsocenterPosition.x) >= allowedDiff || Math.Abs(firstIsoPos.y - beam.IsocenterPosition.y) >= allowedDiff || Math.Abs(firstIsoPos.z - beam.IsocenterPosition.z) >= allowedDiff) &&(Math.Abs(secondIsoPos.x - beam.IsocenterPosition.x) >= allowedDiff || Math.Abs(secondIsoPos.y - beam.IsocenterPosition.y) >= allowedDiff || Math.Abs(secondIsoPos.z - beam.IsocenterPosition.z) >= allowedDiff))
                //    {
                //        if (double.IsNaN(thirdIsoPos.x) && double.IsNaN(thirdIsoPos.y) && double.IsNaN(thirdIsoPos.z))
                //        {
                //            thirdIsoPos = beam.IsocenterPosition;
                //            allIsoPos.Add(thirdIsoPos);
                            

                //        }
                        

                //    }



                //}
                //else allIsoPos.Add(beam.IsocenterPosition);
            }

            IEnumerable<VVector> uniqueIsoPos = allIsoPos.Distinct(); 

            return uniqueIsoPos; 
        }



    }
}