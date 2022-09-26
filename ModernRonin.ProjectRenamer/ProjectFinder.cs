﻿using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Construction;

namespace ModernRonin.ProjectRenamer;

/// <summary>
///     Unfortunately, untestable because SolutionFile is not public constructable
/// </summary>
public sealed class ProjectFinder : IProjectFinder
{
    public Project FindProject(string solutionPath, string projectName)
    {
        var solution = SolutionFile.Parse(solutionPath);
        var project = solution.ProjectsInOrder.FirstOrDefault(matchesProject);
        if (project is null) return null;

        var projectPath = project.AbsolutePath.NormalizePath();
        var solutionFolder = project.ParentProjectGuid is null
            ? null
            : solutionFolderPath(solution.ProjectsByGuid[project.ParentProjectGuid]);
        return new Project(projectPath, solutionFolder, Path.GetExtension(project.AbsolutePath));

        string solutionFolderPath(ProjectInSolution p)
        {
            if (p.ParentProjectGuid == null) return p.ProjectName;
            var parent = solution.ProjectsByGuid[p.ParentProjectGuid];
            var parentPath = solutionFolderPath(parent);
            return $"{parentPath}/{p.ProjectName}";
        }

        bool matchesProject(ProjectInSolution p) =>
            p.ProjectName.EndsWith(projectName,
                StringComparison.InvariantCultureIgnoreCase);
    }
}