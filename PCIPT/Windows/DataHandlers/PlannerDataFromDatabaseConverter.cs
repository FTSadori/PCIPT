using PCIPT.Dtos.Cargoes;
using PCIPT.Dtos.CargoTurnoverPoints;
using PCIPT.Dtos.CostWeight;
using PCIPT.Dtos.Graph;
using PCIPT.Dtos.Node;
using PCIPT.Dtos.Routes;
using PCIPT.Dtos.Vehicles;
using PCIPT.Dtos.VehicleTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Windows.DataHandlers
{
    public sealed class PlannerDataFromDatabaseConverter
    {
        public static List<CostWeightDto> ToCostWeight(DataSet dataSet)
        {
            List<CostWeightDto> list = new();

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                list.Add(new CostWeightDto((string)row["name"], (float)row["cweight"]));
            }

            return list;
        }

        public static List<NodesCoordsDto> ToNodesCoords(DataSet dataSet)
        {
            List<NodesCoordsDto> list = new();

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                list.Add(new NodesCoordsDto((int)row["id"], (float)row["coordx"], (float)row["coordy"]));
            }

            return list;
        }

        public static List<ElectricVehicleDto> ToElectricVehicles(DataSet dataSet)
        {
            List<ElectricVehicleDto> list = new();

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                list.Add(new ElectricVehicleDto((string)row["name"], (string)row["vtype"], (float)row["loadcapacity"], 
                    (float)row["speedwithload"], (float)row["speedwithoutload"], (float)row["loadtime"], (float)row["hoil"], 
                    (float)row["toil"], (float)row["soil"], (int)row["maxquantity"], (float)row["baseelectricity"]));
            }

            return list;
        }

        public static List<FuelVehicleDto> ToFuelVehicles(DataSet dataSet)
        {
            List<FuelVehicleDto> list = new();

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                list.Add(new FuelVehicleDto((string)row["name"], (string)row["vtype"], (float)row["loadcapacity"],
                    (float)row["speedwithload"], (float)row["speedwithoutload"], (float)row["loadtime"], (float)row["hoil"],
                    (float)row["toil"], (float)row["soil"], (int)row["maxquantity"], (string)row["fueltype"], (float)row["fuelconsumption"], (float)row["moil"]));
            }

            return list;
        }

        public static List<RouteDto> ToRoutes(DataSet dataSet)
        {
            List<RouteDto> list = new();

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                list.Add(new RouteDto((int)row["id"], (int)row["sourceid"], (int)row["destinationid"], (float)row["distance"]));
            }

            return list;
        }

        public static List<CargoDto> ToCargoes(DataSet dataSet)
        {
            List<CargoDto> list = new();

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                list.Add(new CargoDto((int)row["code"], (string)row["name"], (string)row["ctype"], (float)row["utilizationrate"]));
            }

            return list;
        }

        public static List<CargoTurnoverPointDto> ToCargoTurnoverPoints(DataSet dataSet)
        {
            List<CargoTurnoverPointDto> list = new();

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                list.Add(new CargoTurnoverPointDto((int)row["id"], (int)row["sourceid"], (int)row["destinationid"], (float)row["outgoingcargo"], (int)row["cargocode"]));
            }

            return list;
        }

        public static List<VehicleTypeDto> ToVehicleTypes(DataSet dataSet)
        {
            List<VehicleTypeDto> list = new();

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                list.Add(new VehicleTypeDto((string)row["vehicletype"], (string)row["cargotype"]));
            }

            return list;
        }

        public static List<NodeDto> ToNodes(DataSet dataSet)
        {
            List<NodeDto> list = new();

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                list.Add(new NodeDto((int)row["id"], (string)row["name"]));
            }

            return list;
        }
    }
}
