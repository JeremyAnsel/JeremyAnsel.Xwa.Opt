// -----------------------------------------------------------------------
// <copyright file="FaceData.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt.Nodes
{
    public sealed class FaceDataNodeData
    {
        public Indices VerticesIndex { get; set; }

        public Indices EdgesIndex { get; set; }

        public Indices TextureCoordinatesIndex { get; set; }

        public Indices VertexNormalsIndex { get; set; }

        public Vector Normal { get; set; }

        public Vector TexturingDirection { get; set; }

        public Vector TexturingMagniture { get; set; }
    }
}
