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
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public sealed class TextureCoordinatesNode : Node
    {
        public TextureCoordinatesNode()
            : base(NodeTypes.TextureCoordinates)
        {
            this.TextureVertices = new List<TextureCoordinates>();
        }

        public IList<TextureCoordinates> TextureVertices { get; private set; }

        protected override int DataSize
        {
            get
            {
                return this.TextureVertices.Count * 8;
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "TextureCoordinatesNode: {0} texture vertices", this.TextureVertices.Count);
        }

        internal override void Parse(byte[] buffer, int globalOffset, int offset)
        {
            base.Parse(buffer, globalOffset, offset);

            int verticesCount = BitConverter.ToInt32(buffer, offset + 16);
            int verticesOffset = BitConverter.ToInt32(buffer, offset + 20);

            if (verticesCount == 0 || verticesOffset == 0)
            {
                return;
            }

            this.TextureVertices = new List<TextureCoordinates>(verticesCount);

            verticesOffset -= globalOffset;

            for (int i = 0; i < verticesCount; i++)
            {
                this.TextureVertices.Add(TextureCoordinates.FromByteArray(buffer, verticesOffset + (i * 8)));
            }
        }

        internal override void Write(System.IO.BinaryWriter file, int offset)
        {
            base.Write(file, offset);

            int dataOffset = this.DataSize == 0 ? 0 : (offset + 24 + this.NameSize + this.NodesOffsetsSize);

            file.Write(this.TextureVertices.Count);
            file.Write(dataOffset);

            this.WriteName(file);

            this.WriteNodesOffsets(file, offset);

            if (dataOffset != 0)
            {
                foreach (var vector in this.TextureVertices)
                {
                    file.Write(vector.ToByteArray());
                }
            }

            this.WriteNodes(file, offset);
        }
    }
}
