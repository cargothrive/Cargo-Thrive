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
    /// <summary>
    /// 角色-权限关联表
    /// </summary>
    public class RolePermission : DefaultEntity
    {
        /// <summary>
        /// 角色ID（外键）
        /// </summary>
        [Required(ErrorMessage = "角色ID不能为空")]
        [DisplayName("角色ID")]
        [Display(Name = "角色ID")]
        [Comment("角色外键")]
        [ForeignKey(nameof(Role))]
        public long RoleId { get; set; }

        /// <summary>
        /// 权限ID（外键）
        /// </summary>
        [Required(ErrorMessage = "权限ID不能为空")]
        [DisplayName("角色ID")]
        [Display(Name = "权限ID")]
        [Comment("权限外键")]
        [ForeignKey(nameof(Permission))]
        public long PermissionId { get; set; }

        #region 导航属性
        /// <summary>
        /// 关联的角色
        /// </summary>
        public Role Role { get; set; }

        /// <summary>
        /// 关联的权限
        /// </summary>
        public Permission Permission { get; set; }
        #endregion
    }
}