using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace ModernRonin.ProjectRenamer;

public sealed record FileSystemPath
{
    readonly ImmutableArray<string> _segments;

    public FileSystemPath(string path) : this(path.Split(new[]
    {
        Path.DirectorySeparatorChar,
        Path.AltDirectorySeparatorChar
    }, StringSplitOptions.RemoveEmptyEntries)) { }

    FileSystemPath(IEnumerable<string> segments) => _segments = segments.ToImmutableArray();

    public override string ToString()
    {
        var separator = IsWindowsSpecific ? Path.DirectorySeparatorChar : Path.AltDirectorySeparatorChar;
        return string.Join(separator, _segments);
    }

    /// <summary>
    ///     Necessary to support indexing.
    /// </summary>

    public FileSystemPath this[int index] => new(ImmutableArray<string>.Empty.Add(_segments[index]));

    public FileSystemPath Append(FileSystemPath other) => new(_segments.AddRange(other._segments));

    /// <summary>
    ///     Necessary to support ranges.
    /// </summary>
    public FileSystemPath Slice(int start, int length)
    {
        var range = _segments.RemoveRange(start + length, _segments.Length - start - length)
            .RemoveRange(0, start);
        return new FileSystemPath(range);
    }

    public FileSystemPath WithExtension(string extension)
    {
        if (!extension.StartsWith(".")) extension = $".{extension}";
        var last = _segments[^1];
        var replacement = replaceExtension(last, extension);

        return new FileSystemPath(_segments.SetItem(_segments.Length - 1, replacement));

        string replaceExtension(string s, string newExtension)
        {
            if (s.EndsWith(newExtension)) return s;
            var i = s.LastIndexOf('.');
            return i < 0 ? s + newExtension : s[..i] + newExtension;
        }
    }

    public bool ContainsDirectory => _segments.Length > 1;
    public static FileSystemPath CurrentDirectory => new(Path.GetFullPath(Directory.GetCurrentDirectory()));
    public static FileSystemPath Empty { get; } = new(Enumerable.Empty<string>());

    public bool HasExtension => _segments[^1].Contains(".");
    public bool IsEmpty => _segments.Length == 0;

    public bool IsWindowsSpecific => _segments.First().Contains(Path.VolumeSeparatorChar);

    /// <summary>
    ///     Necessary to support indexing.
    /// </summary>
    public int Length => _segments.Length;

    public FileSystemPath Parent => ContainsDirectory ? this[..^1] : Empty;
}