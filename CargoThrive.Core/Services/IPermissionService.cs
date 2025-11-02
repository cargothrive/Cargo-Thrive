using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoThrive.Core.Models;

namespace CargoThrive.Core.Services
{
    public interface IPermissionService
    {
        string GetTestMessage();
        Task<bool> HasPermissionAsync(long userId, string permissionPath);
        Task<bool> HasRoleAsync(long userId, string roleName);
        Task<bool> HasPermissionViaRoleAsync(long userId, string permissionPath);
        Task<List<string>> GetUserPermissionPathsAsync(long userId);
    }

   
}
