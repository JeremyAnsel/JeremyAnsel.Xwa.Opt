// -----------------------------------------------------------------------
// <copyright file="FaceDataNode.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    public sealed class FaceDataNode : Node
    {
        public FaceDataNode(int nodesCount = -1, bool alloc = true)
            : base(NodeType.FaceData, nodesCount)
        {
            if (alloc)
            {
                this.Faces = new List<Face>();
            }
        }

        public int EdgesCount { get; set; }

        public IList<Face> Faces { get; set; }

        protected override int DataSize
        {
            get
            {
                return 4 + (this.Faces.Count * 100);
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "FaceDataNode: {0} faces", this.Faces.Count);
        }

        internal override void Parse(System.IO.BinaryReader file, int globalOffset, int offset)
        {
            base.Parse(file, globalOffset, offset);

            file.BaseStream.Position = offset + 16;
            int facesCount = file.ReadInt32();
            int dataOffset = file.ReadInt32();

            if (dataOffset == 0)
            {
                return;
            }

            this.Faces = new List<Face>(facesCount);

            dataOffset -= globalOffset;

            file.BaseStream.Position = dataOffset;
            this.EdgesCount = file.ReadInt32();

            for (int i = 0; i < facesCount; i++)
            {
                Face face = new Face
                {
                    VerticesIndex = Indices.Read(file),
                    EdgesIndex = Indices.Read(file),
                    TextureCoordinatesIndex = Indices.Read(file),
                    VertexNormalsIndex = Indices.Read(file)
                };

                this.Faces.Add(face);
            }

            for (int i = 0; i < facesCount; i++)
            {
                Face face = this.Faces[i];

                face.Normal = Vector.Read(file);
            }

            for (int i = 0; i < facesCount; i++)
            {
                Face face = this.Faces[i];

                face.TexturingDirection = Vector.Read(file);
                face.TexturingMagniture = Vector.Read(file);
            }
        }

        internal override void Write(System.IO.BinaryWriter file, int offset)
        {
            base.Write(file, offset);

            int dataOffset = offset + 24 + this.NameSize + this.NodesOffsetsSize;

            file.Write(this.Faces.Count);
            file.Write(dataOffset);

            this.WriteName(file);

            this.WriteNodesOffsets(file, offset);

            file.Write(this.EdgesCount);

            for (int i = 0; i < this.Faces.Count; i++)
            {
                Face face = this.Faces[i];
                face.VerticesIndex.Write(file);
                face.EdgesIndex.Write(file);
                face.TextureCoordinatesIndex.Write(file);
                face.VertexNormalsIndex.Write(file);
            }

            for (int i = 0; i < this.Faces.Count; i++)
            {
                Face face = this.Faces[i];
                face.Normal.Write(file);
            }

            for (int i = 0; i < this.Faces.Count; i++)
            {
                Face face = this.Faces[i];
                face.TexturingDirection.Write(file);
                face.TexturingMagniture.Write(file);
            }

            this.WriteNodes(file, offset);
        }
    }
}
