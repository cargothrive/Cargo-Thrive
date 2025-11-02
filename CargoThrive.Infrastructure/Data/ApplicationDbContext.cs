using Microsoft.EntityFrameworkCore;
using CargoThrive.Core.Models; // 引用 Core 层的实体模型

namespace CargoThrive.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet 属性用于代表数据库中的每个实体
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserManagement> UserManagements { get; set; }
        public DbSet<UserManagementSign> UserManagementSigns { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<TestModel> Tests { get; set; }
        public DbSet<LoginLog> LoginLogs { get; set; }

        // 配置数据库模型（可选，使用 Fluent API）
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置模型
            modelBuilder.Entity<Permission>().ToTable("Permission");  // 配置表名
            modelBuilder.Entity<Role>().ToTable("Role");  // 配置表名
            modelBuilder.Entity<RolePermission>().ToTable("RolePermission");  // 配置表名
            modelBuilder.Entity<UserRole>().ToTable("UserRole");  // 配置表名
            modelBuilder.Entity<UserManagement>().ToTable("UserManagement");  // 配置表名
            modelBuilder.Entity<UserManagementSign>().ToTable("UserManagementSign");  // 配置表名
            modelBuilder.Entity<Tenant>().ToTable("Tenant"); // 配置表名
            modelBuilder.Entity<TestModel>().ToTable("Test");  // 配置表名
            modelBuilder.Entity<LoginLog>().ToTable("LoginLog");  // 配置表名

            // 你还可以在这里添加更多的模型配置（如索引、关系等）
        }
    }
}
