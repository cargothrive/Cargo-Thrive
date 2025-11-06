using CargoThrive.Core.Converters;
using CargoThrive.Core.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Composition;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CargoThrive.Core.Models
{
    /// <summary>
    /// 基础实体类
    /// </summary>
    public class DefaultEntity
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

        /// <summary>​
        /// 实体状态​
        /// <para>0 - 启用：实体可正常使用</para>​
        /// <para>1 - 禁用：实体暂不可用（逻辑删除/冻结）</para>​
        /// </summary>
        [DisplayName("状态（1启用true，0禁用false）")]
        [Display(Name = "状态（1启用true，0禁用false）")]
        [Comment("状态（1启用true，0禁用false）")]
        public bool Status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [DisplayName("创建时间")]
        [Display(Name = "创建时间")]
        [Comment("创建时间")]
        //[JsonConverter(typeof(NullableDateTimeTextConverter))]   // ← 关键：属性级转换器
        public DateTime? CreateTime { get; set; }  // 或使用UtcNow（推荐跨时区场景）


        ///// <summary>
        ///// 修改时间
        ///// </summary>
        //[DisplayName("修改时间")]
        //[Display(Name = "修改时间")]
        //[Comment("修改时间")]
        //[DefaultValue(null)]
        //public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 时间戳
        /// 并发控制戳（防止并发修改冲突）
        /// </summary>
        [Display(Name = "时间戳")]
        [MaxLength(60)]
        [DefaultValue(null)]
        public string? ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
    }
}
