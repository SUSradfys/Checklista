//André Haraldsson 2016-04-08
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;
using System.Windows;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Windows.Forms;
using System.IO;




namespace Checklist
{
    public partial class Checklist
    {
        private string ElektaMLCCheck(PlanSetup planSetup)
        {
            //return string for the method
            string outString = "";

            bool ElektaTest = false;

            CheckMLC checkMlCcurr = new CheckMLC();
            int bmFailCount = 0;

            //Check the mlc positions for each beam/colmtrol point
            foreach (var bm in planSetup.Beams)
            {
                int cpCount = 0;

                //method to check if Elekta machine                    
                string tmUnit = bm.TreatmentUnit.ToString().Split(':')[0];
                bool ElektaTrue = checkMlCcurr.CheckMachine(tmUnit);
                ElektaTest = ElektaTrue;

                if (ElektaTrue)
                {

                    foreach (var cp in bm.ControlPoints)
                    {
                        //check if there is an mlc fitted and only first controlpoint per beam
                        if (cp.LeafPositions.Length > 0 && cpCount < 1)
                        {

                            //Check leaf gap, return list of positions that failed
                            var debugList = checkMlCcurr.CheckMlcGap(cp.LeafPositions, cp.JawPositions.Y1, cp.JawPositions.Y2);

                            //add beam and controlpoint number to list and add to debuglist and clear the temp lists
                            var beamCpDebuglist = debugList.Select(x => "For beam " + bm.Id + ", " + x + ". ").ToList();
                            checkMlCcurr.DebugMess.AddRange(beamCpDebuglist);
                            beamCpDebuglist.Clear();
                            debugList.Clear();

                            //check the interdigitation, machine limit in checkMLC class
                            debugList = checkMlCcurr.CheckMlcInterdigitation(cp.LeafPositions, cp.JawPositions.Y1, cp.JawPositions.Y2);
                            beamCpDebuglist = debugList.Select(x => "For beam " + bm.Id + ", " + x + ". ").ToList();
                            checkMlCcurr.DebugMess.AddRange(beamCpDebuglist);
                            beamCpDebuglist.Clear();
                            debugList.Clear();

                            //Check open leaf pair at collimator Y1 and Y2
                            debugList = checkMlCcurr.CheckMlcOpenPair(cp.LeafPositions, cp.JawPositions.Y1, cp.JawPositions.Y2);
                            beamCpDebuglist = debugList.Select(x => "For beam " + bm.Id + ", " + x + ". ").ToList();
                            checkMlCcurr.DebugMess.AddRange(beamCpDebuglist);
                            beamCpDebuglist.Clear();
                            debugList.Clear();


                        }
                        cpCount++;
                    }//foreach cp

                }//if treatment unit elekta
                else
                {
                    //only on the first "failed" ie. non elekta beam
                    if (bmFailCount < 1)
                    {
                        //System.Windows.Forms.MessageBox.Show("Script only valid for Elekta, wrong Linac: " + bm.TreatmentUnit.ToString());
                        outString = "Inte en Elekta linac";

                    }

                    bmFailCount++;
                }

            }//foreach beam                             

            //print the failed positions
            if (ElektaTest)
            {
                int noElementList = checkMlCcurr.DebugMess.Count();

                if (checkMlCcurr.DebugMess.Any())
                {
                    outString = "MLC positioner inte OK: Kör separat scrpt för detaljerad information.";
                }
                else
                {
                    outString = "MLC positioner OK.";
                }
            }

            return outString;
        }
    }//class scripts  


    /// <summary>
    /// Class that holds the collimator and mlc values for mlc check, one instance per field
    /// </summary>
    class CheckMLC
    {

        private const double MLCleafgap = 1.0; //Allowed minimum leaf gap for elekta
        private const double CollimatorLeafopen = 0.3; //allowed gap to leaf pair of collimator before extra pair needs to align to open pair
        private const double AllowedMlcAlignGap = 0.3;
        private const double mmToCm = 0.1; //mlc pos is in mm handled like cm
        private bool AllMlcGapOutsideField0 = false;

        private List<string> debugMess = new List<string>();
        public List<string> DebugMess { get { return debugMess; } set { debugMess = value; } }



        /// <summary>
        /// Checks if the MLC leafs are inside collimator X positions for all leafs in field. returns failed positions
        /// </summary>
        /// <param name="mlc"></param>
        /// <param name="collX1"></param>
        /// <param name="collX2"></param>
        /// <param name="collY1"></param>
        /// <param name="collY2"></param>
        /// <returns></returns>
        public List<string> CheckMlcLimitX(float[,] mlc, double collX1, double collX2, double collY1, double collY2)
        {
            List<string> debugList = new List<string>();

            //Y1 goes from postion 0 to 20, Y2 from 20-39
            int posLeafY1 = (int)(20 + Math.Ceiling(collY1 * mmToCm));
            int posLeafY2 = (int)(20 + Math.Floor(collY2 * mmToCm)) + 1;

            //MessageBox.Show(collX1.ToString() +" X2: "+collX2.ToString());
            for (int i = posLeafY1 - 1; i < posLeafY2; i++)
            {
                bool diffX1 = Math.Round(mlc[0, i] * mmToCm, 3) >= Math.Round(collX1 * mmToCm, 3);
                bool diffX2 = Math.Round(mlc[1, i] * mmToCm, 3) <= Math.Round(collX2 * mmToCm, 3);

                //if (i == 21 - 1) { MessageBox.Show(diffX1.ToString() + " round" + Math.Round(mlc[0, i] * mmToCm, 3).ToString() + " collx1 " +  Math.Round(collX1 * mmToCm,3).ToString()); }
                //if false then mlc is outside limit (collimator pos) 
                if (!diffX1) { debugList.Add("MLC outside collimator X1 at position: " + (i + 1).ToString()); }
                if (!diffX2) { debugList.Add("MLC outside collimator X2 at position: " + (i + 1).ToString()); }
            }

            return debugList;
        }


        /// <summary>
        /// TAkes mlc as float multi array and returns a list of positions that violates leaf gap set as const in class
        /// </summary>
        /// <param name="mlc"></param>
        /// <returns></returns>
        public List<string> CheckMlcGap(float[,] mlc, double collY1, double collY2)
        {
            List<string> debugList = new List<string>();
            bool allMlcGap0 = true;

            //does not check extra leaf if coll is closer then 0,3mm or does not check at all!
            //int CloseToLeaf = ((collY1 * mmToCm - Math.Floor(collY1 * mmToCm)) <= 0.3) ? -1 : 0; 

            //Check if ALL MLC has zero gap outside field. first from pos 0 to coll y1
            for (int i = 0; i < Math.Floor(collY1 * mmToCm) + 20 - 1; i++)
            {
                double diff = Math.Round(mlc[1, i] * mmToCm - mlc[0, i] * mmToCm, 3);
                allMlcGap0 = (diff == 0) ? allMlcGap0 : false;
            }

            //does not check extra leaf if coll is closer then 0,3mm
            //int CloseToLeafY2 = ((Math.Ceiling(collY2 * mmToCm) - collY2 * mmToCm) <= 0.3) ? 1 : 0;

            for (int i = (int)Math.Ceiling(collY2 * mmToCm) + 20 + 1; i < 40; i++)
            {
                double diff = Math.Round(mlc[1, i] * mmToCm - mlc[0, i] * mmToCm, 3);
                allMlcGap0 = (diff == 0) ? allMlcGap0 : false;
            }

            if (allMlcGap0)
            {
                //check only inside field if all mlc gap = 0
                for (int i = (int)(Math.Floor(collY1 * mmToCm) + 20); i < (int)Math.Ceiling(collY2 * mmToCm) + 20; i++)
                {
                    double diff = Math.Round(mlc[1, i] * mmToCm - mlc[0, i] * mmToCm, 3);
                    //if (i == 0) { MessageBox.Show("diff: " + diff.ToString()); }               

                    if (diff < MLCleafgap) { debugList.Add("Wrong Leaf gap at position: " + (i + 1).ToString()); }
                }

            }

            else
            {
                //iterate over the mlc length and check leaf gap, X1 is bank 0, X2 is bank 1
                for (int i = 0; i < mlc.GetLength(1); i++)
                {
                    double diff = Math.Round(mlc[1, i] * mmToCm - mlc[0, i] * mmToCm, 3);
                    //if (i == 0) { MessageBox.Show("diff: " + diff.ToString()); }               

                    if (diff < MLCleafgap) { debugList.Add("Wrong Leaf gap at position: " + (i + 1).ToString()); }
                }
            }

            AllMlcGapOutsideField0 = allMlcGap0;
            return debugList;
        }

        /// <summary>
        /// Takes mlc as multi array float in and return a list of position that are closer then MLCleafgap set in  class
        /// </summary>
        /// <param name="mlc"></param>
        /// <returns></returns>
        public List<string> CheckMlcInterdigitation(float[,] mlc, double collY1, double collY2)
        {
            List<string> debugList = new List<string>();

            //Check if ALL MLC has zero gap outside field. first from pos 0 to coll y1


            if (AllMlcGapOutsideField0)
            {
                for (int i = (int)(Math.Floor(collY1 * mmToCm) + 20); i < (int)Math.Ceiling(collY2 * mmToCm) + 19; i++)
                {
                    double diff = Math.Round(mlc[1, i + 1] * mmToCm - mlc[0, i] * mmToCm, 3);

                    if (diff < MLCleafgap) { debugList.Add("Interdigation error at position: " + (i + 1).ToString() + "-" + (i + 2).ToString()); }
                }
                for (int i = (int)(Math.Floor(collY1 * mmToCm) + 20); i < (int)Math.Ceiling(collY2 * mmToCm) + 19; i++)
                {
                    double diff = Math.Round(mlc[1, i] * mmToCm - mlc[0, i + 1] * mmToCm, 3);

                    if (diff < MLCleafgap) { debugList.Add("Interdigation error at position: " + (i + 1).ToString() + "-" + (i + 2).ToString()); }
                }

            }
            else
            {
                //iterate over the mlc length -1 and with one bank shifted 1 position. check leaf gap, X1 is bank 0, X2 is bank 1
                for (int i = 0; i < (mlc.GetLength(1) - 1); i++)
                {
                    double diff = Math.Round(mlc[1, i + 1] * mmToCm - mlc[0, i] * mmToCm, 3);

                    if (diff < MLCleafgap) { debugList.Add("Interdigation error at position: " + (i + 1).ToString() + "-" + (i + 2).ToString()); }
                }
                for (int i = 0; i < (mlc.GetLength(1) - 1); i++)
                {
                    double diff = Math.Round(mlc[1, i] * mmToCm - mlc[0, i + 1] * mmToCm, 3);

                    if (diff < MLCleafgap) { debugList.Add("Interdigation error at position: " + (i + 1).ToString() + "-" + (i + 2).ToString()); }
                }
            }
            return debugList;
        }


        /// <summary>
        /// Takes the mlc and colliamtor position and checks if and extra leaf pair is opened if collimator is closer
        /// then constant set in class. Return list of failed position. 
        /// </summary>
        /// <param name="mlc"></param>
        /// <param name="collY1"></param>
        /// <param name="collY2"></param>
        /// <returns></returns>
        public List<string> CheckMlcOpenPair(float[,] mlc, double collY1, double collY2)
        {
            List<string> debugList = new List<string>();
            collY1 = Math.Round(collY1, 2);
            collY2 = Math.Round(collY2, 2);

            //Y1 goes from postion 0 to 20, Y2 from 20-39
            int posLeafY1 = (int)(20 + Math.Floor(collY1 * mmToCm)); //20 + -20 (to) 0 => 0-20
            int posLeafY2 = (int)(19 + Math.Ceiling(collY2 * mmToCm)); //19-39 in mlc

            double decLeafY1 = Math.Round(collY1 * mmToCm - Math.Floor(collY1 * mmToCm), 2);
            double decLeafY2 = Math.Round(Math.Ceiling(collY2 * mmToCm) - collY2 * mmToCm, 2);

            //if closer to next leaf pair then constant  CollimatorLeafopen
            if (decLeafY1 <= CollimatorLeafopen && collY2 * 0.1 > -19)
            {
                //do not check if max coll (-19.01 to -20)
                if (posLeafY1 > 0)
                {
                    double diffMlcBank0 = Math.Abs(mlc[0, posLeafY1] * mmToCm - mlc[0, posLeafY1 - 1] * mmToCm);
                    double diffMlcBank1 = Math.Abs(mlc[1, posLeafY1] * mmToCm - mlc[1, posLeafY1 - 1] * mmToCm);

                    //change to bank 0 "failed right" bank 1 "failed left"? for y1 
                    /*
                    if (diffMlcBank0 > AllowedMlcAlignGap || diffMlcBank1 > AllowedMlcAlignGap ) 
                    {
                        debugList.Add("MLC pair not align at Y1 position: " + (posLeafY1-1).ToString()); 
                    }
                     old code
                     */

                    //left bank
                    if (diffMlcBank0 > AllowedMlcAlignGap)
                    {
                        debugList.Add("MLC pair not align at Y1 for bank 0 \t left bank when collimator in 0 degrees");
                    }
                    if (diffMlcBank1 > AllowedMlcAlignGap)
                    {
                        debugList.Add("MLC pair not align at Y1 for bank 1 \t right bank when collimator in 0 degrees");
                    }

                }
            }//Y1

            //if closer to next leaf pair then constant  CollimatorLeafopen
            if (decLeafY2 <= CollimatorLeafopen && collY2 * 0.1 < 19)
            {
                //do not check if max coll (-19.01 to -20)
                if (posLeafY2 < 39)
                {
                    double diffMlcBank0 = Math.Abs(mlc[0, posLeafY2 + 1] * mmToCm - mlc[0, posLeafY2] * mmToCm);
                    double diffMlcBank1 = Math.Abs(mlc[1, posLeafY2 + 1] * mmToCm - mlc[1, posLeafY2] * mmToCm);
                    /*
                    if (diffMlcBank0 > AllowedMlcAlignGap || diffMlcBank1 > AllowedMlcAlignGap)
                    {
                        debugList.Add("MLC pair not align at Y2 position: " + posLeafY2.ToString());
                    }*/

                    //devide into right and left failed
                    //left bank
                    if (diffMlcBank0 > AllowedMlcAlignGap)
                    {
                        debugList.Add("MLC pair not align at Y2 for bank 0 \t left bank when collimator in 0 degrees");
                    }
                    if (diffMlcBank1 > AllowedMlcAlignGap)
                    {
                        debugList.Add("MLC pair not align at Y2 for bank 1 \t right bank when collimator in 0 degrees");
                    }
                }
            }//Y2

            return debugList;
        }//method



        /// <summary>
        /// Check if the machine is an elekta, return true if
        /// </summary>
        /// <param name="machine"></param>
        /// <returns></returns>
        public bool CheckMachine(string machine)
        {
            return machine == "L07" || machine == "L05" || machine == "L09" || machine == "L03";
        }


    }//calss CheckMLC
}//namspace
