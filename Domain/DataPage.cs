using System.Collections.Generic;

namespace Domain
{
    /// <summary>
    /// �������� � ���������
    /// </summary>
    /// <typeparam name="TData">��� ������</typeparam>
    public class DataPage<TData>
    {
        /// <summary>
        /// ����� ���������� �������� �� �������
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// �������
        /// </summary>
        public IReadOnlyCollection<TData> Objects { get; set; }
    }
}