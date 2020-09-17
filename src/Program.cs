using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Ical.Net.Serialization;
using PlasticMetal.MobileSuit;
using HCGStudio.HITScheduleMasterCore;
using PlasticMetal.MobileSuit.ObjectModel;

using static Newtonsoft.Json.JsonConvert;
using PlasticMetal.MobileSuit.ObjectModel.Future;
using PlasticMetal.MobileSuit.Parsing;

namespace HITScheduleMasterCLI
{
    /// <summary>
    /// 核心驱动类
    /// </summary>
    [SuitInfo("HIT-Schedule-Master")]
    public class Program : CommandLineApplication<StartUpArgs>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Suit.GetBuilder()
                .UsePrompt<PowerLineThemedPromptServer>()
                //.UseLog(ILogger.OfDirectory("D:\\"))
                .Build<Program>()
                .Run(args);


        }
        private Schedule Schedule { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Program()
        {
            Schedule = new Schedule();
            Text = "HIT-Schedule-Master";
        }
        /// <summary>
        /// 向课表添加一个课程
        /// </summary>
        [SuitInfo("向课表添加一个课程")]
        public void Add()
        {
            var name = IO.ReadLine("输入课程名称");
            if (!int.TryParse(IO.ReadLine("输入星期(0-6,0表示周日)"), out var week)
            || week < 0 || week > 6) goto WrongInput;
            var teacher = IO.ReadLine("输入教师");
            var location = IO.ReadLine("输入地点");
            if (!TimeSpan.TryParse(IO.ReadLine("输入开始时间(hh:mm:ss)"), out var startTime))
                goto WrongInput;
            if (!TimeSpan.TryParse(IO.ReadLine("输入持续时间(hh:mm:ss)"), out var length))
                goto WrongInput;

            var weekExpression = IO.ReadLine("输入周数(周数/起始-截至[单/双/(无)])");
            Schedule.AddScheduleEntry(new ScheduleEntry((DayOfWeek)week, default,
                    name, $"{teacher}[{weekExpression}]@{location}")
            { StartTime = startTime, Length = length });
            return;
        WrongInput:
            IO.WriteLine("非法输入。", OutputType.Error);

        }
        /// <summary>
        /// 向课表导入一个JSON描述的课程
        /// </summary>
        /// <param name="path"></param>
        [SuitAlias("ImC")]
        [SuitInfo("向课表导入一个JSON描述的课程: ImpCse <.json>")]
        public void ImportCourse(string path)
        {
            if (!File.Exists(path))
            {
                IO.WriteLine("未找到文件。", OutputType.Error);
                return;
            }
            Schedule.AddScheduleEntry(DeserializeObject<ScheduleEntry>(File.ReadAllText(path)));
        }
        /// <summary>
        /// 从课表导出一个JSON描述的课程
        /// </summary>
        /// <param name="id"></param>
        /// <param name="path"></param>
        [SuitInfo("从课表导出一个JSON描述的课程：ExpCse <ID>[ <.json>]")]
        [SuitAlias("ExC")]
        public void ExportCourse([SuitParser(typeof(Parsers), nameof(Parsers.ParseInt))]int id, string path = "")
        {
            if (Schedule is null) return;

            if ( id < 0 || id >= Schedule.Count) return;
            if (path == "")
                path = IO.ReadLine("输入保存文件位置")??"";
            try
            {
                File.WriteAllText(path, SerializeObject(Schedule[id]));
            }
            catch
            {
                IO.WriteLine("写入错误。", OutputType.Error);
                Environment.Exit(0);
            }
        }
        /// <summary>
        /// 从课表移除一个课程
        /// </summary>
        /// <param name="id"></param>
        [SuitAlias("Rm")]
        [SuitInfo("从课表移除一个课程：Remove <ID>")]
        public void Remove([SuitParser(typeof(Parsers), nameof(Parsers.ParseInt))]int id)
        {
            if (Schedule is null) return;
            if (id >= 0 && id < Schedule.Count)
            {
                Schedule.RemoveAt(id);
            }
        }
        /// <summary>
        /// 禁用指定课程/所有课程通知
        /// </summary>
        /// <param name="id"></param>
        [SuitAlias("DsNtf")]
        [SuitInfo("启用指定课程/所有课程通知：EnNtf[ <ID>]")]
        public void DisableNotification(
            [SuitParser(typeof(Parsers),nameof(Parsers.ParseInt))]
            int id=-1)
        {
            if (Schedule is null || id < -1 || id >= Schedule.Count)
            {
                IO.WriteLine("课程id不合条件。");
                return;
            }
            if (id < 0)
            {
                Schedule.EnableNotification = false;
            }
            else
            {
                Schedule[id].EnableNotification = false;

            }
        }
        /// <summary>
        /// 启用指定课程/所有课程通知
        /// </summary>
        /// <param name="id"></param>
        [SuitAlias("EnNtf")]
        [SuitInfo("启用指定课程/所有课程通知：EnNtf[ <ID>]")]
        public void EnableNotification(
            [SuitParser(typeof(Parsers),nameof(Parsers.ParseInt))]
            int id=-1)
        {
            if (Schedule is null || id < -1 || id >= Schedule.Count)
            {
                IO.WriteLine("课程id不合条件。");
                return;
            }
            if (id < 0)
            {
                Schedule.EnableNotification = true;
            }
            else
            {
                Schedule[id].EnableNotification = true;

            }
        }
        /// <summary>
        /// 编辑课表中的课程
        /// </summary>
        /// <param name="id"></param>
        [SuitAlias("Ed")]
        [SuitInfo("编辑课表中的课程：Edit <ID>")]
        public void Edit([SuitParser(typeof(Parsers), nameof(Parsers.ParseInt))] int id)
        {
            if (Schedule is null) return;
            if (id >= 0 && id < Schedule.Count)
            {
                var course = Schedule[id];
                course.CourseName = IO.ReadLine($"输入课程名称={course.CourseName}", course.CourseName,
                    null);
                if (!int.TryParse(IO.ReadLine($"输入星期(0-6,0表示周日)={((int)course.DayOfWeek)}",
                            ((int)course.DayOfWeek).ToString(), null)
                    , out var week)
                    || week < 0 || week > 6) goto WrongInput;
                course.DayOfWeek = (DayOfWeek)week;
                course.Teacher = IO.ReadLine($"输入教师={course.Teacher}", course.Teacher, null);
                course.Location = IO.ReadLine($"输入地点={course.Location}", course.Location, null);
                if (!TimeSpan.TryParse(IO.ReadLine($"输入开始时间(hh:mm:ss)={course.StartTime}",
                    course.StartTime.ToString(), null), out var startTime))
                    goto WrongInput;
                course.StartTime = startTime;
                if (!TimeSpan.TryParse(IO.ReadLine($"输入持续时间(hh:mm:ss)={course.Length}",
                    course.Length.ToString(), null), out var length))
                    goto WrongInput;
                course.Length = length;
                var weekExpression = IO.ReadLine(
                    $"输入周数(周数/起始-截至[单/双/(无)]，空格隔开)={course.WeekExpression}", course.WeekExpression, null);
                if (weekExpression != course.WeekExpression)
                {
                    course.WeekExpression = weekExpression;
                }
                return;

            }
        WrongInput:
            IO.WriteLine("非法输入。", OutputType.Error);
        }
        /// <summary>
        /// 
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
                var calendar = Schedule.GetCalendar();
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
        /// <summary>
        /// 
        /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        [SuitAlias("Lj")]
        [SuitInfo("从json导入整个课表：LoadJson <.json>")]
        public void LoadJson(string? path)
        {
            if (!File.Exists(path??""))
            {
                IO.WriteLine("未找到文件。", OutputType.Error);
                return;
            }
            Schedule = DeserializeObject<Schedule>(File.ReadAllText(path));
        }
        /// <summary>
        /// 
        /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
        [SuitAlias("Dl")]
        [SuitInfo("打开浏览器下载课表")]
        public void Download()
        {
            Process.Start(new ProcessStartInfo("http://jwts-hit-edu-cn.ivpn.hit.edu.cn/") { UseShellExecute = true });
        }
        /// <summary>
        /// 
        /// </summary>
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
                        LoadXls(IO.ReadLine("Xls位置")??"");
                        break;
                    case 2:
                        LoadJson(IO.ReadLine("Json位置"??""));
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        [SuitInfo("保存整张课表到Json：Save <.json>")]
        public void Save(string path = "")
        {
            ScheduleCheck();
            if (Schedule is null) return;
            if (path == "")
                path = IO.ReadLine("输入保存JSON文件位置")??"";
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
        ///<inheritdoc/>
        public override void SuitShowUsage()
        {
            IO.WriteLine("Usage: \n\tHITScheduleMasterCLI\n\tHITScheduleMasterCLI -i <教务处课表.xls> -o <输出.ics>[ -n(启用通知)]");
        }
        ///<inheritdoc/>
        public override int SuitStartUp(StartUpArgs arg)
        {
            try
            {
                LoadXls(arg.Input);
                if (arg.EnableNotification)
                {
                    EnableNotification(-1);
                }
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
