using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace HtaccessParserTest
{
    public partial class TestFrm : Form
    {
        public TestFrm()
        {
            InitializeComponent();
        }

        private void BuildNodes(HtaccessParser.HtaccessNode htaccessNode, ref TreeNode parentNode)
        {
            for (int i = 0; i < htaccessNode.Nodes.Count; i++)
            {
                HtaccessParser.HtaccessNode hNode = htaccessNode.Nodes[i];
                TreeNode treeNode = null;

                if (hNode.NodeType == HtaccessParser.NodeTypes.BLOCK)
                {
                    HtaccessParser.Block block = (HtaccessParser.Block)hNode;
                    string text = block.StartTag;
                    if (block.HasChildren)
                        text += block.EndTag;

                    treeNode = parentNode.Nodes.Add(text);

                    if (block.HasChildren)
                        this.BuildNodes(block, ref treeNode);
                } 
                else 
                    treeNode = parentNode.Nodes.Add(hNode.Text);
            }
        }

        private void cmdParse_Click(object sender, EventArgs e)
        {
            this.treeOutput.Nodes.Clear();

            if (string.IsNullOrEmpty(this.txtInput.Text)) return;
            HtaccessParser.Htaccess htaccess = new HtaccessParser.Htaccess();
            htaccess.Parse(this.txtInput.Text);

            if (htaccess.HasChildren)
            {
                TreeNode treeNode = this.treeOutput.Nodes.Add("[ROOT]");
                this.BuildNodes(htaccess, ref treeNode);
                treeNode.ExpandAll();
            }
        }

        private void cmdClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
