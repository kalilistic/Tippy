using System.Numerics;

namespace Tippy;

/// <summary>
/// Frame spec for animation.
/// </summary>
public class TippyFrame
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TippyFrame"/> class.
    /// </summary>
    /// <param name="size">size.</param>
    /// <param name="uv0">uv0.</param>
    /// <param name="uv1">uv1.</param>
    /// <param name="sound">sound number.</param>
    public TippyFrame(Vector2 size, Vector2 uv0, Vector2 uv1, int sound = 0)
    {
        this.size = size;
        this.uv0 = uv0;
        this.uv1 = uv1;
        this.sound = sound;
    }

    /// <summary>
    /// Gets or sets size.
    /// </summary>
    public Vector2 size { get; set; }

    /// <summary>
    /// Gets or sets uv0.
    /// </summary>
    public Vector2 uv0 { get; set; }

    /// <summary>
    /// Gets or sets uv1.
    /// </summary>
    public Vector2 uv1 { get; set; }

    /// <summary>
    /// Gets or sets sound number.
    /// </summary>
    public int sound { get; set; }
}
