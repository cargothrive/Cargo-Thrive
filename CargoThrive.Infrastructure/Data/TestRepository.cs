using CargoThrive.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;  // 引入 EF Core 的命名空间

namespace CargoThrive.Infrastructure.Data
{
    public class TestRepository
    {
        private readonly ApplicationDbContext _context;

        public TestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TestModel>> GetTenantsAsync()
        {
            return await _context.Tests.ToListAsync();
        }
    }
}
