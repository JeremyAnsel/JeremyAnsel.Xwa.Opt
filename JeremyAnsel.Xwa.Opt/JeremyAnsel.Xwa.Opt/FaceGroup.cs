// -----------------------------------------------------------------------
// <copyright file="FaceGroup.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class FaceGroup
    {
        public IList<Face> Faces { get; } = new List<Face>();

        public IList<string> Textures { get; } = new List<string>();

        public int TrianglesCount
        {
            get
            {
                return this.Faces.Sum(t => t.VerticesIndex.D < 0 ? 1 : 2);
            }
        }

        public int VerticesCount
        {
            get
            {
                return this.Faces.Sum(t => t.VerticesIndex.D < 0 ? 3 : 4);
            }
        }

        public int EdgesCount
        {
            get
            {
                return this.Faces
                .SelectMany(t => new List<int>()
                    {
                        t.EdgesIndex.A,
                        t.EdgesIndex.B,
                        t.EdgesIndex.C,
                        t.EdgesIndex.D
                    })
                .Where(t => t >= 0)
                .Distinct()
                .Count();
            }
        }

        public FaceGroup Clone()
        {
            var faceGroup = new FaceGroup();

            foreach (var face in this.Faces)
            {
                faceGroup.Faces.Add(face.Clone());
            }

            foreach (string texture in this.Textures)
            {
                faceGroup.Textures.Add(texture);
            }

            return faceGroup;
        }

        public void ComputeEdges()
        {
            List<Tuple<int, int>> edges = new List<Tuple<int, int>>();

            int getEdgeIndex(int a, int b)
            {
                var edge = a < b ? new Tuple<int, int>(a, b) : new Tuple<int, int>(b, a);

                for (int i = 0; i < edges.Count; i++)
                {
                    if (edges[i].Item1 == edge.Item1 && edges[i].Item2 == edge.Item2)
                    {
                        return i;
                    }
                }

                edges.Add(edge);
                return edges.Count - 1;
            }

            foreach (var face in this.Faces)
            {
                Indices vertex = face.VerticesIndex;

                if (vertex.D < 0)
                {
                    int a = getEdgeIndex(vertex.A, vertex.B);
                    int b = getEdgeIndex(vertex.B, vertex.C);
                    int c = getEdgeIndex(vertex.C, vertex.A);

                    face.EdgesIndex = new Indices(a, b, c);
                }
                else
                {
                    int a = getEdgeIndex(vertex.A, vertex.B);
                    int b = getEdgeIndex(vertex.B, vertex.C);
                    int c = getEdgeIndex(vertex.C, vertex.D);
                    int d = getEdgeIndex(vertex.D, vertex.A);

                    face.EdgesIndex = new Indices(a, b, c, d);
                }
            }
        }
    }
}
