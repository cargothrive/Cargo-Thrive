using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargoThrive.Core.Models
{
    // 登录响应DTO
    public class LoginResponse
    {
        public string Token { get; set; }
        public long ExpiresIn { get; set; } // 过期时间（秒）
        public UserInfo User { get; set; }
    }

    // 用户信息DTO
    public class UserInfo
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        [DisplayName("用户Id")]
        public long Id { get; set; }

        /// <summary>
        /// 用户姓名
        /// </summary>
        [DisplayName("用户姓名")]
        public string Username { get; set; }

        /// <summary>
        /// 角色ID（外键）
        /// </summary>
        [DisplayName("角色ID")]
        public long RoleId { get; set; }

        /// <summary>
        /// 密码有效期
        /// </summary>
        [DisplayName("密码有效期")]
        public DateTime? PasswordExpireTime { get; set; } // 允许为空（永不过期）
        public List<Role>? Roles { get; set; } = new();
    }
}
