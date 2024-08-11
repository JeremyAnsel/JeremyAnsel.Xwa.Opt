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
        public const int DefaultPaletteLength = 8192;

        public TextureNode(int nodesCount = -1)
            : base(NodeType.Texture, nodesCount)
        {
        }

        public int UniqueId { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public byte[]? Palettes { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public byte[]? Bytes { get; set; }

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
        internal override void Parse(System.IO.BinaryReader file, int globalOffset, int offset)
        {
            base.Parse(file, globalOffset, offset);

            file.BaseStream.Position = offset + 16;
            this.UniqueId = file.ReadInt32();

            int dataOffset = file.ReadInt32();

            if (dataOffset == 0)
            {
                return;
            }

            dataOffset -= globalOffset;

            file.BaseStream.Position = dataOffset + 4;
            int paletteType = file.ReadInt32();
            int textureSize = file.ReadInt32();
            int dataSize = file.ReadInt32();

            this.Width = file.ReadInt32();
            this.Height = file.ReadInt32();

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
            file.BaseStream.Position = dataOffset + 24;
            file.Read(this.Bytes, 0, this.Bytes.Length);

            if (paletteType == 0)
            {
                file.BaseStream.Position = dataOffset;
                int paletteOffset = file.ReadInt32();

                if (paletteOffset == 0)
                {
                    throw new System.IO.InvalidDataException("texture palette not found");
                }

                paletteOffset -= globalOffset;

                this.Palettes = new byte[DefaultPaletteLength];
                file.BaseStream.Position = paletteOffset;
                file.Read(this.Palettes, 0, this.Palettes.Length);
            }
            else
            {
                this.Palettes = new byte[DefaultPaletteLength];
                file.BaseStream.Position = dataOffset + 24 + bytesSize;
                file.Read(this.Palettes, 0, this.Palettes.Length);
            }
        }

        internal override void Write(System.IO.BinaryWriter file, int offset)
        {
            base.Write(file, offset);

            int dataOffset = this.DataSize == 0 ? 0 : (offset + 24 + this.NameSize + this.NodesOffsetsSize);
            int paletteOffset = this.DataSize == 0 ? 0 : (dataOffset + 24 + this.Bytes!.Length);

            file.Write(this.UniqueId);
            file.Write(dataOffset);

            this.WriteName(file);

            this.WriteNodesOffsets(file, offset);

            if (dataOffset != 0)
            {
                file.Write(paletteOffset);
                file.Write((int)0);
                file.Write((this.Width * this.Height == this.Bytes!.Length) ? (int)0 : (int)(this.Width * this.Height));
                file.Write(this.Bytes.Length);
                file.Write(this.Width);
                file.Write(this.Height);
                file.Write(this.Bytes);

                if (this.Palettes != null)
                {
                    file.Write(this.Palettes);
                }
                else
                {
                    for (int i = 0; i < DefaultPaletteLength; i++)
                    {
                        file.Write((byte)0);
                    }
                }
            }

            this.WriteNodes(file, offset);
        }
    }
}
