using CargoThrive.Core.Models;
using CargoThrive.Core.Services;
using CargoThrive.Infrastructure.Data;
using CargoThrive.Infrastructure.Helpers;
using CargoThrive.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace CargoThrive.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            // 1. 配置 Redis 配置项并注册
            services.Configure<RedisSettings>(redisSettings =>
            {
                config.GetSection("Redis").Bind(redisSettings);
            });

            // 2. 注册 Redis 操作类（单例模式，因为 ConnectionMultiplexer 是线程安全的）
            services.AddSingleton<RedisHelper>();

            // 3. 注册 StartupService 为单例（IHostedService 默认通常是单例）
            services.AddSingleton<IHostedService, StartupService>();  // 注册为 Singleton，确保只实例化一次

            // 4. 注册数据库上下文（PostgreSQL）
            services.AddDbContext<ApplicationDbContext>(options =>
              options.UseNpgsql(config.GetConnectionString("DefaultConnection"),
                  b => b.MigrationsAssembly("CargoThrive.Infrastructure")));  // 指定迁移程序集

            // 5. 配置 Redis 缓存服务
            services.AddStackExchangeRedisCache(options =>
            {
                // 获取 Redis 连接字符串配置
                options.Configuration = config["Redis:ConnectionString"];
                options.InstanceName = "CargoThriveCache:";  // 可选：设置缓存实例的前缀
            });

            // 6. 注册 JWT 配置（绑定 appsettings.json 的 Jwt 节点）
            services.Configure<JwtSettings>(jwtSettings =>
            {
                // 将"Jwt"节点的配置绑定到 JwtSettings 对象
                config.GetSection("Jwt").Bind(jwtSettings);
            });

            // 7. 注册核心服务（AuthService、RoleManagementService、PermissionService 等）
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRoleManagementService, RoleManagementService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<ITestService, TestService>();

            // 8. 注册 HttpContextAccessor（AuthService 中获取 IP/Token 需要）
            services.AddHttpContextAccessor();

            return services;
        }
    }
}
