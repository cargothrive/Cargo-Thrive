using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoThrive.Core.Models;

namespace CargoThrive.Core.Services
{
    public interface ITestService
    {
        string GetTestMessage();
        Task<List<TestModel>> GetAllTestsAsync();
        Task<TestModel> GetTestByIdAsync(int tenantId);
        Task<TestModel> CreateTestAsync(TestModel test);
        Task<TestModel> UpdateTestAsync(TestModel test);
        Task<bool> DeleteTestAsync(int tenantId);
    }

   
}
