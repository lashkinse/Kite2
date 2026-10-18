using System;
using System.Reflection;

namespace Kite2.Tests
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("Проверки Kite2");
            Console.WriteLine();

            return TestRunner.Run(Assembly.GetExecutingAssembly());
        }
    }
}
