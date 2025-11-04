using CargoThrive.API.Middlewares;
using CargoThrive.Infrastructure;
using CargoThrive.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. 注册基础设施层服务（解耦API层，统一管理数据库、服务、配置）
builder.Services.AddInfrastructure(builder.Configuration);

// 2. 配置JWT认证（基于appsettings.json的Jwt节点）
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<CargoThrive.Core.Models.JwtSettings>()!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // 必须验证项
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            // 配置值（从JwtSettings读取，避免硬编码）
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            // 允许5分钟时钟偏差（避免服务器时间差异导致令牌失效）
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });

// 3. 启用授权服务
builder.Services.AddAuthorization();

// 4. 添加控制器支持
builder.Services.AddControllers();

// 5. 配置Swagger（支持JWT认证、中文文档信息）
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // 5.1 配置API文档基本信息
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CargoThrive 货运系统 API",
        Version = "v1",
        Description = "基于.NET 8的货运系统API文档，支持JWT认证、权限控制",
        Contact = new OpenApiContact
        {
            Name = "jcj"
        }
    });

    // 5.2 添加JWT认证支持（Swagger显示Authorize按钮）
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "请输入JWT令牌（格式：Bearer {你的Token}）"
    });

    // 5.3 全局应用认证要求（所有接口默认需要JWT，公开接口需加[AllowAnonymous]）
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
// 注册定时任务服务
builder.Services.AddHostedService<TimedTaskService>();

var app = builder.Build();

// 6. 配置HTTP请求管道（按顺序执行）
if (app.Environment.IsDevelopment())
{
    // 开发环境启用Swagger
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CargoThrive API v1");
        // 可选：设置Swagger UI默认路径（访问根路径直接进入文档）
        // options.RoutePrefix = string.Empty;
    });
}

// 7. 强制HTTPS（生产环境建议启用）  
app.UseHttpsRedirection();

// 8. 启用认证（解析JWT令牌）
app.UseAuthentication();

// 9. 启用权限校验中间件（在授权前执行，校验接口权限）
app.UsePermissionMiddleware();

// 10. 启用授权（基于角色/权限的访问控制）
app.UseAuthorization();

// 11. 映射控制器路由
app.MapControllers();

app.Run();
