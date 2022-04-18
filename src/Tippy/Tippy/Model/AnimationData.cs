using System.Collections.Generic;

using Newtonsoft.Json;

namespace Tippy;

/// <summary>
/// Animation data.
/// </summary>
public class AnimationData
{
    /// <summary>
    /// Gets or sets animation type.
    /// </summary>
    [JsonProperty("type")]
    public AnimationType Type { get; set; }

    /// <summary>
    /// Gets or sets animation type name.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets list of frames for animation.
    /// </summary>
    [JsonProperty("frames")]
    public List<AnimationFrame> Frames { get; set; } = null!;
}
