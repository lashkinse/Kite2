// Одна строка журнала — одна запись: файл дописывается и закрывается на
// каждый вызов, чтобы «убитый по поводку» ребёнок всё равно оставил после
// себя хронику вплоть до последнего начатого триплета.
using System;
using System.IO;
using System.Text;

namespace Kite2.Probe
{
    internal sealed class LineLog
    {
        private readonly string _path;

        public LineLog(string path)
        {
            _path = path;
        }

        public string Path => _path;

        public void Info(string format, params object[] args)
        {
            string line = args != null && args.Length > 0 ? string.Format(format, args) : format;
            Write(line);
        }

        private void Write(string line)
        {
            try
            {
                File.AppendAllText(
                    _path,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "|"
                        + line.TrimEnd() + Environment.NewLine,
                    new UTF8Encoding(false));
            }
            catch
            {
                // Ничего не делаем: журнал не должен ломать сам эксперимент.
            }
        }
    }
}
