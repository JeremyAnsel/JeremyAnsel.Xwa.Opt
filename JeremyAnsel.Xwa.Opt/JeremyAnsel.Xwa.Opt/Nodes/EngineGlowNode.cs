// -----------------------------------------------------------------------
// <copyright file="EngineGlowNode.cs" company="">
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

    public sealed class EngineGlowNode : Node
    {
        public EngineGlowNode()
            : base(NodeTypes.EngineGlow)
        {
        }

        public bool IsDisabled { get; set; }

        public uint CoreColor { get; set; }

        public uint OuterColor { get; set; }

        public Vector Format { get; set; }

        public Vector Position { get; set; }

        public Vector Look { get; set; }

        public Vector Up { get; set; }

        public Vector Right { get; set; }

        protected override int DataSize
        {
            get
            {
                return 72;
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "EngineGlowNode: {0:x8},{1:x8} at {2}", this.CoreColor, this.OuterColor, this.Position);
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

            this.IsDisabled = BitConverter.ToInt32(buffer, dataOffset + 0) != 0;
            this.CoreColor = BitConverter.ToUInt32(buffer, dataOffset + 4);
            this.OuterColor = BitConverter.ToUInt32(buffer, dataOffset + 8);
            this.Format = Vector.FromByteArray(buffer, dataOffset + 12);
            this.Position = Vector.FromByteArray(buffer, dataOffset + 24);
            this.Look = Vector.FromByteArray(buffer, dataOffset + 36);
            this.Up = Vector.FromByteArray(buffer, dataOffset + 48);
            this.Right = Vector.FromByteArray(buffer, dataOffset + 60);
        }

        internal override void Write(System.IO.BinaryWriter file, int offset)
        {
            base.Write(file, offset);

            int dataOffset = offset + 24 + this.NameSize + this.NodesOffsetsSize;

            file.Write((int)0);
            file.Write(dataOffset);

            this.WriteName(file);

            this.WriteNodesOffsets(file, offset);

            file.Write(this.IsDisabled ? (int)1 : (int)0);
            file.Write(this.CoreColor);
            file.Write(this.OuterColor);
            file.Write(this.Format.ToByteArray());
            file.Write(this.Position.ToByteArray());
            file.Write(this.Look.ToByteArray());
            file.Write(this.Up.ToByteArray());
            file.Write(this.Right.ToByteArray());

            this.WriteNodes(file, offset);
        }
    }
}
