// -----------------------------------------------------------------------
// <copyright file="HardPointNode.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt.Nodes
{
    using System;
    using System.Globalization;

    public sealed class HardpointNode : Node
    {
        public HardpointNode(int nodesCount = -1)
            : base(NodeType.Hardpoint, nodesCount)
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

            file.BaseStream.Position = dataOffset;
            this.HardpointType = (HardpointType)file.ReadInt32();
            this.Position = Vector.Read(file);
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
            this.Position.Write(file);

            this.WriteNodes(file, offset);
        }
    }
}
