using CargoThrive.Core.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargoThrive.Core.Services
{
    public interface IPermissionService
    {
        string GetTestMessage();
        Task<(bool isValid, string errorMsg)> ValidateRequestAsync(HttpContext context);
    }

   
}
