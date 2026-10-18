using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Kite2.Tests
{
    public static class TestRunner
    {
        public static int Run(Assembly assembly)
        {
            MethodInfo[] tests = assembly
                .GetTypes()
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                .Where(method => method.GetCustomAttribute<TestAttribute>() != null)
                .OrderBy(method => method.DeclaringType.Name)
                .ThenBy(method => method.MetadataToken)
                .ToArray();

            var failures = new List<string>();
            var clock = Stopwatch.StartNew();

            foreach (MethodInfo test in tests)
            {
                string title = test.DeclaringType.Name + "." + test.Name;

                try
                {
                    test.Invoke(null, null);
                    Console.WriteLine("  успех  " + title);
                }
                catch (TargetInvocationException error)
                {
                    Exception cause = error.InnerException ?? error;
                    Console.WriteLine("  СБОЙ    " + title);
                    Console.WriteLine("          " + cause.Message);

                    failures.Add(title + ": " + cause.Message);
                }
            }

            clock.Stop();

            Console.WriteLine();
            Console.WriteLine($"Проверок: {tests.Length}, не прошло: {failures.Count}, время: {clock.ElapsedMilliseconds} мс");

            return failures.Count == 0 ? 0 : 1;
        }
    }
}
