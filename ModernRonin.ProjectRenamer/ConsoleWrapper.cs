using System;

namespace ModernRonin.ProjectRenamer
{
    public class ConsoleWrapper : ILogger, IInput
    {
        public bool AskUser(string question)
        {
            Console.WriteLine($"{question} [Enter=Yes, any other key=No]");
            var key = Console.ReadKey();
            return key.Key == ConsoleKey.Enter;
        }

        public void Error(string msg) => Console.Error.WriteLine(msg);
        public void Info(string msg) => Console.WriteLine(msg);
    }
}