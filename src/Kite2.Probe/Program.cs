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
                    try
                    {
                        StackSnap.Foreign(int.Parse(args[1]), args[2]);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("снимок стеков не удался: " + ex.Message);
                        return 5;
                    }
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
            int wall = 90;
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
            report.AppendLine("версия пробника: " + Assembly.GetEntryAssembly().GetName().Version);
            report.AppendLine("время: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            report.AppendLine("источник: по умолчанию (или по фильтру «" + filter + "»); окно скрытое; разрешение драйвера");
            report.AppendLine("поводок на эксперимент: 30 с (тихим сценариям), 240 с (пачкам 21 и 22)");
            report.AppendLine();

            Console.WriteLine("Обследование TWAIN-драйвера: тихая пачка, сценарий 22.");
            Console.WriteLine("ЗАДАНИЕ: положите в податчик 3–4 листа и НЕ нажимайте ничего.");
            Console.WriteLine("Окно драйвера не откроется и кнопки не нужны: драйвер обязан начать сам.");
            Console.WriteLine("Готово — файл отчёта: " + dir + "\\REPORT.txt");
            Console.WriteLine();
            Console.WriteLine("Папка прогона: " + dir);
            Console.WriteLine("Каждый эксперимент — свой процесс с коротким поводком:");
            Console.WriteLine("если триплет зависает, ребёнок убивается (с снимком стеков), остальные идут дальше.");
            Console.WriteLine();

            string dllNote = Dsm.LoadLibraries();
            if (dllNote != null)
                Console.WriteLine("ПРЕДУПРЕЖДЕНИЕ: " + dllNote);
            report.AppendLine("twaindsm.dll около программы: " + (Dsm.NewAvailable ? "есть" : "НЕТ"));
            report.AppendLine("twain_32.dll системы: " + (Dsm.OldAvailable ? "есть" : "НЕТ"));

            // Лог DS-шима Kyocera (kcTwain03DS2.ds) пишет в C:\twsrc.log.
            // Судя по коду, файл должен существовать — заводим пустой.
            // На системах без прав записи в корень диска остается честно отметить.
            string twsrcNote;
            try
            {
                string twsrcLog = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\", "twsrc.log");
                if (!File.Exists(twsrcLog))
                    File.WriteAllText(twsrcLog, "", new UTF8Encoding(false));
                twsrcNote = "заведён/существует, будет забран в конце отчёта";
            }
            catch (Exception ex)
            {
                twsrcNote = "создать не удалось: " + ex.Message;
            }
            report.AppendLine("C:\\twsrc.log (лог шима Kyocera): " + twsrcNote);
            report.AppendLine("TWAINDSM_LOG включается в каждом ребёнке (лог DSM 2.x = expNN.dsmlog.txt).");
            report.AppendLine();

            string self = Assembly.GetEntryAssembly().Location;

            // Подсказка человеку: первый сценарий в батарее — родной диалог
            // драйвера, без нажатия «Скан» он просто отключится по поводку.
            foreach (int windowIdx in Cases.Battery)
            {
                var windowCase = Cases.Get(windowIdx);
                if (windowCase != null && windowCase.ShowUi)
                {
                    Console.WriteLine("Внимание: в эксперименте " + windowIdx
                        + " появится окно драйвера — нажмите в нём «Скан».");
                }
            }

            foreach (int idx in Cases.Battery)
            {
                var caseInfo = Cases.Get(idx);
                // 30 с хватает тихим сценариям; пачкам — подругой:
                // 21 и 22 ждут кормёжку податчика по 10–20 с на лист.
                int childWall = caseInfo.ShowUi || idx == 22 ? 240 : 30;
                Console.WriteLine("=== Эксперимент {0} (поводок {1} с): {2}", idx, childWall, caseInfo.Title);
                var logPath = Path.Combine(dir, "exp" + idx.ToString("00") + ".txt");
                var stackFile = Path.Combine(dir, "exp" + idx.ToString("00") + ".stacks.txt");

                var startTime = DateTime.Now;
                int verdict;
                bool killed;
                using (var child = new Process())
                {
                    var quoted = string.Join(" ", new[]
                    {
                        "exp", idx.ToString(), (childWall * 1000).ToString(),
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
                    // Встроенный лог 2.x DSM: env-переменная TWAINDSM_LOG.
                    // Свой файл на каждый эксперимент, чтобы не перемешивать.
                    child.StartInfo.EnvironmentVariables["TWAINDSM_LOG"] =
                        Path.Combine(dir, "exp" + idx.ToString("00") + ".dsmlog.txt");
                    child.Start();
                    // Плюс 30 с сверх поводка ребёнка: висящий нативный вызов
                    // дожимает внутренние таймауты позже внутреннего плана.
                    bool finished = child.WaitForExit(childWall * 1000 + 30000);
                    if (!finished)
                    {
                        killed = true;
                        // Сначала снимок стеков, только потом убийство: снимок и
                        // есть источник знаний о том, где именно ребёнок висит.
                        // Сбой снимка (нет ClrMD и т.п.) сам прогон не саботирует.
                        try
                        {
                            StackSnap.Foreign(child.Id, stackFile);
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                File.WriteAllText(stackFile, "снимок стеков не удался: " + ex.Message, new UTF8Encoding(false));
                            }
                            catch { }
                        }
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
                if (!killed)
                    report.AppendLine("код выхода ребёнка: " + verdict + " (0x" + unchecked((uint)verdict).ToString("X8") + ")");
                // Мини-дампы нативного падения ребёнка (если успели записаться).
                var dumps = Directory.GetFiles(dir, "exp" + idx.ToString("00") + ".crash*.dmp");
                if (dumps.Length > 0)
                {
                    report.AppendLine();
                    report.AppendLine("── мини-дампы падения (WinDbg: !analyze -v / Ghidra: демпон ок) ──");
                    foreach (var d in dumps)
                    {
                        var fi = new FileInfo(d);
                        report.AppendLine(fi.Name + " (" + fi.Length.ToString("N0") + " байт)");
                    }
                }
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
                // Хвост лога DSM 2.x — самая честная хроника: там видно, что
                // DSM узнал и что отправил приложению (включая DAT_EVENT).
                string dsmLogPath = Path.Combine(dir, "exp" + idx.ToString("00") + ".dsmlog.txt");
                if (File.Exists(dsmLogPath))
                {
                    try
                    {
                        var dsmLines = File.ReadAllLines(dsmLogPath, Encoding.UTF8);
                        report.AppendLine();
                        report.AppendLine("── хвост лога TWAINDSM (" + dsmLines.Length + " строк, целиком: exp" + idx.ToString("00") + ".dsmlog.txt) ──");
                        foreach (var line in dsmLines.Skip(Math.Max(0, dsmLines.Length - 150)))
                            report.AppendLine(line);
                    }
                    catch { }
                }
                report.AppendLine();
            }

            // Лог DS-шима Kyocera — в хвост отчёта (там вся хроника самого DS).
            try
            {
                string twsrcLog = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\", "twsrc.log");
                if (File.Exists(twsrcLog))
                {
                    var dsLines = File.ReadAllLines(twsrcLog, Encoding.GetEncoding(1251));
                    if (dsLines.Length > 0)
                    {
                        report.AppendLine();
                        report.AppendLine("━━━ Лог DS-шима C:\\twsrc.log (" + dsLines.Length + " строк) ━━━");
                        foreach (var line in dsLines.Skip(Math.Max(0, dsLines.Length - 200)))
                            report.AppendLine(line);
                    }
                }
            }
            catch { }

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
                case -1: return "УБИТ ПО ПОВОДКУ — процесс висел; где именно: снимок стеков expNN.stacks.txt";
                default:
                    // Остальное — ребёнок умер сам: код выхода Windows.
                    // 0xC0000005 (3221225477 / -1073741819) — нарушение доступа.
                    uint code = unchecked((uint)verdict);
                    if ((code & 0xC0000000u) == 0xC0000000u)
                        return "РЕБЁНОК УПАЛ САМ — сбой в коде драйвера/диспетчера (код 0x"
                            + code.ToString("X8") + ")";
                    return "РЕБЁНОК УПАЛ САМ (код выхода " + verdict + ")";
            }
        }
    }
}

