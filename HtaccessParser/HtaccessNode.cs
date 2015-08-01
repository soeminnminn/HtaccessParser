using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace HtaccessParser
{
    public enum NodeTypes
    {
        NONE,
        DOCUMENT,
        BLOCK,
        COMMENT,
        DIRECTIVE,
        WHITELINE
    }

    internal enum MatchTypes
    {
        NONE,
        ALL,
        START,
        END
    }

    public class HtaccessNode : ICloneable
    {
        #region Variables
        protected static int MAX_ARGUMENTS = 5;

        internal int m_childCount = 0;
        internal int m_index = 0;

        internal HtaccessNode[] m_children = null;
        internal HtaccessNode m_parent = null;

        protected string m_name = string.Empty;
        protected NodeTypes m_nodeType = NodeTypes.NONE;
        protected HtaccessNodeCollection m_nodes = null;
        #endregion

        #region Constructor
        public HtaccessNode()
        {
        }

        public HtaccessNode(string name)
        {
            this.m_name = name;
        }
        #endregion

        #region Methods
        internal virtual void Clear()
        {
            while (this.m_childCount > 0)
            {
                this.m_children[this.m_childCount - 1].Remove();
            }
            this.m_children = null;
        }

        internal virtual void EnsureCapacity(int num)
        {
            int num2 = num;
            if (num2 < 4)
            {
                num2 = 4;
            }

            if (this.m_children == null)
            {
                this.m_children = new HtaccessNode[num2];
            }
            else if ((this.m_childCount + num) > this.m_children.Length)
            {
                int num3 = this.m_childCount + num;
                if (num == 1)
                {
                    num3 = this.m_childCount * 2;
                }

                HtaccessNode[] destinationArray = new HtaccessNode[num3];
                Array.Copy(this.m_children, 0, destinationArray, 0, this.m_childCount);
                this.m_children = destinationArray;
            }
        }

        internal virtual void InsertNodeAt(int index, HtaccessNode node)
        {
            this.EnsureCapacity(1);
            node.m_parent = this;
            node.m_index = index;

            for (int i = this.m_childCount; i > index; i--)
            {
                HtaccessNode node2 = null;
                this.m_children[i] = node2 = this.m_children[i - 1];
                node2.m_index = i;
            }

            this.m_children[index] = node;
            this.m_childCount++;
        }

        internal virtual void ParseInternal(string line)
        {
            throw new NotImplementedException();
        }

        internal virtual void ParseInternal(StringReader reader)
        {
            throw new NotImplementedException();
        }

        internal virtual void FindInternal(Predicate<HtaccessNode> match, ref List<HtaccessNode> list)
        {
            for (int i = 0; i < this.m_nodes.Count; i++)
            {
                if (this.m_nodes[i] == null) continue;

                if (match(this.m_nodes[i]))
                    list.Add(this.m_nodes[i]);

                if (this.m_nodes[i].HasChildren)
                    this.m_nodes[i].FindInternal(match, ref list);
            }
        }

        public virtual object Clone()
        {
            Type type = this.GetType();
            HtaccessNode obj = null;
            if (type == typeof(HtaccessNode))
            {
                obj = new HtaccessNode();
            }
            else
            {
                obj = (HtaccessNode)Activator.CreateInstance(type);
            }

            obj.m_childCount = this.m_childCount;
            obj.m_index = this.m_index;
            if (this.m_parent != null)
            {
                obj.m_parent = (HtaccessNode)this.m_parent.Clone();
            }

            if (this.m_children != null)
            {
                obj.m_children = new HtaccessNode[this.m_childCount];
                Array.Copy(this.m_children, obj.m_children, this.m_childCount);
            }
            return obj;
        }

        public virtual int GetNodeCount(bool includeSubTrees)
        {
            int childCount = this.m_childCount;

            if (includeSubTrees)
            {
                for (int i = 0; i < this.m_childCount; i++)
                {
                    childCount += this.m_children[i].GetNodeCount(true);
                }
            }

            return childCount;
        }

        public virtual void Remove()
        {
            for (int i = 0; i < this.m_childCount; i++)
            {
                this.m_children[i].Remove();
            }

            if (this.m_parent != null)
            {
                if (this.m_index == (this.m_parent.m_childCount - 1))
                    this.m_parent.m_children[this.m_index] = null;

                for (int j = this.m_index; j < (this.m_parent.m_childCount - 1); j++)
                {
                    HtaccessNode node = null;
                    this.m_parent.m_children[j] = node = this.m_parent.m_children[j + 1];
                    node.m_index = j;
                }

                this.m_parent.m_childCount--;
                this.m_parent = null;
            }
        }

        public virtual HtaccessNode Find(Predicate<HtaccessNode> match)
        {
            if (match == null)
                throw new ArgumentNullException();

            for (int i = 0; i < this.m_nodes.Count; i++)
            {
                if (this.m_nodes[i] == null) continue;

                if (match(this.m_nodes[i]))
                    return this.m_nodes[i];

                if (this.m_nodes[i].HasChildren)
                {
                    HtaccessNode child = this.m_nodes[i].Find(match);
                    if (child != null) return child;
                }
            }
            return null;
        }

        public virtual List<HtaccessNode> FindAll(Predicate<HtaccessNode> match)
        {
            if (match == null)
                throw new ArgumentNullException();

            List<HtaccessNode> list = new List<HtaccessNode>();
            this.FindInternal(match, ref list);

            return list;
        }

        public override int GetHashCode()
        {
            string text = this.Text;
            if (!string.IsNullOrEmpty(text))
                return text.GetHashCode();

            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj != null)
            {
                string text = this.Text;
                if (!string.IsNullOrEmpty(text))
                {
                    if (obj is string)
                        return text.Equals(obj);
                    else if (obj is HtaccessNode)
                        return text.Equals(((HtaccessNode)obj).Text);
                }
            }
            return base.Equals(obj);
        }

        public override string ToString()
        {
            string text = this.Text;
            if (!string.IsNullOrEmpty(text))
            {
                string format = this.TabsIndex + "{0}";
                return string.Format(format, text);
            }
            return text;
        }
        #endregion

        #region Properties
        protected virtual string TabsIndex
        {
            get 
            {
                string format = "";
                for (int i = 0; i < this.Level; i++)
                {
                    format += "\t";
                }
                return format;
            }
        }

        protected virtual bool IsChildrenEmpty
        {
            get
            {
                if (this.m_children != null)
                {
                    if (this.m_children.Length < 1) return true;
                    return false;
                }

                return true;
            }
        }

        public virtual string Name
        {
            get { return this.m_name; }
            set { this.m_name = value; }
        }

        public virtual string Text
        {
            get { return string.Empty; }
            set { }
        }

        public virtual NodeTypes NodeType
        {
            get { return this.m_nodeType; }
        }

        public virtual int Level
        {
            get
            {
                if (this.Parent == null) return 0;
                return (this.Parent.Level + 1);
            }
        }

        public virtual int Index
        {
            get { return this.m_index; }
        }

        public virtual HtaccessNodeCollection Nodes
        {
            get
            {
                if (this.m_nodes == null) 
                    this.m_nodes = new HtaccessNodeCollection(this);
                return this.m_nodes;
            }
        }

        public virtual HtaccessNode Parent
        {
            get { return this.m_parent; }
        }

        public virtual bool HasChildren
        {
            get { return (this.m_childCount > 0); }
        }

        public virtual HtaccessNode FirstNode
        {
            get
            {
                if (this.m_childCount == 0) return null;
                return this.m_children[0];
            }
        }

        protected virtual HtaccessNode FirstParent
        {
            get
            {
                HtaccessNode parent = this;
                while (parent != null)
                {
                    parent = parent.Parent;
                }
                return parent;
            }
        }

        public virtual HtaccessNode LastNode
        {
            get
            {
                if (this.m_childCount == 0) return null;
                return this.m_children[this.m_childCount - 1];
            }
        }

        public virtual HtaccessNode NextNode
        {
            get
            {
                if ((this.m_index + 1) < this.m_parent.Nodes.Count)
                {
                    return this.m_parent.Nodes[this.m_index + 1];
                }
                return null;
            }
        }

        public virtual HtaccessNode PrevNode
        {
            get
            {
                int index = this.m_index;
                int fixedIndex = this.m_parent.Nodes.FixedIndex;
                if (fixedIndex > 0)
                {
                    index = fixedIndex;
                }
                if ((index > 0) && (index <= this.m_parent.Nodes.Count))
                {
                    return this.m_parent.Nodes[index - 1];
                }
                return null;
            }
        }

        public virtual bool IsEmpty
        {
            get
            {
                return (string.IsNullOrEmpty(this.Text) && this.IsChildrenEmpty);
            }
        }
        #endregion
    }
}
