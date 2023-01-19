// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Mapsui.UI;

namespace Mapsui.Samples.Common;

public interface ISampleTest
{
    Task InitializeTestAsync(IMapControl mapControl);
}
