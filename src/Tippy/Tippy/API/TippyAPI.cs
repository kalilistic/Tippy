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
            if (!this.initialized)
            {
                Logger.LogInfo("Tippy API is not initialized.");
                return false;
            }

            return true;
        }
    }
}
