using System;

using Dalamud.DrunkenToad;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;

namespace Tippy
{
    /// <summary>
    /// IPC for Tippy plugin.
    /// </summary>
    public class TippyProvider
    {
        /// <summary>
        /// API Version.
        /// </summary>
        public const string LabelProviderApiVersion = "Tippy.APIVersion";

        /// <summary>
        /// Register Tip.
        /// </summary>
        public const string LabelProviderRegisterTip = "Tippy.RegisterTip";

        /// <summary>
        /// Register Message.
        /// </summary>
        public const string LabelProviderRegisterMessage = "Tippy.RegisterMessage";

        /// <summary>
        /// API.
        /// </summary>
        public readonly ITippyAPI API;

        /// <summary>
        /// Provider API Version.
        /// </summary>
        public ICallGateProvider<int>? ProviderAPIVersion;

        /// <summary>
        /// Register Tip.
        /// </summary>
        public ICallGateProvider<string, bool>? ProviderRegisterTip;

        /// <summary>
        /// Register Message.
        /// </summary>
        public ICallGateProvider<string, bool>? ProviderRegisterMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="TippyProvider"/> class.
        /// </summary>
        /// <param name="pluginInterface">plugin interface.</param>
        /// <param name="api">plugin api.</param>
        public TippyProvider(DalamudPluginInterface pluginInterface, ITippyAPI api)
        {
            this.API = api;

            try
            {
                this.ProviderAPIVersion = pluginInterface.GetIpcProvider<int>(LabelProviderApiVersion);
                this.ProviderAPIVersion.RegisterFunc(() => api.APIVersion);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error registering IPC provider for {LabelProviderApiVersion}:\n{ex}");
            }

            try
            {
                this.ProviderRegisterTip = pluginInterface.GetIpcProvider<string, bool>(LabelProviderRegisterTip);
                this.ProviderRegisterTip.RegisterFunc(api.RegisterTip);
            }
            catch (Exception e)
            {
                Logger.LogError($"Error registering IPC provider for {LabelProviderRegisterTip}:\n{e}");
            }

            try
            {
                this.ProviderRegisterMessage = pluginInterface.GetIpcProvider<string, bool>(LabelProviderRegisterMessage);
                this.ProviderRegisterMessage.RegisterFunc(api.RegisterMessage);
            }
            catch (Exception e)
            {
                Logger.LogError($"Error registering IPC provider for {LabelProviderRegisterMessage}:\n{e}");
            }
        }

        /// <summary>
        /// Dispose IPC.
        /// </summary>
        public void Dispose()
        {
            this.ProviderAPIVersion?.UnregisterFunc();
            this.ProviderRegisterTip?.UnregisterFunc();
            this.ProviderRegisterMessage?.UnregisterFunc();
        }
    }
}
