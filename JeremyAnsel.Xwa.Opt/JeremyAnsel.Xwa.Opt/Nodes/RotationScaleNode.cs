// -----------------------------------------------------------------------
// <copyright file="RotationScaleNode.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public sealed class RotationScaleNode : Node
    {
        public RotationScaleNode()
            : base(NodeType.RotationScale)
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

        internal override void Parse(byte[] buffer, int globalOffset, int offset)
        {
            base.Parse(buffer, globalOffset, offset);

            int dataOffset = BitConverter.ToInt32(buffer, offset + 20);

            if (dataOffset == 0)
            {
                return;
            }

            dataOffset -= globalOffset;

            this.Pivot = Vector.FromByteArray(buffer, dataOffset + 0);
            this.Look = Vector.FromByteArray(buffer, dataOffset + 12);
            this.Up = Vector.FromByteArray(buffer, dataOffset + 24);
            this.Right = Vector.FromByteArray(buffer, dataOffset + 36);
        }

        internal override void Write(System.IO.BinaryWriter file, int offset)
        {
            base.Write(file, offset);

            int dataOffset = offset + 24 + this.NameSize + this.NodesOffsetsSize;

            file.Write((int)0);
            file.Write(dataOffset);

            this.WriteName(file);

            this.WriteNodesOffsets(file, offset);

            file.Write(this.Pivot.ToByteArray());
            file.Write(this.Look.ToByteArray());
            file.Write(this.Up.ToByteArray());
            file.Write(this.Right.ToByteArray());

            this.WriteNodes(file, offset);
        }
    }
}
