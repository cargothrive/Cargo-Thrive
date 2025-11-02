using CargoThrive.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using CargoThrive.Core.Services;
using CargoThrive.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// 1. 读取 JWT 配置
var jwtConfig = builder.Configuration.GetSection("Jwt");
var secretKey = jwtConfig["Key"];
var issuer = jwtConfig["Issuer"];
var audience = jwtConfig["Audience"];

// 2. 添加 JWT 认证服务
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(options =>
  {
      options.TokenValidationParameters = new TokenValidationParameters
      {
          // 必须验证的参数
          ValidateIssuer = true,         // 验证签发者
          ValidateAudience = true,       // 验证受众
          ValidateLifetime = true,       // 验证过期时间
          ValidateIssuerSigningKey = true, // 验证签名密钥

          // 对应配置的值
          ValidIssuer = issuer,
          ValidAudience = audience,
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),

          // 可选：允许的时钟偏差（避免服务器时间差异导致令牌提前失效）
          ClockSkew = TimeSpan.FromMinutes(5)
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
