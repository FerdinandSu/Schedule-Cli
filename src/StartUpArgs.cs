using PlasticMetal.MobileSuit.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace HitScheduleMaster
{
    /// <summary>
    /// 启动参数
    /// </summary>
    public class StartUpArgs:AutoDynamicParameter
    {
        /// <summary>
        /// 输入文件
        /// </summary>
        [Option("i")]
        public string Input { set; get; } = "";
        /// <summary>
        /// 输出文件
        /// </summary>
        [Option("o")]
        public string Output { set; get; } = "";
        /// <summary>
        /// 启用通知
        /// </summary>
        [Switch("n")]
        public bool EnableNotification { get; set; } = false;
    }
}
