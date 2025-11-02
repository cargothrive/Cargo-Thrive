using CargoThrive.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using CargoThrive.Core.Services;
using CargoThrive.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
// Program.cs 中添加认证服务
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"], // 从配置文件读取
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// 添加授权服务
builder.Services.AddAuthorization();


#region 注册 Service 作为 IService 的实现
// 注册 TestService 作为 ITestService 的实现
builder.Services.AddScoped<ITestService, TestService>();  // 或者 AddTransient/Singleton
builder.Services.AddScoped<IRoleManagementService, RoleManagementService>();  // 或者 AddTransient/Singleton
builder.Services.AddScoped<IPermissionService, PermissionService>();  // 或者 AddTransient/Singleton
#endregion

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// 从 appsettings.json 中读取数据库连接字符串
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
