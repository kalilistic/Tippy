using Dalamud.Configuration;
#pragma warning disable CS1591

namespace Tippy
{
    public class TippyConfig : IPluginConfiguration
    {
        public bool Enabled = true;
        public int Version { get; set; }
    }
}
