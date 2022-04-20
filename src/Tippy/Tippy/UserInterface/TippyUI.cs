using System;
using System.Numerics;

using CheapLoc;
using Dalamud.DrunkenToad;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;
using ImGuiScene;

namespace Tippy;

/// <summary>
/// Tippy character.
/// </summary>
public class TippyUI
{
    /// <summary>
    /// Config window.
    /// </summary>
    public readonly ConfigWindow ConfigWindow;

    private readonly TippyPlugin plugin;
    private readonly TextureWrap tippyTexture;
    private readonly TextureWrap bubbleTexture;
    private readonly string[] debugAnimationNames;

    private Vector2 windowSize;
    private Vector2 contentPos;
    private Vector2 bubbleSize;
    private Vector2 bubbleOffset;
    private Vector2 bubbleSpeechOffset;
    private Vector2 buttonSize;
    private Vector2 bubbleButtonOffset;
    private Vector2 tippyOffset;
    private int debugSelectedAnimationIndex;
    private string debugMessageText = string.Empty;
    private string debugTipText = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="TippyUI"/> class.
    /// </summary>
    /// <param name="plugin">plugin.</param>
    public TippyUI(TippyPlugin plugin)
    {
        this.plugin = plugin;
        this.ConfigWindow = new ConfigWindow();
        this.debugAnimationNames = Enum.GetNames(typeof(AnimationType));
        var spriteTexturePath = plugin.GetResourcePath("map.png");
        this.tippyTexture = TippyPlugin.PluginInterface.UiBuilder.LoadImage(spriteTexturePath);
        var bubbleTexturePath = plugin.GetResourcePath("bubble.png");
        this.bubbleTexture = TippyPlugin.PluginInterface.UiBuilder.LoadImage(bubbleTexturePath);
        TippyPlugin.PluginInterface.UiBuilder.BuildFonts += this.OnBuildFonts;
        TippyPlugin.PluginInterface.UiBuilder.RebuildFonts();
        TippyPlugin.PluginInterface.UiBuilder.Draw += this.Draw;
        TippyPlugin.PluginInterface.UiBuilder.OpenConfigUi += this.OnOpenConfigUi;
    }

    private static ImFontPtr MicrosoftSansSerifFont { get; set; }

    private static ImFontPtr MSSansSerifFont { get; set; }

    /// <summary>
    /// Get window flags.
    /// </summary>
    /// <returns>window flags.</returns>
    public static ImGuiWindowFlags GetWindowFlags()
    {
        var imGuiWindowFlags = ImGuiWindowFlags.NoTitleBar |
                               ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoFocusOnAppearing |
                               ImGuiWindowFlags.NoBringToFrontOnFocus;
        if (TippyPlugin.Config.IsLocked)
        {
            imGuiWindowFlags |= ImGuiWindowFlags.NoMove;
        }

        return imGuiWindowFlags;
    }

    /// <summary>
    /// Get button (window) flags.
    /// </summary>
    /// <returns>window flags.</returns>
    public static ImGuiWindowFlags GetButtonFlags()
    {
        return ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoResize |
               ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoScrollbar |
               ImGuiWindowFlags.NoScrollWithMouse;
    }

    /// <summary>
    /// Dispose.
    /// </summary>
    public void Dispose()
    {
        TippyPlugin.PluginInterface.UiBuilder.BuildFonts -= this.OnBuildFonts;
        TippyPlugin.PluginInterface.UiBuilder.Draw -= this.Draw;
        TippyPlugin.PluginInterface.UiBuilder.OpenConfigUi -= this.OnOpenConfigUi;
        this.tippyTexture.Dispose();
        this.bubbleTexture.Dispose();
    }

    private void OnBuildFonts()
    {
        var microsoftSansSerifFontPath = this.plugin.GetResourcePath("micross.ttf");
        MicrosoftSansSerifFont = ImGui.GetIO().Fonts.AddFontFromFileTTF(microsoftSansSerifFontPath, 14.0f);
        var msSansSerifFontPath = this.plugin.GetResourcePath("mssansserif.ttf");
        MSSansSerifFont = ImGui.GetIO().Fonts.AddFontFromFileTTF(msSansSerifFontPath, 14.0f);
    }

    private void Draw()
    {
        try
        {
            this.ConfigWindow.Draw();
            if (!TippyPlugin.Config.IsEnabled) return;
            this.CalcUI();
            this.StartContainer();
            if (TippyPlugin.TippyController.ShouldShowMessage())
            {
                this.DrawBubble();
                this.DrawBubbleButton();
            }

            this.DrawTippy();
            this.EndContainer();
            this.DrawDebug();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Fools exception OnDraw caught.");
        }
    }

    private void CalcUI()
    {
        // adjust some values for dalamud scaling
        var bubbleHeight = 155;
        var bubbleSpeechOffsetVal = 10;
        if (ImGuiHelpers.GlobalScale is > 1 and < 2)
        {
            bubbleHeight += 10;
            bubbleSpeechOffsetVal -= 5;
        }

        // set size / pos
        this.windowSize = ImGuiHelpers.ScaledVector2(220, 280);
        this.contentPos = ImGuiHelpers.ScaledVector2(120, 105);
        this.bubbleSize = ImGuiHelpers.ScaledVector2(200, bubbleHeight);
        this.bubbleOffset = ImGuiHelpers.ScaledVector2(85, bubbleHeight);
        this.bubbleSpeechOffset = ImGuiHelpers.ScaledVector2(bubbleSpeechOffsetVal, bubbleSpeechOffsetVal);
        this.bubbleButtonOffset = ImGuiHelpers.ScaledVector2(64, -50);
        this.buttonSize = ImGuiHelpers.ScaledVector2(40, 22);
        this.tippyOffset = ImGuiHelpers.ScaledVector2(20, 0);
    }

    private void StartContainer()
    {
        ImGui.SetNextWindowSize(this.windowSize, ImGuiCond.Always);
        ImGui.SetNextWindowPos(ImGui.GetIO().DisplaySize - this.windowSize, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, Vector4.Zero);
        ImGui.PushStyleColor(ImGuiCol.Border, Vector4.Zero);
        ImGui.PushStyleColor(ImGuiCol.PopupBg, ImGuiColors.DalamudViolet);
        ImGui.Begin("###TippyWindow", GetWindowFlags());
        ImGui.PushFont(TippyPlugin.Config.UseClassicFont ? MSSansSerifFont : MicrosoftSansSerifFont);
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0, 0, 0, 1));
    }

    private void EndContainer()
    {
        ImGui.End();
        ImGui.PopStyleColor(4);
        ImGui.PopFont();
    }

    private void DrawBubble()
    {
        ImGui.SetCursorPos(ImGui.GetWindowSize() - this.contentPos - this.bubbleOffset);
        ImGui.Image(this.bubbleTexture.ImGuiHandle, this.bubbleSize);
        ImGui.SetCursorPos(ImGui.GetWindowSize() - this.contentPos - this.bubbleOffset + this.bubbleSpeechOffset);
        ImGui.TextUnformatted(TippyPlugin.TippyController.CurrentMessage.Text);
    }

    private void DrawBubbleButton()
    {
        ImGui.SetCursorPos(ImGui.GetWindowSize() - this.contentPos + this.bubbleButtonOffset);
        ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, ImGuiColors.DalamudGrey3);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ImGuiColors.DalamudGrey);
        ImGui.PushStyleColor(ImGuiCol.Border, ImGuiColors.DalamudGrey3);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1.2f);

        if (ImGui.Button("OK", this.buttonSize))
        {
            TippyPlugin.TippyController.CloseMessage();
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(4);
    }

    private void DrawTippy()
    {
        ImGui.BeginGroup();
        ImGui.SetCursorPos(ImGui.GetWindowSize() - this.contentPos - this.tippyOffset);
        var frameSpec = TippyPlugin.TippyController.GetTippyFrame();
        ImGui.Image(this.tippyTexture.ImGuiHandle, frameSpec.size, frameSpec.uv0, frameSpec.uv1);
        ImGui.EndGroup();
        this.DrawTippyMenu();
    }

    private void DrawTippyMenu()
    {
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            ImGui.OpenPopup("###Tippy_Tippy_Popup");
        }

        if (ImGui.BeginPopup("###Tippy_Tippy_Popup"))
        {
            if (ImGui.MenuItem(Loc.Localize("###Tippy_ShowNewTip_MenuItem", "Show new tip")))
            {
                TippyPlugin.TippyController.GetMessageNow();
            }

            if (TippyPlugin.TippyController.CanBlockTip())
            {
                if (ImGui.MenuItem(Loc.Localize("###Tippy_BlockTip_MenuItem", "Don't show tip again")))
                {
                    TippyPlugin.TippyController.BlockTip();
                }
            }

            if (!this.ConfigWindow.IsVisible)
            {
                if (ImGui.MenuItem(Loc.Localize("###Tippy_OpenSettings_MenuItem", "Open settings")))
                {
                    this.OnOpenConfigUi();
                }
            }

            ImGui.EndPopup();
        }
    }

    private void DrawDebug()
    {
        if (TippyPlugin.Config.ShowDebugWindow)
        {
            ImGui.SetNextWindowSize(ImGuiHelpers.ScaledVector2(260, 350), ImGuiCond.Appearing);
            ImGui.Begin("Tippy Debug Window");
            ImGui.Text($"State: {TippyPlugin.TippyController.TippyState}");
            ImGui.Text($"JobId: {TippyPlugin.JobId}");
            ImGui.Text($"RoleId: {TippyPlugin.RoleId}");
            ImGui.Text($"Tip Queue: {TippyPlugin.TippyController.TipQueue.Count}");
            ImGui.Text($"Msg Queue: {TippyPlugin.TippyController.MessageQueue.Count}");
            ImGui.Text($"Anim Queue: {TippyPlugin.TippyController.AnimationQueue.Count}");
            ImGui.Text($"Last Tip: {TippyPlugin.TippyController.LastMessageFinished}");
            ImGui.Text($"Animation: {TippyPlugin.TippyController.CurrentAnimationType}");
            ImGui.Text($"Frame Index: {TippyPlugin.TippyController.CurrentFrameIndex}");
            ImGui.Text($"Elapsed Time: {TippyPlugin.TippyController.MessageTimer.ElapsedMilliseconds / 1000} s");
            ImGuiHelpers.ScaledDummy(5f);
            ImGui.SetNextItemWidth(150f * ImGuiHelpers.GlobalScale);
            ImGui.Combo("####Animation", ref this.debugSelectedAnimationIndex, this.debugAnimationNames, this.debugAnimationNames.Length);
            ImGui.SameLine();
            if (ImGui.SmallButton("Add"))
            {
                TippyPlugin.TippyController.DebugMessage((AnimationType)this.debugSelectedAnimationIndex);
            }

            ImGui.InputTextWithHint("###SendMessage", "Message Text", ref this.debugMessageText, 200);
            ImGui.SameLine();
            if (ImGui.SmallButton("Send###Message"))
            {
                TippyPlugin.TippyController.CloseMessage();
                TippyPlugin.TippyController.AddMessage(this.debugMessageText, MessageSource.Debug);
                this.debugMessageText = string.Empty;
            }

            ImGui.InputTextWithHint("###TipText", "Tip Text", ref this.debugTipText, 200);
            ImGui.SameLine();
            if (ImGui.SmallButton("Send###Tip"))
            {
                TippyPlugin.TippyController.AddTip(this.debugTipText, MessageSource.Debug);
                this.debugTipText = string.Empty;
            }

            ImGui.End();
        }
    }

    private void OnOpenConfigUi()
    {
        this.ConfigWindow.IsVisible ^= true;
    }
}
