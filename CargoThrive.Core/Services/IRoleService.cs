using CargoThrive.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargoThrive.Core.Services
{
    public interface IRoleService
    {
        string GetTestMessage();
        Task<List<Role>> GetAllRolesAsync(long tenantId, CancellationToken token = default);
        Task<Role?> GetRoleByIdAsync(long id, long tenantId, CancellationToken token = default);
        Task<bool> CreateRoleAsync(RoleRequest role, long tenantId, CancellationToken token = default);
        Task<bool> UpdateRoleAsync(RoleRequest role, long tenantId, CancellationToken token = default);
        Task<bool> DeleteRoleAsync(long id, long tenantId, CancellationToken token = default);
    }
}
