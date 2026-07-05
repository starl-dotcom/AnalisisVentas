using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using AnalisisVentas.Data.Interfaces;

namespace AnalisisVentas.Data.Services
{
    public class CsvDataExtractor : IDataExtractor
    {
        public IEnumerable<T> Extract<T>(string filePath) where T : class
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: File not found {filePath}");
                return Enumerable.Empty<T>();
            }

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null
            };

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);
            return csv.GetRecords<T>().ToList();
        }
    }
}
