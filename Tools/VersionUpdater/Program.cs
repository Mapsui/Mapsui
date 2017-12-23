using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CmdLine;

namespace VersionUpdater
{
    static class Program
    {
        static void Main(string[] args)
        {
            var arguments = VersionUpdaterArguments.Parse();
            Console.WriteLine($"{nameof(arguments.Major)}  {arguments.Major}");
            Console.WriteLine($"{nameof(arguments.Minor)}  {arguments.Minor}");
            Console.WriteLine($"{nameof(arguments.Patch)} {arguments.Patch}");
            Console.WriteLine($"{nameof(arguments.Prerelease)} {arguments.Prerelease}");

            var files = GetFiles();
            UpdateFiles(arguments, files);
        }

        public static IEnumerable<string> GetFiles()
        {
            foreach (string file in Directory.EnumerateFiles(
                ".", "*.csproj", SearchOption.AllDirectories))
            {
                yield return file;
            }
        }

        public static void UpdateFiles(VersionUpdaterArguments arguments, IEnumerable<string> files)
        {
            var regex = new Regex("(AssemblyVersion|AssemblyFileVersionAttribute|AssemblyFileVersion)\\(&quot;(.*?)?&quot;\\)");
            foreach (var file in files)
            {
                string text = File.ReadAllText(file);
                text = regex.Replace(text, $"{arguments.Major}.{arguments.Minor}.{arguments.Patch}");
                File.WriteAllText(file, text);
            }
        }
    }
}
