using AnalisisVentas.Data.Models;

namespace AnalisisVentas.Data.Interfaces
{
    public interface IDataLoader
    {
        Task<int> RegisterFuenteDatosAsync();
        Task LoadCustomersAsync(IEnumerable<Customer> customers);
        Task<Dictionary<string, int>> LoadCategoriasAsync(IEnumerable<string> categoryNames);
        Task LoadProductsAsync(IEnumerable<Product> products, Dictionary<string, int> categoryMap);
        Task LoadOrdersAndDetailsAsync(IEnumerable<Order> orders, IEnumerable<OrderDetail> details, int idFuente);
    }
}
