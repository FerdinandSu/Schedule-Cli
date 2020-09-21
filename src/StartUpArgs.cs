using PlasticMetal.MobileSuit.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace HitRefresh.Schedule
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
        [Switch("-enable-notification")]
        public bool EnableNotification { get; set; } = false;
        /// <summary>
        /// 关闭周数
        /// </summary>
        [Switch("-disable-week-index")]
        public bool DisableWeekIndex { get; set; } = false;
        /// <summary>
        /// 通知时间
        /// </summary>
        [WithDefault]
        [SuitParser(typeof(Parsers),nameof(Parsers.ParseInt))]
        [Option("t")]
        public int NotificationTime { get; set; } = 25;
    }
}
