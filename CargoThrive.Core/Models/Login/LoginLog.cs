using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.Composition;

namespace CargoThrive.Core.Models
{
    /// <summary>
    /// 租户表
    /// </summary>
    public class LoginLog 
    {


        /// <summary>
        /// 实体主键 ID
        /// </summary>
        [Key]
        [Export]
        [DisplayName("实体主键 ID")]
        [Display(Name = "实体主键 ID")]
        [Comment("实体主键 ID")]
        public long Id { get; set; }  // 租户ID

        /// <summary>
        /// 登录时间
        /// </summary>
        [Required(ErrorMessage = "登录时间不能为空")]
        [DisplayName("登录时间")]
        [Description("用户发起登录请求的时间")]
        public DateTime LoginTime { get; set; }

        /// <summary>
        /// 登录IP地址
        /// </summary>
        [Required(ErrorMessage = "IP地址不能为空")]
        [DisplayName("IP地址")]
        [Description("用户登录时的客户端IP地址")]
        [StringLength(50, ErrorMessage = "IP地址长度不能超过50字符")] // 支持IPv6最长格式
        public string IpAddress { get; set; }

        /// <summary>
        /// 用户代理（客户端信息）
        /// </summary>
        [DisplayName("用户代理")]
        [Description("登录时的客户端标识（如浏览器版本、设备信息等）")]
        [StringLength(500, ErrorMessage = "用户代理信息不能超过500字符")]
        public string UserAgent { get; set; } // 允许为空（部分客户端可能不传递）

        /// <summary>
        /// 登录状态
        /// </summary>
        [Required(ErrorMessage = "登录状态不能为空")]
        [DisplayName("登录状态")]
        [Description("登录结果状态：Success（成功）、Failed（失败）、Locked（账户锁定）、Expired（密码过期）等")]
        [StringLength(20, ErrorMessage = "登录状态不能超过20字符")]
        public string LoginStatus { get; set; }

        /// <summary>
        /// 失败原因（可选）
        /// </summary>
        [DisplayName("失败原因")]
        [Description("当登录失败时，记录具体原因（如密码错误、账户锁定等）")]
        [StringLength(100, ErrorMessage = "失败原因不能超过200字符")]
        public string? FailReason { get; set; } // 允许为空（登录成功时无需记录）

        /// <summary>
        /// 用户（外键）
        /// </summary>
        [Comment("用户外键")]
        [ForeignKey(nameof(UserManagement))]
        public long UserManagementId { get; set; }  // 用户ID（外键）

        #region 导航属性
        public UserManagement UserManagement { get; set; }
        #endregion
    }

}