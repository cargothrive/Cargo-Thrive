using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoThrive.Core.Models;
using CargoThrive.Core.Services;
using Microsoft.EntityFrameworkCore;  // 引入 EF Core 的命名空间
using CargoThrive.Infrastructure.Data;
using CargoThrive.Core.Enums;


namespace CargoThrive.Infrastructure.Services
{

    public class RoleManagementService : IRoleManagementService
    {
        private readonly ApplicationDbContext _dbContext;

        public RoleManagementService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public string GetTestMessage()
        {
            return "Hello from RoleManagementService!";
        }
        /// <summary>
        /// 为角色分配权限
        /// </summary>
        public async Task AssignPermissionsToRoleAsync(long roleId, IEnumerable<long> permissionIds)
        {
            // 先移除现有权限
            var existingPermissions = await _dbContext.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();

            _dbContext.RolePermissions.RemoveRange(existingPermissions);

            // 添加新权限
            var newPermissions = permissionIds.Select(pid => new RolePermission
            {
                RoleId = roleId,
                PermissionId = pid,
                Status = true,
                CreateTime = DateTime.Now,
                ConcurrencyStamp = Guid.NewGuid().ToString()
            });

            await _dbContext.RolePermissions.AddRangeAsync(newPermissions);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// 为用户分配角色
        /// </summary>
        public async Task AssignRolesToUserAsync(long userId, IEnumerable<long> roleIds)
        {
            // 先移除现有角色
            var existingRoles = await _dbContext.UserRoles
                .Where(ur => ur.UserManagementId == userId)
                .ToListAsync();

            _dbContext.UserRoles.RemoveRange(existingRoles);

            // 添加新角色
            var newRoles = roleIds.Select(rid => new UserRole
            {
                UserManagementId = userId,
                RoleId = rid,
                Status = true,
                CreateTime = DateTime.Now,
                ConcurrencyStamp = Guid.NewGuid().ToString()
            });

            await _dbContext.UserRoles.AddRangeAsync(newRoles);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// 获取用户所有权限
        /// </summary>
        public async Task<List<Permission>> GetUserPermissionsAsync(long userId)
        {
            return await _dbContext.UserRoles
                .Where(ur => ur.UserManagementId == userId && ur.Status)
                .Join(_dbContext.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => new { ur.RoleId, r.TenantId })
                .Join(_dbContext.RolePermissions,
                    r => r.RoleId,
                    rp => rp.RoleId,
                    (r, rp) => new { rp.PermissionId, r.TenantId })
                .Join(_dbContext.Permissions,
                    p => p.PermissionId,
                    perm => perm.Id,
                    (p, perm) => perm)
                .Where(perm => perm.Status)
                .Distinct()
                .ToListAsync();
        }
    }
}
