using CmdLine;
using System;

namespace VersionUpdater
{
    [CommandLineArguments(Program = "VersionUpdater")]
    class VersionUpdaterArguments
    {
        public int Major { get; private set; }
        public int Minor { get; private set; }
        public int Patch { get; private set; }
        public string Prerelease { get; private set; }

        [CommandLineParameter(Command = "version", Default = "", Description = "The version number to set",
            Required = true)]
        public string Version { get; set; }

        private (int, int, int, string) ParseVersion(string version)
        {
            var parts = version.Trim().Split(new[] {'-', ' '});

            if (parts.Length > 2) throw new Exception("Only one dash or one space is allowed.");

            var elements = parts[0].Split('.');

            if (elements.Length > 4) throw new Exception("Version can only have 4 or less elements");
            
            return (
                int.Parse(elements[0]),
                elements.Length > 1 ? int.Parse(elements[1]) : 0,
                elements.Length > 2 ? int.Parse(elements[2]) : 0,
                elements.Length > 3 ? elements[3] : "");
        }

        public void ParseVersion()
        {
            (Major, Minor, Patch, Prerelease) = ParseVersion(Version);
        }


        public static VersionUpdaterArguments Parse()
        {
            VersionUpdaterArguments args = null;
            try
            {
                args = CommandLine.Parse<VersionUpdaterArguments>();
                args.ParseVersion();

            }
            catch (CommandLineException e)
            {
                Console.WriteLine(e.ArgumentHelp.Message);
                Console.WriteLine(e.ArgumentHelp.GetHelpText(Console.BufferWidth));
                Environment.Exit(1);
            }
            return args;
        }
    }
}