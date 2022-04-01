// -----------------------------------------------------------------------
// <copyright file="MeshDescriptorNode.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt.Nodes
{
    using System;
    using System.Globalization;

    public sealed class MeshDescriptorNode : Node
    {
        public MeshDescriptorNode(int nodesCount = -1)
            : base(NodeType.MeshDescriptor, nodesCount)
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

        internal override void Parse(System.IO.BinaryReader file, int globalOffset, int offset)
        {
            base.Parse(file, globalOffset, offset);

            file.BaseStream.Position = offset + 20;
            int dataOffset = file.ReadInt32();

            if (dataOffset == 0)
            {
                return;
            }

            dataOffset -= globalOffset;

            file.BaseStream.Position = dataOffset;
            this.MeshType = (MeshType)file.ReadInt32();
            this.ExplosionType = (ExplosionTypes)file.ReadInt32();
            this.Span = Vector.Read(file);
            this.Center = Vector.Read(file);
            this.Min = Vector.Read(file);
            this.Max = Vector.Read(file);
            this.TargetId = file.ReadInt32();
            this.Target = Vector.Read(file);
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
            this.Span.Write(file);
            this.Center.Write(file);
            this.Min.Write(file);
            this.Max.Write(file);
            file.Write(this.TargetId);
            this.Target.Write(file);

            this.WriteNodes(file, offset);
        }
    }
}
