// -----------------------------------------------------------------------
// <copyright file="FaceData.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public sealed class FaceDataNodeData
    {
        public Index VerticesIndex { get; set; }

        public Index EdgesIndex { get; set; }

        public Index TextureCoordinatesIndex { get; set; }

        public Index VertexNormalsIndex { get; set; }

        public Vector Normal { get; set; }

        public Vector TexturingDirection { get; set; }

        public Vector TexturingMagniture { get; set; }
    }
}
