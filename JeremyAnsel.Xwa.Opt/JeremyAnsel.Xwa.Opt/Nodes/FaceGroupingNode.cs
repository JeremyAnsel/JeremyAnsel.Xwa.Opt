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
        public FaceGroupingNode()
            : base(NodeType.FaceGrouping)
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

        internal override void Parse(byte[] buffer, int globalOffset, int offset)
        {
            base.Parse(buffer, globalOffset, offset);

            int distancesCount = BitConverter.ToInt32(buffer, offset + 16);
            int distancesOffset = BitConverter.ToInt32(buffer, offset + 20);

            if (distancesCount == 0 || distancesOffset == 0)
            {
                return;
            }

            this.Distances = new List<float>(distancesCount);

            distancesOffset -= globalOffset;

            for (int i = 0; i < distancesCount; i++)
            {
                this.Distances.Add(BitConverter.ToSingle(buffer, distancesOffset + (i * 4)));
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
                foreach (float distance in this.Distances)
                {
                    file.Write(distance);
                }
            }

            this.WriteNodes(file, offset);
        }
    }
}
