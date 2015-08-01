using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace HtaccessParser
{
    public class Block : HtaccessNode
    {
        #region Variables
        private static string PATTERN = "^<(?<name>[^\\s]+)((?<args>[^\\>]+)\\/\\>|(\\/\\>))$";
        private static string PATTERN_START = "^(\\<[\\s]+|[\\<])(?<name>[^\\s]+)((?<args>[^\\>]+)\\>|[\\>])$";
        private static string PATTERN_END = "^<(\\/[\\s]+|[\\/])(?<name>[^\\s]+)>$";
        private static string PATTERN_ARGUMENT = " (?<arg>([^\\s]+)|(\\\"[\\s\\S]+)\\\")";

        private ArgumentsCollection m_arguments = null;
        private string m_text = string.Empty;
        #endregion

        #region Constructor
        public Block()
            : this(string.Empty)
        {
        }

        public Block(string name)
            : base(name)
        {
            this.m_nodeType = NodeTypes.BLOCK;
            this.m_arguments = new ArgumentsCollection();
            this.m_text = name;
        }

        public Block(string name, string[] arguments)
            : this(name)
        {
            this.m_arguments = new ArgumentsCollection(arguments);
            this.BuildText();
        }
        #endregion

        #region Methods
        private void BuildText()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(this.StartTag);
            if (this.HasChildren)
            {
                foreach (HtaccessNode node in this.Nodes)
                {
                    if (node != null) builder.AppendLine(node.ToString());
                }
                builder.AppendLine(this.EndTag);
            }
            this.m_text = builder.ToString();
        }

        internal static MatchTypes IsMatch(string line)
        {
            string temp = line == null ? string.Empty : line.Trim();
            if (string.IsNullOrEmpty(temp)) return MatchTypes.NONE;

            if (Regex.IsMatch(temp, PATTERN)) return MatchTypes.ALL;
            if (Regex.IsMatch(temp, PATTERN_END)) return MatchTypes.END;
            if (Regex.IsMatch(temp, PATTERN_START)) return MatchTypes.START;
            
            return MatchTypes.NONE;
        }

        internal override void ParseInternal(string line)
        {
            if (Block.IsMatch(line) == MatchTypes.NONE) return;

            this.m_text = line.Trim();
            string arguments = string.Empty;
            Match match = Regex.Match(this.m_text, PATTERN);
            if (match.Success)
            {
                this.m_name = match.Groups["name"].Value.Trim();
                if (match.Groups["args"].Success)
                    arguments = match.Groups["args"].Value.Trim();
            }
            else
            {
                match = Regex.Match(this.m_text, PATTERN_START);
                if (match.Success)
                {
                    this.m_name = match.Groups["name"].Value.Trim();
                    if (match.Groups["args"].Success)
                        arguments = match.Groups["args"].Value.Trim();
                }
            }

            if (!string.IsNullOrEmpty(arguments))
            {
                match = Regex.Match(" " + arguments, PATTERN_ARGUMENT);
                while (match.Success)
                {
                    this.m_arguments.Add(match.Groups["arg"].Value.Trim());
                    match = match.NextMatch();
                }
            }

            this.BuildText();
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
                    break;
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
            this.BuildText();
        }

        public bool HasArgumentValue(string value)
        {
            if (this.m_arguments.Count == 0) return false;
            foreach (string arg in this.m_arguments)
            {
                if (!string.IsNullOrEmpty(arg) && arg.Equals(value, StringComparison.CurrentCultureIgnoreCase))
                    return true;
            }
            return false;
        }

        public override string ToString()
        {
            this.BuildText();
            string text = this.Text;
            if (!string.IsNullOrEmpty(text))
            {
                StringBuilder builder = new StringBuilder();
                string[] lines = text.Replace("\r\n", "\n").Split('\n');
                string format = this.TabsIndex + "{0}";
                foreach (string line in lines)
                {
                    builder.AppendLine(string.Format(format, line));
                }
                return builder.ToString();
            }
            return "";
        }
        #endregion

        #region Properties
        public override string Text
        {
            get { return this.m_text; }
            set { this.m_text = value; }
        }

        public string StartTag
        {
            get 
            {
                string arguments = "";
                if (this.m_arguments != null && this.m_arguments.Count > 0)
                    arguments = this.m_arguments.ToString();

                if (!this.HasChildren)
                {
                    if (string.IsNullOrEmpty(arguments))
                        return string.Format("<{0} />", this.Name);
                    else
                        return string.Format("<{0} {1} />", this.Name, arguments);
                }
                else
                {
                    if (string.IsNullOrEmpty(arguments))
                        return string.Format("<{0}>\r\n", this.Name);
                    else
                        return string.Format("<{0} {1}>\r\n", this.Name, arguments);
                }
            }
        }

        public string EndTag
        {
            get { return string.Format("</{0}>", this.Name); }
        }

        public string[] Arguments
        {
            get { return this.m_arguments.ToArray(); }
            set
            {
                this.m_arguments = new ArgumentsCollection(value);
                this.BuildText();
            }
        }
        #endregion
    }
}
