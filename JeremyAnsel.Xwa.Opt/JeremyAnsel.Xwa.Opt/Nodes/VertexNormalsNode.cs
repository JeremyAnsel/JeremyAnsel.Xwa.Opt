// -----------------------------------------------------------------------
// <copyright file="VertexNormalsNode.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    public sealed class VertexNormalsNode : Node
    {
        public VertexNormalsNode()
            : base(NodeType.VertexNormals)
        {
        }

        public IList<Vector> Normals { get; private set; } = new List<Vector>();

        protected override int DataSize
        {
            get
            {
                return this.Normals.Count * 12;
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "VectexNormalsNode: {0} vertex normals", this.Normals.Count);
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

            this.Normals = new List<Vector>(verticesCount);

            verticesOffset -= globalOffset;

            for (int i = 0; i < verticesCount; i++)
            {
                this.Normals.Add(Vector.FromByteArray(buffer, verticesOffset + (i * 12)));
            }
        }

        internal override void Write(System.IO.BinaryWriter file, int offset)
        {
            base.Write(file, offset);

            int dataOffset = this.DataSize == 0 ? 0 : (offset + 24 + this.NameSize + this.NodesOffsetsSize);

            file.Write(this.Normals.Count);
            file.Write(dataOffset);

            this.WriteName(file);

            this.WriteNodesOffsets(file, offset);

            if (dataOffset != 0)
            {
                foreach (var vector in this.Normals)
                {
                    file.Write(vector.ToByteArray());
                }
            }

            this.WriteNodes(file, offset);
        }
    }
}
