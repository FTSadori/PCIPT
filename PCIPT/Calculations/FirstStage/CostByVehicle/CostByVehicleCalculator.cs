using CsvHelper.Configuration.Attributes;
using PCIPT.Calculations.FirstStage.CostByVehicle.Dtos;
using PCIPT.Dtos.CostWeight;
using PCIPT.Dtos.Vehicles;
using PCIPT.Calculations.FirstStage.CostByVehicle;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Calculations.FirstStage.LossByVehicle
{
    public sealed class CostByVehicleCalculator
    {
        private enum CostType
        {
            HydraulicOilConsumption,
            RepairCosts,
            TransmissionOilConsumption,
            MotorOilConsumption,
            BaseElectricityConsumption,
            FuelConsumption,
            MaxCostTypes,
        };

        public static CostTableRow ConvertVehicleToCostData(VehicleDto dto)
        {
            float[] costs = new float[(int)CostType.MaxCostTypes];

            costs[(int)CostType.HydraulicOilConsumption] = dto.HydraulicOilConsumption;
            costs[(int)CostType.RepairCosts] = dto.RepairCosts;
            costs[(int)CostType.TransmissionOilConsumption] = dto.TransmissionOilConsumption;
        
            FuelVehicleDto? fuelVehicleDto = dto as FuelVehicleDto;
            string fuelType = "";
            if (fuelVehicleDto != null)
            {
                costs[(int)CostType.MotorOilConsumption] = fuelVehicleDto.MotorOilConsumption;
                costs[(int)CostType.FuelConsumption] = fuelVehicleDto.FuelConsumption;
                fuelType = fuelVehicleDto.FuelType;
            }
            else
            {
                costs[(int)CostType.MotorOilConsumption] = 0f;
                costs[(int)CostType.FuelConsumption] = 0f;
            }

            ElectricVehicleDto? electricVehicleDto = dto as ElectricVehicleDto;
            if (electricVehicleDto != null)
            {
                costs[(int)CostType.BaseElectricityConsumption] = electricVehicleDto.BaseElectricityConsumption;
            }
            else
            {
                costs[(int)CostType.BaseElectricityConsumption] = 0f;
            }

            return new CostTableRow(dto.Name, costs, fuelType);
        }

        private static CostTableRowEntity ConvertTableRowToRecord(CostTableRow tableRow)
        {
            return new CostTableRowEntity(tableRow.Name,
                tableRow.Costs[(int)CostType.HydraulicOilConsumption],
                tableRow.Costs[(int)CostType.TransmissionOilConsumption],
                tableRow.Costs[(int)CostType.RepairCosts],
                tableRow.Costs[(int)CostType.BaseElectricityConsumption],
                tableRow.Costs[(int)CostType.MotorOilConsumption],
                tableRow.Costs[(int)CostType.FuelConsumption]);
        }

        public static List<CostTableRowEntity> CalculateCostsByVehicles(List<VehicleDto> vehicleDtos, List<CostWeightDto> costWeightDtos)
        {
            var costTable = new List<CostTableRow>();

            // Кожне ТЗ перетворюємо у необхідний тип та шукаємо максимальні значення по стовпцям
            foreach (var vehicleDto in vehicleDtos)
            {
                var line = ConvertVehicleToCostData(vehicleDto);

                costTable.Add(line);
            }

            // Знаходимо у таблиці ваги для всіх витрат, крім палива
            float[] costsWeight = new float[(int)CostType.MaxCostTypes];
            for (int i = 0; i < (int)CostType.MaxCostTypes; i++)
            {
                if (i != (int)CostType.FuelConsumption)
                    costsWeight[i] = costWeightDtos.Find(d => d.Name == Enum.GetName(typeof(CostType), i))?.Weight ?? 1f;
            }

            // Нормалізуємо дані та застосовуємо ваги
            foreach (var costRow in costTable)
            {
                for (int i = 0; i < costRow.Costs.Length; i++)
                {
                    if (i == (int)CostType.FuelConsumption)
                    {
                        if (costRow.FuelType != "")
                            costRow.Costs[i] *= costWeightDtos.Find(d => d.Name == costRow.FuelType)?.Weight ?? 1f;
                    }
                    else
                        costRow.Costs[i] *= costsWeight[i];
                }
            }

            // Перетворюємо дані у форму для виводу
            var costByVehicles = new List<CostTableRowEntity>();
            foreach (var costRow in costTable)
            {
                costByVehicles.Add(ConvertTableRowToRecord(costRow));
            }

            return costByVehicles;
        }
    }
}
