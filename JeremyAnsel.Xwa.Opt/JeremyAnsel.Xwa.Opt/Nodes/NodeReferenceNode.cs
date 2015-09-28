// -----------------------------------------------------------------------
// <copyright file="NodeReferenceNode.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public sealed class NodeReferenceNode : Node
    {
        public NodeReferenceNode()
            : base(NodeTypes.NodeReference)
        {
        }

        public string Reference { get; set; }

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

        internal override void Parse(byte[] buffer, int globalOffset, int offset)
        {
            base.Parse(buffer, globalOffset, offset);

            int dataOffset = BitConverter.ToInt32(buffer, offset + 20);

            if (dataOffset == 0)
            {
                return;
            }

            dataOffset -= globalOffset;

            this.Reference = Utils.GetNullTerminatedString(buffer, dataOffset);
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
                file.Write(Encoding.ASCII.GetBytes(this.Reference));
                file.Write((byte)0);
            }

            this.WriteNodes(file, offset);
        }
    }
}
