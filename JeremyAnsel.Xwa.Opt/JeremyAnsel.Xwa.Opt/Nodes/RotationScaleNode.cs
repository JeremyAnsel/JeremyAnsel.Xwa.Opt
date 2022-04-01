// -----------------------------------------------------------------------
// <copyright file="RotationScaleNode.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt.Nodes
{
    using System;

    public sealed class RotationScaleNode : Node
    {
        public RotationScaleNode(int nodesCount = -1)
            : base(NodeType.RotationScale, nodesCount)
        {
        }

        public Vector Pivot { get; set; }

        public Vector Look { get; set; }

        public Vector Up { get; set; }

        public Vector Right { get; set; }

        protected override int DataSize
        {
            get
            {
                return 48;
            }
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
            this.Pivot = Vector.Read(file);
            this.Look = Vector.Read(file);
            this.Up = Vector.Read(file);
            this.Right = Vector.Read(file);
        }

        internal override void Write(System.IO.BinaryWriter file, int offset)
        {
            base.Write(file, offset);

            int dataOffset = offset + 24 + this.NameSize + this.NodesOffsetsSize;

            file.Write((int)0);
            file.Write(dataOffset);

            this.WriteName(file);

            this.WriteNodesOffsets(file, offset);

            this.Pivot.Write(file);
            this.Look.Write(file);
            this.Up.Write(file);
            this.Right.Write(file);

            this.WriteNodes(file, offset);
        }
    }
}
