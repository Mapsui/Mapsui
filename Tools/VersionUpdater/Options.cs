using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CmdLine;

namespace VersionUpdater
{

    [CommandLineArguments(Program = "VersionUpdater")]
    class VersionUpdaterArguments
    {
        private string _version;

        public int Major { get; private set; }
        public int Minor { get; private set; }
        public int Patch { get; private set; }
        public string PrereleaseString { get; private set; }

        [CommandLineParameter(Command = "version", Default = "", Description = "The version number to set",
            Required = true)]
        public string Version

        {
            get => _version;
            set
            {

                _version = value;

            }
        }

        private (int, int, int, string) ParseVersion(string version)
        {
            var elements = version.Split('.');
            
            if (elements.Length > 4) throw new Exception("Version can only have 4 or less elements");
            
            return (
                int.Parse(elements[0]),
                elements.Length > 1 ? int.Parse(elements[1]) : 0,
                elements.Length > 2 ? int.Parse(elements[2]) : 0,
                elements.Length > 3 ? elements[3] : "");
        }

        public void ParseVersion()
        {
            (Major, Minor, Patch, PrereleaseString) = ParseVersion(_version);
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