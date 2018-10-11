using System.Collections.Generic;

namespace Domain
{
    /// <summary>
    /// Страница с объектами
    /// </summary>
    /// <typeparam name="TData">Тип данных</typeparam>
    public class DataPage<TData>
    {
        /// <summary>
        /// Общее количество объектов по запросу
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Объекты
        /// </summary>
        public IReadOnlyCollection<TData> Objects { get; set; }
    }
}