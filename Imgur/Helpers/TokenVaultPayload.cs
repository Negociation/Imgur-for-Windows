using System;
using System.Globalization;

namespace Imgur.Helpers
{
    /// <summary>
    /// Vault password payload: prefers JSON
    /// {"accessToken":"...","refreshToken":"...","expiresAt":123}
    /// and still reads legacy "access|refresh" pipe format.
    /// </summary>
    public static class TokenVaultPayload
    {
        public static string Serialize(string accessToken, string refreshToken, long? expiresAtUnix = null)
        {
            string access = Escape(accessToken);
            string refresh = Escape(refreshToken);
            if (expiresAtUnix.HasValue)
            {
                return "{\"accessToken\":\"" + access +
                       "\",\"refreshToken\":\"" + refresh +
                       "\",\"expiresAt\":" + expiresAtUnix.Value.ToString(CultureInfo.InvariantCulture) + "}";
            }

            return "{\"accessToken\":\"" + access +
                   "\",\"refreshToken\":\"" + refresh + "\"}";
        }

        public static bool TryParse(
            string password,
            out string accessToken,
            out string refreshToken,
            out long? expiresAtUnix)
        {
            accessToken = null;
            refreshToken = null;
            expiresAtUnix = null;

            if (string.IsNullOrWhiteSpace(password))
                return false;

            string trimmed = password.Trim();
            if (trimmed.Length > 0 && trimmed[0] == '{')
                return TryParseJson(trimmed, out accessToken, out refreshToken, out expiresAtUnix);

            string[] parts = trimmed.Split('|');
            if (parts.Length < 2)
                return false;

            accessToken = parts[0];
            refreshToken = parts[1];
            if (parts.Length > 2 && long.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out long exp))
                expiresAtUnix = exp;
            return !string.IsNullOrEmpty(accessToken);
        }

        private static bool TryParseJson(
            string json,
            out string accessToken,
            out string refreshToken,
            out long? expiresAtUnix)
        {
            accessToken = ExtractString(json, "accessToken");
            refreshToken = ExtractString(json, "refreshToken");
            expiresAtUnix = ExtractLong(json, "expiresAt");
            return !string.IsNullOrEmpty(accessToken);
        }

        private static string ExtractString(string json, string key)
        {
            string needle = "\"" + key + "\"";
            int keyIdx = json.IndexOf(needle, StringComparison.Ordinal);
            if (keyIdx < 0) return null;
            int colon = json.IndexOf(':', keyIdx + needle.Length);
            if (colon < 0) return null;
            int startQuote = json.IndexOf('"', colon + 1);
            if (startQuote < 0) return null;
            int endQuote = startQuote + 1;
            while (endQuote < json.Length)
            {
                if (json[endQuote] == '"' && json[endQuote - 1] != '\\')
                    break;
                endQuote++;
            }
            if (endQuote >= json.Length) return null;
            return Unescape(json.Substring(startQuote + 1, endQuote - startQuote - 1));
        }

        private static long? ExtractLong(string json, string key)
        {
            string needle = "\"" + key + "\"";
            int keyIdx = json.IndexOf(needle, StringComparison.Ordinal);
            if (keyIdx < 0) return null;
            int colon = json.IndexOf(':', keyIdx + needle.Length);
            if (colon < 0) return null;
            int i = colon + 1;
            while (i < json.Length && char.IsWhiteSpace(json[i])) i++;
            int start = i;
            while (i < json.Length && (char.IsDigit(json[i]) || json[i] == '-')) i++;
            if (start == i) return null;
            long value;
            if (long.TryParse(json.Substring(start, i - start), NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                return value;
            return null;
        }

        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private static string Unescape(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Replace("\\\"", "\"").Replace("\\\\", "\\");
        }
    }
}
