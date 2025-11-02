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
        Task LogoutAsync(long userId);
        Task<bool> ValidateTokenAsync(string token);
    }

   
}
