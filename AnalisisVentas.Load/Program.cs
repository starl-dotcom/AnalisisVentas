using System.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AnalisisVentas.Data.Interfaces;
using AnalisisVentas.Data.Models;
using AnalisisVentas.Load.Host;

namespace AnalisisVentas.Load
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Sistema de Análisis de Ventas - Proceso ETL ===");
            Console.WriteLine($"Inicio: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            var host = AppHost.CreateHost(args);
            using var scope = host.Services.CreateScope();

            var extractor  = scope.ServiceProvider.GetRequiredService<IDataExtractor>();
            var transformer = scope.ServiceProvider.GetRequiredService<IDataTransformer>();
            var loader     = scope.ServiceProvider.GetRequiredService<IDataLoader>();

            string basePath = ConfigurationManager.AppSettings["CsvBasePath"] ?? string.Empty;
            basePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, basePath));
            Console.WriteLine($"Leyendo CSVs desde: {basePath}\n");

            try
            {
                // EXTRACCIÓN 
                Console.WriteLine("--- [1/4] Extracción ---");
                var rawCustomers     = extractor.Extract<Customer>(Path.Combine(basePath, "customers.csv")).ToList();
                var rawProducts      = extractor.Extract<Product>(Path.Combine(basePath, "products.csv")).ToList();
                var rawOrders        = extractor.Extract<Order>(Path.Combine(basePath, "orders.csv")).ToList();
                var rawOrderDetails  = extractor.Extract<OrderDetail>(Path.Combine(basePath, "order_details.csv")).ToList();

                Console.WriteLine($"  Customers: {rawCustomers.Count}");
                Console.WriteLine($"  Products:  {rawProducts.Count}");
                Console.WriteLine($"  Orders:    {rawOrders.Count}");
                Console.WriteLine($"  Details:   {rawOrderDetails.Count}");

                // TRANSFORMACIÓN
                Console.WriteLine("\n--- [2/4] Transformación ---");
                var validCustomers = transformer.TransformCustomers(rawCustomers).ToList();
                var uniqueCategories = transformer.ExtractUniqueCategories(rawProducts).ToList();
                var validOrders    = transformer.TransformOrders(rawOrders, validCustomers).ToList();


                var validOrderDetails = transformer.TransformOrderDetails(rawOrderDetails, validOrders, rawProducts).ToList();

                Console.WriteLine($"\n  Customers válidos:    {validCustomers.Count} | Rechazados: {rawCustomers.Count - validCustomers.Count}");
                Console.WriteLine($"  Categorías únicas:    {uniqueCategories.Count}");
                Console.WriteLine($"  Orders válidas:       {validOrders.Count} | Rechazadas: {rawOrders.Count - validOrders.Count}");
                Console.WriteLine($"  Details válidos:      {validOrderDetails.Count} | Rechazados: {rawOrderDetails.Count - validOrderDetails.Count}");

                // CARGA 
                Console.WriteLine("\n--- [3/4] Carga en BD ---");

                int idFuente = await loader.RegisterFuenteDatosAsync();
                Console.WriteLine($"  FuenteDatos registrada. ID: {idFuente}");

                // Clientes
                await loader.LoadCustomersAsync(validCustomers);
                Console.WriteLine($"  Customers cargados:  {validCustomers.Count}");

                // Categorías
                var categoryMap = await loader.LoadCategoriasAsync(uniqueCategories);
                Console.WriteLine($"  Categorías cargadas: {categoryMap.Count}");

                // Productos 
                var validProducts = transformer.TransformProducts(rawProducts, categoryMap).ToList();
                await loader.LoadProductsAsync(validProducts, categoryMap);
                Console.WriteLine($"  Productos cargados:  {validProducts.Count}");

                // Ventas + Detalles 
                await loader.LoadOrdersAndDetailsAsync(validOrders, validOrderDetails, idFuente);
                Console.WriteLine($"  Ventas cargadas:     {validOrders.Count}");
                Console.WriteLine($"  Detalles cargados:   {validOrderDetails.Count}");

                // FIN 
                Console.WriteLine($"\n--- [4/4] Completado ---");
                Console.WriteLine($"Fin: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine("Proceso ETL finalizado con ÉXITO.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[ERROR] {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine("\nPresiona ENTER para salir...");
            Console.ReadLine();
        }
    }
}
