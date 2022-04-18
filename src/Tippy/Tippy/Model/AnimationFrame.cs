using Newtonsoft.Json;

namespace Tippy;

/// <summary>
/// Animation frame.
/// </summary>
public class AnimationFrame
{
    /// <summary>
    /// Gets or sets image coordinates in sprite sheet.
    /// </summary>
    [JsonProperty("images")]
    public int[] Images { get; set; } = null!;

    /// <summary>
    /// Gets or sets duration to show current frame.
    /// </summary>
    [JsonProperty("duration")]
    public int Duration { get; set; }

    /// <summary>
    /// Gets or sets sound to play for current frame.
    /// </summary>
    [JsonProperty("sound")]
    public int Sound { get; set; }
}
