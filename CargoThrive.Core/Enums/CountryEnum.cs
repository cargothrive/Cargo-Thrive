using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargoThrive.Core.Enums
{
    #region 国家字典
    /// <summary>
    /// 国家字典
    /// </summary>
    public enum CountryEnum
    {
        /// <summary>中国</summary>
        [Description("中国")]
        China = 0,

        /// <summary>加拿大</summary>
        [Description("Canada")]
        Canada = 1,

        /// <summary>美国</summary>
        [Description("U.S.A")]
        USA = 2
    }
    #endregion
}