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
    /// 用户-角色关联表
    /// </summary>
    public class UserRole : DefaultEntity
    {
        /// <summary>
        /// 用户（外键）
        /// </summary>
        [Comment("用户外键")]
        [ForeignKey(nameof(UserManagement))]
        public long UserManagementId { get; set; }  // 用户ID（外键）

        /// <summary>
        /// 角色ID（外键）
        /// </summary>
        [Required(ErrorMessage = "角色ID不能为空")]
        [DisplayName("角色ID")]
        [Display(Name = "角色ID")]
        [Comment("角色外键")]
        [ForeignKey(nameof(Role))]
        public long RoleId { get; set; }  // 角色ID（外键）

        #region 导航属性
        public UserManagement UserManagement { get; set; }
        public Role Role { get; set; }
        #endregion
    }
}