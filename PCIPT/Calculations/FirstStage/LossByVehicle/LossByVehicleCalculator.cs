using CsvHelper.Configuration.Attributes;
using PCIPT.Dtos.CostWeight;
using PCIPT.Dtos.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Calculations.FirstStage.LossByVehicle
{
    public sealed class LossByVehicleCalculator
    {
        private LossByVehicle ConvertVehicle(VehicleDto dto)
        {
            var lossByVehicle = new LossByVehicle
            {
                Name = dto.Name,
                HydraulicOilConsumption = dto.HydraulicOilConsumption,
                SpecialOilConsumption = dto.SpecialOilConsumption,
                TransmissionOilConsumption = dto.TransmissionOilConsumption,
            };

            FuelVehicleDto? fuelVehicleDto = dto as FuelVehicleDto;
            if (fuelVehicleDto != null)
            {
                lossByVehicle.MotorOilConsumption = fuelVehicleDto.MotorOilConsumption;
                lossByVehicle.FuelConsumption = fuelVehicleDto.FuelConsumption;
            }

            ElectricVehicleDto? electricVehicleDto = dto as ElectricVehicleDto;
            if (electricVehicleDto != null)
            {
                lossByVehicle.BaseElectricityConsumption = electricVehicleDto.BaseElectricityConsumption;
            }

            return lossByVehicle;
        }

        public List<LossByVehicle> CalculateLossesByVehicles(List<VehicleDto> vehicleDtos, List<CostWeightDto> costWeightDtos)
        {
            var lossByVehicles = new List<LossByVehicle>();
            float weightHydraulicOil = costWeightDtos.Find(d => d.Name == "HydraulicOilConsumption")?.Weight ?? 1f;
            float weightSpecialOil = costWeightDtos.Find(d => d.Name == "SpecialOilConsumption")?.Weight ?? 1f;
            float weightTransmissionOil = costWeightDtos.Find(d => d.Name == "TransmissionOilConsumption")?.Weight ?? 1f;
            float weightMotorOil = costWeightDtos.Find(d => d.Name == "MotorOilConsumption")?.Weight ?? 1f;
            float weightElecticity = costWeightDtos.Find(d => d.Name == "BaseElectricityConsumption")?.Weight ?? 1f;

            

            // at the end
            foreach (var vehicleDto in vehicleDtos)
            {
                var line = ConvertVehicle(vehicleDto);
                line.HydraulicOilConsumption *= weightHydraulicOil;
                line.SpecialOilConsumption *= weightSpecialOil;
                line.TransmissionOilConsumption *= weightTransmissionOil;
                line.MotorOilConsumption *= weightMotorOil;
                line.BaseElectricityConsumption *= weightElecticity;
                if (vehicleDto is FuelVehicleDto)
                {
                    var dto = (FuelVehicleDto)vehicleDto;
                    line.FuelConsumption *= costWeightDtos.Find(d => d.Name == dto.FuelType)?.Weight ?? 1f;
                }
            }
        }
    }
}
