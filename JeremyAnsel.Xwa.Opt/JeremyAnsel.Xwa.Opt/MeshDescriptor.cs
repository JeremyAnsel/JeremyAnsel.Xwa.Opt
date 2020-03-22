// -----------------------------------------------------------------------
// <copyright file="MeshDescriptor.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt
{
    public class MeshDescriptor
    {
        public MeshDescriptor()
        {
            this.MeshType = Opt.MeshType.Default;
            this.ExplosionType = Opt.ExplosionTypes.None;
            this.Span = Vector.Empty;
            this.Center = Vector.Empty;
            this.Min = Vector.Empty;
            this.Max = Vector.Empty;
            this.TargetId = 0;
            this.Target = Vector.Empty;
        }

        public MeshType MeshType { get; set; }

        public ExplosionTypes ExplosionType { get; set; }

        public Vector Span { get; set; }

        public Vector Center { get; set; }

        public Vector Min { get; set; }

        public Vector Max { get; set; }

        public int TargetId { get; set; }

        public Vector Target { get; set; }

        public MeshDescriptor Clone()
        {
            var descriptor = new MeshDescriptor
            {
                MeshType = this.MeshType,
                ExplosionType = this.ExplosionType,
                Span = this.Span,
                Center = this.Center,
                Min = this.Min,
                Max = this.Max,
                TargetId = this.TargetId,
                Target = this.Target
            };

            return descriptor;
        }
    }
}
