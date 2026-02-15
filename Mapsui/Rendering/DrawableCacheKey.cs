// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace Mapsui.Rendering;

/// <summary>
/// Composite key for caching drawables, combining feature and style generation IDs.
/// When either the feature or style is modified (calling Modified()), the GenerationId
/// changes, causing the old cache entry to become stale and a new drawable to be created.
/// </summary>
/// <param name="FeatureGenerationId">The feature's GenerationId at the time the drawable was created.</param>
/// <param name="StyleGenerationId">The style's GenerationId at the time the drawable was created.</param>
public readonly record struct DrawableCacheKey(long FeatureGenerationId, long StyleGenerationId);
