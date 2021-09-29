using System;
using static System.IO.Directory;


namespace Compiler.Extensions
{
    public static class EnvironmentExt
    {
        public static string? GetContentPath()
        {
            var currentDir = Environment.CurrentDirectory;
            var s = GetParent(currentDir)?.FullName;
            return s?[..^3];
        }
    }
}