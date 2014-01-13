using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    static class Extensions
    {
        public static bool IsNanOrZero(this double target)
        {
            if (double.IsNaN(target)) return true;
            return target == 0;
        }
    }
}
