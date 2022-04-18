namespace Tippy
{
    /// <summary>
    /// Interface to communicate with Tippy.
    /// </summary>
    public interface ITippyAPI
    {
        /// <summary>
        /// Gets api version.
        /// </summary>
        public int APIVersion { get; }

        /// <summary>
        /// Register Tip.
        /// This will be added to the standard tip queue and will be displayed eventually at random.
        /// This can be used when you want to add your own tips.
        /// </summary>
        /// <param name="text">the text of the tip.</param>
        /// <returns>indicator if tip was successfully registered.</returns>
        public bool RegisterTip(string text);

        /// <summary>
        /// Register Message.
        /// This will be added to the priority message queue and likely display right away.
        /// This can be used to have Tippy display messages in near real-time you want to show to the user.
        /// </summary>
        /// <param name="text">the text of the message.</param>
        /// <returns>indicator if message was successfully registered.</returns>
        public bool RegisterMessage(string text);
    }
}
