using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargoThrive.Core.Models
{
    /// <summary>
    /// 角色请求Model
    /// </summary>
    public class RoleRequest
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
        /// 角色名称
        /// </summary>
        [Required(ErrorMessage = "角色名称不能为空")]
        [DisplayName("角色名称")]
        [Display(Name = "角色名称")]
        [Comment("角色名称（同一租户内不重复）")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "角色名称需在2-50字符之间")]
        public string RoleName { get; set; }

        /// <summary>
        /// 角色描述
        /// </summary>
        [DisplayName("角色描述")]
        [Display(Name = "角色描述")]
        [Comment("角色功能说明")]
        [StringLength(200, ErrorMessage = "角色描述不超过200字符")]
        public string? Description { get; set; }
    }
}
