using CargoThrive.Core.Models;
using CargoThrive.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace CargoThrive.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous] // 允许匿名访问
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var result = await _authService.LoginAsync(request);
                if (result == null)
                {
                    return Unauthorized(new { message = "无效的用户名或密码" });
                }

                return Ok(new { token = result.Token, message = "登录成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登录失败");
                return StatusCode(500, new { message = "服务器错误，登录失败" });
            }
        }

        [HttpPost("switch-role")]
        [Authorize] // 需要登录才能访问
        public async Task<IActionResult> SwitchRoles([FromBody] SwitchRoleRequest request)
        {
            try
            {
                // 从Token中获取用户ID
                var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                if (userId <= 0)
                {
                    return BadRequest(new { message = "无效的用户ID" });
                }

                // 调用AuthService进行角色切换
                var result = await _authService.SwitchRolesAsync(userId, request.RoleId);
                if (result == null)
                {
                    return Unauthorized(new { message = "未找到对应角色" });
                }

                return Ok(new { token = result.Token, message = "切换成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "角色切换失败");
                return StatusCode(500, new { message = "服务器错误，角色切换失败" });
            }
        }


        /// <summary>
        /// 用户退出登录
        /// </summary>
        [HttpPost("logout")]
        [Authorize] // 需要登录才能访问
        public async Task<IActionResult> Logout()
        {
            try
            {
                // 获取当前用户的 JWT 令牌
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // 将 JWT 令牌加入黑名单
                await _authService.LogoutAsync(token);
                return Ok(new { message = "退出成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "退出失败");
                return StatusCode(500, new { message = "服务器错误，退出失败" });
            }
        }

        /// <summary>
        /// 获取当前登录用户信息
        /// </summary>
        [HttpGet("user-info")]
        [Authorize] // 需要登录才能访问
        public IActionResult GetUserInfo()
        {
            try
            {
                // 获取当前用户的基本信息
                var userInfo = new UserInfo
                {
                    Id = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)), // 获取用户ID
                    Username = User.FindFirstValue(ClaimTypes.Name), // 获取用户名
                    RoleId = Convert.ToInt64(User.FindFirstValue("RoleId")), // 获取角色ID (如果你在 JWT 中存储了 RoleId)
                                                                             // 如果你在 Claims 中存储了多个角色，可以使用以下方式：
                                                                             // Roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
                };

                // 如果获取到的用户信息无效，返回 BadRequest
                if (userInfo.Id <= 0 || string.IsNullOrEmpty(userInfo.Username))
                {
                    return BadRequest(new { message = "无法获取用户信息" });
                }

                // 返回用户信息（可以包括角色信息等）
                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户信息失败");
                return StatusCode(500, new { message = "服务器错误，获取用户信息失败" });
            }
        }
    }
}
