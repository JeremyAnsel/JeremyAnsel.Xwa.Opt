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
        public FaceDataNode()
            : base(NodeType.FaceData)
        {
        }

        public int EdgesCount { get; set; }

        public IList<FaceDataNodeData> Faces { get; private set; } = new List<FaceDataNodeData>();

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

        internal override void Parse(byte[] buffer, int globalOffset, int offset)
        {
            base.Parse(buffer, globalOffset, offset);

            int facesCount = BitConverter.ToInt32(buffer, offset + 16);
            int dataOffset = BitConverter.ToInt32(buffer, offset + 20);

            if (dataOffset == 0)
            {
                return;
            }

            this.Faces = new List<FaceDataNodeData>(facesCount);

            dataOffset -= globalOffset;

            this.EdgesCount = BitConverter.ToInt32(buffer, dataOffset + 0);
            dataOffset += 4;

            for (int i = 0; i < facesCount; i++)
            {
                FaceDataNodeData face = new FaceDataNodeData
                {
                    VerticesIndex = Indices.FromByteArray(buffer, dataOffset + 0),
                    EdgesIndex = Indices.FromByteArray(buffer, dataOffset + 16),
                    TextureCoordinatesIndex = Indices.FromByteArray(buffer, dataOffset + 32),
                    VertexNormalsIndex = Indices.FromByteArray(buffer, dataOffset + 48)
                };

                this.Faces.Add(face);
                dataOffset += 64;
            }

            for (int i = 0; i < facesCount; i++)
            {
                FaceDataNodeData face = this.Faces[i];

                face.Normal = Vector.FromByteArray(buffer, dataOffset);

                dataOffset += 12;
            }

            for (int i = 0; i < facesCount; i++)
            {
                FaceDataNodeData face = this.Faces[i];

                face.TexturingDirection = Vector.FromByteArray(buffer, dataOffset + 0);
                face.TexturingMagniture = Vector.FromByteArray(buffer, dataOffset + 12);

                dataOffset += 24;
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

            foreach (var face in this.Faces)
            {
                file.Write(face.VerticesIndex.ToByteArray());
                file.Write(face.EdgesIndex.ToByteArray());
                file.Write(face.TextureCoordinatesIndex.ToByteArray());
                file.Write(face.VertexNormalsIndex.ToByteArray());
            }

            foreach (var face in this.Faces)
            {
                file.Write(face.Normal.ToByteArray());
            }

            foreach (var face in this.Faces)
            {
                file.Write(face.TexturingDirection.ToByteArray());
                file.Write(face.TexturingMagniture.ToByteArray());
            }

            this.WriteNodes(file, offset);
        }
    }
}
