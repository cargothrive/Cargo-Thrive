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
    /// 用户-角色关联表（多对多关系中间表）
    /// </summary>
    // 正确：在类上声明单字段索引（RoleId）
    [Index(nameof(RoleId), Name = "IX_UserRole_RoleId")]
    // 复合索引保留不变（用户ID+角色ID唯一索引）
    [Index(nameof(UserManagementId), nameof(RoleId), IsUnique = true, Name = "IX_UserRole_User_Role")]
    public class UserRole : DefaultEntity
    {
        /// <summary>
        /// 用户外键
        /// </summary>
        [Comment("用户外键")]
        [ForeignKey(nameof(UserManagement))]
        public long UserManagementId { get; set; }

        /// <summary>
        /// 角色外键
        /// </summary>
        [Required(ErrorMessage = "角色ID不能为空")]
        [DisplayName("角色ID")]
        [Display(Name = "角色ID")]
        [Comment("角色外键")]
        [ForeignKey(nameof(Role))]
        public long RoleId { get; set; }

        #region 导航属性
        /// <summary>
        /// 关联的用户
        /// </summary>
        public required UserManagement UserManagement { get; set; }

        /// <summary>
        /// 关联的角色
        /// </summary>
        public required Role Role { get; set; }
        #endregion

        #region 构造函数
        /// <summary>
        /// EF Core 映射需要的无参构造函数
        /// </summary>
        protected UserRole() { }

        /// <summary>
        /// 创建用户角色关联的构造函数
        /// </summary>
        /// <param name="userManagementId">用户ID</param>
        /// <param name="roleId">角色ID</param>
        public UserRole(long userManagementId, long roleId)
        {
            UserManagementId = userManagementId;
            RoleId = roleId;
        }
        #endregion

        /// <summary>
        /// 验证实体数据是否有效
        /// </summary>
        /// <returns>是否有效</returns>
        public bool IsValid()
        {
            return UserManagementId > 0 && RoleId > 0;
        }
    }
}