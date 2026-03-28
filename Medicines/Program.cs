using Medicines.Context;
using Medicines.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Medicines
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<Worker>();
            builder.Services.ConfigureTelegramBotServiceOptions(builder.Configuration);
            builder.Services.ConvigureTelegramBotService();
            builder.Services.ConfigureRepositories();
            builder.Services.ConvigureMedicinesService();
            builder.Services.ConfigureSqlContext(builder.Configuration);

            var host = builder.Build();

            using (var scope = host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<RepositoryContext>();
                db.Database.Migrate();
            }

            host.Run();
        }
    }
}