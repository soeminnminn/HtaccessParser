using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace HtaccessParser
{
    public class Directive : HtaccessNode
    {
        #region Variables
        private static string PATTERN_NAME = "^(?<name>[^\\s]+) (?<args>[\\s\\S]+)";
        private static string PATTERN_ARGUMENT = " (?<arg>([^\\s]+)|(\\\"[\\s\\S]+)\\\")";

        private ArgumentsCollection m_arguments = null;
        private string m_text = string.Empty;
        #endregion

        #region Constructor
        public Directive()
            : this(string.Empty)
        {   
        }

        public Directive(string name)
            : base(name)
        {
            this.m_nodeType = NodeTypes.DIRECTIVE;
            this.m_arguments = new ArgumentsCollection();
            this.m_text = name;
        }

        public Directive(string name, string[] arguments)
            : this(name)
        {
            this.m_arguments = new ArgumentsCollection(arguments);
            this.BuildText();
        }
        #endregion

        #region Methods
        private void BuildText()
        {
            if (!string.IsNullOrEmpty(this.m_name))
            {
                this.m_text = this.m_name;
                if (this.m_arguments != null && this.m_arguments.Count > 0)
                    this.m_text += " " + this.m_arguments.ToString();
            }
        }

        internal static MatchTypes IsMatch(string line)
        {
            string temp = line == null ? string.Empty : line.Trim();
            if (string.IsNullOrEmpty(temp)) return MatchTypes.NONE;
            return Regex.IsMatch(temp, PATTERN_NAME) ? MatchTypes.ALL : MatchTypes.NONE;
        }

        internal override void ParseInternal(string line)
        {
            if (Directive.IsMatch(line) == MatchTypes.NONE) return;

            this.m_text = line.Trim();
            Match match = Regex.Match(this.m_text, PATTERN_NAME);
            if (match.Success)
            {
                this.m_name = match.Groups["name"].Value.Trim();

                string arguments = " " + match.Groups["args"].Value.Trim();
                match = Regex.Match(arguments, PATTERN_ARGUMENT);
                while (match.Success)
                {
                    this.m_arguments.Add(match.Groups["arg"].Value.Trim());
                    match = match.NextMatch();
                }
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
            return string.Format(this.TabsIndex + "{0}", this.Text);
        }
        #endregion

        #region Properties
        public override string Text
        {
            get { return this.m_text; }
            set { this.m_text = value; }
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
