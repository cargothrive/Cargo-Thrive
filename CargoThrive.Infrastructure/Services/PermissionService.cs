using CargoThrive.Core.Enums;
using CargoThrive.Core.Models;
using CargoThrive.Core.Services;
using CargoThrive.Infrastructure.Data;
using CargoThrive.Infrastructure.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;  // 引入 EF Core 的命名空间
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


namespace CargoThrive.Infrastructure.Services
{

    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly RedisHelper _redisHelper;

        public PermissionService(ApplicationDbContext dbContext,RedisHelper redisHelper)
        {
            _dbContext = dbContext;
            _redisHelper = redisHelper;
        }
        public string GetTestMessage()
        {
            return "Hello from TestService!";
        }

        /// <summary>
        /// 校验请求的权限
        /// </summary>
        public async Task<(bool isValid, string errorMsg)> ValidateRequestAsync(HttpContext context)
        {// 1. 获取 JWT Token
            var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
            {
                return (false, "Token 不存在");
            }

            var tokenKey = $"blacklist:{token}";
             ;
            // 2. 检查 JWT 是否在黑名单中
            if (await _redisHelper.KeyExistsAsync(tokenKey))
            {
                return (false, "令牌已失效");
            }
            // 1. 获取当前用户
            var user = context.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                return (false, "用户未登录");
            }

            // 获取用户的角色信息
            var roles = user.FindFirst(ClaimTypes.Role)?.Value;  // 这是一个逗号分隔的角色ID字符串

            if (string.IsNullOrEmpty(roles))
            {
                return (false, "用户没有分配角色");
            }
            // 获取当前登录用户的Claims
            var userClaims = user.Claims.ToList();

            // 获取具体的Claims值
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = user.FindFirst(JwtRegisteredClaimNames.Name)?.Value;
            var tenantId = user.FindFirst("TenantId")?.Value;
            var account = user.FindFirst("Account")?.Value;
            var roleId = user.FindFirst("RoleId")?.Value;

            if (roleId == null || !roleId.Any())
            {
                return (false, "用户没有分配角色");
            }
            //// 2. 获取用户角色
            //var userRoles = await _dbContext.UserRoles
            //    .Where(ur => ur.UserId == user.Identity.Name)
            //    .Select(ur => ur.Role)
            //    .ToListAsync();

            //if (userRoles == null || !userRoles.Any())
            //{
            //    return (false, "用户没有分配角色");
            //}

            long roleIdLong = Convert.ToInt64(roleId);

            // 3. 获取请求的路径和HTTP方法
            var requestPath = context.Request.Path.Value?.ToLower(); // 请求的路径（例如 /api/resource）
            var httpMethod = context.Request.Method.ToUpper(); // HTTP方法（GET, POST, PUT, DELETE）

            // 4. 根据角色和权限验证
            // 获取角色拥有的权限
            var permissions = await _dbContext.RolePermissions
                .Where(rp => rp.RoleId == roleIdLong)
                .Select(rp => rp.Permission)
                .ToListAsync();

            // 根据请求路径和方法查找是否有权限访问
            var hasPermission = permissions.Any(p =>
                p.Path.ToLower() == requestPath);

            if (hasPermission)
            {
                return (true, string.Empty); // 权限验证通过
            }


            return (false, "权限不足"); // 没有权限
        }

      
    }
}
