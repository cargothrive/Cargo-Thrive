using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoThrive.Core.Models;
using CargoThrive.Core.Services;
using Microsoft.EntityFrameworkCore;  // 引入 EF Core 的命名空间
using CargoThrive.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore.Internal;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;


namespace CargoThrive.Infrastructure.Services
{

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        // 从配置文件获取最大错误次数（默认5次）
        private readonly int _maxPasswordErrorCount;

        public AuthService(ApplicationDbContext dbContext,IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _maxPasswordErrorCount = int.Parse(_configuration["Jwt:MaxPasswordErrorCount"] ?? "5");
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
                    u.Account == request.Account &&
                    u.Status);
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
            var passwordValid = VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt);

            if (!passwordValid)
            {
                await HandlePasswordErrorAsync(user); // 处理错误逻辑（含冷却期设置）
                await RecordLoginLog(user.Id, "Failed", "用户名或密码错误");
                throw new Exception("用户名或密码错误");
            }

            // 7. 登录成功 - 重置所有错误信息（含冷却期）
            await ResetPasswordErrorInfoAsync(user);

            // 7. 获取用户角色
            var roles = await _dbContext.Roles
                .Where(ur => ur.Status)
                .ToListAsync();
            if (!roles.Any())
            {
                await RecordLoginLog(user.Id, "Failed", "用户未赋予角色");
                throw new Exception("用户未赋予角色");
            }

            // 8. 生成JWT Token
            var token = GenerateJwtToken(user, roles);
            var expiresInMinutes = int.Parse(_configuration["Jwt:ExpiresInMinutes"]);
            // 6. 记录登录日志（可选）
            await RecordLoginLog(user.Id, "Success", null);

            return new LoginResponse
            {
                Token = token,
                ExpiresIn = expiresInMinutes * 60,
                User = new UserInfo
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Roles = roles,
                    PasswordExpireTime=user.PasswordExpireTime
                }
            };
        }
        private async Task HandlePasswordErrorAsync(UserManagement user)
        {
            var now = DateTime.Now;
            const int coolDownMinutes = 5; // 3-5次错误时的冷却时间（5分钟）

            // 累计错误次数（基于时间窗口逻辑不变）
            if (user.LoginCountTimeLimit.HasValue)
            {
                var timeWindow = TimeSpan.FromMinutes(user.LoginCountTimeLimit.Value);
                if (user.LastPasswordErrorTime.HasValue && now - user.LastPasswordErrorTime.Value <= timeWindow)
                {
                    user.PasswordErrorCount++; // 时间窗口内累计
                }
                else
                {
                    user.PasswordErrorCount = 1; // 超出窗口重置为1
                }
            }
            else
            {
                user.PasswordErrorCount++; // 无时间限制时直接累计
            }

            // 关键：3-5次错误时，设置5分钟冷却期
            if (user.PasswordErrorCount >= 3 && user.PasswordErrorCount <= 5)
            {
                user.PasswordErrorLockEndTime = now.AddMinutes(coolDownMinutes); // 冷却结束时间 = 现在+5分钟
            }
            // 可选：超过5次错误时，直接锁定账户（终极限制）
            else if (user.PasswordErrorCount > 5)
            {
                user.Status = false; // 账户锁定
                user.PasswordErrorLockEndTime = null; // 无需冷却，直接锁定
            }
            else
            {
                user.PasswordErrorLockEndTime = null; // 不足3次错误，无冷却
            }

            // 更新上次错误时间
            user.LastPasswordErrorTime = now;

            // 保存更新
            _dbContext.UserManagements.Update(user);
            await _dbContext.SaveChangesAsync();
        }

        private async Task ResetPasswordErrorInfoAsync(UserManagement user)
        {
            user.PasswordErrorCount = 0;
            user.LastPasswordErrorTime = null;
            user.PasswordErrorLockEndTime = null; // 清除冷却期
            _dbContext.UserManagements.Update(user);
            await _dbContext.SaveChangesAsync();
        }


        /// <summary>
        /// 退出登录（实际项目可能需要黑名单处理）
        /// </summary>
        public async Task LogoutAsync(long userId)
        {
            // 记录退出日志
            //await RecordLogoutLog(userId);

            // 如果需要立即失效Token，可以将Token加入黑名单（需配合缓存实现）
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
        /// 生成JWT Token
        /// </summary>
        private string GenerateJwtToken(UserManagement user, List<Role> roles)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),//用户Id
            new Claim(ClaimTypes.Name, user.UserName),//用户姓名
            new Claim("Account", user.Account),//登录账号
            new Claim("TenantId", user.TenantId.ToString()),//对应租户Id
            new Claim("RoleId", roles.Count()>0?roles[0].Id.ToString():"0")//当前角色Id
        };
            List<long> roleIds=roles.Select(x => x.Id).ToList();
            // 添加角色声明
            claims.AddRange(roleIds.Select(role => new Claim(ClaimTypes.Role, role.ToString())));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(int.Parse(_configuration["Jwt:ExpiresInMinutes"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// 验证密码（接收string类型的哈希和盐值，内部转换为byte[]）
        /// </summary>
        private bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
        {
            // 将存储的盐值（Base64字符串）转换为byte[]
            byte[] storedSaltBytes = Convert.FromBase64String(storedSalt);
            // 将存储的哈希值（Base64字符串）转换为byte[]
            byte[] storedHashBytes = Convert.FromBase64String(storedHash);

            using (var hmac = new HMACSHA512(storedSaltBytes))
            {
                // 计算输入密码的哈希
                byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

                // 对比计算出的哈希与存储的哈希
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHashBytes[i])
                        return false;
                }
                return true;
            }
        }

        /// <summary>
        /// 生成密码哈希和盐值（返回Base64字符串，适合存储到数据库）
        /// </summary>
        private void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                // 生成随机盐值（HMACSHA512自带随机密钥作为盐值）
                passwordSalt = Convert.ToBase64String(hmac.Key); // 转换为Base64字符串

                // 计算密码哈希并转换为Base64字符串
                byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                passwordHash = Convert.ToBase64String(hashBytes);
            }
        }

        /// <summary>
        /// 记录登录日志（可选）
        /// </summary>
        private async Task RecordLoginLog(long userId,string loginStatus,string failReason)
        {
            // 获取IP地址（示例代码，根据实际情况调整）
            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            // 获取用户代理（使用上面的正确写法）
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
