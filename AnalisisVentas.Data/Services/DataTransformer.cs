using AnalisisVentas.Data.Interfaces;
using AnalisisVentas.Data.Models;

namespace AnalisisVentas.Data.Services
{
    public class DataTransformer : IDataTransformer
    {
        public IEnumerable<Customer> TransformCustomers(IEnumerable<Customer> rawData)
        {
            return rawData
                .Where(c => c.CustomerID > 0
                    && !string.IsNullOrWhiteSpace(c.FirstName)
                    && !string.IsNullOrWhiteSpace(c.LastName))
                .GroupBy(c => c.CustomerID)
                .Select(g => g.First());
        }

        public IEnumerable<string> ExtractUniqueCategories(IEnumerable<Product> rawData)
        {
            return rawData
                .Where(p => !string.IsNullOrWhiteSpace(p.Category))
                .Select(p => p.Category!)
                .Distinct();
        }

        public IEnumerable<Product> TransformProducts(IEnumerable<Product> rawData)
        {
            return rawData
                .Where(p => p.ProductID > 0
                    && !string.IsNullOrWhiteSpace(p.ProductName)
                    && p.Price >= 0
                    && p.Stock >= 0
                    && !string.IsNullOrWhiteSpace(p.Category))
                .GroupBy(p => p.ProductID)
                .Select(g => g.First());
        }

        public IEnumerable<Order> TransformOrders(IEnumerable<Order> rawData, IEnumerable<Customer> validCustomers)
        {
            var customerIds = new HashSet<int>(validCustomers.Select(c => c.CustomerID));
            return rawData
                .Where(o => o.OrderID > 0 && customerIds.Contains(o.CustomerID))
                .GroupBy(o => o.OrderID)
                .Select(g => g.First());
        }

        public IEnumerable<OrderDetail> TransformOrderDetails(IEnumerable<OrderDetail> rawData,
            IEnumerable<Order> validOrders, IEnumerable<Product> validProducts)
        {
            var orderIds = new HashSet<int>(validOrders.Select(o => o.OrderID));
            var productDict = validProducts.ToDictionary(p => p.ProductID, p => p.Price);

            return rawData
                .Where(od => od.OrderID > 0
                    && od.ProductID > 0
                    && od.Quantity > 0
                    && orderIds.Contains(od.OrderID)
                    && productDict.ContainsKey(od.ProductID))
                .GroupBy(od => new { od.OrderID, od.ProductID })
                .Select(g =>
                {
                    var od = g.First();
                    if (od.TotalPrice <= 0)
                        od.TotalPrice = od.Quantity * productDict[od.ProductID];
                    return od;
                });
        }
    }
}
