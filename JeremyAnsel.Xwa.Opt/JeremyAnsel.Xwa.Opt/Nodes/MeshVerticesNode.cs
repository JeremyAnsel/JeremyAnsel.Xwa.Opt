// -----------------------------------------------------------------------
// <copyright file="MeshVerticesNode.cs" company="">
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

    public sealed class MeshVerticesNode : Node
    {
        public MeshVerticesNode()
            : base(NodeTypes.MeshVertices)
        {
            this.Vertices = new List<Vector>();
        }

        public IList<Vector> Vertices { get; private set; }

        protected override int DataSize
        {
            get
            {
                return this.Vertices.Count * 12;
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "MeshVerticesNode: {0} vertices", this.Vertices.Count);
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

            this.Vertices = new List<Vector>(verticesCount);

            verticesOffset -= globalOffset;

            for (int i = 0; i < verticesCount; i++)
            {
                this.Vertices.Add(Vector.FromByteArray(buffer, verticesOffset + (i * 12)));
            }
        }

        internal override void Write(System.IO.BinaryWriter file, int offset)
        {
            base.Write(file, offset);

            int dataOffset = this.DataSize == 0 ? 0 : (offset + 24 + this.NameSize + this.NodesOffsetsSize);

            file.Write(this.Vertices.Count);
            file.Write(dataOffset);

            this.WriteName(file);

            this.WriteNodesOffsets(file, offset);

            if (dataOffset != 0)
            {
                foreach (var vector in this.Vertices)
                {
                    file.Write(vector.ToByteArray());
                }
            }

            this.WriteNodes(file, offset);
        }
    }
}
