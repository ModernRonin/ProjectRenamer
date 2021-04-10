using System;
using System.Collections.Generic;

namespace ModernRonin.ProjectRenamer
{
    public interface IDotnet
    {
        void AddReference(string project, string reference);
        void AddToSolution(string pathToProject, string solutionFolder);
        void AddToSolution(string pathToProject);
        void BuildSolution(Action onNonZeroExitCode);
        IEnumerable<string> GetReferencedProjects(string project);
        void PaketInstall();
        void RemoveFromSolution(string pathToProject);
        void RemoveReference(string project, string reference);
    }
}