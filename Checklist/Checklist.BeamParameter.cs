using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMS.TPS.Common.Model.API;

namespace Checklist
{
    public partial class Checklist
    {
        private string reorderBeamParam(string input, string delimeter)
        {
            input = input.Replace(delimeter, "¤");
            string output = string.Empty;
            List<BeamParam> beamParameters = new List<BeamParam>();
            // split input string to array
            string[] stringArray = Array.ConvertAll(input.Split('¤'), p => p.Trim());
            // loop through add instance of class to list
            foreach (string s in stringArray)
            {
                beamParameters.Add(new BeamParam(s.Split(':')[0].Trim(), s.Substring(s.IndexOf(':') + 1)));//s.Split(':')[1].Trim()));
            }
            // reorder
            beamParameters = beamParameters.OrderBy(o => o.OrderId).ToList();
            // build output
            foreach (BeamParam bp in beamParameters)
            {
                if (bp.BeamId.Length > 0)
                    output += (output.Length == 0 ? string.Empty : delimeter + " ") + bp.BeamId + ":" + bp.Value;
            }
            
            return output;
        }

        public class BeamParam
        {
            private int orderId;
            private string beamId;
            private string value;
            private bool convert;

            public BeamParam(string beamId, string value)
            {
                this.beamId = beamId;
                this.value = value;
                convert = Int32.TryParse(beamId, out this.orderId);
                if (!convert)
                {
                    if (beamId.IndexOf("Sida") == -1)
                        this.orderId = int.MaxValue;
                    else
                        this.orderId = int.MinValue;
                }

            }

            public int OrderId
            {
                get { return orderId; }
            }

            public string BeamId
            {
                get { return beamId; }
            }

            public string Value
            {
                get { return value; }
            }
        }

    }
}
