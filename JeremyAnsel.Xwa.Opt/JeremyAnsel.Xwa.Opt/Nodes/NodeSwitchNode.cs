// -----------------------------------------------------------------------
// <copyright file="NodeSwitchNode.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt.Nodes
{
    public sealed class NodeSwitchNode : Node
    {
        public NodeSwitchNode(int nodesCount = -1)
            : base(NodeType.NodeSwitch, nodesCount)
        {
        }

        internal override void Parse(System.IO.BinaryReader file, int globalOffset, int offset)
        {
            base.Parse(file, globalOffset, offset);
        }

        internal override void Write(System.IO.BinaryWriter file, int offset)
        {
            base.Write(file, offset);

            file.Write((int)0);
            file.Write((int)0);

            this.WriteName(file);

            this.WriteNodesOffsets(file, offset);

            //// this node has no data

            this.WriteNodes(file, offset);
        }
    }
}
