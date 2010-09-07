using System;
using System.Diagnostics;
using System.Text;

namespace HashLib
{
    internal abstract class HashAdapter : Hash
    {
        protected IHash m_hash;

        public HashAdapter(IHash a_hash) 
            : base(a_hash.HashSize, a_hash.BlockSize)
        {
            Debug.Assert(a_hash != null);

            m_hash = a_hash;
        }

        public override void Initialize()
        {
            m_hash.Initialize();
        }

        public override void TransformBytes(byte[] a_data, int a_index, int a_length)
        {
            Debug.Assert(a_data != null);
            Debug.Assert(a_index >= 0);
            Debug.Assert(a_length >= 0);
            Debug.Assert(a_index + a_length <= a_data.Length);

            m_hash.TransformBytes(a_data, a_index, a_length);
        }

        public override HashResult TransformFinal()
        {
            return m_hash.TransformFinal();
        }
    }
}