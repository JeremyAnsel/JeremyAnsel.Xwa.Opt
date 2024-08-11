// -----------------------------------------------------------------------
// <copyright file="NodeReferenceNode.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt.Nodes
{
    using System;
    using System.Globalization;
    using System.Text;

    public sealed class NodeReferenceNode : Node
    {
        public NodeReferenceNode(int nodesCount = -1)
            : base(NodeType.NodeReference, nodesCount)
        {
        }

        public string? Reference { get; set; }

        protected override int DataSize
        {
            get
            {
                return this.Reference == null || this.Reference.Length == 0 ? 0 : (this.Reference.Length + 1);
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "NodeReferenceNode: {0}", this.Reference);
        }

        internal override void Parse(System.IO.BinaryReader file, int globalOffset, int offset)
        {
            base.Parse(file, globalOffset, offset);

            file.BaseStream.Position = offset + 20;
            int dataOffset = file.ReadInt32();

            if (dataOffset == 0)
            {
                return;
            }

            dataOffset -= globalOffset;

            this.Reference = Utils.GetNullTerminatedString(file, dataOffset);
        }

        internal override void Write(System.IO.BinaryWriter file, int offset)
        {
            base.Write(file, offset);

            int dataOffset = this.DataSize == 0 ? 0 : (offset + 24 + this.NameSize + this.NodesOffsetsSize);

            file.Write((int)0);
            file.Write(dataOffset);

            this.WriteName(file);

            this.WriteNodesOffsets(file, offset);

            if (dataOffset != 0)
            {
                for (int i = 0; i < this.Reference!.Length; i++)
                {
                    file.Write(this.Reference[i]);
                }

                file.Write((byte)0);
            }

            this.WriteNodes(file, offset);
        }
    }
}
