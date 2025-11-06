using CargoThrive.Core.Models;
using CargoThrive.Core.Services;
using CargoThrive.Infrastructure.Helpers;
using CargoThrive.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Security.Claims;

namespace CargoThrive.API.Controllers
{
    [ApiController]
    [Route("api/role")] // 统一路由小写规范，与其他接口保持一致
    [Authorize] // 全局需要登录，匿名接口单独标记 [AllowAnonymous]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RolesController> _logger; // 新增日志记录
        private readonly CurrentUserHelper _currentUser;

        // 构造函数注入：添加 ILogger 便于问题排查
        public RolesController(IRoleService roleService, ILogger<RolesController> logger, CurrentUserHelper currentUser)
        {
            _roleService = roleService;
            _logger = logger;
            _currentUser = currentUser;
        }


        /// <summary>
        /// 获取所有角色列表
        /// </summary>
        /// <returns>角色列表数据</returns>
        /// <response code="200">成功返回角色列表</response>
        /// <response code="401">未登录或令牌失效</response>
        /// <response code="500">服务器内部错误</response>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<Role>>>> GetAllRoles(CancellationToken token)
        {
            try
            {
                var user = _currentUser.GetCurrentUser();
                var roles = await _roleService.GetAllRolesAsync(user.TenantId, token);

                return Ok(new ApiResponse<List<Role>>
                {
                    Result = roles,
                    Message = "查询成功"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取所有角色失败");
                return StatusCode(500, new { message = "服务器错误，获取角色列表失败" });
            }
        }

        /// <summary>
        /// 根据ID获取角色详情
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>角色详情数据</returns>
        /// <response code="200">成功返回角色详情</response>
        /// <response code="400">无效的角色ID</response>
        /// <response code="401">未登录或令牌失效</response>
        /// <response code="404">角色不存在</response>
        /// <response code="500">服务器内部错误</response>
        [HttpGet("{id:long}")] // 限制参数为长整型，避免无效输入
        public async Task<ActionResult<ApiResponse<Role>>> GetRoleById(long id, CancellationToken token)
        {
            if (id <= 0) return BadRequest(new { message = "无效的角色ID" });

            try
            {
                var user = _currentUser.GetCurrentUser();
                var role = await _roleService.GetRoleByIdAsync(id, user.TenantId, token);
                if (role == null) return NotFound(new { message = "未找到该角色" });

                return Ok(new ApiResponse<Role>
                {
                    Result = role,
                    Message = "查询成功"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取角色ID:{Id}详情失败", id);
                return StatusCode(500, new { message = "服务器错误，获取角色详情失败" });
            }
        }

        /// <summary>
        /// 新建角色
        /// </summary>
        /// <param name="role">角色创建参数</param>
        /// <returns>创建成功的角色信息</returns>
        /// <response code="200">角色创建成功</response>
        /// <response code="400">请求参数无效</response>
        /// <response code="401">未登录或令牌失效</response>
        /// <response code="500">服务器内部错误</response>
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] RoleRequest role, CancellationToken token)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "请求参数无效", errors = ModelState.Values });

            try
            {
                var user = _currentUser.GetCurrentUser();
                var ok = await _roleService.CreateRoleAsync(role, user.TenantId, token);
                if (!ok) return StatusCode(500, new { message = "角色创建失败" });

                return Ok(new ApiResponse<object>
                {
                    Result = null,
                    Message = "角色创建成功"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建角色失败，参数:{Payload}", System.Text.Json.JsonSerializer.Serialize(role));
                return StatusCode(500, new { message = "服务器错误，角色创建失败" });
            }
        }

        /// <summary>
        /// 修改角色信息
        /// </summary>
        /// <param name="id">角色ID（路径参数）</param>
        /// <param name="role">角色修改参数</param>
        /// <returns>修改成功的角色信息</returns>
        /// <response code="200">角色修改成功</response>
        /// <response code="400">请求参数无效或ID不匹配</response>
        /// <response code="401">未登录或令牌失效</response>
        /// <response code="404">角色不存在</response>
        /// <response code="500">服务器内部错误</response>
        [HttpPut("{id:long}")]
        public async Task<IActionResult> UpdateRole(long id, [FromBody] RoleRequest role, CancellationToken token)
        {
            if (!ModelState.IsValid || id != role.Id)
                return BadRequest(new { message = "请求参数无效或角色ID不匹配" });

            try
            {
                var user = _currentUser.GetCurrentUser();
                var existing = await _roleService.GetRoleByIdAsync(id, user.TenantId, token);
                if (existing == null) return NotFound(new { message = "未找到该角色" });

                var ok = await _roleService.UpdateRoleAsync(role, user.TenantId, token);
                if (!ok) return StatusCode(500, new { message = "角色修改失败" });

                return Ok(new ApiResponse<object> { Result = null, Message = "角色修改成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "修改角色ID:{Id}失败，参数:{Payload}", id, System.Text.Json.JsonSerializer.Serialize(role));
                return StatusCode(500, new { message = "服务器错误，角色修改失败" });
            }
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>删除结果</returns>
        /// <response code="200">角色删除成功</response>
        /// <response code="400">无效的角色ID</response>
        /// <response code="401">未登录或令牌失效</response>
        /// <response code="404">角色不存在</response>
        /// <response code="500">服务器内部错误</response>
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> DeleteRole(long id, CancellationToken token)
        {
            if (id <= 0) return BadRequest(new { message = "无效的角色ID" });

            try
            {
                var user = _currentUser.GetCurrentUser();
                var ok = await _roleService.DeleteRoleAsync(id, user.TenantId, token);
                if (!ok) return NotFound(new { message = "未找到该角色，删除失败" });

                return Ok(new ApiResponse<object> { Result = null, Message = "角色删除成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除角色ID:{Id}失败", id);
                return StatusCode(500, new { message = "服务器错误，角色删除失败" });
            }
        }
    }
}