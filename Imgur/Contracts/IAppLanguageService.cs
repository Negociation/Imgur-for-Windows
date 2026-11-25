using Imgur.Models;
using System.Threading.Tasks;

namespace Imgur.Contracts
{
    /// <summary>
    /// Applies <see cref="AppLanguage"/> via PrimaryLanguageOverride and restarts when changed.
    /// </summary>
    public interface IAppLanguageService
    {
        /// <summary>Reads LocalSettings and sets PrimaryLanguageOverride (call early on launch).</summary>
        void ApplyFromSettings();

        /// <summary>Localized label for <see cref="AppLanguage.System"/> in the language ComboBox.</summary>
        string GetSystemDisplayLabel();

        Task ChangeLanguageAndRestartAsync(AppLanguage language);
    }
}
