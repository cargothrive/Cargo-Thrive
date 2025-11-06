using CargoThrive.API.Middlewares;
using CargoThrive.Core.Converters;
using CargoThrive.Infrastructure;
using CargoThrive.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ 注册基础设施层服务
builder.Services.AddInfrastructure(builder.Configuration);

// 2️⃣ 配置 JWT 认证
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<CargoThrive.Core.Models.JwtSettings>()!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });

// 3️⃣ 启用授权服务
builder.Services.AddAuthorization();

// 4️⃣ 添加控制器并配置 JSON 序列化
builder.Services.AddControllers().AddJsonOptions(o =>
{
    //返回时间序列化
    o.JsonSerializerOptions.Converters.Add(new FlexibleDateTimeConverterFactory("yyyy-MM-dd HH:mm:ss"));
    o.JsonSerializerOptions.PropertyNamingPolicy = null;
    o.JsonSerializerOptions.AllowTrailingCommas = true;
    o.JsonSerializerOptions.ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip;
});

// 5️⃣ Swagger 配置
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CargoThrive 货运系统 API",
        Version = "v1",
        Description = "基于 .NET 8 的货运系统 API 文档，支持 JWT 权限控制",
        Contact = new OpenApiContact { Name = "jcj" }
    });

    // 载入 XML 注释
    var apiXmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var apiXmlPath = Path.Combine(AppContext.BaseDirectory, "Properties", apiXmlFile);
    if (File.Exists(apiXmlPath))
        options.IncludeXmlComments(apiXmlPath, includeControllerXmlComments: true);

    var coreXmlFile = "CargoThrive.Core.xml";
    var coreXmlPath = Path.Combine(AppContext.BaseDirectory, "Properties", coreXmlFile);
    if (File.Exists(coreXmlPath))
        options.IncludeXmlComments(coreXmlPath);

    // 启用 JWT 鉴权
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT 认证格式: Bearer {token}"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
    // 请求序列化统一把 DateTime 看作字符串（避免内部用错误的默认值去反序列化）
    options.MapType<DateTime>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "date-time",
        Example = new OpenApiString(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:dd"))
    });
    options.MapType<DateTime?>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "date-time",
        Nullable = true,
        Example = new OpenApiString(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:dd"))
    });
});

// 6️⃣ 控制台日志
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// 7️⃣ 注册定时任务服务
builder.Services.AddHostedService<TimedTaskService>();

var app = builder.Build();

// 8️⃣ HTTP 请求管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CargoThrive API v1");
        options.RoutePrefix = string.Empty;
        options.DocumentTitle = "货运系统 API 文档";
    });
    app.MapSwagger().AllowAnonymous();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UsePermissionMiddleware();

app.MapControllers();

app.Run();
