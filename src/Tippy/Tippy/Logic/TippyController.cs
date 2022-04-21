using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;

using Dalamud.DrunkenToad;
using Dalamud.Interface;
using NAudio.Wave;
using Newtonsoft.Json;

namespace Tippy;

/// <summary>
/// Tippy animation controller.
/// </summary>
public class TippyController
{
    private readonly Vector2 spriteSize = new(124, 93);
    private readonly Vector2 sheetSize = new(3348, 3162);
    private readonly List<AnimationData> tippyDataList;
    private readonly WaveOut waveOut = new();
    private readonly Dictionary<int, byte[]> sounds = new();
    private TippyFrame current = null!;
    private bool animationIsFinished;
    private Mp3FileReader? reader;
    private Vector2 size;
    private Vector2 coords;
    private Vector2 uv0;
    private Vector2 uv1;
    private bool isSoundPlaying;
    private int framesCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="TippyController"/> class.
    /// </summary>
    /// <param name="plugin">plugin.</param>
    public TippyController(TippyPlugin plugin)
    {
        // build tips
        Tips.BuildAllTips();

        // load animations
        var json = File.ReadAllText(plugin.GetResourcePath("agent.json"));
        this.tippyDataList = JsonConvert.DeserializeObject<List<AnimationData>>(json)!;
        this.AnimationQueue.Enqueue(AnimationType.Arrive);
        for (var i = 0; i < 2; i++) this.AnimationQueue.Enqueue(AnimationType.TapScreen);

        // load messages
        this.SetupMessages(true);
        this.SetupNextMessage();

        // set sounds
        for (var i = 1; i < 16; i++)
        {
            this.sounds.Add(i, File.ReadAllBytes(plugin.GetResourcePath($"sound_{i}.mp3")));
        }

        this.FrameTimer.Start();
    }

    /// <summary>
    /// Gets or sets get current message.
    /// </summary>
    public Message CurrentMessage { get; set; } = null!;

    /// <summary>
    /// Gets or sets a value indicating whether job changed.
    /// </summary>
    public bool JobChanged { get; set; }

    /// <summary>
    /// Gets frame timer.
    /// </summary>
    public Stopwatch FrameTimer { get; private set; } = new();

    /// <summary>
    /// Gets message timer.
    /// </summary>
    public Stopwatch MessageTimer { get; private set; } = new();

    /// <summary>
    /// Gets current frame index.
    /// </summary>
    public int CurrentFrameIndex { get; private set; }

    /// <summary>
    /// Gets last message finished timestamp.
    /// </summary>
    public long LastMessageFinished { get; private set; }

    /// <summary>
    /// Gets tip queue.
    /// </summary>
    public Queue<Tip> TipQueue { get; private set; } = new();

    /// <summary>
    /// Gets IPC Tip list.
    /// </summary>
    public List<Tip> IPCTips { get; private set; } = new();

    /// <summary>
    /// Gets message queue.
    /// </summary>
    public Queue<Message> MessageQueue { get; private set; } = new();

    /// <summary>
    /// Gets animation queue.
    /// </summary>
    public Queue<AnimationType> AnimationQueue { get; private set; } = new();

    /// <summary>
    /// Gets or sets animation type.
    /// </summary>
    public AnimationType CurrentAnimationType { get; set; }

    /// <summary>
    /// Gets or sets get tippy state.
    /// </summary>
    public TippyState TippyState { get; set; } = TippyState.NotStarted;

    /// <summary>
    /// Setup tips on load or if job changes.
    /// </summary>
    /// <param name="initialLoad">indicator whether initial load.</param>
    public void SetupMessages(bool initialLoad = false)
    {
        // reset queue
        this.TipQueue.Clear();

        // add intro tips
        if (initialLoad && TippyPlugin.Config.ShowIntroMessages)
        {
            foreach (var message in Messages.IntroMessages.ToArray())
            {
                this.MessageQueue.Enqueue(message);
            }
        }

        // determine role/job codes
        IEnumerable<Tip> allTips = Tips.GeneralTips;
        if (TippyPlugin.JobId != 0)
        {
            var jobCode = (JobCode)TippyPlugin.JobId;
            var roleCode = TippyPlugin.RoleId is 2 or 3 ? RoleCode.DPS : (RoleCode)TippyPlugin.RoleId;
            allTips = allTips.Concat(Tips.RoleTips[roleCode]).Concat(Tips.JobTips[jobCode]);
        }

        // add tips from other plugins if any
        if (this.IPCTips.Count > 0)
        {
            allTips = allTips.Concat(this.IPCTips);
        }

        // create tip list
        var shuffledTips = this.ShuffleTips(allTips);
        foreach (var tip in shuffledTips)
        {
            if (!TippyPlugin.Config.BannedTipIds.Contains(tip.Id))
            {
                this.TipQueue.Enqueue(tip);
            }
        }
    }

    /// <summary>
    /// Gets indicator whether to show message.
    /// </summary>
    /// <returns>indicator whether to show message.</returns>
    public bool ShouldShowMessage()
    {
        return this.TippyState is TippyState.GivingTip or TippyState.GivingMessage;
    }

    /// <summary>
    /// Gets indicator whether can block this tip.
    /// </summary>
    /// <returns>indicator whether able to block tip..</returns>
    public bool CanBlockTip()
    {
        return this.TippyState is TippyState.GivingTip && this.CurrentMessage.Source == MessageSource.Default;
    }

    /// <summary>
    /// Get next message now.
    /// </summary>
    public void GetMessageNow()
    {
        this.CloseMessage();
        this.LastMessageFinished = 0;
    }

    /// <summary>
    /// Finish and close message.
    /// </summary>
    public void CloseMessage()
    {
        this.MessageTimer.Stop();
        this.TippyState = TippyState.Idle;
        this.LastMessageFinished = DateUtil.CurrentTime();
        this.CurrentAnimationType = AnimationType.Idle;
    }

    /// <summary>
    /// Play sound.
    /// </summary>
    /// <param name="num">sound to play.</param>
    public void PlaySound(int num)
    {
        try
        {
            if (!TippyPlugin.Config.IsSoundEnabled || this.isSoundPlaying) return;
            if (num == 0) return;
            this.isSoundPlaying = true;
            this.reader = new Mp3FileReader(new MemoryStream(this.sounds[num]));
            this.waveOut.Init(this.reader);
            this.waveOut.Play();
            this.waveOut.PlaybackStopped += this.WaveOutOnPlaybackStopped;
        }
        catch (Exception)
        {
            this.isSoundPlaying = false;
        }
    }

    /// <summary>
    /// Draw tippy animation.
    /// </summary>
    /// <returns>animation spec for imgui.</returns>
    public TippyFrame GetTippyFrame()
    {
        try
        {
            // reset tip queue if job changed
            if (this.JobChanged)
            {
                this.SetupMessages();
                this.JobChanged = false;
            }

            // check if new message in queue to show
            if (this.MessageQueue.Count > 0 && this.TippyState != TippyState.GivingMessage)
            {
                this.CloseMessage();
                this.CurrentFrameIndex = 0;
                this.SetupNextMessage();
            }

            // check if waiting for new tip
            else if (this.TippyState == TippyState.Idle &&
                      DateUtil.CurrentTime() - this.LastMessageFinished > TippyPlugin.Config.TipCooldown)
            {
                this.SetupNextMessage();
            }

            // check if timeout exceeded
            else if ((this.TippyState == TippyState.GivingMessage &&
                      this.MessageTimer.ElapsedMilliseconds > TippyPlugin.Config.MessageTimeout) || (this.TippyState == TippyState.GivingTip &&
                     this.MessageTimer.ElapsedMilliseconds > TippyPlugin.Config.TipTimeout))
            {
                this.CloseMessage();
            }

            // loop animation
            if (this.animationIsFinished) this.SetupNextAnimation(this.CurrentMessage.LoopAnimation);

            // get all frames for animation
            var frames = this.tippyDataList.First(data => data.Type == this.CurrentAnimationType).Frames;
            this.framesCount = frames.Count;

            // get current frame
            var frame = frames[this.CurrentFrameIndex];

            // if still within duration show last image
            if (this.FrameTimer.ElapsedMilliseconds < frame.Duration)
            {
                this.size = ImGuiHelpers.ScaledVector2(this.spriteSize.X, this.spriteSize.Y);
                this.current = new TippyFrame(this.size, this.uv0, this.uv1, frame.Sound);
                TippyPlugin.TippyController.PlaySound(this.current.sound);
                return this.current;
            }

            // set to final if next frame is last
            if (this.CurrentFrameIndex == frames.Count - 1)
            {
                this.CurrentFrameIndex += 1;
                this.animationIsFinished = true;
            }

            // move to next frame
            else
            {
                this.CurrentFrameIndex += 1;
                this.FrameTimer.Restart();
            }

            // update frame parameters
            this.size = ImGuiHelpers.ScaledVector2(this.spriteSize.X, this.spriteSize.Y);
            this.coords = new Vector2(frame.Images[0], frame.Images[1]);
            this.uv0 = this.ToSpriteSheetScale(this.coords);
            this.uv1 = this.ToSpriteSheetScale(this.coords + this.spriteSize);
            this.current = new TippyFrame(this.size, this.uv0, this.uv1, frame.Sound);

            // play sound
            TippyPlugin.TippyController.PlaySound(this.current.sound);

            // return animation
            return this.current;
        }
        catch (Exception)
        {
            // show previous frame in case something went wrong
            Logger.LogDebug("Failed frame at index " + this.CurrentFrameIndex + "/" + this.framesCount);
            this.CurrentFrameIndex = this.framesCount - 1;
            this.animationIsFinished = true;
            return this.current;
        }
    }

    /// <summary>
    /// Dispose animator.
    /// </summary>
    public void Dispose()
    {
        try
        {
            while (this.isSoundPlaying)
            {
                Thread.Sleep(100);
            }

            this.isSoundPlaying = true;
            this.MessageQueue.Clear();
            this.CloseMessage();
            this.FrameTimer.Stop();
            this.reader?.Dispose();
            this.waveOut.Dispose();
        }
        catch (Exception ex)
        {
            Logger.LogError("Failed to dispose tippy controller", ex);
        }
    }

    /// <summary>
    /// Add priority tip by message.
    /// </summary>
    /// <param name="message">message.</param>
    /// <param name="animationType">animation type.</param>
    public void AddMessage(string message, AnimationType animationType)
    {
        this.MessageQueue.Enqueue(new Message(message, animationType));
    }

    /// <summary>
    /// Add tip by text.
    /// </summary>
    /// <param name="text">message.</param>
    /// <param name="messageSource">message source.</param>
    /// <returns>indicator whether successful.</returns>
    public bool AddTip(string text, MessageSource messageSource)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        var tip = new Tip(text)
        {
            Source = messageSource,
        };
        TippyPlugin.TippyController.IPCTips.Add(tip);
        TippyPlugin.TippyController.SetupMessages();
        return true;
    }

    /// <summary>
    /// Add message by text.
    /// </summary>
    /// <param name="text">message.</param>
    /// <param name="messageSource">message source.</param>
    /// <returns>indicator whether successful.</returns>
    public bool AddMessage(string text, MessageSource messageSource)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        var message = new Message(text)
        {
            Source = messageSource,
        };
        this.MessageQueue.Enqueue(message);
        return true;
    }

    /// <summary>
    /// Block message.
    /// </summary>
    public void BlockTip()
    {
        if (this.TippyState == TippyState.GivingTip)
        {
            TippyPlugin.Config.BannedTipIds.Add(this.CurrentMessage.Id);
            TippyPlugin.SaveConfig();
            this.CloseMessage();
        }
    }

    /// <summary>
    /// Debug message.
    /// </summary>
    /// <param name="animationType">animation.</param>
    public void DebugMessage(AnimationType animationType)
    {
        this.MessageQueue.Clear();
        TippyPlugin.TippyController.CloseMessage();
        var msg = new Message("Hello! This is a message from Tippy. Please enjoy.", animationType)
        {
            LoopAnimation = true,
        };
        this.MessageQueue.Enqueue(msg);
    }

    private void WaveOutOnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        this.waveOut.PlaybackStopped -= this.WaveOutOnPlaybackStopped;
        this.isSoundPlaying = false;
    }

    private IEnumerable<Tip> ShuffleTips(IEnumerable<Tip> tips)
    {
        var rnd = new Random();
        return tips.OrderBy(_ => rnd.Next()).ToArray();
    }

    private void SetupNextMessage()
    {
        while (true)
        {
            if (this.MessageQueue.Count != 0)
            {
                this.MessageQueue.TryDequeue(out var message);
                if (message != null) this.SetMessage(message, TippyState.GivingMessage);
            }
            else if (this.TipQueue.Count != 0)
            {
                this.TipQueue.TryDequeue(out var tip);
                if (tip != null) this.SetMessage(tip, TippyState.GivingTip);
            }
            else
            {
                this.SetupMessages();
                continue;
            }

            break;
        }
    }

    private void SetMessage(Message message, TippyState tippyState)
    {
        message.Text = TextHelper.SanitizeText(message.Text);
        message.Text = TextHelper.WordWrap(message.Text, 30);
        this.CurrentMessage = message;
        if (this.AnimationQueue.Count > 0)
        {
            this.AnimationQueue.TryDequeue(out var animationType);
            this.CurrentAnimationType = animationType;
        }
        else
        {
            this.CurrentAnimationType = message.AnimationType ?? AnimationType.Idle;
        }

        this.TippyState = tippyState;
        this.MessageTimer.Restart();
    }

    private void SetupNextAnimation(bool loop = false)
    {
        if (this.AnimationQueue.Count > 0)
        {
            this.AnimationQueue.TryDequeue(out var animationType);
            this.CurrentAnimationType = animationType;
        }
        else if (!loop)
        {
            this.CurrentAnimationType = this.GetIdleAnimation();
        }

        this.animationIsFinished = false;
        this.CurrentFrameIndex = 0;
        this.FrameTimer.Restart();
    }

    private Vector2 ToSpriteSheetScale(Vector2 input) => new(input.X / this.sheetSize.X, input.Y / this.sheetSize.Y);

    private AnimationType GetIdleAnimation()
    {
        var random = new Random();
        var rand = random.Next(0, 100);
        if (rand < 90) return AnimationType.Idle;
        var tipAnimations = new[]
        {
            AnimationType.Headphones,
            AnimationType.Searching,
            AnimationType.Snooze,
            AnimationType.PaperAirplane,
            AnimationType.WindChimes,
            AnimationType.ScratchHead,
        };
        var index = random.Next(0, tipAnimations.Length);
        return tipAnimations[index];
    }
}
