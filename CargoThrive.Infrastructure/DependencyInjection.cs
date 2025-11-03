using CargoThrive.Core.Models;
using CargoThrive.Core.Services;
using CargoThrive.Infrastructure.Data;
using CargoThrive.Infrastructure.Helpers;
using CargoThrive.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;


namespace CargoThrive.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {

            // 注册Redis配置
            services.Configure<RedisSettings>(redisSettings =>
            {
                config.GetSection("Redis").Bind(redisSettings);
            });

            // 注册Redis操作类（单例模式，因为ConnectionMultiplexer是线程安全的）
            services.AddSingleton<RedisHelper>();
            // 1. 注册数据库上下文（PostgreSQL）
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

            // 1. 配置Redis缓存
            services.AddStackExchangeRedisCache(options =>
            {
                // 获取Redis连接字符串配置
                options.Configuration = config["Redis:ConnectionString"];
                options.InstanceName = "CargoThriveCache:";  // 可选：设置缓存实例的前缀
            });

            // 2. 注册JWT配置（绑定appsettings.json的Jwt节点）
            services.Configure<JwtSettings>(jwtSettings =>
            {
                // 将"Jwt"节点的配置绑定到JwtSettings对象
                config.GetSection("Jwt").Bind(jwtSettings);
            });


            // 3. 注册核心服务（AuthService、RoleManagementService、PermissionService等）
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRoleManagementService, RoleManagementService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<ITestService, TestService>();

            // 4. 注册HttpContextAccessor（AuthService中获取IP/Token需要）
            services.AddHttpContextAccessor();

            return services;
        }
    }
}
