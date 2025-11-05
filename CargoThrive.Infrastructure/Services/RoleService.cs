using CargoThrive.Core.Models;
using CargoThrive.Core.Services;
using CargoThrive.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargoThrive.Infrastructure.Services
{
    public class RoleService : IRoleService
    {
        private readonly ApplicationDbContext _dbContext;

        public RoleService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _dbContext.Roles.ToListAsync();
        }

        public async Task<Role?> GetRoleByIdAsync(long id)
        {
            return await _dbContext.Roles.FindAsync(id);
        }

        public async Task<Role> CreateRoleAsync(Role role)
        {
            _dbContext.Roles.Add(role);
            await _dbContext.SaveChangesAsync();
            return role;
        }

        public async Task<Role> UpdateRoleAsync(Role role)
        {
            _dbContext.Roles.Update(role);
            await _dbContext.SaveChangesAsync();
            return role;
        }

        public async Task<bool> DeleteRoleAsync(long id)
        {
            var role = await _dbContext.Roles.FindAsync(id);
            if (role == null)
            {
                return false;
            }

            _dbContext.Roles.Remove(role);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
