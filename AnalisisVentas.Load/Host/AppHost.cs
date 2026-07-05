using AnalisisVentas.Data.Interfaces;
using AnalisisVentas.Data.Services;
using AnalisisVentas.Data.Context;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace AnalisisVentas.Load.Host
{
    public static class AppHost
    {
        public static IHost CreateHost(string[] args)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DbAnalisisContext"].ConnectionString;

            return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<DbAnalisisContext>(options =>
                    {
                        options.UseSqlServer(connectionString);
                    });

                    services.AddTransient<IDataExtractor, CsvDataExtractor>();
                    services.AddTransient<IDataTransformer, DataTransformer>();
                    services.AddTransient<IDataLoader, EfDataLoader>();
                })
                .Build();
        }
    }
}
