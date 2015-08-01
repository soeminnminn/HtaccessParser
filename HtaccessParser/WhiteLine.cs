using System;
using System.Collections.Generic;
using System.Text;

namespace HtaccessParser
{
    public class WhiteLine : HtaccessNode
    {
        #region Variables
        #endregion

        #region Constructor
        public WhiteLine()
            : base()
        {
            this.m_nodeType = NodeTypes.WHITELINE;
        }
        #endregion

        #region Methods
        internal static MatchTypes IsMatch(string line)
        {
            string temp = line == null ? string.Empty : line.Trim();
            return string.IsNullOrEmpty(temp) ? MatchTypes.ALL : MatchTypes.NONE;
        }

        internal override void ParseInternal(string line)
        {
        }
        #endregion

        #region Properties
        #endregion
    }
}
