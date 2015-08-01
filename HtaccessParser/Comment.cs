using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace HtaccessParser
{
    public class Comment : HtaccessNode
    {
        #region Variables
        // # xxxx
        private static string PATTERN = @"^# ([\s\S]+)$"; 

        private string m_text = string.Empty;
        #endregion

        #region Constructor
        public Comment()
            : base("#")
        {
            this.m_nodeType = NodeTypes.COMMENT;
        }

        public Comment(string text)
            : this()
        {
            this.m_text = text;
        }
        #endregion

        #region Methods
        internal static MatchTypes IsMatch(string line)
        {
            string temp = line == null ? string.Empty : line.Trim();
            if (string.IsNullOrEmpty(temp)) return MatchTypes.NONE;
            return Regex.IsMatch(temp, Comment.PATTERN) ? MatchTypes.ALL : MatchTypes.NONE;
        }

        internal override void ParseInternal(string line)
        {
            if (Comment.IsMatch(line) == MatchTypes.NONE) return;

            string temp = line.Trim();
            Match match = Regex.Match(line, Comment.PATTERN);

            if (match.Success && match.Groups != null && match.Groups.Count > 0)
                this.m_text = match.Groups[1].Value.Trim();
            else
                this.m_text = string.Empty;
        }

        public override string ToString()
        {
            return string.Format(this.TabsIndex + "# {0}", this.Text);
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
