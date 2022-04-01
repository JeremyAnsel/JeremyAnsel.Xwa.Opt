// -----------------------------------------------------------------------
// <copyright file="FaceGroupingNode.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt.Nodes
{
    using System;
    using System.Collections.Generic;

    public sealed class FaceGroupingNode : Node
    {
        public FaceGroupingNode(int nodesCount = -1)
            : base(NodeType.FaceGrouping, nodesCount)
        {
        }

        public IList<float> Distances { get; private set; } = new List<float>();

        protected override int DataSize
        {
            get
            {
                return this.Distances.Count * 4;
            }
        }

        internal override void Parse(System.IO.BinaryReader file, int globalOffset, int offset)
        {
            base.Parse(file, globalOffset, offset);

            file.BaseStream.Position = offset + 16;
            int distancesCount = file.ReadInt32();
            int distancesOffset = file.ReadInt32();

            if (distancesCount == 0 || distancesOffset == 0)
            {
                return;
            }

            this.Distances = new List<float>(distancesCount);

            distancesOffset -= globalOffset;

            file.BaseStream.Position = distancesOffset;
            for (int i = 0; i < distancesCount; i++)
            {
                this.Distances.Add(file.ReadSingle());
            }
        }

        internal override void Write(System.IO.BinaryWriter file, int offset)
        {
            base.Write(file, offset);

            int dataOffset = this.DataSize == 0 ? 0 : (offset + 24 + this.NameSize + this.NodesOffsetsSize);

            file.Write(this.Distances.Count);
            file.Write(dataOffset);

            this.WriteName(file);

            this.WriteNodesOffsets(file, offset);

            if (dataOffset != 0)
            {
                for (int i = 0; i < this.Distances.Count; i++)
                {
                    float distance = this.Distances[i];
                    file.Write(distance);
                }
            }

            this.WriteNodes(file, offset);
        }
    }
}
