using CsvHelper;
using PCIPT.Dtos.CostWeight;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Core.DataHandler
{
    public sealed class CsvHandler
    {
        public static List<T> GetAllFromFile<T>(string fileName) {
            using var reader = new StreamReader(fileName);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            return csv.GetRecords<T>().ToList();
        }

        public static void PutAllToFile<T>(string fileName, IEnumerable<T> data)
        {
            using var writer = new StreamWriter(fileName);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(data);
        }
    }
}
