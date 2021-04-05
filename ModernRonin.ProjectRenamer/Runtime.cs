using System;

namespace ModernRonin.ProjectRenamer
{
    public static class Runtime
    {
        public static void Abort() => Environment.Exit(-1);
    }
}