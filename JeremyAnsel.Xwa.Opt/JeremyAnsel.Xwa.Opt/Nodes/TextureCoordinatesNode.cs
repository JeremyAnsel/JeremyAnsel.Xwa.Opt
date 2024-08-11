// -----------------------------------------------------------------------
// <copyright file="TextureCoordinatesNode.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    public sealed class TextureCoordinatesNode : Node
    {
        public TextureCoordinatesNode(int nodesCount = -1, bool alloc = true)
            : base(NodeType.TextureCoordinates, nodesCount)
        {
            if (alloc)
            {
                this.TextureVertices = new List<TextureCoordinates>();
            }
        }

        public IList<TextureCoordinates>? TextureVertices { get; set; }

        protected override int DataSize
        {
            get
            {
                return this.TextureVertices is null ? 0 : this.TextureVertices.Count * 8;
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "TextureCoordinatesNode: {0} texture vertices", this.TextureVertices is null ? 0 : this.TextureVertices.Count);
        }

        internal override void Parse(System.IO.BinaryReader file, int globalOffset, int offset)
        {
            base.Parse(file, globalOffset, offset);

            file.BaseStream.Position = offset + 16;
            int verticesCount = file.ReadInt32();
            int verticesOffset = file.ReadInt32();

            if (verticesCount == 0 || verticesOffset == 0)
            {
                return;
            }

            this.TextureVertices = new List<TextureCoordinates>(verticesCount);

            verticesOffset -= globalOffset;

            file.BaseStream.Position = verticesOffset;
            for (int i = 0; i < verticesCount; i++)
            {
                this.TextureVertices.Add(TextureCoordinates.Read(file));
            }
        }

        internal override void Write(System.IO.BinaryWriter file, int offset)
        {
            base.Write(file, offset);

            int dataOffset = this.DataSize == 0 ? 0 : (offset + 24 + this.NameSize + this.NodesOffsetsSize);
            int textureVerticesCount = this.TextureVertices is null ? 0 : this.TextureVertices.Count;

            file.Write(textureVerticesCount);
            file.Write(dataOffset);

            this.WriteName(file);

            this.WriteNodesOffsets(file, offset);

            if (dataOffset != 0)
            {
                for (int i = 0; i < textureVerticesCount; i++)
                {
                    this.TextureVertices![i].Write(file);
                }
            }

            this.WriteNodes(file, offset);
        }
    }
}
