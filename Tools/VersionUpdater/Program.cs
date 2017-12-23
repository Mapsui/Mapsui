using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CmdLine;

namespace VersionUpdater
{
    static class Program
    {
        static void Main(string[] args)
        {
            var arguments = VersionUpdaterArguments.Parse();
        }
    }
}
