using CargoThrive.Core.Models;
using CargoThrive.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CargoThrive.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
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
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 用户退出登录
        /// </summary>
        [HttpPost("logout")]
        [Authorize] // 需要登录才能访问
        public async Task<IActionResult> Logout()
        {
            // 从Token中获取用户ID
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _authService.LogoutAsync(userId);
            return Ok(new { message = "退出成功" });
        }

        /// <summary>
        /// 获取当前登录用户信息
        /// </summary>
        [HttpGet("user-info")]
        [Authorize]
        public IActionResult GetUserInfo()
        {
            var userInfo = new UserInfo
            {
                Id = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                Username = User.FindFirstValue(ClaimTypes.Name)
                //Roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
            };
            return Ok(userInfo);
        }
    }
}
