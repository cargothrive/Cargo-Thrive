using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargoThrive.Core.Models
{
    /// <summary>
    /// JWT配置参数（与appsettings.json的"Jwt"节点对应）
    /// </summary>
    public class JwtSettings
    {
        public string Issuer { get; set; } = string.Empty;       // 签发者
        public string Audience { get; set; } = string.Empty;     // 受众
        public string Key { get; set; } = string.Empty;          // 签名密钥
        public int ExpiresInMinutes { get; set; }                // 过期时间（分钟）
        public int MaxPasswordErrorCount { get; set; }           // 最大密码错误次数
    }
}
