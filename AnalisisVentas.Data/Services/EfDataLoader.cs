using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using AnalisisVentas.Data.Context;
using AnalisisVentas.Data.Interfaces;
using AnalisisVentas.Data.Models;

namespace AnalisisVentas.Data.Services
{
    public class EfDataLoader : IDataLoader
    {
        private readonly DbAnalisisContext _context;

        public EfDataLoader(DbAnalisisContext context)
        {
            _context = context;
        }

        public async Task<int> RegisterFuenteDatosAsync()
        {
            var idTipoFuenteParam = new SqlParameter("@IdTipoFuente", SqlDbType.Int) { Direction = ParameterDirection.Output };
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC SP_InsertarTipoFuente @Nombre, @Descripcion, @IdTipoFuente OUTPUT",
                new SqlParameter("@Nombre", "CSV"),
                new SqlParameter("@Descripcion", "Carga inicial desde archivos CSV"),
                idTipoFuenteParam);

            int idTipoFuente = (int)idTipoFuenteParam.Value;

            var idFuenteParam = new SqlParameter("@IdFuente", SqlDbType.Int) { Direction = ParameterDirection.Output };
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC SP_InsertarFuenteDatos @IdTipoFuente, @FechaCarga, @IdFuente OUTPUT",
                new SqlParameter("@IdTipoFuente", idTipoFuente),
                new SqlParameter("@FechaCarga", DateTime.Now),
                idFuenteParam);

            return (int)idFuenteParam.Value;
        }

        public async Task LoadCustomersAsync(IEnumerable<Customer> customers)
        {
            foreach (var c in customers)
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC SP_InsertarCliente @p0, @p1, @p2, @p3, @p4, @p5",
                    c.CustomerID,
                    c.FirstName + " " + c.LastName,  
                    c.Email   ?? (object)DBNull.Value,
                    c.Phone   ?? (object)DBNull.Value,
                    c.City    ?? (object)DBNull.Value, 
                    c.Country ?? (object)DBNull.Value); 
            }
        }

        public async Task<Dictionary<string, int>> LoadCategoriasAsync(IEnumerable<string> categoryNames)
        {
            var map = new Dictionary<string, int>();
            foreach (var name in categoryNames)
            {
                var idParam = new SqlParameter("@IdCategoria", SqlDbType.Int) { Direction = ParameterDirection.Output };
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC SP_InsertarCategoria @Nombre, @IdCategoria OUTPUT",
                    new SqlParameter("@Nombre", name),
                    idParam);
                map[name] = (int)idParam.Value;
            }
            return map;
        }

        public async Task LoadProductsAsync(IEnumerable<Product> products, Dictionary<string, int> categoryMap)
        {
            foreach (var p in products)
            {
                int idCategoria = categoryMap.TryGetValue(p.Category ?? string.Empty, out int id) ? id : 0;
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC SP_InsertarProducto @p0, @p1, @p2, @p3, @p4",
                    p.ProductID, p.ProductName, idCategoria, p.Price, p.Stock); 
            }
        }

        public async Task LoadOrdersAndDetailsAsync(IEnumerable<Order> orders, IEnumerable<OrderDetail> details, int idFuente)
        {
            var detailsByOrder = details
                .GroupBy(d => d.OrderID)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var order in orders)
            {
                var idVentaParam = new SqlParameter("@IdVenta", SqlDbType.Int) { Direction = ParameterDirection.Output };
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC SP_InsertarVenta @IdVenta_Origen, @IdCliente, @IdFuente, @Fecha, @Estado, @IdVenta OUTPUT",
                    new SqlParameter("@IdVenta_Origen", order.OrderID),
                    new SqlParameter("@IdCliente", order.CustomerID),
                    new SqlParameter("@IdFuente", idFuente),
                    new SqlParameter("@Fecha", order.OrderDate),
                    new SqlParameter("@Estado", order.Status ?? (object)DBNull.Value), 
                    idVentaParam);

                int newIdVenta = (int)idVentaParam.Value;

                if (!detailsByOrder.TryGetValue(order.OrderID, out var orderDetails)) continue;
                foreach (var detail in orderDetails)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "EXEC SP_InsertarDetalleVenta @p0, @p1, @p2, @p3, @p4",
                        newIdVenta, detail.ProductID, detail.Quantity, detail.TotalPrice / detail.Quantity, detail.TotalPrice);
                }
            }
        }
    }
}
