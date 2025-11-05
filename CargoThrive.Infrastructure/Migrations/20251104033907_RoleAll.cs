using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CargoThrive.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RoleAll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeatureFlag",
                table: "Test");

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "Test",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                comment: "实体主键 ID")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "Test",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateTime",
                table: "Test",
                type: "timestamp with time zone",
                nullable: true,
                comment: "创建时间");

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "Test",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "状态（1启用true，0禁用false）");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Test",
                table: "Test",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Permission",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "实体主键 ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PermissionType = table.Column<int>(type: "integer", nullable: false, comment: "权限类型（0=菜单，1=按钮，2=接口）"),
                    Path = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true, comment: "菜单URL或接口地址（按钮权限可为空）"),
                    ParentId = table.Column<long>(type: "bigint", nullable: false, comment: "父权限ID（0表示顶级权限）"),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true, comment: "描述"),
                    Sort = table.Column<int>(type: "integer", nullable: false, comment: "排序）"),
                    Status = table.Column<bool>(type: "boolean", nullable: false, comment: "状态（1启用true，0禁用false）"),
                    CreateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "创建时间"),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permission_Permission_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Permission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tenant",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "实体主键 ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "租户名称"),
                    Status = table.Column<bool>(type: "boolean", nullable: false, comment: "状态（1启用true，0禁用false）"),
                    CreateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "创建时间"),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenant", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "实体主键 ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "角色名称（同一租户内不重复）"),
                    TenantId = table.Column<long>(type: "bigint", nullable: false, comment: "租户外键（0=系统用户，关联系统级角色）"),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true, comment: "角色功能说明"),
                    Status = table.Column<bool>(type: "boolean", nullable: false, comment: "状态（1启用true，0禁用false）"),
                    CreateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "创建时间"),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Role_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserManagement",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "实体主键 ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Account = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "用户登录标识，支持手机号/邮箱格式"),
                    PasswordHash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "密码哈希值，禁止明文存储"),
                    PasswordSalt = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "密码盐值"),
                    UserName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "用户在系统中显示的真实姓名"),
                    StartTime = table.Column<string>(type: "text", nullable: true, comment: "格式为HH:mm，如09:00，非必填项"),
                    EndTime = table.Column<string>(type: "text", nullable: true, comment: "格式为HH:mm，如18:00，非必填项"),
                    TenantId = table.Column<long>(type: "bigint", nullable: false, comment: "租户外键（0=系统管理员，关联总管理员）"),
                    PasswordErrorCount = table.Column<int>(type: "integer", nullable: false, comment: "初始值为0，输错时递增，成功登录时重置为0，非必填（默认0）"),
                    PasswordExpireTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "格式为DateTime，为空表示密码永不过期，非必填项"),
                    LoginCountTimeLimit = table.Column<int>(type: "integer", nullable: true, comment: "例如：30表示30分钟内限制登录次数，为空表示无时间范围限制，非必填项"),
                    LastPasswordErrorTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "密码错误时更新为当前时间，成功登录或错误次数重置时可设为null，非必填项"),
                    PasswordErrorLockEndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "冷却期内禁止登录，冷却结束后可重新尝试；成功登录时重置为null"),
                    Status = table.Column<bool>(type: "boolean", nullable: false, comment: "状态（1启用true，0禁用false）"),
                    CreateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "创建时间"),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserManagement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserManagement_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermission",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "实体主键 ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<long>(type: "bigint", nullable: false, comment: "角色外键"),
                    PermissionId = table.Column<long>(type: "bigint", nullable: false, comment: "权限外键"),
                    Status = table.Column<bool>(type: "boolean", nullable: false, comment: "状态（1启用true，0禁用false）"),
                    CreateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "创建时间"),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermission_Permission_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermission_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoginLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "实体主键 ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LoginTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    LoginStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FailReason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UserManagementId = table.Column<long>(type: "bigint", nullable: false, comment: "用户外键")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoginLog_UserManagement_UserManagementId",
                        column: x => x.UserManagementId,
                        principalTable: "UserManagement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserManagementSign",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "实体主键 ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Sign = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false, comment: "管理员签名（用于身份标识或文件签署）"),
                    Country = table.Column<int>(type: "integer", nullable: false, comment: "管理员所属国家/地区（关联CountryEnum枚举）"),
                    ShowNickName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "管理员签名的显示名称"),
                    UserManagementId = table.Column<long>(type: "bigint", nullable: false, comment: "用户外键"),
                    Status = table.Column<bool>(type: "boolean", nullable: false, comment: "状态（1启用true，0禁用false）"),
                    CreateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "创建时间"),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserManagementSign", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserManagementSign_UserManagement_UserManagementId",
                        column: x => x.UserManagementId,
                        principalTable: "UserManagement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "实体主键 ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserManagementId = table.Column<long>(type: "bigint", nullable: false, comment: "用户外键"),
                    RoleId = table.Column<long>(type: "bigint", nullable: false, comment: "角色外键"),
                    Status = table.Column<bool>(type: "boolean", nullable: false, comment: "状态（1启用true，0禁用false）"),
                    CreateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "创建时间"),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRole_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRole_UserManagement_UserManagementId",
                        column: x => x.UserManagementId,
                        principalTable: "UserManagement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoginLog_UserManagementId",
                table: "LoginLog",
                column: "UserManagementId");

            migrationBuilder.CreateIndex(
                name: "IX_Permission_ParentId",
                table: "Permission",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Role_TenantId",
                table: "Role",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_PermissionId",
                table: "RolePermission",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_Role_Permission",
                table: "RolePermission",
                columns: new[] { "RoleId", "PermissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserManagement_TenantId",
                table: "UserManagement",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserManagementSign_User_Status",
                table: "UserManagementSign",
                columns: new[] { "UserManagementId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_RoleId",
                table: "UserRole",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_User_Role",
                table: "UserRole",
                columns: new[] { "UserManagementId", "RoleId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoginLog");

            migrationBuilder.DropTable(
                name: "RolePermission");

            migrationBuilder.DropTable(
                name: "UserManagementSign");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "Permission");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "UserManagement");

            migrationBuilder.DropTable(
                name: "Tenant");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Test",
                table: "Test");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Test");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "Test");

            migrationBuilder.DropColumn(
                name: "CreateTime",
                table: "Test");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Test");

            migrationBuilder.AddColumn<string>(
                name: "FeatureFlag",
                table: "Test",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
