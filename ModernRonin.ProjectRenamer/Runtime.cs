using System;
using static ModernRonin.ProjectRenamer.Executor;

namespace ModernRonin.ProjectRenamer
{
    public static class Runtime
    {
        public static void Abort() => Environment.Exit(-1);

        public static void Error(string msg, bool doResetGit = false)
        {
            Console.Error.WriteLine(msg);
            if (doResetGit)
            {
                Console.Error.WriteLine("...running git reset to undo any changes...");
                RollbackGit();
            }

            Abort();
        }

        public static void RollbackGit() => Git("reset --hard HEAD", () => { });
    }
}