using AnalisisVentas.Data.Models;

namespace AnalisisVentas.Data.Interfaces
{
    public interface IDataTransformer
    {
        IEnumerable<Customer> TransformCustomers(IEnumerable<Customer> rawData);
        IEnumerable<string> ExtractUniqueCategories(IEnumerable<Product> rawData);
        IEnumerable<Product> TransformProducts(IEnumerable<Product> rawData);
        IEnumerable<Order> TransformOrders(IEnumerable<Order> rawData, IEnumerable<Customer> validCustomers);
        IEnumerable<OrderDetail> TransformOrderDetails(IEnumerable<OrderDetail> rawData, IEnumerable<Order> validOrders, IEnumerable<Product> validProducts);
    }
}
