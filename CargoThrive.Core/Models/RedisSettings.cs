using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargoThrive.Core.Models
{
    /// <summary>
    /// Redis配置参数
    /// </summary>
    public class RedisSettings
    {
        /// <summary>
        /// 连接字符串（格式："127.0.0.1:6379,password=xxx,defaultDatabase=0"）
        /// </summary>
        public string ConnectionString { get; set; } = "127.0.0.1:6379";

        /// <summary>
        /// 默认数据库索引（0-15）
        /// </summary>
        public int DefaultDatabase { get; set; } = 0;

        /// <summary>
        /// 连接超时时间（毫秒）
        /// </summary>
        public int ConnectTimeout { get; set; } = 5000;

        /// <summary>
        /// 同步操作超时时间（毫秒）
        /// </summary>
        public int SyncTimeout { get; set; } = 1000;
    }
}
