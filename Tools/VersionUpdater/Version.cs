using System;

namespace VersionUpdater
{
    public class Version
    {
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Patch { get; set; }
        public int Build { get; set; }
        public string? PreRelease { get; set; }
        public string? FullVersion { get; set; }

        public static Version Parse(string version)
        {
            var (firstPart, secondPart) = SplitOnDash(version);

            var elements = firstPart.Split('.');

            if (elements.Length > 4) throw new Exception("Version can only have 4 or less elements");

            return new Version
            {
                Major = int.Parse(elements[0]),
                Minor = elements.Length > 1 ? int.Parse(elements[1]) : 0,
                Patch = elements.Length > 2 ? int.Parse(elements[2]) : 0,
                Build = elements.Length > 3 ? int.Parse(elements[3]) : 0,
                PreRelease = secondPart,
                FullVersion = version
            };
        }

        private static (string firstPart, string secondPart) SplitOnDash(string version)
        {
            var parts = version.Split('-');
            return (parts[0], parts.Length > 1 ? "-" + parts[1] : "");
        }
    }
}
