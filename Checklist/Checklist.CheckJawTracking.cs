//Added by JSR, 2018-01-09
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
        private Tuple<string,AutoCheckStatus> CheckJawTracking(PlanSetup planSetup)
        {
            string stringOut = string.Empty;
            string beamWithJTText = string.Empty;
            bool isTB = false;
            foreach (Beam beam in planSetup.Beams)
            {
                IEnumerable<double> delta_x1 = new List<double>();
                IEnumerable<double> delta_x2 = new List<double>();
                IEnumerable<double> delta_y1 = new List<double>();
                IEnumerable<double> delta_y2 = new List<double>();
                
                double jaw_x1_0 = beam.ControlPoints[0].JawPositions.X1;
                double jaw_x2_0 = beam.ControlPoints[0].JawPositions.X2;
                double jaw_y1_0 = beam.ControlPoints[0].JawPositions.Y1;
                double jaw_y2_0 = beam.ControlPoints[0].JawPositions.Y2;
                if (!beam.IsSetupField)
                {

                    if (beam.TreatmentUnit.Id.IndexOf("TB") == 0)
                        isTB = true;
                    
                    delta_x1 = beam.ControlPoints.Select(j => beam.ControlPoints[0].JawPositions.X1 - j.JawPositions.X1);
                    delta_x2 = beam.ControlPoints.Select(j => beam.ControlPoints[0].JawPositions.X2 - j.JawPositions.X2);
                    delta_y1 = beam.ControlPoints.Select(j => beam.ControlPoints[0].JawPositions.Y1 - j.JawPositions.Y1);
                    delta_x2 = beam.ControlPoints.Select(j => beam.ControlPoints[0].JawPositions.Y2 - j.JawPositions.Y2);
                    
                    //for (int i = 1; i < beam.ControlPoints.Count; i++)
                    //{
                        
                    //    delta_x1.Add(jaw_x1_0 - beam.ControlPoints[i].JawPositions.X1);
                    //    delta_x2.Add(jaw_x2_0 - beam.ControlPoints[i].JawPositions.X2);
                    //    delta_y1.Add(jaw_y1_0 - beam.ControlPoints[i].JawPositions.Y1);
                    //    delta_x2.Add(jaw_y2_0 - beam.ControlPoints[i].JawPositions.Y2);

                    //}
                    if (delta_x1.Sum() != 0 || delta_x2.Sum() != 0 || delta_y1.Sum() != 0 || delta_y2.Sum() != 0)
                        beamWithJTText += (String.IsNullOrEmpty(beamWithJTText) ? "" : ", ") + beam.Id;
                }
            }

            if (String.IsNullOrEmpty(beamWithJTText))
            { 
                return new Tuple<string,AutoCheckStatus>("Jawtracking är ej aktivt, verifiera",(isTB ? AutoCheckStatus.WARNING : AutoCheckStatus.MANUAL ));
            }
            else
                return new Tuple<string, AutoCheckStatus>("Jawtracking är aktivt för fält: " + beamWithJTText,AutoCheckStatus.MANUAL); 
        }
        private void GetFieldSizeGridSize(PlanSetup planSetup, double lim, ChecklistType checklistType, out string outText, out AutoCheckStatus checkstatusOut) // Section for controlling field size in all types of plans Lim in mm!
        {
            //Note Lim is in mm! 
            //This part checks the size of the fields. 
            outText = string.Empty;
            checkstatusOut = AutoCheckStatus.MANUAL;
            string calcSize;
            int tol = 25;
            bool isOverTol = false;
            foreach (Beam beam in planSetup.Beams)
            {
                if (!beam.IsSetupField)
                {
                    var xSz = beam.ControlPoints.Select(i => Math.Abs(i.JawPositions.X2 - i.JawPositions.X1));
                    var ySz = beam.ControlPoints.Select(i => Math.Abs(i.JawPositions.Y2 - i.JawPositions.Y1));
                    double percX = 0;
                    double percY = 0;
                    
                    if (xSz.Min() < lim)
                    {
                        percX = (double)xSz.Where(v => v <= lim).Count()/(double)xSz.Count()*100;
                    }
                    if (ySz.Min() < lim)
                    {
                        percY = (double)ySz.Where(v => v <= lim).Count()/ (double)ySz.Count()*100;
                    }
                    if (percX > 0 || percY > 0)
                    { 
                        if (checklistType == ChecklistType.Eclipse || checklistType == ChecklistType.EclipseGating)
                            outText += (String.IsNullOrEmpty(outText) ? "" : ", ") + beam.Id + ": " + (percX > 0 ? "X-kollimator < " + lim + " mm" : "") + (percY > 0 ? " Y-kollimator < " + lim + " mm" : "");
                        else
                            outText += (String.IsNullOrEmpty(outText) ? "" : ", ") + beam.Id + ": " + (percX > 0 ? "X-kollimator < " + lim + " mm i " + percX.ToString("0.0") + "% av segmenten" : "") + (percY > 0 ? " Y-kollimator < " + lim + " mm i " + percY.ToString("0.0") + "% av segmenten" : "");
                        if (percX > tol || percY > tol)
                            isOverTol = true; 
                    }
                    
                }
            }
            if (!planSetup.PhotonCalculationOptions.TryGetValue("CalculationGridSizeInCM", out calcSize))
            {
                outText = "Ingen beräkningsupplösning " + outText;
                checkstatusOut = AutoCheckStatus.FAIL;
            }
            else
            {
                //Ger följande utfall
                if (calcSize == "0.1" && !String.IsNullOrEmpty(outText)) // outText är ej tom, dvs någon del av något fält < lim mm. Då skall Algoritmen
                    checkstatusOut = AutoCheckStatus.PASS;
                else if (calcSize == "0.25" && String.IsNullOrEmpty(outText))
                    checkstatusOut = AutoCheckStatus.PASS;
                else if (calcSize == "0.25" && !String.IsNullOrEmpty(outText))
                {
                    if (isOverTol) // if more than 25 % of the segments are less than the set value lim this will give a fail, else Warning. 
                        checkstatusOut = AutoCheckStatus.FAIL;
                    else
                        checkstatusOut = AutoCheckStatus.WARNING;
                }

                else if (calcSize == "0.1" && String.IsNullOrEmpty(outText))
                {
                    checkstatusOut = AutoCheckStatus.WARNING;
                    outText = ", Rekommenderat med 0.25 cm för fält > " + lim / 10 + " cm.";
                }
                outText = calcSize + " cm. " + outText;
                
            }
            //return new Tuple<string, AutoCheckStatus>(outText, checkstatusOut); 
        }
    }
}