// -----------------------------------------------------------------------
// <copyright file="TextureNode.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt.Nodes
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    public sealed class TextureNode : Node
    {
        public TextureNode()
            : base(NodeType.Texture)
        {
        }

        public int UniqueId { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public byte[] Palettes { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public byte[] Bytes { get; set; }

        protected override int DataSize
        {
            get
            {
                return this.Bytes == null ? 0 : (24 + this.Bytes.Length + (this.Palettes == null ? 0 : this.Palettes.Length));
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "TextureNode: {0} {1}x{2}", this.Name, this.Width, this.Height);
        }

        [SuppressMessage("Globalization", "CA1303:Ne pas passer de littéraux en paramètres localisés", Justification = "Reviewed.")]
        internal override void Parse(byte[] buffer, int globalOffset, int offset)
        {
            base.Parse(buffer, globalOffset, offset);

            this.UniqueId = BitConverter.ToInt32(buffer, offset + 16);

            int dataOffset = BitConverter.ToInt32(buffer, offset + 20);

            if (dataOffset == 0)
            {
                return;
            }

            dataOffset -= globalOffset;

            int paletteType = BitConverter.ToInt32(buffer, dataOffset + 4);
            int textureSize = BitConverter.ToInt32(buffer, dataOffset + 8);
            int dataSize = BitConverter.ToInt32(buffer, dataOffset + 12);

            this.Width = BitConverter.ToInt32(buffer, dataOffset + 16);
            this.Height = BitConverter.ToInt32(buffer, dataOffset + 20);

            int bytesSize;

            if (this.Width * this.Height == textureSize)
            {
                bytesSize = dataSize;
            }
            else
            {
                bytesSize = this.Width * this.Height;
            }

            this.Bytes = new byte[bytesSize];
            Array.Copy(buffer, dataOffset + 24, this.Bytes, 0, this.Bytes.Length);

            if (paletteType == 0)
            {
                int paletteOffset = BitConverter.ToInt32(buffer, dataOffset + 0);

                if (paletteOffset == 0)
                {
                    throw new System.IO.InvalidDataException("texture palette not found");
                }

                paletteOffset -= globalOffset;

                this.Palettes = new byte[8192];
                Array.Copy(buffer, paletteOffset, this.Palettes, 0, this.Palettes.Length);
            }
            else
            {
                this.Palettes = new byte[8192];
                Array.Copy(buffer, dataOffset + 24 + bytesSize, this.Palettes, 0, this.Palettes.Length);
            }
        }

        internal override void Write(System.IO.BinaryWriter file, int offset)
        {
            base.Write(file, offset);

            int dataOffset = this.DataSize == 0 ? 0 : (offset + 24 + this.NameSize + this.NodesOffsetsSize);
            int paletteOffset = this.DataSize == 0 ? 0 : (dataOffset + 24 + this.Bytes.Length);

            file.Write(this.UniqueId);
            file.Write(dataOffset);

            this.WriteName(file);

            this.WriteNodesOffsets(file, offset);

            if (dataOffset != 0)
            {
                file.Write(paletteOffset);
                file.Write((int)0);
                file.Write((this.Width * this.Height == this.Bytes.Length) ? (int)0 : (int)(this.Width * this.Height));
                file.Write(this.Bytes.Length);
                file.Write(this.Width);
                file.Write(this.Height);
                file.Write(this.Bytes);
                file.Write(this.Palettes ?? new byte[8192]);
            }

            this.WriteNodes(file, offset);
        }
    }
}
