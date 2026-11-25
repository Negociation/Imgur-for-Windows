namespace Imgur.Models
{
    /// <summary>
    /// UI languages shipped under Strings/{tag}/Resources.resw.
    /// <see cref="System"/> follows the OS (PrimaryLanguageOverride empty).
    /// Persisted as <see cref="Constants.LocalSettingsConstants.SelectedLanguage"/>.
    /// </summary>
    public enum AppLanguage
    {
        System = -1,
        English = 0,
        PortugueseBrazil = 1,
        Spanish = 2,
        ChineseSimplified = 3,
        Russian = 4,
        Ukrainian = 5,
        Indonesian = 6,
        Vietnamese = 7
    }
}
