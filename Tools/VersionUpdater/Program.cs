using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CommandLine;

namespace VersionUpdater
{
    public class Options
    {
        [Option('v', Required = true, HelpText = "Specifies the version to set in semver format")]
        public string Version { get; set; }
    }

    static class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o =>
                {
                    var version = Version.Parse(o.Version);

                    Console.WriteLine($"{nameof(version.FullVersion)}: {version.FullVersion}");
                    Console.WriteLine($"{nameof(version.Major)}:       {version.Major}");
                    Console.WriteLine($"{nameof(version.Minor)}:       {version.Minor}");
                    Console.WriteLine($"{nameof(version.Patch)}:       {version.Patch}");
                    Console.WriteLine($"{nameof(version.PreRelease)}:  {version.PreRelease}");

                    UpdateAssemblyInfoFiles(version);
                    UpdateVersionXmlNodeInFile(version, "Directory.Build.props");
                })
                .WithNotParsed(HandleParseError);
        }

        private static void UpdateVersionXmlNodeInFile(Version version, string file)
        {
            var text = File.ReadAllText(file);
            var assemblyVersionRegex = new Regex("<Version>(.*?)</Version>");
            text = assemblyVersionRegex.Replace(text, $"<Version>{version.FullVersion}</Version>");
            Encoding utf8WithBom = new UTF8Encoding(true);
            File.WriteAllText(file, text, utf8WithBom);
        }

        private static IEnumerable<string> GetAssemblyInfoFiles()
        {
            foreach (string file in Directory.EnumerateFiles(
                Environment.CurrentDirectory, "AssemblyInfo.cs", SearchOption.AllDirectories))
            {
                yield return file;
            }
        }

        private static void UpdateAssemblyInfoFiles(Version version)
        {
            var files = GetAssemblyInfoFiles().ToList();

            foreach (var file in files)
            {
                var text = File.ReadAllText(file);
                var assemblyVersionRegex = new Regex("AssemblyVersion[(](.*?)?[)]");
                text = assemblyVersionRegex.Replace(text, $"AssemblyVersion(\"{version.Major}.{version.Minor}.{version.Patch}\")");
                var assemblyFileVersionRegex = new Regex("AssemblyFileVersion[(](.*?)?[)]");
                text = assemblyFileVersionRegex.Replace(text, $"AssemblyFileVersion(\"{version.Major}.{version.Minor}.{version.Patch}\")");
                var assemblyInformationalVersionRegex = new Regex("AssemblyInformationalVersion[(](.*?)?[)]");
                text = assemblyInformationalVersionRegex.Replace(text, $"AssemblyInformationalVersion(\"{version.Major}.{version.Minor}.{version.Patch}{version.PreRelease}\")");
                Encoding utf8WithBom = new UTF8Encoding(true);
                File.WriteAllText(file, text, utf8WithBom);
            }
        }

        private static void HandleParseError(IEnumerable<Error> errors)
        {
            Console.WriteLine("Parse error");

            foreach (var error in errors)
            {
                Console.WriteLine(error.Tag);
            }
        }
    }
}
