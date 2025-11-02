using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoThrive.Core.Models;

namespace CargoThrive.Core.Services
{
    public interface IRoleManagementService
    {
        string GetTestMessage();
        Task AssignPermissionsToRoleAsync(long roleId, IEnumerable<long> permissionIds);
        Task AssignRolesToUserAsync(long userId, IEnumerable<long> roleIds);
        Task<List<Permission>> GetUserPermissionsAsync(long userId);
    }

   
}
