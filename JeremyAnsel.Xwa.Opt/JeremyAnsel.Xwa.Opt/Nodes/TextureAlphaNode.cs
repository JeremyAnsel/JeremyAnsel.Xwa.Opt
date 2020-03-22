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
        public TextureAlphaNode()
            : base(NodeType.TextureAlpha)
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

        internal override void Parse(byte[] buffer, int globalOffset, int offset)
        {
            base.Parse(buffer, globalOffset, offset);

            int bytesCount = BitConverter.ToInt32(buffer, offset + 16);
            int dataOffset = BitConverter.ToInt32(buffer, offset + 20);

            if (dataOffset == 0)
            {
                return;
            }

            dataOffset -= globalOffset;

            this.Bytes = new byte[bytesCount];

            if (bytesCount != 0)
            {
                Array.Copy(buffer, dataOffset, this.Bytes, 0, bytesCount);
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
