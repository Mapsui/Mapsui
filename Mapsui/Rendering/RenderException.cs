// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Morten Nielsen (www.iter.dk) as part of SharpMap

using System;

namespace Mapsui.Rendering;

/// <summary>
/// Exception thrown when a layer rendering fails
/// </summary>
public class RenderException : Exception
{
    /// <summary>
    /// Exception thrown when layer rendering has failed
    /// </summary>
    /// <param name="message"></param>
    public RenderException(string message) : base(message)
    {
    }

    /// <summary>
    /// Exception thrown when layer rendering has failed
    /// </summary>
    /// <param name="message"></param>
    /// <param name="inner"></param>
    public RenderException(string message, Exception inner) : base(message, inner)
    {
    }
}
