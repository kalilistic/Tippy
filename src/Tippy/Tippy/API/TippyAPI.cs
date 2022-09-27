using Dalamud.DrunkenToad;

namespace Tippy
{
    /// <inheritdoc cref="ITippyAPI" />
    public class TippyAPI : ITippyAPI
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TippyAPI"/> class.
        /// </summary>
        public TippyAPI()
        {
            this.IsInitialized = true;
        }

        /// <inheritdoc />
        public int APIVersion => 1;

        /// <inheritdoc />
        public bool IsInitialized { get; set; }

        /// <inheritdoc />
        public bool RegisterTip(string text)
        {
            if (!this.CheckInitialized()) return false;
            return TippyPlugin.TippyController.AddTip(text, MessageSource.IPC);
        }

        /// <inheritdoc />
        public bool RegisterMessage(string text)
        {
            if (!this.CheckInitialized()) return false;
            return TippyPlugin.TippyController.AddMessage(text, MessageSource.IPC);
        }

        private bool CheckInitialized()
        {
            if (!this.IsInitialized)
            {
                Logger.LogInfo("Tippy API is not initialized.");
                return false;
            }

            return true;
        }
    }
}
