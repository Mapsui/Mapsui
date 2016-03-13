// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of Mapsui.
// Mapsui is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Mapsui is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with Mapsui; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mapsui.Utilities
{
    /// <summary>
    /// Provider helper utilities
    /// </summary>
    public class Providers
    {
        /// <summary>
        /// Returns a list of available data providers in this assembly
        /// </summary>
        public static IEnumerable<TypeInfo> GetProviders()
        {
            Assembly assembly = typeof(Providers).GetTypeInfo().Assembly;
            var typeInfos = assembly.DefinedTypes.Where(m => m.IsInterface && m.Name == nameof(Mapsui.Providers.IProvider));
            foreach (var typeInfo in typeInfos)
            {
                if (typeInfo.ImplementedInterfaces.Any(i => i == typeof (Mapsui.Providers.IProvider)))
                    yield return typeInfo;
            }
        }
    }
}