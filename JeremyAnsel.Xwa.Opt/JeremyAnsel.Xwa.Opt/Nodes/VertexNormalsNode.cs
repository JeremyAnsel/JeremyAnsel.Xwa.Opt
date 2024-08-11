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
        public VertexNormalsNode(int nodesCount = -1, bool alloc = true)
            : base(NodeType.VertexNormals, nodesCount)
        {
            if (alloc)
            {
                this.Normals = new List<Vector>();
            }
        }

        public IList<Vector>? Normals { get; set; }

        protected override int DataSize
        {
            get
            {
                return this.Normals is null ? 0 : this.Normals.Count * 12;
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "VectexNormalsNode: {0} vertex normals", this.Normals is null ? 0 : this.Normals.Count);
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

            this.Normals = new List<Vector>(verticesCount);

            verticesOffset -= globalOffset;

            file.BaseStream.Position = verticesOffset;
            for (int i = 0; i < verticesCount; i++)
            {
                this.Normals.Add(Vector.Read(file));
            }
        }

        internal override void Write(System.IO.BinaryWriter file, int offset)
        {
            base.Write(file, offset);

            int dataOffset = this.DataSize == 0 ? 0 : (offset + 24 + this.NameSize + this.NodesOffsetsSize);
            int normalsCount = this.Normals is null ? 0 : this.Normals.Count;

            file.Write(normalsCount);
            file.Write(dataOffset);

            this.WriteName(file);

            this.WriteNodesOffsets(file, offset);

            if (dataOffset != 0)
            {
                for (int i = 0; i < normalsCount; i++)
                {
                    this.Normals![i].Write(file);
                }
            }

            this.WriteNodes(file, offset);
        }
    }
}
