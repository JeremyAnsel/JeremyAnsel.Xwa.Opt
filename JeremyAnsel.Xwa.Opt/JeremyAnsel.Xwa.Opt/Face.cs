// -----------------------------------------------------------------------
// <copyright file="Face.cs" company="">
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

    public class Face
    {
        public Face()
        {
            this.VerticesIndex = Index.Empty;
            this.EdgesIndex = Index.Empty;
            this.TextureCoordinatesIndex = Index.Empty;
            this.VertexNormalsIndex = Index.Empty;
            this.Normal = Vector.Empty;
            this.TexturingDirection = Vector.Empty;
            this.TexturingMagniture = Vector.Empty;
        }

        public Index VerticesIndex { get; set; }

        public Index EdgesIndex { get; set; }

        public Index TextureCoordinatesIndex { get; set; }

        public Index VertexNormalsIndex { get; set; }

        public Vector Normal { get; set; }

        public Vector TexturingDirection { get; set; }

        public Vector TexturingMagniture { get; set; }
    }
}
