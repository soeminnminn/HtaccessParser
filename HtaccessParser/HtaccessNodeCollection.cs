using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;

namespace HtaccessParser
{
    public class HtaccessNodeCollection : IEnumerable<HtaccessNode>
    {
        #region Variables
        private HtaccessNode m_parent = null;
        private int m_fixedIndex = -1;
        private int m_lastAccessedIndex = -1;
        #endregion

        #region Constructor
        public HtaccessNodeCollection()
        {
        }

        public HtaccessNodeCollection(HtaccessNode parent)
        {
            this.m_parent = parent;
        }
        #endregion

        #region Methods
        private int AddInternal(HtaccessNode node, int delta)
        {
            if (node == null) return -1;

            node.m_parent = this.m_parent;
            int fixedIndex = this.m_parent.Nodes.FixedIndex;
            if (fixedIndex != -1)
            {
                node.m_index = fixedIndex + delta;
            }
            else
            {
                this.m_parent.EnsureCapacity(1);
                node.m_index = this.m_parent.m_childCount;
            }

            this.m_parent.m_children[node.m_index] = node;
            this.m_parent.m_childCount++;

            return node.m_index;
        }

        private bool IsValidIndex(int index)
        {
            return ((index >= 0) && (index < this.Count));
        }

        private HtaccessNode FindInternal(Predicate<HtaccessNode> match, HtaccessNodeCollection collection)
        {
            if (collection == null)
            {
                return null;
            }

            for (int i = 0; i < collection.Count; i++)
            {
                if (match(collection[i])) return collection[i];

                HtaccessNode foundNode = this.FindInternal(match, collection[i].Nodes);
                if (foundNode != null)
                {
                    return foundNode;
                }
            }

            return null;
        }

        private ArrayList FindAllInternal(Predicate<HtaccessNode> match, HtaccessNodeCollection collection, ArrayList foundNodes)
        {
            if (collection == null) return null;

            for (int i = 0; i < collection.Count; i++)
            {
                if (match(collection[i])) foundNodes.Add(collection[i]);
                foundNodes = this.FindAllInternal(match, collection[i].Nodes, foundNodes);
            }

            return foundNodes;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public HtaccessNode Add(string value)
        {
            HtaccessNode node = new HtaccessNode(value);
            if (this.Add(node) > -1) return node;
            return null;
        }

        public int Add(HtaccessNode value)
        {
            return this.AddInternal(value, 0);
        }

        public void Clear()
        {
            this.m_parent.Clear();
        }

        public bool Contains(HtaccessNode value)
        {
            return (this.IndexOf(value) != -1);
        }

        public virtual int IndexOf(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (this.IsValidIndex(this.m_lastAccessedIndex) && (this[this.m_lastAccessedIndex].Text.Equals(value)))
                {
                    return this.m_lastAccessedIndex;
                }
                for (int i = 0; i < this.Count; i++)
                {
                    if (this[this.m_lastAccessedIndex].Text.Equals(value))
                    {
                        this.m_lastAccessedIndex = i;
                        return i;
                    }
                }
                this.m_lastAccessedIndex = -1;
            }
            return -1;
        }

        public int IndexOf(HtaccessNode value)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i] == value) return i;
            }
            return -1;
        }

        public void Insert(int index, HtaccessNode value)
        {
            if (index < 0) index = 0;
            if (index > this.m_parent.m_childCount) index = this.m_parent.m_childCount;
            this.m_parent.InsertNodeAt(index, value);
        }

        public void Remove(HtaccessNode value)
        {
            value.Remove();
        }

        public void RemoveAt(int index)
        {
            this[index].Remove();
        }

        public void CopyTo(Array array, int index)
        {
            if (this.m_parent.m_childCount > 0)
            {
                Array.Copy(this.m_parent.m_children, 0, array, index, this.m_parent.m_childCount);
            }
        }

        public IEnumerator<HtaccessNode> GetEnumerator()
        {
            return new Enumerator<HtaccessNode>(this.m_parent.m_children);
            //foreach (HtaccessNode node in this.m_parent.m_children)
            //{
            //    yield return node;
            //}
        }

        public void ForEach(Action<HtaccessNode> action)
        {
            if (action == null) throw new ArgumentNullException();

            for (int i = 0; i < this.m_parent.m_childCount; i++)
            {
                action(this.m_parent.m_children[i]);
            }
        }

        #endregion

        #region Properties
        internal int FixedIndex
        {
            get { return this.m_fixedIndex; }
            set { this.m_fixedIndex = value; }
        }

        public HtaccessNode this[int index]
        {
            get
            {
                if ((index < 0) || (index >= this.m_parent.m_childCount))
                    throw new ArgumentOutOfRangeException("NodeCollection.index");

                return this.m_parent.m_children[index];
            }
            set
            {
                if ((index < 0) || (index >= this.m_parent.m_childCount))
                    throw new ArgumentOutOfRangeException("NodeCollection.index");

                value.m_parent = this.m_parent;
                value.m_index = index;
                this.m_parent.m_children[index] = value;
            }
        }

        public int Count
        {
            get { return this.m_parent.m_childCount; }
        }
        #endregion

        #region Nested Types

        #region Enumerator<T> Class
        [Serializable, StructLayout(LayoutKind.Sequential)]
        public class Enumerator<T> : IEnumerator<T>, IDisposable, IEnumerator
        {
            #region Variables
            private T[] list = null;
            private int index = -1;
            private T current = default(T);
            #endregion

            #region Constructor
            public Enumerator(T[] list)
            {
                this.list = list;
                this.index = 0;
                this.current = default(T);
            }
            #endregion

            #region Private Methods
            private bool MovePreviousRare()
            {
                this.index = -1;
                this.current = default(T);
                return false;
            }

            private bool MoveNextRare()
            {
                this.index = this.list.Length + 1;
                this.current = default(T);
                return false;
            }

            void IEnumerator.Reset()
            {
                this.index = 0;
                this.current = default(T);
            }
            #endregion

            #region Public Methods
            public void Dispose()
            {
            }

            public bool MovePrevious()
            {
                if (this.index > -1)
                {
                    this.current = list[this.index];
                    this.index--;
                    return true;
                }

                return this.MovePreviousRare();
            }

            public bool MoveNext()
            {
                if (this.index < this.list.Length)
                {
                    this.current = list[this.index];
                    this.index++;
                    return true;
                }

                return this.MoveNextRare();
            }
            #endregion

            #region Properties
            public T Current
            {
                get
                {
                    return this.current;
                }
            }

            public int Index
            {
                get { return this.index; }
                set
                {
                    if (this.index != value)
                    {
                        if (value < 0)
                        {
                            this.index = 0;
                        }
                        else if (value > this.list.Length)
                        {
                            this.index = this.list.Length;
                        }
                        else
                        {
                            this.index = value;
                        }
                    }
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    if ((this.index == 0) || (this.index == (this.list.Length + 1)))
                    {
                        throw new InvalidOperationException();
                    }

                    return this.Current;
                }
            }
            #endregion
        }
        #endregion

        #endregion
    }
}
