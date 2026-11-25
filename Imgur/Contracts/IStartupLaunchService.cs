using System.Threading.Tasks;

namespace Imgur.Contracts
{
    public enum StartupLaunchState
    {
        NotSupported,
        Disabled,
        Enabled,
        DisabledByUser,
        DisabledByPolicy,
        EnabledByPolicy
    }

    public interface IStartupLaunchService
    {
        bool IsSupported { get; }
        Task<StartupLaunchState> GetStateAsync();
        Task<StartupLaunchState> SetEnabledAsync(bool enabled);
    }
}
