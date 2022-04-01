using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Numerics;
using Dalamud.Game.ClientState;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using ImGuiNET;
using ImGuiScene;

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable MemberCanBeMadeStatic.Local
#pragma warning disable CS0414, CS1591

namespace Tippy
{
    public class TippyPlugin : IDalamudPlugin
    {
        public bool IsEnabled;
        private bool isTippyDrawing;
        public readonly Stopwatch tippyLogicTimer = new ();
        private readonly Stopwatch tippyFrameTimer = new ();
        private string tippyText = string.Empty;
        private TippyState tippyState = TippyState.BeforeIntro;
        private bool showTippyButton;
        private TextureWrap welcomeTex;
        private readonly TextureWrap tippySpriteSheet;
        private TextureWrap bubbleTex;
        private SoundPlayer sounds = new ();
        private string resourceDir;
        private int startTippyFrame;
        private int endTippyFrame;
        private int currentTippyFrame;
        private bool tippyAnimUp = true;
        private bool tippyAnimDone;
        private bool tippyLoopAnim;
        private Random rand = new ();
        private Queue<string> lastTips = new ();
        private long currentFrameTime;
        private long minFrameTime = 150;
        private const float BubbleScale = 1;
        private const float TippyScale = 1;

        private readonly int tippySpritesheetW = 27; // amount in row + 1
        private readonly int tippySpritesheetH = 27;
        private readonly Vector2 tippySingleSize = new Vector2(124, 93);
        private Vector2 ToSpriteSheetScale(Vector2 input) => new Vector2(input.X / this.tippySpriteSheet.Width, input.Y / this.tippySpriteSheet.Height);

        public static ImFontPtr FoolsFont { get; private set; }
        private int FramesInTippyAnimation => this.endTippyFrame - this.startTippyFrame;
        
        public TippyPlugin()
        {
            resourceDir = Path.Combine(PluginInterface.AssemblyLocation.DirectoryName!, "Resource");
            this.welcomeTex = PluginInterface.UiBuilder.LoadImage(Path.Combine(resourceDir, "welcome.png"));
            this.tippySpriteSheet = PluginInterface.UiBuilder.LoadImage(Path.Combine(resourceDir, "map.png"));
            this.bubbleTex = PluginInterface.UiBuilder.LoadImage(Path.Combine(resourceDir, "bubble.png"));

            this.tippyFrameTimer.Start();
            PluginInterface.UiBuilder.BuildFonts += this.OnBuildFonts;
            PluginInterface.UiBuilder.Draw += this.Draw;
            
            this.isTippyDrawing = true;
            this.IsEnabled = true;
            this.tippyLogicTimer.Restart();
        }

        private void OnBuildFonts()
        {
            var fontPathFools = Path.Combine(this.resourceDir, "MS Sans Serif.ttf");
            FoolsFont = ImGui.GetIO().Fonts.AddFontFromFileTTF(fontPathFools, 14.0f);
        }

        public string Name => "Tippy";
        
        [PluginService]
        public static ClientState ClientState { get; private set; } = null!;
        
        [PluginService]
        public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
        
        public TippyConfig Configuration { get; set; } = new ();

        public void Dispose()
        {
            PluginInterface.UiBuilder.BuildFonts -= this.OnBuildFonts;
            PluginInterface.UiBuilder.Draw -= this.Draw;
            this.welcomeTex.Dispose();
            this.tippySpriteSheet.Dispose();
            this.bubbleTex.Dispose();
        }

        public void Draw()
        {
            try {
                if (this.isTippyDrawing)
                    DrawTippy();
            } catch (Exception ex) {
                PluginLog.LogError(ex, "Fools exception OnDraw caught");
            }
        }

        public void DrawTippy()
        {
            if (this.tippyState == TippyState.BeforeIntro && this.tippyLogicTimer.ElapsedMilliseconds > 8000) {
                PlayTada();
                SetTippyAnim(TippyAnimState.GetAttention, true);
                this.tippyText = Tips.Intro;
                this.showTippyButton = true;
                this.tippyState = TippyState.Intro;
            }
            
            switch (tippyState) {
                case TippyState.Tips:
                    if (this.tippyLogicTimer.ElapsedMilliseconds > 300000 && string.IsNullOrEmpty(this.tippyText)) // New tip every 5 minutes
                        SetNewTip();
                    break;
                case TippyState.Timeout:
                    if (this.tippyLogicTimer.ElapsedMilliseconds > 60000) {
                        SetTippyAnim(TippyAnimState.Idle, true);
                        this.tippyText = string.Empty;
                        this.showTippyButton = false;

                        this.tippyLogicTimer.Restart();
                        this.tippyState = TippyState.Tips;
                    }
                    break;
            }
            
            var displaySize = ImGui.GetIO().DisplaySize;

            var tippyPos = new Vector2(displaySize.X - 400, displaySize.Y - 350);

            ImGui.SetNextWindowPos(tippyPos, ImGuiCond.Always);
            ImGui.SetNextWindowSize(new Vector2(500, 500), ImGuiCond.Always);

            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0, 0, 0, 0));
            
            ImGui.Begin("###TippyWindow", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoMouseInputs | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoBringToFrontOnFocus);

            ImGui.PushFont(FoolsFont);
            
            if (!string.IsNullOrEmpty(this.tippyText))
            {
                DrawTextBox(this.tippyText);
            }

            ImGui.SameLine();

            ImGui.SetCursorPosX(230);
            ImGui.SetCursorPosY(18 + 55);

            DrawTippyAnim();

            ImGui.End();
            
            
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.6f, 0.6f, 0.6f, 1f));

            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0, 0, 0, 1));
            
            if (this.showTippyButton) {
                ImGui.SetNextWindowPos(tippyPos + new Vector2(117, 117), ImGuiCond.Always);
                ImGui.SetNextWindowSize(new Vector2(95, 40), ImGuiCond.Always);
                ImGui.Begin("###TippyButton", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

                if (ImGui.Button("OK!", new Vector2(80, 20)))
                {
                    switch (tippyState) {
                        case TippyState.Intro:
                            this.showTippyButton = false;
                            this.tippyText = string.Empty;
                            SetTippyAnim(TippyAnimState.Idle, true);

                            this.tippyState = TippyState.Tips;
                            this.tippyLogicTimer.Restart();
                            break;
                        case TippyState.Tips:
                            SetTippyAnim(TippyAnimState.Idle, true);
                            this.tippyText = string.Empty;
                            this.showTippyButton = false;

                            this.tippyLogicTimer.Restart();
                            break;

                        case TippyState.Parse:
                            Process.Start("https://na.finalfantasyxiv.com/jobguide/pvp/");

                            this.showTippyButton = false;
                            this.tippyText = string.Empty;
                            SetTippyAnim(TippyAnimState.Idle, true);

                            this.tippyState = TippyState.Tips;
                            this.tippyLogicTimer.Restart();
                            break;
                    }
                }

                ImGui.End();
            }

            ImGui.PopStyleColor();

            ImGui.PopFont();

            ImGui.PopStyleColor();

            ImGui.PopStyleColor();
        }

        private void DrawTextBox(string text) {

            var beforeBubbleCursor = ImGui.GetCursorPos();
            ImGui.Image(this.bubbleTex.ImGuiHandle, new Vector2(this.bubbleTex.Width, this.bubbleTex.Height) * ImGui.GetIO().FontGlobalScale * BubbleScale);
            var afterBubbleCursor = ImGui.GetCursorPos();

            ImGui.SetCursorPos(beforeBubbleCursor + new Vector2(10, 10));
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0, 0, 0, 1));
            ImGui.TextUnformatted(text);
            ImGui.PopStyleColor();

            ImGui.SetCursorPos(afterBubbleCursor);
        }
        
        private void DrawTippyAnim() {
            var frameCoords = GetTippyTexCoords(this.startTippyFrame + this.currentTippyFrame);
            var botRight = ToSpriteSheetScale(frameCoords + this.tippySingleSize);

            if (this.currentTippyFrame > FramesInTippyAnimation - 2) {
                this.tippyAnimDone = true;
                if (!this.tippyLoopAnim)
                    return;
                this.tippyAnimUp = false;
            }

            if (this.currentTippyFrame == 0) {
                this.tippyAnimUp = true;
            }

            ImGui.Image(this.tippySpriteSheet.ImGuiHandle, this.tippySingleSize * ImGui.GetIO().FontGlobalScale * TippyScale, ToSpriteSheetScale(frameCoords), botRight);

            this.currentFrameTime += this.tippyFrameTimer.ElapsedMilliseconds;
            this.tippyFrameTimer.Restart();

            if (this.currentFrameTime >= this.minFrameTime) {
                if (this.tippyAnimUp)
                    this.currentTippyFrame++;
                else
                    this.currentTippyFrame--;

                this.currentFrameTime -= this.minFrameTime;

                if (this.currentFrameTime >= this.minFrameTime)
                    this.currentFrameTime = 0;
            }
        }
        
        private Vector2 GetTippyTexCoords(int spriteIndex) {
            var w = spriteIndex % this.tippySpritesheetW;
            var h = spriteIndex / this.tippySpritesheetH;

            return new Vector2(this.tippySingleSize.X * w, this.tippySingleSize.Y * h);
        }
        
        private void SetNewTip()
        {
            var anim = this.rand.Next(0, 3);
            switch (anim) {
                case 0: SetTippyAnim(TippyAnimState.PointLeft, true);
                    break;
                case 1: SetTippyAnim(TippyAnimState.GetAttention2, true);
                    break;
                case 2: SetTippyAnim(TippyAnimState.GetAttention, true);
                    break;
            }
            
            var generalTip = GetGeneralTip();

            var choice = this.rand.Next(0, 28);


            PluginLog.Information($"Choice: {choice}");
            
            if (choice == 1 || choice == 2) {
                this.tippyState = TippyState.Timeout;
                this.tippyText = "Analyzing ERP logs...";
                SetTippyAnim(TippyAnimState.Reading, true);
                this.showTippyButton = false;

                this.tippyLogicTimer.Restart();

                PlayChord();
            }
            else if (choice == 3 || choice == 5) {
                this.tippyState = TippyState.Timeout;
                this.tippyText = "I'm always watching.";
                SetTippyAnim(TippyAnimState.CheckingYouOut, true);
                this.showTippyButton = false;

                this.tippyLogicTimer.Restart();

                PlayChord();
            }
            else if (choice == 4 || choice == 6)
            {
                this.tippyState = TippyState.Parse;
                this.tippyText = "It seems like you are parsing grey.\n\nDo you want me to help you play your\njob better?";
                SetTippyAnim(TippyAnimState.Reading, true);
                this.showTippyButton = true;

                this.tippyLogicTimer.Restart();

                PlayChord();
            }
            else
            {
                this.tippyText = generalTip;

                PlayTada();
                this.showTippyButton = true;
            }
        }
        
        private string GetGeneralTip()
        {
            var lp = ClientState.LocalPlayer;

            var classJob = 0u;
            if (lp != null)
                classJob = lp.ClassJob.Id;

            var tAry = Tips.GeneralTips;

            if (Tips.jobTipDict.TryGetValue(classJob, out var ccTips))
                tAry = tAry.Concat(ccTips).Concat(ccTips).ToArray(); // Concat job tips twice so they have a greater chance of being seen

            var index = this.rand.Next(0, tAry.Length);
            var tip = tAry[index];

            while (this.lastTips.Any(x => x == tip)) {
                index = this.rand.Next(0, tAry.Length);
                tip = tAry[index];
            }

            this.lastTips.Enqueue(tip);

            if (this.lastTips.Count > 5)
                this.lastTips.Dequeue();

            return tip;
        }
        
        private void PlayTada() {
            this.sounds.SoundLocation = Path.Combine(this.resourceDir, "tada.wav");
            this.sounds.Play();
        }
        
        private void PlayChord()
        {
            this.sounds.SoundLocation = Path.Combine(this.resourceDir, "chord.wav");
            this.sounds.Play();
        }
        
        private void SetTippyAnim(TippyAnimState anim, bool loop) {
            var animData = TippyAnimData.tippyAnimDatas[anim];

            this.startTippyFrame = animData.Start;
            this.endTippyFrame = animData.Stop;

            this.currentTippyFrame = 0;
            this.tippyAnimUp = true;
            this.tippyLoopAnim = loop;
        }
    }
}
