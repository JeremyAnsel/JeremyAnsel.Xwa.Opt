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

    public sealed class MeshVerticesNode : Node
    {
        public MeshVerticesNode(int nodesCount = -1, bool alloc = true)
            : base(NodeType.MeshVertices, nodesCount)
        {
            if (alloc)
            {
                this.Vertices = new List<Vector>();
            }
        }

        public IList<Vector>? Vertices { get; set; }

        protected override int DataSize
        {
            get
            {
                return this.Vertices is null ? 0 : this.Vertices.Count * 12;
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "MeshVerticesNode: {0} vertices", this.Vertices is null ? 0 : this.Vertices.Count);
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

            this.Vertices = new List<Vector>(verticesCount);

            verticesOffset -= globalOffset;

            file.BaseStream.Position = verticesOffset;
            for (int i = 0; i < verticesCount; i++)
            {
                this.Vertices.Add(Vector.Read(file));
            }
        }

        internal override void Write(System.IO.BinaryWriter file, int offset)
        {
            base.Write(file, offset);

            int dataOffset = this.DataSize == 0 ? 0 : (offset + 24 + this.NameSize + this.NodesOffsetsSize);
            int verticesCount = this.Vertices is null ? 0 : this.Vertices.Count;

            file.Write(verticesCount);
            file.Write(dataOffset);

            this.WriteName(file);

            this.WriteNodesOffsets(file, offset);

            if (dataOffset != 0)
            {
                for (int i = 0; i < verticesCount; i++)
                {
                    this.Vertices![i].Write(file);
                }
            }

            this.WriteNodes(file, offset);
        }
    }
}
