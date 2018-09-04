using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMS.TPS.Common.Model.API;

namespace Checklist
{
    public partial class Checklist
    {
        public void D()
        {
            checklistItems.Add(new ChecklistItem("D. Dosberäkning"));

            string d1_value = planSetup.PhotonCalculationModel;            
            if (d1_value.Length == 0)
                d1_value = "-";
            AutoCheckStatus d1_status = CheckResult(string.Compare(d1_value, "AAA_13.6.23") == 0);
            if (checklistType == ChecklistType.MasterPlan || checklistType == ChecklistType.MasterPlanIMRT)
            {
                d1_status = AutoCheckStatus.UNKNOWN;
                checklistItems.Add(new ChecklistItem("D1. Beräkningsalgoritm är korrekt vald", "Kontrollera att korrekt beräkningsalgoritm (PB) har använts vid dosplaneringen.", d1_value, d1_status));
            }
            else
                checklistItems.Add(new ChecklistItem("D1. Beräkningsalgoritm är korrekt vald", "Kontrollera att korrekt beräkningsalgoritm (AAA_13.6.26) har använts vid dosplaneringen.", d1_value, d1_status));

            string d2_value;
            if (!planSetup.PhotonCalculationOptions.TryGetValue("CalculationGridSizeInCM", out d2_value))
                d2_value = "-";
            else
                d2_value += " cm";

            AutoCheckStatus d2_status=CheckResult(string.Compare(d2_value, "0.25 cm") == 0);
            GetFieldSizeGridSize(planSetup, 30, checklistType, out d2_value,out d2_status); //30 är beräsningen satt för kollimatorstorleken. 
            
            if (checklistType == ChecklistType.MasterPlan || checklistType == ChecklistType.MasterPlanIMRT)
            {
                d2_status = AutoCheckStatus.UNKNOWN;
                checklistItems.Add(new ChecklistItem("D2. Beräkningsupplösningen är korrekt", "Kontrollera att korrekt beräkningsupplösning (0.30 cm) har använts.", d2_value, d2_status));
            }
            else
                
                checklistItems.Add(new ChecklistItem("D2. Beräkningsupplösningen är korrekt", "Kontrollera att korrekt beräkningsupplösning \r\n• 0.10 cm (fält < 3 cm i > 25% av segmenten) eller \r\n• 0.25 cm har använts.", d2_value, d2_status));

            string d3_value;
            if (!planSetup.PhotonCalculationOptions.TryGetValue("HeterogeneityCorrection", out d3_value))
                d3_value = "-";
            AutoCheckStatus d3_status = CheckResult(string.Compare(d3_value, "ON") == 0);
            if (checklistType == ChecklistType.MasterPlan || checklistType == ChecklistType.MasterPlanIMRT)
            {
                d3_status = AutoCheckStatus.UNKNOWN;
            }
            checklistItems.Add(new ChecklistItem("D3. Heterogenitetskorrektion har applicerats korrekt", "Kontrollera att heterogenitetskorrektion har använts om ej särskilda skäl föreligger", d3_value, d3_status));

            // VMATFluenceResolution removed from checklist
            /*
            if (checklistType == ChecklistType.EclipseVMAT)
            {
                string d4_value = string.Empty;
                if (!planSetup.PhotonCalculationOptions.TryGetValue("VMATFluenceResolution", out d4_value))
                    d4_value = "-";
                AutoCheckStatus d4_status = CheckResult(string.Compare(d4_value, "High") == 0);
                checklistItems.Add(new ChecklistItem("D4. Fluensupplösningen är korrekt", "Kontrollera att korrekt fluensupplösning har använts", d4_value, d4_status));
            }
            */
                                                
            string d5_value = string.Empty;
            AutoCheckStatus d5_status = AutoCheckStatus.UNKNOWN;
            string couchModel = string.Empty;
            double couchSurfaceHU = double.NaN;
            double couchInteriorHU = double.NaN;
            if (structureSet != null)
            {
                foreach (Structure structure in structureSet.Structures)
                {
                    double assignedHU;
                    structure.GetAssignedHU(out assignedHU);
                    if (string.Compare(structure.DicomType, "SUPPORT") == 0)
                    {
                        if (structure.Id.IndexOf("Surface") != -1)
                        {
                            structure.GetAssignedHU(out couchSurfaceHU);
                            couchModel = structure.Name;
                        }
                        if (structure.Id.IndexOf("Interior") != -1)
                            structure.GetAssignedHU(out couchInteriorHU);
                    }
                }
            }
            // If a patient have had a support structure connected in a previous course, the name of that structure will be maintained even though it is obsolete.
            // Specifically: The substring "Top" was part of the name in 13.0, but disappeared in 13.6. Returning patient treated during 13.0 will have that substring retained.
            // Solution. Replace(" Top".String.Empty)
            if (treatmentUnitManufacturer == TreatmentUnitManufacturer.Varian && string.Compare(couchModel.Replace(" Top", String.Empty), "Exact IGRT Couch, medium") == 0 && couchInteriorHU == -950 && couchSurfaceHU == -300)
                d5_status = AutoCheckStatus.PASS;
            else if (treatmentUnitManufacturer == TreatmentUnitManufacturer.Elekta && string.Compare(couchModel.Replace(" Top", String.Empty), "BrainLAB/iBeam Couch") == 0 && couchInteriorHU == -950 && couchSurfaceHU == -300)
                d5_status = AutoCheckStatus.PASS;
            else if (couchModel.Length == 0)
            {
                d5_status = AutoCheckStatus.WARNING;
                couchModel = "Saknas";
            }
            else
                d5_status = AutoCheckStatus.FAIL;
            // add on test if plan is non coplanar VMAT
            if (checklistType == ChecklistType.EclipseVMAT && GetVMATCoplanar(planSetup) == false)
                d5_status = AutoCheckStatus.MANUAL;
            if (string.Compare(couchModel, "Saknas") == 0)
                d5_value = "Saknas (" + treatmentUnitManufacturer + ")";
              
            else
                d5_value = "Model: " + couchModel + ", Interior: " + couchInteriorHU.ToString() + " HU, Surface: " + couchSurfaceHU.ToString() + " HU (" + treatmentUnitManufacturer + ")";
            checklistItems.Add(new ChecklistItem("D5. Britsprofil och HU har valts korrekt", "Kontrollera att korrekt britsprofil och korrekta HU valts under Structure Properties för britsstrukturerna och fliken General samt CT Value and Material.\r\n• Varian: Exact IGRT Couch, medium (CouchSurface: -300, CouchInterior: -950)\r\n• Elekta: BrainLAB/iBeam Couch (CouchSurface: -300, CouchInterior: -950)\r\n• Notera att brits inte ska inkluderas för icke coplanara VMAT behandlingar av intra- och extra-kraniella target kraniellt om C1.", d5_value, d5_status));

            // if synthetic CT, compute the distance between body and couch surface
            if (syntheticCT)
            { 
                string d6_value = string.Empty;
                AutoCheckStatus d6_status = AutoCheckStatus.UNKNOWN;
                double distance = 15.0;
                double threshold = 5.0;

                Structure body = structureSet.Structures.Where(s => String.CompareOrdinal(s.Id, "BODY") == 0).ToList().FirstOrDefault();
                Structure couchSurface = structureSet.Structures.Where(s => String.CompareOrdinal(s.DicomType, "SUPPORT") == 0).Where(s => s.Id.IndexOf("Surface") != -1).ToList().FirstOrDefault();
            
                if (body == null)
                {
                    d6_status = AutoCheckStatus.FAIL;
                    d6_value = "Body struktur saknas. ";
                }
                if (couchSurface == null)
                {
                    d6_status = AutoCheckStatus.FAIL;
                    d6_value += "Britsstruktur saknas. ";
                }
                if (d6_status != AutoCheckStatus.FAIL)
                {
                    // compute separation between structures and distance to threshold
                    double separation = 0;
                    if (image.ImagingOrientation.ToString().IndexOf("Prone") > 0)
                    {
                        separation = body.MeshGeometry.Bounds.Location.Y - (couchSurface.MeshGeometry.Bounds.Location.Y + couchSurface.MeshGeometry.Bounds.Size.Y);
                    }
                    else
                    {
                        separation = -1 * (body.MeshGeometry.Bounds.Location.Y + body.MeshGeometry.Bounds.Size.Y - couchSurface.MeshGeometry.Bounds.Location.Y);
                    } 
                    if (Math.Round(distance - separation, 0) > threshold)
                        d6_status = AutoCheckStatus.WARNING;
                    else
                        d6_status = AutoCheckStatus.PASS;
                    d6_value = "Avstånd mellan Body och brits: " + separation.ToString("N0") + " mm";
                }
            checklistItems.Add(new ChecklistItem("D6. Kontrollera avstånd mellan Body och britsstruktur", "Kontrollera att avståndet mellan Body och britsstuktur är [10, 20] mm för syntetisk CT", d6_value, d6_status));
            }

            string d7_value = string.Empty;
            string d7_value_detailed = string.Empty;
            AutoCheckStatus d7_status = AutoCheckStatus.UNKNOWN;
            bool calculationErrorOrWarning = false;
            foreach (Beam beam in planSetup.Beams)
            {
                if (!beam.IsSetupField)
                {
                    d7_value_detailed += beam.Id + ":\r\n";
                    foreach (BeamCalculationLog beamCalculationLog in beam.CalculationLogs)
                    {
                        foreach (string messageLine in beamCalculationLog.MessageLines)
                        {
                            if (messageLine.IndexOf("Warning") == 0 || messageLine.IndexOf("Error") == 0)
                            {
                                d7_value_detailed += "• " + messageLine + "\r\n";
                                calculationErrorOrWarning = true;
                            }
                        }
                    }
                    d7_value_detailed += "\r\n";
                }
            }
            if (calculationErrorOrWarning == false)
            {
                d7_status = AutoCheckStatus.PASS;
                d7_value = "Inga error eller varningar";
            }
            else
            {
                d7_status = AutoCheckStatus.WARNING;
                d7_value = "Error eller varningar";
            }
            d7_value_detailed = "Fält:\r\n" + reorderBeamParam(d7_value_detailed, "\r\n\r\n");
            checklistItems.Add(new ChecklistItem("D7. Eventuella felmeddelanden under Errors And Warnings är acceptabla", "Kontrollera att eventuella meddelanden under Errors And Warnings är acceptabla och bekräfta detta med signatur i protokollet.", d7_value, d7_value_detailed, d7_status));
        }
    }
}
