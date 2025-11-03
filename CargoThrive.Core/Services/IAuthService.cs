using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoThrive.Core.Models;

namespace CargoThrive.Core.Services
{
    public interface IAuthService
    {
        string GetTestMessage();
        Task<LoginResponse> LoginAsync(LoginRequest request); 
        Task<LoginResponse> SwitchRolesAsync(long userId, long roleId);  // 角色切换方法
        Task LogoutAsync(string token);
        Task<bool> ValidateTokenAsync(string token);
    }

   
}
