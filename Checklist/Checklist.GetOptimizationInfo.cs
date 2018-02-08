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
        private Tuple<string, string, AutoCheckStatus> GetOptimizationInfo(StructureSet structureSet, PlanSetup planSetup)
        {
            string value = string.Empty;
            string details = string.Empty;
            AutoCheckStatus status = AutoCheckStatus.MANUAL; 
            List<Structure> strList = structureSet.Structures.Where(s => s.Id.ToLower().StartsWith("z_ptv")).ToList();

            List<string> lowerPTVObjectiveStructures = new List<string>(); //J Add list for name of all structures that have an lower objective

            foreach (OptimizationObjective optimizationObjective in planSetup.OptimizationSetup.Objectives)
                if (optimizationObjective.GetType() == typeof(OptimizationPointObjective))
                {
                    OptimizationPointObjective optimizationPointObjective = (OptimizationPointObjective)optimizationObjective;
                    if (((optimizationPointObjective.Operator.ToString().ToLower() == "lower") || (optimizationPointObjective.Operator.ToString().ToLower() == "upper")) && optimizationPointObjective.StructureId.StartsWith("Z_PTV"))
                    {
                        // Generates a list for with name of all structures that have a lower objective (ie finds the PTVs). 
                        lowerPTVObjectiveStructures.Add(optimizationPointObjective.StructureId);
                    }
                    details += (details.Length == 0 ? "Optimization objectives:\r\n  " : "\r\n  ") + optimizationPointObjective.StructureId + ": " + optimizationPointObjective.Operator.ToString() + ", dose: " + optimizationPointObjective.Dose.Dose.ToString("0.000") + ", volume: " + optimizationPointObjective.Volume.ToString("0.0") + ", priority: " + optimizationPointObjective.Priority.ToString();
                }
            if (!strList.Any() && !lowerPTVObjectiveStructures.Any())
            {
                value += "Inget optimeringsPTV hittat, verifera";
                status = AutoCheckStatus.MANUAL;
            }
            else if (strList.Any() && !lowerPTVObjectiveStructures.Any())
            {
                value += "OptimeringsPTV har ritats men ej används i optimering, vänligen verifera";
                status = AutoCheckStatus.WARNING;
            }
            else if (strList.Any() && lowerPTVObjectiveStructures.Any())
            {
                value += "OptimeringsPTV har ritats och använts optimering, vänligen verifiera";
                status = AutoCheckStatus.MANUAL;
            }

            // JSR 
            foreach (OptimizationParameter optimizationParameter in planSetup.OptimizationSetup.Parameters)
            {
                if (optimizationParameter.GetType() == typeof(OptimizationPointCloudParameter))
                {
                    OptimizationPointCloudParameter optimizationPointCloudParameter = (OptimizationPointCloudParameter)optimizationParameter;
                    details += (details.Length == 0 ? string.Empty : "\r\n") + "Point cloud parameter: " + optimizationPointCloudParameter.Structure.Id + "=" + optimizationPointCloudParameter.Structure.DicomType.ToString();
                }
                else if (optimizationParameter.GetType() == typeof(OptimizationNormalTissueParameter))
                {
                    OptimizationNormalTissueParameter optimizationNormalTissueParameter = (OptimizationNormalTissueParameter)optimizationParameter;
                    details += (details.Length == 0 ? string.Empty : "\r\n") + "Normal tissue parameter: priority=" + optimizationNormalTissueParameter.Priority.ToString();
                }
                else if (optimizationParameter.GetType() == typeof(OptimizationExcludeStructureParameter))
                {
                    OptimizationExcludeStructureParameter optimizationExcludeStructureParameter = (OptimizationExcludeStructureParameter)optimizationParameter;
                    details += (details.Length == 0 ? string.Empty : "\r\n") + "Exclude structure parameter: " + optimizationExcludeStructureParameter.Structure.Id;
                }
            }
            return new Tuple<string, string, AutoCheckStatus>(value, details, status); 
        }
    }
}
