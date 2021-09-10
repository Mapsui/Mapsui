using System;

namespace VersionUpdater
{
    public class Version
    {
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Patch { get; set; }
        public int Build { get; set; }
        public string PreRelease { get; set; }
        public string FullVersion { get; set; }

        public static Version Parse(string version)
        {
            string preRelease = "";
            var startPreRelease = version.Trim().IndexOfAny(new[] { '-', ' ' });

            if (startPreRelease > 0)
            {
                preRelease = version.Substring(startPreRelease);
                version = version.Substring(0, startPreRelease);
            }

            var elements = version.Split('.');

            if (elements.Length > 4) throw new Exception("Version can only have 4 or less elements");

            return new Version
            {
                Major = int.Parse(elements[0]),
                Minor = elements.Length > 1 ? int.Parse(elements[1]) : 0,
                Patch = elements.Length > 2 ? int.Parse(elements[2]) : 0,
                Build = elements.Length > 3 ? int.Parse(elements[3]) : 0,
                PreRelease = preRelease
            };
        }
    }
}
