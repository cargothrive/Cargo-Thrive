using CargoThrive.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace CargoThrive.Infrastructure.Helpers
{
    public class StartupService : IHostedService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly RedisHelper _redisHelper;
        private readonly ILogger<StartupService> _logger;

        public StartupService(IServiceScopeFactory serviceScopeFactory, RedisHelper redisHelper, ILogger<StartupService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _redisHelper = redisHelper;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // 使用 IServiceScopeFactory 创建作用域
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // 1. 查询角色信息并批量添加到 Redis
                var roles = await dbContext.Roles.ToListAsync();
                foreach (var role in roles)
                {
                    var jsonRole = JsonSerializer.Serialize(role);
                    await _redisHelper.SetAddAsync("Role:" + role.Id, jsonRole);
                }

                // 2. 查询用户角色信息并批量添加到 Redis
                var userRoles = await dbContext.UserRoles.ToListAsync();
                foreach (var userRole in userRoles)
                {
                    var jsonUserRole = JsonSerializer.Serialize(userRole);
                    await _redisHelper.SetAddAsync("UserRoleRole:" + userRole.RoleId, jsonUserRole);
                    await _redisHelper.SetAddAsync("UserRoleManagement:" + userRole.UserManagementId, jsonUserRole);
                }

                // 3. 查询角色权限信息
                var rolePermissions = await dbContext.RolePermissions.ToListAsync();

                // 4. 查询所有权限信息，并缓存角色权限
                var permissions = await dbContext.Permissions.ToListAsync();
                var permissionsDict = permissions.ToDictionary(p => p.Id, p => p);

                // 5. 对角色权限进行分组，并将未找到的权限缓存到 Redis
                var groupRoleP = rolePermissions.GroupBy(r => r.RoleId);
                foreach (var groupRole in groupRoleP)
                {
                    foreach (var rolePermission in groupRole)
                    {
                        if (!permissionsDict.TryGetValue(rolePermission.PermissionId, out var permission))
                        {
                            var jsonRolePermission = JsonSerializer.Serialize(rolePermission);
                            await _redisHelper.SetAddAsync("RolePermission:" + rolePermission.RoleId, jsonRolePermission);
                        }
                    }
                }
            }

            _logger.LogInformation("StartupService has successfully initialized Redis data.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping StartupService.");
            return Task.CompletedTask;
        }
    }
}
