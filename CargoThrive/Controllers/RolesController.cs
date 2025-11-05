using CargoThrive.Core.Models;
using CargoThrive.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargoThrive.API.Controllers
{
    [ApiController]
    [Route("api/Role")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        /// <summary>
        /// 获取所有角色
        /// </summary>
        [HttpGet]
        [Authorize] // 需要登录才能访问
        public async Task<ActionResult<List<Role>>> GetRoles()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(roles);
        }

        /// <summary>
        /// 获取角色根据ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize] // 需要登录才能访问
        public async Task<ActionResult<Role>> GetRoleById(long id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return Ok(role);
        }

        /// <summary>
        /// 创建角色
        /// </summary>
        [HttpPost]
        [Authorize] // 需要登录才能访问
        public async Task<ActionResult<Role>> CreateRole([FromBody] Role role)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdRole = await _roleService.CreateRoleAsync(role);
            return CreatedAtAction(nameof(GetRoleById), new { id = createdRole.Id }, createdRole);
        }

        /// <summary>
        /// 更新角色
        /// </summary>
        [HttpPut("{id}")]
        [Authorize] // 需要登录才能访问
        public async Task<ActionResult<Role>> UpdateRole(long id, [FromBody] Role role)
        {
            if (id != role.Id)
            {
                return BadRequest("Role ID mismatch");
            }

            var updatedRole = await _roleService.UpdateRoleAsync(role);
            if (updatedRole == null)
            {
                return NotFound();
            }

            return Ok(updatedRole);
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize] // 需要登录才能访问
        public async Task<IActionResult> DeleteRole(long id)
        {
            var success = await _roleService.DeleteRoleAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
