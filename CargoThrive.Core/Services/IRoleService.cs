using CargoThrive.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargoThrive.Core.Services
{
    public interface IRoleService
    {
        Task<List<Role>> GetAllRolesAsync();
        Task<Role?> GetRoleByIdAsync(long id);
        Task<Role> CreateRoleAsync(Role role);
        Task<Role> UpdateRoleAsync(Role role);
        Task<bool> DeleteRoleAsync(long id);
    }
}
