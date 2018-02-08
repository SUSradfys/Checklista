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
                List<double> delta_x1 = new List<double>();
                List<double> delta_x2 = new List<double>();
                List<double> delta_y1 = new List<double>();
                List<double> delta_y2 = new List<double>();
                
                double jaw_x1_0 = beam.ControlPoints[0].JawPositions.X1;
                double jaw_x2_0 = beam.ControlPoints[0].JawPositions.X2;
                double jaw_y1_0 = beam.ControlPoints[0].JawPositions.Y1;
                double jaw_y2_0 = beam.ControlPoints[0].JawPositions.Y2;
                if (!beam.IsSetupField)
                {

                    if (beam.TreatmentUnit.Id.IndexOf("TB") == 0)
                        isTB = true; 
                    for (int i = 1; i < beam.ControlPoints.Count; i++)
                    {

                        delta_x1.Add(jaw_x1_0 - beam.ControlPoints[i].JawPositions.X1);
                        delta_x2.Add(jaw_x2_0 - beam.ControlPoints[i].JawPositions.X2);
                        delta_y1.Add(jaw_y1_0 - beam.ControlPoints[i].JawPositions.Y1);
                        delta_x2.Add(jaw_y2_0 - beam.ControlPoints[i].JawPositions.Y2);

                    }
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
        
    }
}