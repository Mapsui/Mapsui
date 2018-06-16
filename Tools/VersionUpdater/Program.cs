using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace VersionUpdater
{
    static class Program
    {
        static void Main()
        {
            var arguments = VersionUpdaterArguments.Parse();
            Console.WriteLine($"{nameof(arguments.Major)}  {arguments.Major}");
            Console.WriteLine($"{nameof(arguments.Minor)}  {arguments.Minor}");
            Console.WriteLine($"{nameof(arguments.Patch)} {arguments.Patch}");
            Console.WriteLine($"{nameof(arguments.Prerelease)} {arguments.Prerelease}");

            var files = GetAssemblyInfoFiles().ToList();
            UpdateAssemblyInfoFiles(arguments, files);
            UpdateCommonPropsFile(arguments, "Mapsui.common.props");
        }

        private static void UpdateCommonPropsFile(VersionUpdaterArguments arguments, string file)
        {
            var text = File.ReadAllText(file);
            var assemblyVersionRegex = new Regex("<Version>(.*?)</Version>");
            text = assemblyVersionRegex.Replace(text, $"<Version>{arguments.Version}</Version>");
            Encoding utf8WithBom = new UTF8Encoding(true);
            File.WriteAllText(file, text, utf8WithBom);
        }

        public static IEnumerable<string> GetAssemblyInfoFiles()
        {
            foreach (string file in Directory.EnumerateFiles(
                Environment.CurrentDirectory, "AssemblyInfo.cs", SearchOption.AllDirectories))
            {
                yield return file;
            }
        }

        public static void UpdateAssemblyInfoFiles(VersionUpdaterArguments arguments, IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                var text = File.ReadAllText(file);
                var assemblyVersionRegex = new Regex("AssemblyVersion[(](.*?)?[)]");
                text = assemblyVersionRegex.Replace(text, $"AssemblyVersion(\"{arguments.Major}.{arguments.Minor}.{arguments.Patch}\")");
                var assemblyFileVersionRegex = new Regex("AssemblyFileVersion[(](.*?)?[)]");
                text = assemblyFileVersionRegex.Replace(text, $"AssemblyFileVersion(\"{arguments.Major}.{arguments.Minor}.{arguments.Patch}\")");
                var assemblyInformationalVersionRegex = new Regex("AssemblyInformationalVersion[(](.*?)?[)]");
                text = assemblyInformationalVersionRegex.Replace(text, $"AssemblyInformationalVersion(\"{arguments.Major}.{arguments.Minor}.{arguments.Patch}{arguments.Prerelease}\")");
                Encoding utf8WithBom = new UTF8Encoding(true);
                File.WriteAllText(file, text, utf8WithBom);
            }
        }
    }
}
