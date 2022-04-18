using System.Collections.Generic;

using Dalamud.Configuration;

namespace Tippy
{
    /// <summary>
    /// Plugin configuration.
    /// </summary>
    public class TippyConfig : IPluginConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether tippy is enabled or not.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether tippy is locked in-place.
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether tippy will make sounds.
        /// </summary>
        public bool IsSoundEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to use classic font.
        /// </summary>
        public bool UseClassicFont { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show intro messages.
        /// </summary>
        public bool ShowIntroMessages { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to show debug window.
        /// </summary>
        public bool ShowDebugWindow { get; set; }

        /// <summary>
        /// Gets or sets a value indicating delay between tips.
        /// </summary>
        public int TipCooldown { get; set; } = 300000;

        /// <summary>
        /// Gets or sets a value indicating how long to keep tip open.
        /// </summary>
        public int TipTimeout { get; set; } = 60000;

        /// <summary>
        /// Gets or sets a value indicating how long to keep message open.
        /// </summary>
        public int MessageTimeout { get; set; } = 5000;

        /// <summary>
        /// Gets or sets list of banned tip ids.
        /// </summary>
        public List<string> BannedTipIds { get; set; } = new();

        /// <inheritdoc />
        public int Version { get; set; }
    }
}
