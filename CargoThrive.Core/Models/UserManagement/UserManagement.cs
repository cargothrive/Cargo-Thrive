using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using CargoThrive.Core.Enums;

namespace CargoThrive.Core.Models
{
    /// <summary>
    /// 用户管理员
    /// </summary>
    public class UserManagement: DefaultEntity
    {



        /// <summary>
        /// 用户登录帐号
        /// </summary>
        [Required(ErrorMessage = "登录帐号不能为空")]
        [DisplayName("登录帐号")]
        [Display(Name = "登录帐号")]
        [Description("用户登录系统的账号（唯一）")]
        [Comment("用户登录标识，支持手机号/邮箱格式")]
        [StringLength(50, ErrorMessage = "登录帐号长度不超过50字符")]
        [RegularExpression(@"^[a-zA-Z0-9_@.]+$", ErrorMessage = "登录帐号仅支持字母、数字及@._符号")]
        public string Account { get; set; }

        /// <summary>
        /// 密码哈希值（存储HMACSHA512加密后的结果）
        /// </summary>
        [Required(ErrorMessage = "密码哈希值")]
        [DisplayName("密码哈希值")]
        [Display(Name = "密码哈希值")]
        [Description("密码哈希值（HMACSHA512加密存储）")]
        [Comment("密码哈希值，禁止明文存储")]
        [MaxLength(200, ErrorMessage = "密码哈希值为200字符")] // 加密后固定长度
        public string PasswordHash { get; set; }

        /// <summary>
        /// 密码盐值（随机生成，用于增强哈希安全性）
        /// </summary>
        [Required(ErrorMessage = "密码盐值")]
        [DisplayName("密码盐值")]
        [Display(Name = "密码盐值")]
        [Description("密码盐值")]
        [Comment("密码盐值")]
        [MaxLength(100, ErrorMessage = "密码盐值为200字符")] // 加密后固定长度
        public string PasswordSalt { get; set; } // 新增该字段，解决CS1061错误

        /// <summary>
        /// 用户姓名
        /// </summary>
        [Required(ErrorMessage = "用户姓名不能为空")]
        [DisplayName("用户姓名")]
        [Display(Name = "用户姓名")]
        [Description("用户真实姓名")]
        [Comment("用户在系统中显示的真实姓名")]
        [StringLength(20, ErrorMessage = "用户姓名不超过20字符")]
        public string UserName { get; set; }

        /// <summary>
        /// 上班时间
        /// </summary>
        [DisplayName("上班时间")]
        [Display(Name = "上班时间")]
        [Description("用户每日上班时间点（可空）")]
        [Comment("格式为HH:mm，如09:00，非必填项")]
        [RegularExpression(@"^([01]\d|2[0-3]):([0-5]\d)$", ErrorMessage = "上班时间格式应为HH:mm")]
        public string? StartTime { get; set; } // 允许为空

        /// <summary>
        /// 下班时间
        /// </summary>
        [DisplayName("下班时间")]
        [Display(Name = "下班时间")]
        [Description("用户每日下班时间点（可空）")]
        [Comment("格式为HH:mm，如18:00，非必填项")]
        [RegularExpression(@"^([01]\d|2[0-3]):([0-5]\d)$", ErrorMessage = "下班时间格式应为HH:mm")]
        public string? EndTime { get; set; } // 允许为空


        /// <summary>
        /// 租户ID（外键）
        /// <para>特殊值：0 = 全局租户（仅总管理员的UserManagement使用）</para>
        /// </summary>
        [Required(ErrorMessage = "租户ID不能为空")]
        [DisplayName("租户ID")]
        [Display(Name = "租户ID")]
        [Description("管理员所属租户ID，总管理员填0")]
        [Comment("租户外键（0=系统管理员，关联总管理员）")]
        [ForeignKey(nameof(Tenant))]
        public long TenantId { get; set; }


        #region 登录错误及强制密码修改限制


        /// <summary>
        /// 密码错误次数
        /// </summary>
        [DisplayName("密码错误次数")]
        [Display(Name = "密码错误次数")]
        [Description("用户连续输入错误密码的累计次数（用于锁定账户等逻辑）")]
        [Comment("初始值为0，输错时递增，成功登录时重置为0，非必填（默认0）")]
        [Range(0, int.MaxValue, ErrorMessage = "密码错误次数不能为负数")]
        public int PasswordErrorCount { get; set; } = 0; // 默认为0，非负整数


        /// <summary>
        /// 密码有效期
        /// </summary>
        [DisplayName("密码有效期")]
        [Display(Name = "密码有效期")]
        [Description("用户密码的有效截止时间（超过此时间需强制修改密码）")]
        [Comment("格式为DateTime，为空表示密码永不过期，非必填项")]
        public DateTime? PasswordExpireTime { get; set; } // 允许为空（永不过期）


        /// <summary>
        /// 登录次数时间限制（分钟）
        /// </summary>
        [DisplayName("登录次数时间限制（分钟）")]
        [Display(Name = "登录次数时间限制（分钟）")]
        [Description("限制单位时间内最大登录次数的时间范围（单位：分钟）")]
        [Comment("例如：30表示30分钟内限制登录次数，为空表示无时间范围限制，非必填项")]
        [Range(1, int.MaxValue, ErrorMessage = "时间限制应为正整数（单位：分钟）")]
        public int? LoginCountTimeLimit { get; set; } // 允许为空（无限制）

        /// <summary>
        /// 上次密码错误时间
        /// </summary>
        [DisplayName("上次密码错误时间")]
        [Display(Name = "上次密码错误时间")]
        [Description("用户最近一次输入错误密码的时间（用于判断错误次数是否在有效时间窗口内）")]
        [Comment("密码错误时更新为当前时间，成功登录或错误次数重置时可设为null，非必填项")]
        public DateTime? LastPasswordErrorTime { get; set; } // 允许为空（无错误记录时为null）

        /// <summary>
        /// 登录冷却结束时间
        /// </summary>
        [DisplayName("登录冷却结束时间")]
        [Display(Name = "登录冷却结束时间")]
        [Description("密码错误3-5次时，限制登录的冷却截止时间（5分钟）")]
        [Comment("冷却期内禁止登录，冷却结束后可重新尝试；成功登录时重置为null")]
        public DateTime? PasswordErrorLockEndTime { get; set; } // 允许为空（无冷却时为null）

        #endregion

        #region 导航属性

        /// <summary>
        /// 关联租户导航属性
        /// </summary>
        [DisplayName("关联租户")]
        [Description("管理员所属租户（TenantId=0时为null，对应总管理员）")]
        public Tenant? Tenant { get; set; }
        #endregion





    }
}
