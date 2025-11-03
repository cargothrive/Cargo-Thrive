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
            // 1. 配置UserRole索引（多对多关系中间表）
            modelBuilder.Entity<UserRole>()
                // 复合唯一索引：确保一个用户不会重复关联同一个角色
                .HasIndex(ur => new { ur.UserManagementId, ur.RoleId })
                .IsUnique()
                .HasDatabaseName("IX_UserRole_User_Role");

            modelBuilder.Entity<UserRole>()
                // 单字段索引：优化按角色查询用户的场景
                .HasIndex(ur => ur.RoleId)
                .HasDatabaseName("IX_UserRole_RoleId");

            // 2. 配置RolePermission索引（角色-权限中间表）
            modelBuilder.Entity<RolePermission>()
                .HasIndex(rp => new { rp.RoleId, rp.PermissionId })
                .IsUnique()
                .HasDatabaseName("IX_RolePermission_Role_Permission");

            // 1. 复合索引：查询“指定用户的有效签名”（高频场景）
            // 字段顺序：UserManagementId（筛选用户）→ Status（筛选启用）→ ExpireTime（筛选未过期）
            modelBuilder.Entity<UserManagementSign>().HasIndex(s => new { s.UserManagementId, s.Status })
                .HasDatabaseName("IX_UserManagementSign_User_Status");



        }

        /// <summary>
        /// 批量保存实体（支持新增/修改/删除，自动处理状态）
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="entities">实体集合</param>
        /// <param name="cancellationToken">取消令牌</param>
        public async Task<int> BulkSaveAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
            where TEntity : class, new()
        {
            foreach (var entity in entities)
            {
                var entry = Entry(entity);
                if (entry.State == EntityState.Detached)
                {
                    // 假设实体有 Id 字段，通过 Id 判断是新增还是修改
                    var idProperty = entry.Property("Id");
                    var idValue = idProperty.CurrentValue;
                    if (idValue == null || Convert.ToInt64(idValue) == 0)
                    {
                        entry.State = EntityState.Added;
                    }
                    else
                    {
                        entry.State = EntityState.Modified;
                    }
                }
            }

            return await SaveChangesAsync(cancellationToken);
        }
    }
}
