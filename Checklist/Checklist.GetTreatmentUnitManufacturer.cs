using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMS.TPS.Common.Model.API;

namespace Checklist
{
    public partial class Checklist
    {
        private TreatmentUnitManufacturer GetTreatmentUnitManufacturer()
        {
            // Loopa genom samtliga fält och ta reda på vilken typ av accelerator (tillverkare) som är associerad med planen
            TreatmentUnitManufacturer treatmentUnitManufacturer = TreatmentUnitManufacturer.None;
            foreach (Beam beam in planSetup.Beams)
            {
                TreatmentUnitManufacturer treatmentUnitManufacturerBeam = GetTreatmentUnitManufacturer(beam);

                if (treatmentUnitManufacturerBeam != treatmentUnitManufacturer && treatmentUnitManufacturer != TreatmentUnitManufacturer.None)
                    treatmentUnitManufacturer = TreatmentUnitManufacturer.Multiple;
                else
                    treatmentUnitManufacturer = treatmentUnitManufacturerBeam;
            }

            return treatmentUnitManufacturer;
        }

        private TreatmentUnitManufacturer GetTreatmentUnitManufacturer(Beam beam)
        {
            TreatmentUnitManufacturer treatmentUnitManufacturerBeam;

            if (beam.TreatmentUnit.Id.IndexOf("TB") == 0)
                treatmentUnitManufacturerBeam = TreatmentUnitManufacturer.Varian;
            else if (beam.TreatmentUnit.Id.IndexOf("IX") == 0)
                treatmentUnitManufacturerBeam = TreatmentUnitManufacturer.Varian;
            else if (beam.TreatmentUnit.Id.IndexOf("L") == 0)
                treatmentUnitManufacturerBeam = TreatmentUnitManufacturer.Elekta;
            else
                treatmentUnitManufacturerBeam = TreatmentUnitManufacturer.Unknown;

            return treatmentUnitManufacturerBeam;
        }
    }
}
