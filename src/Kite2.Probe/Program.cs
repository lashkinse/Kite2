// Диспетчер обследования. Режимы:
//   (без аргументов | run [wallSec] [фильтрИмени]) — прогнать всю батарею
//        сценариев и собрать единый файл отчёта;
//   exp <idx> <wallMs> <logPath> [фильтр] — один сценарий (запускается и
//        убивается по поводку снаружи);
//   stackdump <pid> <outPath> — снимок стеков чужого процесса.
// Консоль показывает прогресс; главное — файл отчёта в
// %APPDATA%\Kite2\probe\<штамп>\REPORT.txt (прикладываем к чату).
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Kite2.Probe
{
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            try
            {
                if (args.Length == 0 || args[0] == "run")
                    return RunAll(args);
                if (args[0] == "exp")
                    return Experiment.Run(args);
                if (args[0] == "stackdump")
                {
                    StackSnap.Foreign(int.Parse(args[1]), args[2]);
                    return 0;
                }
                Console.WriteLine("Режимы программы: run [wallSec] [фильтр] | exp <idx> <wallMs> <logPath> | stackdump <pid> <out>");
                return 4;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("невозвратный сбой: " + ex);
                return 3;
            }
        }

        private static int RunAll(string[] args)
        {
            int wall = 120;
            if (args.Length > 1)
                int.TryParse(args[1], out wall);
            if (wall < 30)
                wall = 30;
            string filter = args.Length > 2 ? args[2] : string.Empty;

            var stamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Kite2", "probe", stamp);
            Directory.CreateDirectory(dir);

            var report = new StringBuilder();
            report.AppendLine("Kite2.Probe — обследование TWAIN-драйвера");
            report.AppendLine("время: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            report.AppendLine("источник: по умолчанию (или по фильтру «" + filter + "»); окно скрытое; разрешение драйвера");
            report.AppendLine("поводок на эксперимент: " + wall + " с");
            report.AppendLine();

            Console.WriteLine("Обследование TWAIN-драйвера.");
            Console.WriteLine("Папка прогона: " + dir);
            Console.WriteLine("Каждый эксперимент — свой процесс с коротким поводком:");
            Console.WriteLine("если триплет зависает, ребёнок убивается (с снимком стеков), остальные идут дальше.");
            Console.WriteLine();

            string dllNote = Dsm.LoadLibraries();
            if (dllNote != null)
                Console.WriteLine("ПРЕДУПРЕЖДЕНИЕ: " + dllNote);
            report.AppendLine("twaindsm.dll около программы: " + (Dsm.NewAvailable ? "есть" : "НЕТ"));
            report.AppendLine("twain_32.dll системы: " + (Dsm.OldAvailable ? "есть" : "НЕТ"));
            report.AppendLine();

            string self = Assembly.GetEntryAssembly().Location;
            for (int idx = 1; idx <= Cases.Count; idx++)
            {
                var caseInfo = Cases.Get(idx);
                Console.WriteLine("=== Эксперимент {0}/{1}: {2}", idx, Cases.Count, caseInfo.Title);
                var logPath = Path.Combine(dir, "exp" + idx.ToString("00") + ".txt");
                var stackFile = Path.Combine(dir, "exp" + idx.ToString("00") + ".stacks.txt");

                var startTime = DateTime.Now;
                int verdict;
                bool killed;
                using (var child = new Process())
                {
                    var quoted = string.Join(" ", new[]
                    {
                        "exp", idx.ToString(), (wall * 1000).ToString(),
                        "\"" + logPath + "\"",
                        string.IsNullOrEmpty(filter) ? null : "\"" + filter + "\""
                    }.Where(part => part != null));
                    child.StartInfo = new ProcessStartInfo
                    {
                        FileName = self,
                        Arguments = quoted,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    child.Start();
                    bool finished = child.WaitForExit(wall * 1000);
                    if (!finished)
                    {
                        killed = true;
                        // Сначала снимок стеков, только потом убийство: снимок и
                        // есть источник знаний о том, где именно ребёнок висит.
                        StackSnap.Foreign(child.Id, stackFile);
                        try { child.Kill(); } catch { }
                        verdict = -1;
                    }
                    else
                    {
                        killed = false;
                        verdict = child.ExitCode;
                    }
                }

                var dt = DateTime.Now - startTime;
                string verdictLabel = VerdictLabel(verdict);
                Console.WriteLine("    вердикт: " + verdictLabel
                    + (killed ? "" : " (код " + verdict + ")")
                    + ", длительность " + dt.TotalSeconds.ToString("N0") + " с");

                report.AppendLine("━━━ Эксперимент " + idx + ": " + caseInfo.Title + " ━━━");
                report.AppendLine("вердикт: " + verdictLabel + "; длительность " + dt.TotalSeconds.ToString("N0") + " с");
                if (File.Exists(logPath))
                    report.AppendLine();
                if (File.Exists(logPath))
                    report.AppendLine(File.ReadAllText(logPath));
                else
                    report.AppendLine("(журнал эксперимента не записан)");
                if (File.Exists(stackFile))
                {
                    report.AppendLine();
                    report.AppendLine("── stacks (снят при повейровании) ──");
                    report.AppendLine(File.ReadAllText(stackFile));
                }
                report.AppendLine();
            }

            string reportPath = Path.Combine(dir, "REPORT.txt");
            File.WriteAllText(reportPath, report.ToString(), new UTF8Encoding(false));
            Console.WriteLine();
            Console.WriteLine("Готово. Приложите этот файл к чату:");
            Console.WriteLine("    " + reportPath);
            return 0;
        }

        private static string VerdictLabel(int verdict)
        {
            switch (verdict)
            {
                case 0: return "страница получена";
                case 1: return "передача не удалась (вернулся без страницы)";
                case 2: return "готовности нет или открытие не состоялось";
                case 4: return "нет такого сценария";
                default: return "УБИТ ПО ПОВОДКУ — висел в нативном вызове";
            }
        }
    }
}

