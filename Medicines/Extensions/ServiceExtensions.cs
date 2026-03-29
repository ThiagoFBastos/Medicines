using Medicines.Context;
using Medicines.Interfaces;
using Medicines.Repository;
using Medicines.Services;
using Medicines.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medicines.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureTelegramBotServiceOptions(this IServiceCollection services, IConfiguration config)
            => services.Configure<TelegramBotOptions>(config.GetSection("Telegram"));

        public static void ConvigureTelegramBotService(this IServiceCollection services)
            => services.AddScoped<ITelegramBotService, TelegramBotService>();

        public static void ConvigureMedicinesService(this IServiceCollection services)
            => services.AddScoped<IMedicinesService, MedicinesService>();

        public static void ConfigureUserService(this IServiceCollection services)
            => services.AddScoped<IUserService, UserService>();

        public static void ConfigureRepositories(this IServiceCollection services)
            => services.AddScoped<IRepositoryManager, RepositoryManager>();

        public static void ConfigureSqlContext(this IServiceCollection services, IConfiguration configuration)
            => services.AddDbContext<RepositoryContext>(options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
    }
}
