using CargoThrive.Core.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CargoThrive.Infrastructure.Helpers
{
    /// <summary>
    /// 获取当前登录用户上下文信息的辅助类
    /// </summary>
    public class CurrentUserHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 获取当前用户的完整信息
        /// </summary>
        public CurrentUserInfo GetCurrentUser()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || !user.Identity?.IsAuthenticated == true)
                throw new UnauthorizedAccessException("用户未登录或Token无效");

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = user.FindFirst(ClaimTypes.Name)?.Value;
            var tenantId = user.FindFirst("TenantId")?.Value;
            var account = user.FindFirst("Account")?.Value;
            var roleId = user.FindFirst("RoleId")?.Value;

            return new CurrentUserInfo
            {
                UserId = long.TryParse(userId, out var uid) ? uid : 0,
                UserName = userName ?? string.Empty,
                TenantId = long.TryParse(tenantId, out var tid) ? tid : 0,
                Account = account ?? string.Empty,
                RoleId = long.TryParse(roleId, out var rid) ? rid : 0
            };
        }
    }
}
