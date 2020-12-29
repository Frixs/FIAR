using System;
using System.Threading.Tasks;

namespace Fiar
{
    /// <summary>
    /// Interface repository for currently opened session data (ws connections etc.)
    /// </summary>
    public interface IRepository<T>
    {
        /// <summary>
        /// Create a new item into the repository
        /// </summary>
        /// <param name="item">The item</param>
        /// <returns>TRUE on successful addition, FALSE otherwise</returns>
        Task<bool> AddItemAsync(T item);

        /// <summary>
        /// Get the item or return null on failure
        /// </summary>
        /// <param name="predicate">Condition for search</param>
        /// <returns>The item object or null on failure</returns>
        Task<T> GetItemAsync(Func<T, bool> predicate);

        /// <summary>
        /// Delete an item from the repository
        /// </summary>
        /// <param name="item">The item</param>
        Task RemoveItemAsync(T item);
    }
}
