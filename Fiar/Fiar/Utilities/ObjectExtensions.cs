using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Fiar
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Deep copy object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objSource"></param>
        /// <returns></returns>
        public static T CopyObject<T>(this object objSource)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, objSource);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }

        /// <summary>
        /// Remove all where, for <see cref="ObservableCollection{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="coll">The collection to work with</param>
        /// <param name="condition">Condition to removal</param>
        /// <returns>Number of removed items from the collection</returns>
        public static int RemoveAll<T>(this ObservableCollection<T> coll, Func<T, bool> condition)
        {
            int ret = 0;

            foreach (var itemToRemove in coll.Where(condition).ToList())
            {
                if (coll.Remove(itemToRemove))
                    ret++;
            }

            return ret;
        }
    }
}
