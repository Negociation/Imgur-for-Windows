using System.Threading.Tasks;

namespace Imgur.Contracts
{
    /// <summary>
    /// Histórico local de mídias já vistas (primeira visualização apenas).
    /// </summary>
    public interface IViewHistoryStore
    {
        Task InitializeAsync();

        Task<bool> HasBeenViewedAsync(string mediaId, string accountKey);

        /// <summary>
        /// Insere só se ainda não existir. Retorna true se criou a linha.
        /// </summary>
        Task<bool> TryRecordFirstViewAsync(string mediaId, string accountKey);
    }
}
