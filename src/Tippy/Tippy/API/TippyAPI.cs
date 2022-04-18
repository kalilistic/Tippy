using Dalamud.DrunkenToad;

namespace Tippy
{
    /// <inheritdoc cref="ITippyAPI" />
    public class TippyAPI : ITippyAPI
    {
        private readonly bool initialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="TippyAPI"/> class.
        /// </summary>
        public TippyAPI()
        {
            this.initialized = true;
        }

        /// <inheritdoc />
        public int APIVersion => 1;

        /// <inheritdoc />
        public bool RegisterTip(string text)
        {
            if (!this.IsValidRequest(text)) return false;
            var tip = new Tip(text)
            {
                Source = MessageSource.IPC,
            };
            TippyPlugin.TippyController.IPCTips.Add(tip);
            TippyPlugin.TippyController.SetupMessages();
            return true;
        }

        /// <inheritdoc />
        public bool RegisterMessage(string text)
        {
            if (!this.IsValidRequest(text)) return false;
            var message = new Message(text)
            {
                Source = MessageSource.IPC,
            };
            TippyPlugin.TippyController.MessageQueue.Enqueue(message);
            return true;
        }

        private bool IsValidRequest(string text)
        {
            return this.CheckInitialized() && !string.IsNullOrWhiteSpace(text);
        }

        private bool CheckInitialized()
        {
            if (!this.initialized)
            {
                Logger.LogInfo("Tippy API is not initialized.");
                return false;
            }

            return true;
        }
    }
}
