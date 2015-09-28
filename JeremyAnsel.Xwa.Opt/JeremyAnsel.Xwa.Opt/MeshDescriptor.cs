// -----------------------------------------------------------------------
// <copyright file="MeshDescriptor.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class MeshDescriptor
    {
        public MeshDescriptor()
        {
            this.MeshType = Opt.MeshTypes.Default;
            this.ExplosionType = Opt.ExplosionTypes.None;
            this.Span = Vector.Empty;
            this.Center = Vector.Empty;
            this.Min = Vector.Empty;
            this.Max = Vector.Empty;
            this.TargetId = 0;
            this.Target = Vector.Empty;
        }

        public MeshTypes MeshType { get; set; }

        public ExplosionTypes ExplosionType { get; set; }

        public Vector Span { get; set; }

        public Vector Center { get; set; }

        public Vector Min { get; set; }

        public Vector Max { get; set; }

        public int TargetId { get; set; }

        public Vector Target { get; set; }
    }
}
