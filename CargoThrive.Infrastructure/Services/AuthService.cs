using CargoThrive.Core.Models;
using CargoThrive.Core.Services;
using CargoThrive.Infrastructure.Data;
using CargoThrive.Infrastructure.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
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
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDistributedCache _cache;  // Redis 缓存，用于 Token 黑名单
        private readonly int _maxPasswordErrorCount;
        private readonly RedisHelper _redisHelper;

        public AuthService(ApplicationDbContext dbContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IDistributedCache cache,
    RedisHelper redisHelper)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
            _maxPasswordErrorCount = int.Parse(_configuration["Jwt:MaxPasswordErrorCount"] ?? "5");
            _redisHelper = redisHelper;
        }

        public string GetTestMessage()
        {
            return "Hello from TestService!";
        }

        /// <summary>
        /// 用户登录并生成Token
        /// </summary>
        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            // 1. 查询用户（包含角色信息）
            var user = await _dbContext.UserManagements
                .FirstOrDefaultAsync(u =>
                    u.Account == request.Account);  // 用户存在且账户有效

            // 2. 验证用户是否存在
            if (user == null)
                throw new Exception("用户名或密码错误");

            // 3. 检查是否处于冷却期（3-5次错误后限制5分钟）
            if (user.PasswordErrorLockEndTime.HasValue && DateTime.Now < user.PasswordErrorLockEndTime.Value)
            {
                var remainingMinutes = (user.PasswordErrorLockEndTime.Value - DateTime.Now).TotalMinutes;
                throw new Exception($"密码错误次数过多，请{Math.Ceiling(remainingMinutes)}分钟后再试");
            }

            // 4. 检查账户是否已锁定（超过5次错误的终极锁定，可选）
            if (!user.Status)
                throw new Exception("账户已锁定，请联系管理员解锁");

            // 5. 检查密码是否过期
            if (user.PasswordExpireTime.HasValue && DateTime.Now > user.PasswordExpireTime.Value)
                throw new Exception("密码已过期，请修改密码后重新登录");

            // 6. 验证密码
            var passwordValid = PasswordHasher.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt);

            if (!passwordValid)
            {
                await HandlePasswordErrorAsync(user); // 处理错误逻辑（含冷却期设置）
                await RecordLoginLog(user.Id, "Failed", "用户名或密码错误");
                throw new Exception("用户名或密码错误");
            }

            // 7. 登录成功 - 重置所有错误信息（含冷却期）
            await ResetPasswordErrorInfoAsync(user);

            // 8. 获取用户角色信息
            var validRoles = _dbContext.Roles.Where(r => r.Status);  // 提前过滤右表
            var roles = await _dbContext.UserRoles
                .Where(ur => ur.UserManagementId == user.Id && ur.Status)
                .Join(
                    validRoles,  // 使用已过滤的右表
                    ur => ur.RoleId,
                    role => role.Id,
                    (ur, role) => role
                )
                .Distinct()
                .ToListAsync();

            if (!roles.Any())
            {
                await RecordLoginLog(user.Id, "Failed", "用户未赋予角色");
                throw new Exception("用户未赋予角色");
            }

            // 9. 生成JWT Token
            var token = GenerateJwtToken(user, roles.Count() > 0 ? roles[0].Id.ToString() : "0", roles);

            var expiresInMinutes = int.Parse(_configuration["Jwt:ExpiresInMinutes"]);
            await RecordLoginLog(user.Id, "Success", null);  // 登录日志记录

            return new LoginResponse
            {
                Token = token,
                ExpiresIn = expiresInMinutes * 60,  // 转换为秒
                User = new UserInfo
                {
                    Id = user.Id,
                    Username = user.UserName,
                    RoleId = roles.Count() > 0 ? roles[0].Id : 0,
                    Roles = roles,
                    PasswordExpireTime = user.PasswordExpireTime
                }
            };
        }


        private async Task HandlePasswordErrorAsync(UserManagement user)
        {
            var now = DateTime.Now;
            const int coolDownMinutes = 5;  // 错误次数达到3-5次后冷却时间（5分钟）

            // 累计错误次数（基于时间窗口逻辑不变）
            if (user.LoginCountTimeLimit.HasValue)
            {
                var timeWindow = TimeSpan.FromMinutes(user.LoginCountTimeLimit.Value);
                if (user.LastPasswordErrorTime.HasValue && now - user.LastPasswordErrorTime.Value <= timeWindow)
                {
                    user.PasswordErrorCount++;  // 时间窗口内累计
                }
                else
                {
                    user.PasswordErrorCount = 1;  // 超出窗口重置为1
                }
            }
            else
            {
                user.PasswordErrorCount++;  // 无时间限制时直接累计
            }

            // 设置冷却期（3-5次错误时，设置冷却期为5分钟）
            if (user.PasswordErrorCount >= 3 && user.PasswordErrorCount <= 5)
            {
                user.PasswordErrorLockEndTime = now.AddMinutes(coolDownMinutes);  // 设置冷却期结束时间
            }
            // 超过5次错误时锁定账户
            else if (user.PasswordErrorCount > 5)
            {
                user.Status = false;  // 账户锁定
                user.PasswordErrorLockEndTime = null;  // 锁定账户，无需冷却
            }
            else
            {
                user.PasswordErrorLockEndTime = null;  // 错误次数不足3次时，无冷却
            }

            // 更新错误时间
            user.LastPasswordErrorTime = now;

            // 保存更新
            _dbContext.UserManagements.Update(user);
            await _dbContext.SaveChangesAsync();
        }

        private async Task ResetPasswordErrorInfoAsync(UserManagement user)
        {
            user.PasswordErrorCount = 0;
            user.LastPasswordErrorTime = null;
            user.PasswordErrorLockEndTime = null;  // 清除冷却期
            _dbContext.UserManagements.Update(user);
            await _dbContext.SaveChangesAsync();
        }


        /// <summary>
        /// 验证Token有效性
        /// </summary>
        public async Task<bool> ValidateTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]))
                }, out var validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 切换角色
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<LoginResponse> SwitchRolesAsync(long userId, long roleId)
        {

            // 1. 查询用户（包含角色信息）
            var user = await _dbContext.UserManagements
                .FirstOrDefaultAsync(u =>
                    u.Id == userId &&
                    u.Status);  // 用户存在且账户有效

            // 2. 验证用户是否存在
            if (user == null)
                throw new Exception("用户不存在");




            // 3. 获取用户角色信息
            var validRoles = _dbContext.Roles.Where(r => r.Status);  // 提前过滤右表
            var roles = await _dbContext.UserRoles
                .Where(ur => ur.UserManagementId == user.Id && ur.Status)
                .Join(
                    validRoles,  // 使用已过滤的右表
                    ur => ur.RoleId,
                    role => role.Id,
                    (ur, role) => role
                )
                .Distinct()
                .ToListAsync();

            if (!roles.Any())
            {
                await RecordLoginLog(user.Id, "Failed", "用户未赋予角色");
                throw new Exception("用户未赋予角色");
            }
            if (roles.Count(r => r.Id == roleId) == 0)
            {

                throw new Exception("未找到指定角色");
            }
            // 9. 生成JWT Token
            var token = GenerateJwtToken(user, roleId.ToString(), roles);

            var expiresInMinutes = int.Parse(_configuration["Jwt:ExpiresInMinutes"]);

            return new LoginResponse
            {
                Token = token,
                ExpiresIn = expiresInMinutes * 60,  // 转换为秒
                User = new UserInfo
                {
                    Id = user.Id,
                    Username = user.UserName,
                    RoleId = roles.Count() > 0 ? roles[0].Id : 0,
                    Roles = roles,
                    PasswordExpireTime = user.PasswordExpireTime
                }
            };

        }

        /// <summary>
        /// 退出登录时，将 JWT 令牌加入黑名单
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task LogoutAsync(string token)
        {
            var tokenKey = $"blacklist:{token}";
            await _redisHelper.SetStringAsync(tokenKey, "invalid", TimeSpan.FromMinutes(60));  // 设置过期时间
        }

        /// <summary>
        /// 生成JWT Token
        /// </summary>
        private string GenerateJwtToken(UserManagement user, string roleId, List<Core.Models.Role> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // 用户Id
                new Claim(JwtRegisteredClaimNames.Name, user.UserName), // 用户名
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // 唯一标识
                new Claim("Account", user.Account), // 登录账号
                new Claim("TenantId", user.TenantId.ToString()), // 租户Id
                new Claim("RoleId", roleId), // 当前角色Id
                new Claim(ClaimTypes.Role, string.Join(",", roles.Select(r => r.Id))) // 角色Id
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpiresInMinutes"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// 记录登录日志（可选）
        /// </summary>
        private async Task RecordLoginLog(long userId, string loginStatus, string failReason)
        {
            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString() ?? string.Empty;

            _dbContext.LoginLogs.Add(new LoginLog
            {
                UserManagementId = userId,
                LoginTime = DateTime.Now,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                LoginStatus = loginStatus,
                FailReason = failReason // 成功时无需原因
            });

            await _dbContext.SaveChangesAsync();
        }
    }
}
