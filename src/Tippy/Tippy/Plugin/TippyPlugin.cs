using System;
using System.IO;

using Dalamud.DrunkenToad;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;

namespace Tippy;

/// <inheritdoc />
public class TippyPlugin : IDalamudPlugin
{
    private readonly TippyUI tippyUI;
    private readonly string resourceDir;
    private readonly Localization localization;
    private readonly TippyProvider tippyProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="TippyPlugin"/> class.
    /// </summary>
    public TippyPlugin()
    {
        this.localization = new Localization(PluginInterface, CommandManager);
        this.resourceDir = Path.Combine(PluginInterface.AssemblyLocation.DirectoryName!, "Resource");
        Framework.Update += this.FrameworkOnUpdate;
        this.LoadConfig();
        TippyController = new TippyController(this);
        this.tippyUI = new TippyUI(this);
        this.tippyProvider = new TippyProvider(PluginInterface, new TippyAPI());
        CommandManager.AddHandler("/tippy", new CommandInfo(this.ToggleTippy)
        {
            HelpMessage = "Show Tippy.",
            ShowInHelp = true,
        });
        CommandManager.AddHandler("/tippyconfig", new CommandInfo(this.ToggleTippyConfig)
        {
            HelpMessage = "Show Tippy config.",
            ShowInHelp = true,
        });
    }

    /// <summary>
    /// Gets job Id.
    /// </summary>
    public static uint JobId { get; private set; }

    /// <summary>
    /// Gets role Id.
    /// </summary>
    public static uint RoleId { get; private set; }

    /// <summary>
    /// Gets tippy controller.
    /// </summary>
    public static TippyController TippyController { get; private set; } = null!;

    /// <summary>
    /// Gets tippy configuration.
    /// </summary>
    public static TippyConfig Config { get; private set; } = null!;

    /// <summary>
    /// Gets framework.
    /// </summary>
    [PluginService]
    public static Framework Framework { get; private set; } = null!;

    /// <summary>
    /// Gets command manager.
    /// </summary>
    [PluginService]
    public static CommandManager CommandManager { get; private set; } = null!;

    /// <summary>
    /// Gets client state.
    /// </summary>
    [PluginService]
    public static ClientState ClientState { get; private set; } = null!;

    /// <summary>
    /// Gets plugin interface.
    /// </summary>
    [PluginService]
    public static DalamudPluginInterface PluginInterface { get; private set; } = null!;

    /// <inheritdoc />
    public string Name => "Tippy";

    /// <summary>
    /// Save configuration.
    /// </summary>
    public static void SaveConfig()
    {
        PluginInterface.SavePluginConfig(Config);
    }

    /// <summary>
    /// Get resource path.
    /// </summary>
    /// <param name="fileName">resource to retrieve.</param>
    /// <returns>resource path.</returns>
    public string GetResourcePath(string fileName)
    {
        return Path.Combine(this.resourceDir, fileName);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        try
        {
            CommandManager.RemoveHandler("/tippy");
            CommandManager.RemoveHandler("/tippyconfig");
            Framework.Update -= this.FrameworkOnUpdate;
            this.tippyProvider.Dispose();
            this.tippyUI.Dispose();
            TippyController.Dispose();
            this.localization.Dispose();
            GC.SuppressFinalize(this);
        }
        catch (Exception ex)
        {
            PluginLog.Log(ex, "Failed to dispose plugin.");
        }
    }

    private void ToggleTippyConfig(string command, string arguments)
    {
        this.tippyUI.ConfigWindow.IsVisible ^= true;
    }

    private void ToggleTippy(string command, string arguments)
    {
        Config.IsEnabled ^= true;
        SaveConfig();
    }

    private void FrameworkOnUpdate(Framework framework)
    {
        try
        {
            if (ClientState.LocalPlayer == null || ClientState.LocalPlayer.ClassJob.GameData == null) return;
            if (ClientState.LocalPlayer.ClassJob.Id != JobId)
            {
                JobId = ClientState.LocalPlayer.ClassJob.Id;
                RoleId = ClientState.LocalPlayer.ClassJob.GameData.Role;
                TippyController.JobChanged = true;
            }
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private void LoadConfig()
    {
        try
        {
            Config = PluginInterface.GetPluginConfig() as TippyConfig ?? new TippyConfig();
        }
        catch (Exception ex)
        {
            Logger.LogError("Failed to load config so creating new one.", ex);
            Config = new TippyConfig();
            SaveConfig();
        }
    }
}
