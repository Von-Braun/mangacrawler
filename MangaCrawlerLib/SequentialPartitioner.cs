using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace MangaCrawlerLib
{
    internal class SequentialPartitioner<T> : Partitioner<T>
    {
        private readonly IList<T> m_source;

        public SequentialPartitioner(IList<T> a_source)
        {
            m_source = a_source;
        }

        public override bool SupportsDynamicPartitions
        {
            get
            {
                return true;
            }
        }

        public override IList<IEnumerator<T>> GetPartitions(int a_partition_count)
        {
            var dp = GetDynamicPartitions();
            return (from i in Enumerable.Range(0, a_partition_count)
                    select dp.GetEnumerator()).ToList();
        }

        public override IEnumerable<T> GetDynamicPartitions()
        {
            return GetDynamicPartitions(m_source.GetEnumerator());
        }

        private static IEnumerable<T> GetDynamicPartitions(IEnumerator<T> a_enumerator)
        {
            while (true)
            {
                T el;
                lock (a_enumerator)
                {
                    if (a_enumerator.MoveNext())
                        el = a_enumerator.Current;
                    else
                        yield break;
                }

                yield return el;
            }
        }
    }

}
