using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoThrive.Core.Models
{
    /// <summary>
    /// 角色表
    /// </summary>
    public class Role:DefaultEntity
    {
        /// <summary>
        /// 角色名称
        /// </summary>
        [Required(ErrorMessage = "角色名称不能为空")]
        [DisplayName("角色名称")]
        [Display(Name = "角色名称")]
        [Comment("角色名称（同一租户内不重复）")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "角色名称需在2-50字符之间")]
        public string RoleName { get; set; }

        /// <summary>
        /// 租户ID（外键）
        /// <para>特殊值：0 = 全局租户（仅总管理员/系统角色使用）</para>
        /// </summary>
        [Required(ErrorMessage = "租户ID不能为空")]
        [DisplayName("租户ID")]
        [Display(Name = "租户ID")]
        [Comment("租户外键（0=系统用户，关联系统级角色）")]
        [ForeignKey(nameof(Tenant))]
        public long TenantId { get; set; }

        /// <summary>
        /// 角色描述
        /// </summary>
        [DisplayName("角色描述")]
        [Display(Name = "角色描述")]
        [Comment("角色功能说明")]
        [StringLength(200, ErrorMessage = "角色描述不超过200字符")]
        public string? Description { get; set; }

        #region 导航属性
        /// <summary>
        /// 关联租户（TenantId=0时为null）
        /// </summary>
        public Tenant? Tenant { get; set; }

   
        #endregion
    }
}
