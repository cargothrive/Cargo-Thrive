using Microsoft.AspNetCore.Builder;

namespace CargoThrive.API.Middlewares
{
    public static class PermissionMiddlewareExtensions
    {
        /// <summary>
        /// 注册权限中间件到请求管道
        /// </summary>
        public static IApplicationBuilder UsePermissionMiddleware(this IApplicationBuilder app)
        {
            // 将 PermissionMiddleware 添加到请求管道中
            return app.UseMiddleware<PermissionMiddleware>();
        }
    }
}
