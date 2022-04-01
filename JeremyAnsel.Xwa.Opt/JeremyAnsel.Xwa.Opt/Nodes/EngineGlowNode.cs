// -----------------------------------------------------------------------
// <copyright file="EngineGlowNode.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt.Nodes
{
    using System;
    using System.Globalization;

    public sealed class EngineGlowNode : Node
    {
        public EngineGlowNode(int nodesCount = -1)
            : base(NodeType.EngineGlow, nodesCount)
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
            this.IsDisabled = file.ReadInt32() != 0;
            this.CoreColor = file.ReadUInt32();
            this.OuterColor = file.ReadUInt32();
            this.Format = Vector.Read(file);
            this.Position = Vector.Read(file);
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

            file.Write(this.IsDisabled ? (int)1 : (int)0);
            file.Write(this.CoreColor);
            file.Write(this.OuterColor);
            this.Format.Write(file);
            this.Position.Write(file);
            this.Look.Write(file);
            this.Up.Write(file);
            this.Right.Write(file);

            this.WriteNodes(file, offset);
        }
    }
}
