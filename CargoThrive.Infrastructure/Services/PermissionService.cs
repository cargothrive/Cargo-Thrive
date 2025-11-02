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

    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _dbContext;

        public PermissionService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public string GetTestMessage()
        {
            return "Hello from TestService!";
        }
        /// <summary>
        /// 检查用户是否拥有指定权限
        /// </summary>
        public async Task<bool> HasPermissionAsync(long userId, string permissionPath)
        {
            // 查询用户是否拥有指定路径的权限
            var hasPermission = await _dbContext.UserRoles
                .Where(ur => ur.UserManagementId == userId && ur.Status)
                .Join(_dbContext.RolePermissions,
                    ur => ur.RoleId,
                    rp => rp.RoleId,
                    (ur, rp) => rp.PermissionId)
                .Join(_dbContext.Permissions,
                    pid => pid,
                    p => p.Id,
                    (pid, p) => p)
                .Where(p => p.Path == permissionPath && p.Status)
                .AnyAsync();

            return hasPermission;
        }

        /// <summary>
        /// 检查用户是否拥有指定角色
        /// </summary>
        public async Task<bool> HasRoleAsync(long userId, string roleName)
        {
            return await _dbContext.UserRoles
                .Where(ur => ur.UserManagementId == userId && ur.Status)
                .Join(_dbContext.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => r)
                .Where(r => r.RoleName == roleName && r.Status)
                .AnyAsync();
        }

        /// <summary>
        /// 检查用户是否通过角色拥有指定权限（动态关联角色与权限）
        /// </summary>
        public async Task<bool> HasPermissionViaRoleAsync(long userId, string permissionPath)
        {
            // 逻辑：用户 -> 拥有的角色 -> 角色关联的权限 -> 是否包含目标权限
            return await _dbContext.UserRoles
                .Where(ur => ur.UserManagementId == userId && ur.Status) // 用户拥有的有效角色
                .Join(_dbContext.RolePermissions,
                    ur => ur.RoleId,
                    rp => rp.RoleId,
                    (ur, rp) => rp.PermissionId)
                .Join(_dbContext.Permissions,
                    pid => pid,
                    p => p.Id,
                    (pid, p) => p)
                .Where(p => p.Path == permissionPath && p.Status) // 匹配目标权限且权限有效
                .AnyAsync();
        }

        /// <summary>
        /// （可选）获取用户拥有的所有权限（用于前端菜单渲染）
        /// </summary>
        public async Task<List<string>> GetUserPermissionPathsAsync(long userId)
        {
            return await _dbContext.UserRoles
                .Where(ur => ur.UserManagementId == userId && ur.Status)
                .Join(_dbContext.RolePermissions, ur => ur.RoleId, rp => rp.RoleId, (ur, rp) => rp.PermissionId)
                .Join(_dbContext.Permissions, pid => pid, p => p.Id, (pid, p) => p.Path)
                .Distinct()
                .ToListAsync();
        }
    }
}
