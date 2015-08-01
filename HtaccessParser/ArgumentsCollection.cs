using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Runtime.InteropServices;

namespace HtaccessParser
{
    [Serializable, DebuggerDisplay("Count = {Count}")]
    public class ArgumentsCollection : IList<string>, ICollection<string>, IEnumerable<string>, IList, ICollection, IEnumerable
    {
        #region Variables
        protected const int m_defaultCapacity = 4;
        protected static string[] m_emptyArray;
        protected string[] m_items;
        protected int m_size;
        [NonSerialized]
        protected object m_syncRoot;
        protected int m_version;
        #endregion

        #region Constructor
        static ArgumentsCollection()
        {
            ArgumentsCollection.m_emptyArray = new string[0];
        }

        public ArgumentsCollection()
        {
            this.m_items = ArgumentsCollection.m_emptyArray;
        }

        public ArgumentsCollection(IEnumerable<string> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException();
            }

            ICollection<string> is2 = collection as ICollection<string>;
            if (is2 != null)
            {
                int count = is2.Count;
                this.m_items = new string[count];
                is2.CopyTo(this.m_items, 0);
                this.m_size = count;
            }
            else
            {
                this.m_size = 0;
                this.m_items = new string[4];
                using (IEnumerator<string> enumerator = collection.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        this.Add(enumerator.Current);
                    }
                }
            }
        }

        public ArgumentsCollection(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            this.m_items = new string[capacity];
        }
        #endregion

        #region Static Methods
        private static bool IsCompatibleObject(object value)
        {
            if (!(value is string) && ((value != null) || typeof(string).IsValueType))
            {
                return false;
            }
            return true;
        }

        private static void VerifyValueType(object value)
        {
            if (!ArgumentsCollection.IsCompatibleObject(value))
            {
                throw new ArgumentException(value.ToString());
            }
        }
        #endregion

        #region Protected Methods
        protected virtual void EnsureCapacity(int min)
        {
            if (this.m_items.Length < min)
            {
                int num = (this.m_items.Length == 0) ? 4 : (this.m_items.Length * 2);
                if (num < min)
                {
                    num = min;
                }
                this.Capacity = num;
            }
        }
        #endregion

        #region Override Methods
        public override string ToString()
        {
            if (this.Count > 0)
                return "\"" + string.Join("\" \"", this.ToArray()) + "\"";
            return string.Empty;
        }
        #endregion

        #region Public Methods
        public virtual void Add(string item)
        {
            if (this.m_size == this.m_items.Length)
            {
                this.EnsureCapacity(this.m_size + 1);
            }
            this.m_items[this.m_size++] = item;
            this.m_version++;
        }

        public virtual void AddRange(IEnumerable<string> collection)
        {
            this.InsertRange(this.m_size, collection);
        }

        public virtual System.Collections.ObjectModel.ReadOnlyCollection<string> AsReadOnly()
        {
            return new System.Collections.ObjectModel.ReadOnlyCollection<string>(this);
        }

        public virtual int BinarySearch(string item)
        {
            return this.BinarySearch(0, this.Count, item, null);
        }

        public virtual int BinarySearch(string item, IComparer<string> comparer)
        {
            return this.BinarySearch(0, this.Count, item, comparer);
        }

        public virtual int BinarySearch(int index, int count, string item, IComparer<string> comparer)
        {
            if ((index < 0) || (count < 0))
            {
                throw new ArgumentOutOfRangeException();
            }
            if ((this.m_size - index) < count)
            {
                throw new ArgumentException();
            }
            return Array.BinarySearch<string>(this.m_items, index, count, item, comparer);
        }

        public virtual void Clear()
        {
            if (this.m_size > 0)
            {
                Array.Clear(this.m_items, 0, this.m_size);
                this.m_size = 0;
            }
            this.m_version++;
        }

        public virtual bool Contains(string item)
        {
            if (item == null)
            {
                for (int j = 0; j < this.m_size; j++)
                {
                    if (this.m_items[j] == null)
                    {
                        return true;
                    }
                }
                return false;
            }

            EqualityComparer<string> comparer = EqualityComparer<string>.Default;
            for (int i = 0; i < this.m_size; i++)
            {
                if (comparer.Equals(this.m_items[i], item))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual ArgumentsCollection ConvertAll(Converter<string, string> converter)
        {
            if (converter == null)
            {
                throw new ArgumentNullException();
            }

            ArgumentsCollection list = new ArgumentsCollection(this.m_size);
            for (int i = 0; i < this.m_size; i++)
            {
                list.m_items[i] = converter(this.m_items[i]);
            }
            list.m_size = this.m_size;
            return list;
        }

        public virtual void CopyTo(string[] array)
        {
            this.CopyTo(array, 0);
        }

        public virtual void CopyTo(string[] array, int arrayIndex)
        {
            Array.Copy(this.m_items, 0, array, arrayIndex, this.m_size);
        }

        public virtual void CopyTo(int index, string[] array, int arrayIndex, int count)
        {
            if ((this.m_size - index) < count)
            {
                throw new ArgumentException();
            }
            Array.Copy(this.m_items, index, array, arrayIndex, count);
        }

        public virtual bool Exists(Predicate<string> match)
        {
            return (this.FindIndex(match) != -1);
        }

        public virtual string Find(Predicate<string> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException();
            }
            for (int i = 0; i < this.m_size; i++)
            {
                if (match(this.m_items[i]))
                {
                    return this.m_items[i];
                }
            }
            return null;
        }

        public virtual ArgumentsCollection FindAll(Predicate<string> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException();
            }

            ArgumentsCollection list = new ArgumentsCollection();
            for (int i = 0; i < this.m_size; i++)
            {
                if (match(this.m_items[i]))
                {
                    list.Add(this.m_items[i]);
                }
            }
            return list;
        }

        public virtual int FindIndex(Predicate<string> match)
        {
            return this.FindIndex(0, this.m_size, match);
        }

        public virtual int FindIndex(int startIndex, Predicate<string> match)
        {
            return this.FindIndex(startIndex, this.m_size - startIndex, match);
        }

        public virtual int FindIndex(int startIndex, int count, Predicate<string> match)
        {
            if (startIndex > this.m_size)
            {
                throw new ArgumentOutOfRangeException();
            }

            if ((count < 0) || (startIndex > (this.m_size - count)))
            {
                throw new ArgumentOutOfRangeException();
            }

            if (match == null)
            {
                throw new ArgumentNullException();
            }

            int num = startIndex + count;
            for (int i = startIndex; i < num; i++)
            {
                if (match(this.m_items[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public virtual string FindLast(Predicate<string> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException();
            }

            for (int i = this.m_size - 1; i >= 0; i--)
            {
                if (match(this.m_items[i]))
                {
                    return this.m_items[i];
                }
            }
            return null;
        }

        public virtual int FindLastIndex(Predicate<string> match)
        {
            return this.FindLastIndex(this.m_size - 1, this.m_size, match);
        }

        public virtual int FindLastIndex(int startIndex, Predicate<string> match)
        {
            return this.FindLastIndex(startIndex, startIndex + 1, match);
        }

        public virtual int FindLastIndex(int startIndex, int count, Predicate<string> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException();
            }

            if (this.m_size == 0)
            {
                if (startIndex != -1)
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
            else if (startIndex >= this.m_size)
            {
                throw new ArgumentOutOfRangeException();
            }

            if ((count < 0) || (((startIndex - count) + 1) < 0))
            {
                throw new ArgumentOutOfRangeException();
            }

            int num = startIndex - count;
            for (int i = startIndex; i > num; i--)
            {
                if (match(this.m_items[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public virtual void ForEach(Action<string> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException();
            }
            for (int i = 0; i < this.m_size; i++)
            {
                action(this.m_items[i]);
            }
        }

        public virtual Enumerator GetEnumerator()
        {
            return new Enumerator((ArgumentsCollection)this);
        }

        public virtual ArgumentsCollection GetRange(int index, int count)
        {
            if ((index < 0) || (count < 0))
            {
                throw new ArgumentOutOfRangeException();
            }

            if ((this.m_size - index) < count)
            {
                throw new ArgumentException();
            }

            ArgumentsCollection list = new ArgumentsCollection(count);
            Array.Copy(this.m_items, index, list.m_items, 0, count);
            list.m_size = count;
            return list;
        }

        public virtual int IndexOf(string item)
        {
            return Array.IndexOf<string>(this.m_items, item, 0, this.m_size);
        }

        public virtual int IndexOf(string item, int index)
        {
            if (index > this.m_size)
            {
                throw new ArgumentOutOfRangeException();
            }

            return Array.IndexOf<string>(this.m_items, item, index, this.m_size - index);
        }

        public virtual int IndexOf(string item, int index, int count)
        {
            if (index > this.m_size)
            {
                throw new ArgumentOutOfRangeException();
            }

            if ((count < 0) || (index > (this.m_size - count)))
            {
                throw new ArgumentOutOfRangeException();
            }

            return Array.IndexOf<string>(this.m_items, item, index, count);
        }

        public virtual void Insert(int index, string item)
        {
            if (index > this.m_size)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (this.m_size == this.m_items.Length)
            {
                this.EnsureCapacity(this.m_size + 1);
            }

            if (index < this.m_size)
            {
                Array.Copy(this.m_items, index, this.m_items, index + 1, this.m_size - index);
            }

            this.m_items[index] = item;
            this.m_size++;
            this.m_version++;
        }

        public virtual void InsertRange(int index, IEnumerable<string> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException();
            }

            if (index > this.m_size)
            {
                throw new ArgumentOutOfRangeException();
            }

            ICollection<string> is2 = collection as ICollection<string>;
            if (is2 != null)
            {
                int count = is2.Count;
                if (count > 0)
                {
                    this.EnsureCapacity(this.m_size + count);
                    if (index < this.m_size)
                    {
                        Array.Copy(this.m_items, index, this.m_items, index + count, this.m_size - index);
                    }

                    if (this == is2)
                    {
                        Array.Copy(this.m_items, 0, this.m_items, index, index);
                        Array.Copy(this.m_items, (int)(index + count), this.m_items, (int)(index * 2), (int)(this.m_size - index));
                    }
                    else
                    {
                        string[] array = new string[count];
                        is2.CopyTo(array, 0);
                        array.CopyTo(this.m_items, index);
                    }

                    this.m_size += count;
                }
            }
            else
            {
                using (IEnumerator<string> enumerator = collection.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        this.Insert(index++, enumerator.Current);
                    }
                }
            }

            this.m_version++;
        }

        public virtual int LastIndexOf(string item)
        {
            return this.LastIndexOf(item, this.m_size - 1, this.m_size);
        }

        public virtual int LastIndexOf(string item, int index)
        {
            if (index >= this.m_size)
            {
                throw new ArgumentOutOfRangeException();
            }
            return this.LastIndexOf(item, index, index + 1);
        }

        public virtual int LastIndexOf(string item, int index, int count)
        {
            if (this.m_size == 0)
            {
                return -1;
            }

            if ((index < 0) || (count < 0))
            {
                throw new ArgumentOutOfRangeException();
            }

            if ((index >= this.m_size) || (count > (index + 1)))
            {
                throw new ArgumentOutOfRangeException();
            }

            return Array.LastIndexOf<string>(this.m_items, item, index, count);
        }

        public virtual bool Remove(string item)
        {
            int index = this.IndexOf(item);
            if (index >= 0)
            {
                this.RemoveAt(index);
                return true;
            }
            return false;
        }

        public virtual int RemoveAll(Predicate<string> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException();
            }

            int index = 0;
            while ((index < this.m_size) && !match(this.m_items[index]))
            {
                index++;
            }

            if (index >= this.m_size)
            {
                return 0;
            }

            int num2 = index + 1;
            while (num2 < this.m_size)
            {
                while ((num2 < this.m_size) && match(this.m_items[num2]))
                {
                    num2++;
                }
                if (num2 < this.m_size)
                {
                    this.m_items[index++] = this.m_items[num2++];
                }
            }

            Array.Clear(this.m_items, index, this.m_size - index);
            int num3 = this.m_size - index;
            this.m_size = index;
            this.m_version++;
            return num3;
        }

        public virtual void RemoveAt(int index)
        {
            if (index >= this.m_size)
            {
                throw new ArgumentOutOfRangeException();
            }

            this.m_size--;
            if (index < this.m_size)
            {
                Array.Copy(this.m_items, index + 1, this.m_items, index, this.m_size - index);
            }

            this.m_items[this.m_size] = null;
            this.m_version++;
        }

        public virtual void RemoveRange(int index, int count)
        {
            if ((index < 0) || (count < 0))
            {
                throw new ArgumentOutOfRangeException();
            }

            if ((this.m_size - index) < count)
            {
                throw new ArgumentException();
            }

            if (count > 0)
            {
                this.m_size -= count;
                if (index < this.m_size)
                {
                    Array.Copy(this.m_items, index + count, this.m_items, index, this.m_size - index);
                }

                Array.Clear(this.m_items, this.m_size, count);
                this.m_version++;
            }
        }

        public virtual void Reverse()
        {
            this.Reverse(0, this.Count);
        }

        public virtual void Reverse(int index, int count)
        {
            if ((index < 0) || (count < 0))
            {
                throw new ArgumentOutOfRangeException();
            }

            if ((this.m_size - index) < count)
            {
                throw new ArgumentException();
            }

            Array.Reverse(this.m_items, index, count);
            this.m_version++;
        }

        public virtual void Sort()
        {
            this.Sort(0, this.Count, null);
        }

        public virtual void Sort(IComparer<string> comparer)
        {
            this.Sort(0, this.Count, comparer);
        }

        public virtual void Sort(Comparison<string> comparison)
        {
            if (comparison == null)
            {
                throw new ArgumentNullException();
            }

            if (this.m_size > 0)
            {
                IComparer<string> comparer = new FunctorComparer<string>(comparison);
                Array.Sort<string>(this.m_items, 0, this.m_size, comparer);
            }
        }

        public virtual void Sort(int index, int count, IComparer<string> comparer)
        {
            if ((index < 0) || (count < 0))
            {
                throw new ArgumentOutOfRangeException();
            }

            if ((this.m_size - index) < count)
            {
                throw new ArgumentException();
            }

            Array.Sort<string>(this.m_items, index, count, comparer);
            this.m_version++;
        }

        public virtual string[] ToArray()
        {
            string[] destinationArray = new string[this.m_size];
            Array.Copy(this.m_items, 0, destinationArray, 0, this.m_size);
            return destinationArray;
        }

        public virtual void TrimExcess()
        {
            int num = (int)(this.m_items.Length * 0.9);
            if (this.m_size < num)
            {
                this.Capacity = this.m_size;
            }
        }

        public virtual bool TrueForAll(Predicate<string> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException();
            }

            for (int i = 0; i < this.m_size; i++)
            {
                if (!match(this.m_items[i]))
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region Implemented Methods
        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            return new Enumerator((ArgumentsCollection)this);
        }

        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            if ((array != null) && (array.Rank != 1))
            {
                throw new ArgumentException();
            }

            try
            {
                Array.Copy(this.m_items, 0, array, arrayIndex, this.m_size);
            }
            catch (ArrayTypeMismatchException)
            {
                throw new ArgumentException();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator((ArgumentsCollection)this);
        }

        int IList.Add(object item)
        {
            ArgumentsCollection.VerifyValueType(item);
            this.Add((string)item);
            return (this.Count - 1);
        }

        bool IList.Contains(object item)
        {
            return (ArgumentsCollection.IsCompatibleObject(item) && this.Contains((string)item));
        }

        int IList.IndexOf(object item)
        {
            if (ArgumentsCollection.IsCompatibleObject(item))
            {
                return this.IndexOf((string)item);
            }
            return -1;
        }

        void IList.Insert(int index, object item)
        {
            ArgumentsCollection.VerifyValueType(item);
            this.Insert(index, (string)item);
        }

        void IList.Remove(object item)
        {
            if (ArgumentsCollection.IsCompatibleObject(item))
            {
                this.Remove((string)item);
            }
        }
        #endregion

        #region Implemented Properties
        bool ICollection<string>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                if (this.m_syncRoot == null)
                {
                    System.Threading.Interlocked.CompareExchange(ref this.m_syncRoot, new object(), null);
                }
                return this.m_syncRoot;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                ArgumentsCollection.VerifyValueType(value);
                this[index] = (string)value;
            }
        }
        #endregion

        #region Properties
        public virtual int Capacity
        {
            get
            {
                return this.m_items.Length;
            }
            set
            {
                if (value != this.m_items.Length)
                {
                    if (value < this.m_size)
                    {
                        throw new ArgumentOutOfRangeException();
                    }

                    if (value > 0)
                    {
                        string[] destinationArray = new string[value];
                        if (this.m_size > 0)
                        {
                            Array.Copy(this.m_items, 0, destinationArray, 0, this.m_size);
                        }
                        this.m_items = destinationArray;
                    }
                    else
                    {
                        this.m_items = ArgumentsCollection.m_emptyArray;
                    }
                }
            }
        }

        public virtual int Count
        {
            get
            {
                return this.m_size;
            }
        }

        public virtual string this[int index]
        {
            get
            {
                if (index >= this.m_size)
                {
                    throw new ArgumentOutOfRangeException();
                }

                return this.m_items[index];
            }
            set
            {
                if (index >= this.m_size)
                {
                    throw new ArgumentOutOfRangeException();
                }

                this.m_items[index] = value;
                this.m_version++;
            }
        }
        #endregion

        #region Nested Types
        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<string>, IDisposable, IEnumerator
        {
	        #region Variables
            private ArgumentsCollection list;
            private int index;
            private int version;
            private string current;
            #endregion

            #region Constructor
            internal Enumerator(ArgumentsCollection list)
            {
                this.list = list;
                this.index = 0;
                this.version = list.m_version;
                this.current = null;
            }
            #endregion

            #region Private Methods
            private bool MoveNextRare()
            {
                if (this.version != this.list.m_version)
                {
                    throw new InvalidOperationException();
                }

                this.index = this.list.m_size + 1;
                this.current = null;
                return false;
            }

            void IEnumerator.Reset()
            {
                if (this.version != this.list.m_version)
                {
                    throw new InvalidOperationException();
                }

                this.index = 0;
                this.current = null;
            }
            #endregion

            #region Public Methods
            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                ArgumentsCollection list = this.list;

                if ((this.version == list.m_version) && (this.index < list.m_size))
                {
                    this.current = list.m_items[this.index];
                    this.index++;
                    return true;
                }

                return this.MoveNextRare();
            }
            #endregion

            #region Properties
            public string Current
            {
                get
                {
                    return this.current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    if ((this.index == 0) || (this.index == (this.list.m_size + 1)))
                    {
                        throw new InvalidOperationException();
                    }

                    return this.Current;
                }
            }
            #endregion
        }

        internal sealed class FunctorComparer<Tfc> : IComparer<Tfc>
        {
            #region Variables
            private Comparer<Tfc> c;
            private Comparison<Tfc> comparison;
            #endregion

            #region Constructor
            public FunctorComparer(Comparison<Tfc> comparison)
            {
                this.c = Comparer<Tfc>.Default;
                this.comparison = comparison;
            }
            #endregion

            #region Methods
            public int Compare(Tfc x, Tfc y)
            {
                return this.comparison(x, y);
            }
            #endregion
        }
        #endregion
    }
}
