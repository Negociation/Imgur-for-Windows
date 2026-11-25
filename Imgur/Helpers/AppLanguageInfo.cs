using Imgur.Models;
using System;
using System.Collections.Generic;

namespace Imgur.Helpers
{
    /// <summary>
    /// BCP-47 tags and display names for <see cref="AppLanguage"/>.
    /// </summary>
    public static class AppLanguageInfo
    {
        private static readonly AppLanguage[] AllLanguages =
        {
            AppLanguage.System,
            AppLanguage.English,
            AppLanguage.PortugueseBrazil,
            AppLanguage.Spanish,
            AppLanguage.ChineseSimplified,
            AppLanguage.Russian,
            AppLanguage.Ukrainian,
            AppLanguage.Indonesian,
            AppLanguage.Vietnamese
        };

        private static readonly AppLanguage[] ShippedLanguages =
        {
            AppLanguage.English,
            AppLanguage.PortugueseBrazil,
            AppLanguage.Spanish,
            AppLanguage.ChineseSimplified,
            AppLanguage.Russian,
            AppLanguage.Ukrainian,
            AppLanguage.Indonesian,
            AppLanguage.Vietnamese
        };

        public static IReadOnlyList<AppLanguage> All => AllLanguages;

        public static IReadOnlyList<AppLanguage> Shipped => ShippedLanguages;

        public const string SystemDisplayName = "System";

        public static IReadOnlyList<string> GetDisplayNames(string systemLabel = null)
        {
            var label = string.IsNullOrWhiteSpace(systemLabel) ? SystemDisplayName : systemLabel;
            var names = new string[AllLanguages.Length];
            for (int i = 0; i < AllLanguages.Length; i++)
            {
                names[i] = AllLanguages[i] == AppLanguage.System
                    ? label
                    : GetDisplayName(AllLanguages[i]);
            }
            return names;
        }

        public static string GetDisplayName(AppLanguage language)
        {
            switch (language)
            {
                case AppLanguage.System:
                    return SystemDisplayName;
                case AppLanguage.English:
                    return "English";
                case AppLanguage.PortugueseBrazil:
                    return "Português (Brasil)";
                case AppLanguage.Spanish:
                    return "Español";
                case AppLanguage.ChineseSimplified:
                    return "中文（简体）";
                case AppLanguage.Russian:
                    return "Русский";
                case AppLanguage.Ukrainian:
                    return "Українська";
                case AppLanguage.Indonesian:
                    return "Bahasa Indonesia";
                case AppLanguage.Vietnamese:
                    return "Tiếng Việt";
                default:
                    return SystemDisplayName;
            }
        }

        /// <summary>
        /// For <see cref="AppLanguage.System"/> returns empty (clear PrimaryLanguageOverride).
        /// </summary>
        public static string GetTag(AppLanguage language)
        {
            switch (language)
            {
                case AppLanguage.System:
                    return string.Empty;
                case AppLanguage.English:
                    return "en-US";
                case AppLanguage.PortugueseBrazil:
                    return "pt-BR";
                case AppLanguage.Spanish:
                    return "es-ES";
                case AppLanguage.ChineseSimplified:
                    return "zh-CN";
                case AppLanguage.Russian:
                    return "ru-RU";
                case AppLanguage.Ukrainian:
                    return "uk-UA";
                case AppLanguage.Indonesian:
                    return "id-ID";
                case AppLanguage.Vietnamese:
                    return "vi-VN";
                default:
                    return "en-US";
            }
        }

        public static bool IsSystem(AppLanguage language) => language == AppLanguage.System;

        public static bool OsListContainsShipped(IEnumerable<string> osLanguageTags)
        {
            if (osLanguageTags == null)
                return false;

            foreach (string tag in osLanguageTags)
            {
                if (TryMapShipped(tag, out _))
                    return true;
            }

            return false;
        }

        public static AppLanguage FromStored(int raw)
        {
            return Enum.IsDefined(typeof(AppLanguage), raw)
                ? (AppLanguage)raw
                : AppLanguage.System;
        }

        public static bool TryMapShipped(string tag, out AppLanguage language)
        {
            language = AppLanguage.English;
            if (string.IsNullOrWhiteSpace(tag))
                return false;

            var normalized = tag.Trim();

            for (int i = 0; i < ShippedLanguages.Length; i++)
            {
                if (string.Equals(GetTag(ShippedLanguages[i]), normalized, StringComparison.OrdinalIgnoreCase))
                {
                    language = ShippedLanguages[i];
                    return true;
                }
            }

            // Match language part only (e.g. pt-PT → pt-BR if only pt is considered — only exact prefix of known tags)
            var dash = normalized.IndexOf('-');
            var primary = dash > 0 ? normalized.Substring(0, dash) : normalized;
            for (int i = 0; i < ShippedLanguages.Length; i++)
            {
                string known = GetTag(ShippedLanguages[i]);
                var knownDash = known.IndexOf('-');
                var knownPrimary = knownDash > 0 ? known.Substring(0, knownDash) : known;
                if (string.Equals(primary, knownPrimary, StringComparison.OrdinalIgnoreCase))
                {
                    language = ShippedLanguages[i];
                    return true;
                }
            }

            return false;
        }
    }
}
