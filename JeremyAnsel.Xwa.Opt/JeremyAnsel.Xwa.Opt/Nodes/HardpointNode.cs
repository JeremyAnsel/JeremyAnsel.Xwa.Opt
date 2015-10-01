// -----------------------------------------------------------------------
// <copyright file="HardPointNode.cs" company="">
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

    public sealed class HardpointNode : Node
    {
        public HardpointNode()
            : base(NodeType.Hardpoint)
        {
        }

        public HardpointType HardpointType { get; set; }

        public Vector Position { get; set; }

        protected override int DataSize
        {
            get
            {
                return 16;
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "HardpointNode: {0} at {1}", this.HardpointType, this.Position);
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

            this.HardpointType = (HardpointType)BitConverter.ToInt32(buffer, dataOffset + 0);
            this.Position = Vector.FromByteArray(buffer, dataOffset + 4);
        }

        internal override void Write(System.IO.BinaryWriter file, int offset)
        {
            base.Write(file, offset);

            int dataOffset = offset + 24 + this.NameSize + this.NodesOffsetsSize;

            file.Write((int)0);
            file.Write(dataOffset);

            this.WriteName(file);

            this.WriteNodesOffsets(file, offset);

            file.Write((int)this.HardpointType);
            file.Write(this.Position.ToByteArray());

            this.WriteNodes(file, offset);
        }
    }
}
