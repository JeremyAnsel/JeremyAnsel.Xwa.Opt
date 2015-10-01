// -----------------------------------------------------------------------
// <copyright file="MeshDescriptorNode.cs" company="">
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

    public sealed class MeshDescriptorNode : Node
    {
        public MeshDescriptorNode()
            : base(NodeType.MeshDescriptor)
        {
        }

        public MeshType MeshType { get; set; }

        public ExplosionTypes ExplosionType { get; set; }

        public Vector Span { get; set; }

        public Vector Center { get; set; }

        public Vector Min { get; set; }

        public Vector Max { get; set; }

        public int TargetId { get; set; }

        public Vector Target { get; set; }

        protected override int DataSize
        {
            get
            {
                return 72;
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "MeshDescriptorNode: {0}", this.MeshType);
        }

        internal override void Parse(byte[] buffer, int globalOffset, int offset)
        {
            base.Parse(buffer, globalOffset, offset);

            int dataOffset = BitConverter.ToInt32(buffer, offset + 20);

            if (dataOffset == 0)
            {
                return;
            }

            dataOffset -= globalOffset;

            this.MeshType = (MeshType)BitConverter.ToInt32(buffer, dataOffset + 0);
            this.ExplosionType = (ExplosionTypes)BitConverter.ToInt32(buffer, dataOffset + 4);
            this.Span = Vector.FromByteArray(buffer, dataOffset + 8);
            this.Center = Vector.FromByteArray(buffer, dataOffset + 20);
            this.Min = Vector.FromByteArray(buffer, dataOffset + 32);
            this.Max = Vector.FromByteArray(buffer, dataOffset + 44);
            this.TargetId = BitConverter.ToInt32(buffer, dataOffset + 56);
            this.Target = Vector.FromByteArray(buffer, dataOffset + 60);
        }

        internal override void Write(System.IO.BinaryWriter file, int offset)
        {
            base.Write(file, offset);

            int dataOffset = offset + 24 + this.NameSize + this.NodesOffsetsSize;

            file.Write((int)0);
            file.Write(dataOffset);

            this.WriteName(file);

            this.WriteNodesOffsets(file, offset);

            file.Write((int)this.MeshType);
            file.Write((int)this.ExplosionType);
            file.Write(this.Span.ToByteArray());
            file.Write(this.Center.ToByteArray());
            file.Write(this.Min.ToByteArray());
            file.Write(this.Max.ToByteArray());
            file.Write(this.TargetId);
            file.Write(this.Target.ToByteArray());

            this.WriteNodes(file, offset);
        }
    }
}
