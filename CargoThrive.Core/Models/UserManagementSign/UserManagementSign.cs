using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoThrive.Core.Enums;

namespace CargoThrive.Core.Models
{
    /// <summary>
    /// 用户管理员签名
    /// </summary>
    public class UserManagementSign : DefaultEntity
    {
        /// <summary>
        /// 签名（管理员电子签名/标识签名）
        /// </summary>
        [Required(ErrorMessage = "管理员签名不能为空")] // 签名为管理员核心标识，设为必填
        [DisplayName("签名")]
        [Display(Name = "签名")]
        [Description("管理员电子签名或标识签名")]
        [Comment("管理员签名（用于身份标识或文件签署）")]
        [StringLength(400, ErrorMessage = "签名长度不超过400字符")] // 签名可能含特殊符号/短字符串，限制长度
        public string Sign { get; set; }

        /// <summary>
        /// 国家/地区（管理员所属国家）
        /// </summary>
        [Required(ErrorMessage = "国家/地区不能为空")]
        [DisplayName("国家/地区")]
        [Display(Name = "国家/地区")]
        [Description("管理员所属国家/地区")]
        [Comment("管理员所属国家/地区（关联CountryEnum枚举）")]
        public CountryEnum Country { get; set; } // 默认值设为中国，适配多数场景

        /// <summary>
        /// 签名名称（管理员对外显示的签名别名）
        /// </summary>
        [Required(ErrorMessage = "签名名称不能为空")]
        [DisplayName("签名名称")]
        [Display(Name = "签名名称")]
        [Description("管理员对外显示的签名别名")]
        [Comment("管理员签名的显示名称")]
        [StringLength(50, ErrorMessage = "签名名称不超过50字符")] // 显示名称需简洁，限制长度
        public string ShowNickName { get; set; }

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