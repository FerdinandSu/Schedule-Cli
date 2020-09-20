using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using PlasticMetal.MobileSuit;
using PlasticMetal.MobileSuit.ObjectModel;
using PlasticMetal.MobileSuit.Parsing;
using static Newtonsoft.Json.JsonConvert;
using PlasticMetal.MobileSuit.ObjectModel.Future;
using System.Globalization;
using HCGStudio.HitScheduleMaster;
using static HCGStudio.HitScheduleMaster.ScheduleStatic;
using Ical.Net.Serialization;

namespace HitScheduleMaster
{
    /// <summary>
    /// 核心类
    /// </summary>
    [SuitInfo("HIT-Schedule-Master")]
    public class Program : CommandLineApplication<StartUpArgs>
    {
        static void Main(string[] args)
        {
            Suit.GetBuilder()
                .UsePrompt<PowerLineThemedPromptServer>()
                .UseLog(ScheduleStatic.Logger)
                .Build<Program>()
                .Run(args);


        }
        private Schedule Schedule { get; set; } = new Schedule();
        /// <summary>
        /// 构造函数
        /// </summary>
        public Program()
        {
            Text = "HIT-Schedule-Master";
        }
        /// <summary>
        /// 向课表添加一个课程
        /// </summary>
        [SuitInfo("向课表添加一个课程条目:add <课程名称>")]
        public TraceBack Add(string courseName)
        {
            if (!Schedule.Entries.Contains(courseName))
            {
                Schedule.Entries.Add(new CourseEntry(courseName));
            }
            if (!int.TryParse(IO.ReadLine("输入星期(0-6,0表示周日)"), out var week)
            || week < 0 || week > 6) return TraceBack.InvalidCommand;
            for (int i = 0; i < 6; i++)
            {
                IO.WriteLine(Suit.CreateContentArray(
                    ($"{i}.", ConsoleColor.Yellow),
                    (((CourseTime)i).ToFriendlyName(), null)));
            }
            if (!int.TryParse(IO.ReadLine("输入节次(0-5)"), out var startTime) || startTime < 0 || startTime > 5)
                return TraceBack.InvalidCommand;
            var isLong = IO.ReadLine("这是两节连上的课吗？(N|else)", "n")?.ToLower(CultureInfo.CurrentCulture) == "n";
            var isLab = IO.ReadLine("这是实验课课吗？(N|else)", "n")?.ToLower(CultureInfo.CurrentCulture) == "n";

            var weekExpression = IO.ReadLine("输入教师-周数-教室表达式(正则：教师[周数|起始-截至[单|双]?](|[周数|起始-截至[单|双]?])*教室, 如张三[10]|李四[1|2-9单]正心11)", true);

            Schedule.Entries[courseName].SubEntries.Add(new CourseSubEntry((DayOfWeek)week, (CourseTime)startTime, isLong, isLab, weekExpression));
            return TraceBack.AllOk;
        }
        /// <summary>
        /// 向课表导入一个JSON描述的课程
        /// </summary>
        /// <param name="path"></param>
        [SuitAlias("ImpCs")]
        [SuitInfo("向课表导入一个JSON描述的课程: ImpCs <.json>")]
        public void ImportCourse(string path)
        {
            if (!File.Exists(path))
            {
                IO.WriteLine("未找到文件。", OutputType.Error);
                return;
            }
            Schedule.Entries.Add(DeserializeObject<CourseEntry>(File.ReadAllText(path)));
        }
        /// <summary>
        /// 从课表导出一个JSON描述的课程
        /// </summary>
        /// <param name="courseName"></param>
        /// <param name="path"></param>
        [SuitAlias("ExpCs")]
        [SuitInfo("从课表导出一个JSON描述的课程：ExpCs <课程名> <.json>")]
        public void ExportCourse(string courseName, string path = "")
        {
            if (Schedule is null) return;
            if (path == "")
                path = IO.ReadLine("输入保存文件位置") ?? "";
            try
            {
                File.WriteAllText(path, SerializeObject(Schedule.Entries[courseName]));
            }
            catch
            {
                IO.WriteLine("写入错误。", OutputType.Error);
                //Environment.Exit(0);
            }
        }
        /// <summary>
        /// 从课表移除一个课程(或其子条目)
        /// </summary>
        /// <param name="course"></param>
        /// <param name="subId"></param>
        [SuitAlias("Rm")]
        [SuitInfo("从课表移除一个课程(或其子条目)：Remove <课程名>[ <子条目从0序号>]")]
        public void Remove(string course,
            [SuitParser(typeof(Parsers), nameof(Parsers.ParseInt))] int subId = -1)
        {
            if (Schedule is null) return;
            if (subId == -1)
            {
                Schedule.Entries.Remove(course);
            }
            else
            {
                Schedule.Entries[course].SubEntries.RemoveAt(subId);
            }
        }
        /// <summary>
        /// 编辑课表中的课程(子条目)
        /// </summary>
        /// <param name="course"></param>
        /// <param name="subId"></param>
        /// <returns></returns>
        [SuitAlias("Ed")]
        [SuitInfo("编辑课表中的课程(子条目)：Edit <课程名>[ <子条目从0序号>]")]
        public TraceBack Edit(string course,
            [SuitParser(typeof(Parsers), nameof(Parsers.ParseInt))] int subId = -1)
        {
            if (Schedule is null) return TraceBack.InvalidCommand;
            if (subId == -1)
            {
                Schedule.Entries[course].CourseName
                    = IO.ReadLine($"输入课程名称={Schedule.Entries[course].CourseName}", Schedule.Entries[course].CourseName,
                    null);
                return TraceBack.AllOk;
            }
            else
            {
                var targetSub = Schedule.Entries[course].SubEntries[subId];
                if (!int.TryParse(IO.ReadLine($"输入星期(0-6,0表示周日)={(int)targetSub.DayOfWeek}",
                    ((int)targetSub.DayOfWeek).ToString()), out var week)
            || week < 0 || week > 6) return TraceBack.InvalidCommand;
                for (int i = 0; i < 6; i++)
                {
                    IO.WriteLine(Suit.CreateContentArray(
                        ($"{i}.", ConsoleColor.Yellow),
                        (((CourseTime)i).ToFriendlyName(), null)));
                }
                if (!int.TryParse(IO.ReadLine($"输入节次(0-5)=${(int)targetSub.CourseTime}",
                    ((int)targetSub.CourseTime).ToString()), out var startTime) || startTime < 0 || startTime > 5)
                    return TraceBack.InvalidCommand;
                var isLong = IO.ReadLine("这是两节连上的课吗？(N|else)", "n")?.ToLower(CultureInfo.CurrentCulture) == "n";
                var isLab = IO.ReadLine("这是实验课课吗？(N|else)", "n")?.ToLower(CultureInfo.CurrentCulture) == "n";

                var weekExpression = IO.ReadLine("输入教师-周数-教室表达式(正则：教师[周数|起始-截至[单|双]?](|[周数|起始-截至[单|双]?])*教室, 如张三[10]|李四[1|2-9单]正心11)", true);
                Schedule.Entries[course].SubEntries.Remove(targetSub);
                Schedule.Entries[course].SubEntries.Add(new CourseSubEntry((DayOfWeek)week, (CourseTime)startTime, isLong, isLab, weekExpression));
                return TraceBack.AllOk;
            }

        }
        /// <summary>
        /// 导出整张课表
        /// </summary>
        /// <param name="path"></param>
        [SuitAlias("Ex")]
        [SuitInfo("导出整张课表：Export <.ics>")]
        public void Export(string path = "")
        {
            ScheduleCheck();
            if (Schedule is null) return;
            if (path == "")
                path = IO.ReadLine("输入保存文件位置")??"";
            try
            {
                var calendar = Schedule.ToCalendar();
                //calendar.Name = IO.ReadLine($"输入课表名称(默认:{calendar.Name})", calendar.Name, null);
                File.WriteAllText(path,
                    new CalendarSerializer().SerializeToString(calendar),
                    new UTF8Encoding(false));
            }
            catch
            {
                IO.WriteLine("写入错误。", OutputType.Error);
                Environment.Exit(0);
            }
        }
        [SuitInfo("显示整张课表：Export <.ics>")]
        public void Show()
        {
            ScheduleCheck();
            if (Schedule is null) return;
            var maxWeek = (from e in Schedule select e.MaxWeek).Max();
            var outList = new List<(string, ConsoleColor?)>
            {
                ("编号",null),
                ("\t", null),
                ("课程名".PadRight(22,' '), null),
                ("\t", null),
                ("星期", null),
                ("\t\t", null),
                ("起始时间", null),
                ("\t", null),
                ("课程长度",null),
                ("\t", null),
                ("授课教师", null),
                ("\t", null),
                ("课程地点".PadRight(8,' '), null),
                ("\t", null)

            };
            for (var i = 1; i <= maxWeek; i++)
            {
                outList.Add(($"{i} ".PadLeft(4, '0'), null));
            }

            IO.WriteLine(outList, OutputType.ListTitle);
            for (var i = 0; i < Schedule.Count; i++)
            {
                ShowScheduleEntry(i, maxWeek, Schedule[i]);
            }
        }
        [SuitAlias("Ld")]
        [SuitInfo("从xls导入整个课表：LoadXls <.xls>")]
        public void LoadXls(string path)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if (!File.Exists(path))
            {
                IO.WriteLine("未找到文件。", OutputType.Error);
                return;
            }

            using var fs = File.OpenRead(path);
            Schedule = Schedule.LoadFromXlsStream(fs);
        }
        [SuitInfo("从json导入整个课表：LoadJson <.json>")]
        public void LoadJson(string path)
        {
            if (!File.Exists(path))
            {
                IO.WriteLine("未找到文件。", OutputType.Error);
                return;
            }
            Schedule = DeserializeObject<Schedule>(File.ReadAllText(path));
        }
        [SuitInfo("创建新课表")]
        public void New()
        {
            if (!(int.TryParse(
                    IO.ReadLine("输入年份", "1", null), out var year)
                && year >= 2020 && year <= 2021))
            {
                IO.WriteLine("无效输入。", OutputType.Error);
                return;
            }
            IO.WriteLine("选择学期：", OutputType.ListTitle);
            IO.AppendWriteLinePrefix();
            IO.WriteLine("0. 春(默认)");
            IO.WriteLine("1. 夏");
            IO.WriteLine("2. 秋");
            IO.SubtractWriteLinePrefix();
            if (!int.TryParse(
                    IO.ReadLine("", "0", null), out var s) || s > 2 || s < 0)
            {
                IO.WriteLine("无效输入。", OutputType.Error);
                return;
            }

            Schedule = new Schedule(year, (Semester)s);
        }
        [SuitAlias("Dl")]
        [SuitInfo("打开浏览器下载课表")]
        public void Download()
        {
            Process.Start(new ProcessStartInfo("http://jwts-hit-edu-cn.ivpn.hit.edu.cn/") { UseShellExecute = true });
        }

        [SuitInfo("初始化课表")]
        public void Init()
        {
            IO.WriteLine("课表尚未初始化，您可以：", OutputType.ListTitle);
            IO.AppendWriteLinePrefix();
            //IO.WriteLine("0. 自动导入(默认)");
            IO.WriteLine("1. 手动导入XLS(默认)");
            IO.WriteLine("2. 手动导入JSON");
            IO.WriteLine("3. 创建新的课表");
            IO.WriteLine("其它. 退出");
            IO.SubtractWriteLinePrefix();
            if (int.TryParse(IO.ReadLine("选择", "1", null), out var o))
            {
                switch (o)
                {
                    //case 0:
                    //    Auto();
                    //    break;
                    case 1:
                        LoadXls(IO.ReadLine("Xls位置"));
                        break;
                    case 2:
                        LoadJson(IO.ReadLine("Json位置"));
                        break;
                    case 3:
                        New();
                        break;
                    default:
                        Environment.Exit(0);
                        break;
                }
            }
            else
            {
                Environment.Exit(0);
            }

        }
        [SuitInfo("保存整张课表到Json：Save <.json>")]
        public void Save(string path = "")
        {
            ScheduleCheck();
            if (Schedule is null) return;
            if (path == "")
                path = IO.ReadLine("输入保存JSON文件位置");
            try
            {
                File.WriteAllText(path, SerializeObject(Schedule));
            }
            catch
            {
                IO.WriteLine("写入错误。", OutputType.Error);
                Environment.Exit(0);
            }
        }
        private void ScheduleCheck()
        {
            if (Schedule is null) Init();
        }
        private void ShowScheduleEntry(int index, int maxWeek, ScheduleEntry entry)
        {
            var outList = new List<(string, ConsoleColor?)>
            {
                (index.ToString(),null),
                ("\t", null),
                (entry.CourseName.PadRight(22,' '), null),
                ("\t", null),
                (entry.DayOfWeekName, null),
                ("\t\t", null),
                (entry.StartTime.ToString(), null),
                ("\t", null),
                (entry.Length.ToString(),null),
                ("\t", null),
                (entry.Teacher, null),
                ("\t\t", null),
                (entry.Location.PadRight(8,' '), null),
                ("\t", null)

            };
            var week = 1;
            for (var i = entry.Week >> 1; week <= maxWeek; i >>= 1, week++)
            {

                if ((i & 1) == 1)
                {
                    outList.Add((" 1  ", ConsoleColor.Green));
                }
                else
                {
                    outList.Add((" 0  ", ConsoleColor.Red));
                }
            }

            IO.WriteLine(outList);
        }

        public override void SuitShowUsage()
        {
            IO.WriteLine("Usage: \n\tHitScheduleMaster\n\tHitScheduleMaster -i <.xls> -o <.ics>");
        }

        public override int SuitStartUp(StartUpArgs arg)
        {
            try
            {
                LoadXls(arg.Input);
                Export(arg.Output);
                return 0;
            }
            catch (Exception e)
            {
                IO.WriteLine(e.Message);
                return -1;
            }

        }
    }
}
