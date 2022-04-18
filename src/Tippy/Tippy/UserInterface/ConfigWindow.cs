using System.Linq;

using CheapLoc;
using Dalamud.DrunkenToad;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace Tippy;

/// <summary>
/// Config window.
/// </summary>
public class ConfigWindow
{
    private Tab currentTab = Tab.General;
    private bool isVisible;

    private enum Tab
    {
        General,
        Timers,
        Blocked,
    }

    /// <summary>
    /// Gets or sets a value indicating whether config window is visible.
    /// </summary>
    public bool IsVisible
    {
        get => this.isVisible;
        set => this.isVisible = value;
    }

    /// <summary>
    /// Draw.
    /// </summary>
    public void Draw()
    {
        if (!this.IsVisible) return;
        ImGui.SetNextWindowSize(ImGuiHelpers.ScaledVector2(300, 300), ImGuiCond.Appearing);
        if (ImGui.Begin(Loc.Localize("###Tippy_Config_Window", "Tippy Config"), ref this.isVisible, ImGuiWindowFlags.None))
        {
            this.DrawTabs();
            switch (this.currentTab)
            {
                case Tab.General:
                {
                    this.DrawGeneral();
                    break;
                }

                case Tab.Timers:
                {
                    this.DrawTimers();
                    break;
                }

                case Tab.Blocked:
                {
                    this.DrawBlocked();
                    break;
                }

                default:
                    this.DrawGeneral();
                    break;
            }
        }
    }

    private void DrawTabs()
    {
        if (ImGui.BeginTabBar("###Tippy_Config_TabBar", ImGuiTabBarFlags.NoTooltip))
        {
            if (ImGui.BeginTabItem(Loc.Localize("###Tippy_General_Tab", "General")))
            {
                this.currentTab = Tab.General;
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem(Loc.Localize("###Tippy_Timers_Tab", "Timers")))
            {
                this.currentTab = Tab.Timers;
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem(Loc.Localize("###Tippy_Blocked_Tab", "Blocked")))
            {
                this.currentTab = Tab.Blocked;
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
            ImGui.Spacing();
        }
    }

    private void DrawGeneral()
    {
        var isEnabled = TippyPlugin.Config.IsEnabled;
        if (ImGui.Checkbox(Loc.Localize("###Tippy_IsEnabled_Checkbox", "Enable Tippy"), ref isEnabled))
        {
            TippyPlugin.Config.IsEnabled = isEnabled;
            TippyPlugin.SaveConfig();
        }

        var playSounds = TippyPlugin.Config.IsSoundEnabled;
        if (ImGui.Checkbox(Loc.Localize("###Tippy_EnableSounds_Checkbox", "Enable Sounds"), ref playSounds))
        {
            TippyPlugin.Config.IsSoundEnabled = playSounds;
            TippyPlugin.SaveConfig();
        }

        var showIntro = TippyPlugin.Config.ShowIntroMessages;
        if (ImGui.Checkbox(Loc.Localize("###Tippy_ShowIntro_Checkbox", "Show Intro"), ref showIntro))
        {
            TippyPlugin.Config.ShowIntroMessages = showIntro;
            TippyPlugin.SaveConfig();
        }

        var isLocked = TippyPlugin.Config.IsLocked;
        if (ImGui.Checkbox(Loc.Localize("###Tippy_IsLocked_Checkbox", "Is Locked"), ref isLocked))
        {
            TippyPlugin.Config.IsLocked = isLocked;
            TippyPlugin.SaveConfig();
        }

        var useClassicFont = TippyPlugin.Config.UseClassicFont;
        if (ImGui.Checkbox(Loc.Localize("###Tippy_UseClassicFont_Checkbox", "Use Classic Font"), ref useClassicFont))
        {
            TippyPlugin.Config.UseClassicFont = useClassicFont;
            TippyPlugin.SaveConfig();
        }

        var showDebugWindow = TippyPlugin.Config.ShowDebugWindow;
        if (ImGui.Checkbox(Loc.Localize("###Tippy_ShowDebugWindow_Checkbox", "Show Debug Window"), ref showDebugWindow))
        {
            TippyPlugin.Config.ShowDebugWindow = showDebugWindow;
            TippyPlugin.SaveConfig();
        }
    }

    private void DrawTimers()
    {
        ImGui.Text(Loc.Localize("###Tippy_MessageTimeout_Slider", "Message Timeout"));
        var messageTimeout = TippyPlugin.Config.MessageTimeout.FromMillisecondsToSeconds();
        if (ImGui.SliderInt("###MessageTimeout_Slider", ref messageTimeout, 1, 60))
        {
            TippyPlugin.Config.MessageTimeout = messageTimeout.FromSecondsToMilliseconds();
            TippyPlugin.SaveConfig();
        }

        ImGui.Spacing();

        ImGui.Text(Loc.Localize("###Tippy_TipTimeout_Slider", "Tip Timeout"));
        var tipTimeout = TippyPlugin.Config.TipTimeout.FromMillisecondsToSeconds();
        if (ImGui.SliderInt("###TipTimeout_Slider", ref tipTimeout, 1, 60))
        {
            TippyPlugin.Config.TipTimeout = tipTimeout.FromSecondsToMilliseconds();
            TippyPlugin.SaveConfig();
        }

        ImGui.Spacing();

        ImGui.Text(Loc.Localize("###Tippy_TipCooldown_Slider", "Tip Cooldown"));
        var tipCooldown = TippyPlugin.Config.TipCooldown.FromMillisecondsToSeconds();
        if (ImGui.SliderInt("###TipCooldown_Slider", ref tipCooldown, 0, 300))
        {
            TippyPlugin.Config.TipCooldown = tipCooldown.FromSecondsToMilliseconds();
            TippyPlugin.SaveConfig();
        }

        ImGui.Spacing();
    }

    private void DrawBlocked()
    {
        if (TippyPlugin.Config.BannedTipIds.Count > 0)
        {
            ImGui.TextColored(ImGuiColors.DalamudViolet, Loc.Localize("###Tippy_BlockedTips_Text", "Click on a tip to unblock."));
            ImGui.Spacing();
            foreach (var bannedTipId in TippyPlugin.Config.BannedTipIds.ToList())
            {
                ImGui.Text(Tips.AllTips[bannedTipId].Text);
                if (ImGui.IsItemClicked())
                {
                    TippyPlugin.Config.BannedTipIds.Remove(bannedTipId);
                    TippyPlugin.SaveConfig();
                }
            }
        }
        else
        {
            ImGui.TextColored(ImGuiColors.DalamudViolet, Loc.Localize("###Tippy_NoBlockedTips_Text", "You haven't blocked any tips!"));
        }
    }
}
