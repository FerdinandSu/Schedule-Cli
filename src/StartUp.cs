using PlasticMetal.MobileSuit.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace HITScheduleMasterCLI
{
    /// <summary>
    /// 启动参数
    /// </summary>
    public class StartUpArgs:AutoDynamicParameter
    {
        /// <summary>
        /// 输入xls课表路径
        /// </summary>
        [Option("i")]
        public string Input { set; get; } = "";
        /// <summary>
        /// 输出ics课表路径
        /// </summary>
        [Option("o")]
        public string Output { set; get; } = "";
        /// <summary>
        /// 启用通知功能
        /// </summary>
        [Switch("n")]
        public bool EnableNotification { get; set; }
    }
}
