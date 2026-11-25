using Imgur.Api.Services.Models.Enum;

namespace Imgur.Helpers
{
    /// <summary>
    /// Decide qual avatar exibir ao abrir a edição de perfil: sempre o que já existe na conta
    /// (mesmo URL usado na tela de conta/settings). O preset só indica qual item o seletor
    /// deve marcar; a imagem local do preset só substitui a foto depois que o usuário escolhe.
    /// </summary>
    public static class AvatarSeedResolver
    {
        public static AvatarSeed Resolve(string sessionAvatarUrl, string apiAvatarUrl, string apiAvatarName)
        {
            ImgurAvatar preset;
            var hasPreset = ImgurAvatarExtensions.TryParseApiValue(apiAvatarName, out preset);

            return new AvatarSeed(
                ResolveDisplayUrl(sessionAvatarUrl, apiAvatarUrl, hasPreset ? preset.GetLocalImagePath() : null),
                hasPreset,
                preset);
        }

        private static string ResolveDisplayUrl(string sessionAvatarUrl, string apiAvatarUrl, string presetImagePath)
        {
            // Sessão primeiro: é a mesma URL já mostrada em Account/Settings.
            // account/me/avatar costuma devolver o CDN do preset (ex.: default) mesmo quando
            // o perfil já tem foto real — preferir a API aqui causava o flash foto→default.
            if (!string.IsNullOrWhiteSpace(sessionAvatarUrl))
                return sessionAvatarUrl;
            if (!string.IsNullOrWhiteSpace(apiAvatarUrl))
                return apiAvatarUrl;
            return presetImagePath ?? string.Empty;
        }
    }

    public sealed class AvatarSeed
    {
        public AvatarSeed(string displayUrl, bool hasPreset, ImgurAvatar preset)
        {
            DisplayUrl = displayUrl;
            HasPreset = hasPreset;
            Preset = preset;
        }

        public string DisplayUrl { get; }

        public bool HasPreset { get; }

        public ImgurAvatar Preset { get; }

        public string PresetApiValue => HasPreset ? Preset.GetApiValue() ?? string.Empty : string.Empty;
    }
}
