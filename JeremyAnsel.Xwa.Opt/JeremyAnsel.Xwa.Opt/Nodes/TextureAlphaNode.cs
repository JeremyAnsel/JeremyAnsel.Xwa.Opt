// -----------------------------------------------------------------------
// <copyright file="TextureAlphaNode.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt.Nodes
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    public sealed class TextureAlphaNode : Node
    {
        public TextureAlphaNode(int nodesCount = -1)
            : base(NodeType.TextureAlpha, nodesCount)
        {
        }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public byte[] Bytes { get; set; }

        protected override int DataSize
        {
            get
            {
                return this.Bytes == null ? 0 : this.Bytes.Length;
            }
        }

        internal override void Parse(System.IO.BinaryReader file, int globalOffset, int offset)
        {
            base.Parse(file, globalOffset, offset);

            file.BaseStream.Position = offset + 16;
            int bytesCount = file.ReadInt32();
            int dataOffset = file.ReadInt32();

            if (dataOffset == 0)
            {
                return;
            }

            dataOffset -= globalOffset;

            this.Bytes = new byte[bytesCount];

            if (bytesCount != 0)
            {
                file.BaseStream.Position = dataOffset;
                file.Read(this.Bytes, 0, bytesCount);
            }
        }

        internal override void Write(System.IO.BinaryWriter file, int offset)
        {
            base.Write(file, offset);

            int dataOffset = this.Bytes == null ? 0 : (offset + 24 + this.NameSize + this.NodesOffsetsSize);

            file.Write(this.Bytes == null ? (int)0 : this.Bytes.Length);
            file.Write(dataOffset);

            this.WriteName(file);

            this.WriteNodesOffsets(file, offset);

            if (this.Bytes != null && this.Bytes.Length != 0)
            {
                file.Write(this.Bytes);
            }

            this.WriteNodes(file, offset);
        }
    }
}
