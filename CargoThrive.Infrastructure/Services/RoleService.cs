using CargoThrive.Core.Models;
using CargoThrive.Core.Services;
using CargoThrive.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargoThrive.Infrastructure.Services
{
    public class RoleService : IRoleService
    {
        private readonly ApplicationDbContext _db;

        public RoleService(ApplicationDbContext dbContext)
        {
            _db = dbContext;
        }
        public string GetTestMessage() => "Hello from TestService!";

        public async Task<List<Core.Models.Role>> GetAllRolesAsync(long tenantId, CancellationToken token = default)
        {
            return await _db.Roles
                .AsNoTracking() // 读操作不跟踪，降低内存和CPU
                .Where(r => r.TenantId == tenantId)
                .ToListAsync(token);
        }

        public async Task<Core.Models.Role?> GetRoleByIdAsync(long id, long tenantId, CancellationToken token = default)
        {
            return await _db.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId, token);
        }

        public async Task<bool> CreateRoleAsync(RoleRequest req, long tenantId, CancellationToken token = default)
        {
            var entity = new Core.Models.Role
            {
                // 如果 Id 由数据库自增，去掉赋值
                Id = req.Id,
                RoleName = req.RoleName,
                Description = req.Description,
                Status = true,
                TenantId = tenantId,
                CreateTime = DateTime.UtcNow
            };

            _db.Roles.Add(entity);
            return await _db.SaveChangesAsync(token) > 0;
        }

        public async Task<bool> UpdateRoleAsync(RoleRequest req, long tenantId, CancellationToken token = default)
        {
            var entity = await _db.Roles.FirstOrDefaultAsync(r => r.Id == req.Id && r.TenantId == tenantId, token);
            if (entity == null) return false;

            entity.RoleName = req.RoleName;
            entity.Description = req.Description;
            // 若有 UpdateTime 字段，可在此更新：entity.UpdateTime = DateTime.UtcNow;

            return await _db.SaveChangesAsync(token) > 0;
        }

        public async Task<bool> DeleteRoleAsync(long id, long tenantId, CancellationToken token = default)
        {
            var entity = await _db.Roles.FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId, token);
            if (entity == null) return false;

            _db.Roles.Remove(entity);
            return await _db.SaveChangesAsync(token) > 0;
        }
    }
}
