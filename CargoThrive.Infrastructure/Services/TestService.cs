using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoThrive.Core.Models;
using CargoThrive.Core.Services;
using Microsoft.EntityFrameworkCore;  // 引入 EF Core 的命名空间
using CargoThrive.Infrastructure.Data;


namespace CargoThrive.Infrastructure.Services
{

    public class TestService : ITestService
    {
        private readonly ApplicationDbContext _dbContext;

        public TestService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public string GetTestMessage()
        {
            return "Hello from TestService!";
        }
        // 获取所有租户
        public async Task<List<TestModel>> GetAllTestsAsync()
        {
            return await _dbContext.Tests.ToListAsync();
        }

        // 根据 ID 获取单个租户
        public async Task<TestModel> GetTestByIdAsync(int tenantId)
        {
            return await _dbContext.Tests
                .FirstOrDefaultAsync(t => t.TenantId == tenantId);
        }

        // 创建新租户
        public async Task<TestModel> CreateTestAsync(TestModel tenant)
        {
            _dbContext.Tests.Add(tenant);
            await _dbContext.SaveChangesAsync();
            return tenant;
        }

        // 更新租户
        public async Task<TestModel> UpdateTestAsync(TestModel tenant)
        {
            _dbContext.Tests.Update(tenant);
            await _dbContext.SaveChangesAsync();
            return tenant;
        }

        // 删除租户
        public async Task<bool> DeleteTestAsync(int tenantId)
        {
            var tenant = await _dbContext.Tests.FindAsync(tenantId);
            if (tenant == null)
            {
                return false;
            }

            _dbContext.Tests.Remove(tenant);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
