using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace HtaccessParser
{
    public class Htaccess : HtaccessNode
    {
        #region Variables
        private string m_text = string.Empty;
        #endregion

        #region Constructor
        public Htaccess()
            : base()
        {
            this.m_nodeType = NodeTypes.DOCUMENT;
        }

        public Htaccess(string htaccessFilePath)
            : this()
        {
            if (!string.IsNullOrEmpty(htaccessFilePath))
            {
                FileInfo file = new FileInfo(htaccessFilePath);
                if (file.Exists)
                {
                    string text = string.Empty;
                    using (StreamReader reader = new StreamReader(file.Open(FileMode.Open, FileAccess.Read), System.Text.Encoding.UTF8))
                    {
                        text = reader.ReadToEnd();
                    }
                    this.Parse(text);
                }
            }
        }

        public Htaccess(Stream stream)
            : this()
        {
            if (stream != null && stream.CanRead)
            {
                string text = string.Empty;
                using (StreamReader reader = new StreamReader(stream))
                {
                    text = reader.ReadToEnd();
                }
                this.Parse(text);
            }
        }
        #endregion

        #region Methods
        public void Parse(string htaccessContent)
        {
            if (string.IsNullOrEmpty(htaccessContent)) return;
            this.m_text = htaccessContent.Replace("\r\n", "\n").Replace('\r', '\n').Replace("\n", "\r\n");
            using (StringReader reader = new StringReader(this.m_text))
            {
                this.ParseInternal(reader);
            }
        }

        internal override void ParseInternal(System.IO.StringReader reader)
        {
            if (reader == null) return;
            string line = reader.ReadLine();
            while (line != null)
            {
                HtaccessNode node = null;
                if (Block.IsMatch(line) == MatchTypes.ALL)
                {
                    node = new Block();
                    node.ParseInternal(line);
                }
                else if (Block.IsMatch(line) == MatchTypes.START)
                {
                    node = new Block();
                    node.ParseInternal(line);
                    node.ParseInternal(reader);
                }
                else if (Block.IsMatch(line) == MatchTypes.END)
                {
                    // Skip
                }
                else if (WhiteLine.IsMatch(line) == MatchTypes.ALL)
                {
                    node = new WhiteLine();
                }
                else if (Comment.IsMatch(line) == MatchTypes.ALL)
                {
                    node = new Comment();
                    node.ParseInternal(line);
                }
                else if (Directive.IsMatch(line) == MatchTypes.ALL)
                {
                    node = new Directive();
                    node.ParseInternal(line);
                }

                if (node != null)
                {
                    this.Nodes.Add(node);
                }
                line = reader.ReadLine();
            }
        }
        #endregion

        #region Properties
        public override string Text
        {
            get { return this.m_text; }
            set { this.m_text = value; }
        }
        #endregion
    }
}
