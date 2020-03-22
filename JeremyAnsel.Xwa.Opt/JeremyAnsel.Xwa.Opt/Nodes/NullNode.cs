// -----------------------------------------------------------------------
// <copyright file="NullNode.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt.Nodes
{
    internal sealed class NullNode : Node
    {
        internal NullNode()
            : base(NodeType.NullNode)
        {
        }

        public override string ToString()
        {
            return "Null Node";
        }

        internal override void Parse(byte[] buffer, int globalOffset, int offset)
        {
            base.Parse(buffer, globalOffset, offset);
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
