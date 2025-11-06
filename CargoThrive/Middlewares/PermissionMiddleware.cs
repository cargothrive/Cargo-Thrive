using CargoThrive.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;

namespace CargoThrive.API.Middlewares
{
    public class PermissionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PermissionMiddleware> _logger;

        public PermissionMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory, ILogger<PermissionMiddleware> logger)
        {
            _next = next;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {  
            // 检查当前请求是否允许匿名访问，若允许则直接跳过
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null)
            {
                await _next(context);
                return;
            }
            // 1. 跳过 Swagger 相关路径（允许匿名访问）
            var path = context.Request.Path.Value?.ToLowerInvariant();
            if (path?.StartsWith("/swagger") == true ||
                path == "/swagger.json" ||
                path == "/swagger/v1/swagger.json")
            {
                await _next(context);
                return;
            }
            try
            {
                // 创建一个新的作用域
                using (var scope = _scopeFactory.CreateScope())
                {
                    // 从作用域中解析Scoped服务
                    var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();

                    // 调用权限服务校验请求
                    (bool isValid, string errorMsg) = await permissionService.ValidateRequestAsync(context);

                    if (!isValid)
                    {
                        // 已登录但权限不足 → 403；未登录 → 401
                        var statusCode = context.User.Identity?.IsAuthenticated ?? false
                            ? (int)HttpStatusCode.Forbidden
                            : (int)HttpStatusCode.Unauthorized;

                        // 记录日志
                        _logger.LogWarning("Permission denied for {User} on {Path}. Error: {ErrorMsg}", context.User.Identity?.Name, context.Request.Path, errorMsg);

                        context.Response.StatusCode = statusCode;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(new
                        {
                            Code = statusCode,
                            Message = errorMsg,
                            Success = false
                        });
                        return;
                    }

                    // 校验通过，继续处理请求
                    await _next(context);
                }
            }
            catch (Exception ex)
            {
                // 记录异常日志
                _logger.LogError(ex, "An error occurred while validating permissions for {User} on {Path}.", context.User.Identity?.Name, context.Request.Path);

                // 返回 500 错误，说明权限服务出错
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    Code = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occurred while validating your request.",
                    Success = false
                });
            }
        }
    }
}
