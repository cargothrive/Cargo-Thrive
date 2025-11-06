using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargoThrive.Core.Models
{
    /// <summary>
    /// 当前登录用户信息模型
    /// </summary>
    public class CurrentUserInfo
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public long TenantId { get; set; }
        public string Account { get; set; }
        public long RoleId { get; set; }
    }
}
