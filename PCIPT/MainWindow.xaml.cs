using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CsvHelper;
using PCIPT.Dtos.CostWeight;
using PCIPT.Dtos.Vehicles;
using PCIPT.Dtos.VehicleTypes;

namespace PCIPT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var CostWeightDto = new List<CostWeightDto>
            {
                new CostWeightDto("HydraulicOilConsumption", 0.7f),
            };

            using (var writer = new StreamWriter("CostTypeDto.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(CostWeightDto);
            }




            var FuelVehicle = new List<FuelVehicleDto>
            {
                new FuelVehicleDto ("Ітачі3000", "Штабелер", 3f, 1.5f, 2f, 9f, 1.4f, 0.5f, 0.4f, "Дизель", 2.5f, 4f),
                new FuelVehicleDto ("Амогус", "Штабелер", 4f, 1f, 1.6f, 8f, 1.3f, 0.45f, 0.3f, "Бензин", 3f, 3.8f),
                new FuelVehicleDto ("Іначі2000", "Штабелер", 3.5f, 1.3f, 1.8f, 10f, 1.5f, 0.3f, 0.25f, "Бензин", 3f, 3.6f),
            };

            using (var writer = new StreamWriter("FuelVehicles.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(FuelVehicle);
            }

            using (var reader = new StreamReader("FuelVehicles.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<FuelVehicleDto>();

            }
        }
    }
}
