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
        public FaceGroup(bool alloc = true)
        {
            if (alloc)
            {
                this.Faces = new List<Face>();
                this.Textures = new List<string>();
            }
        }

        public IList<Face>? Faces { get; set; }

        public IList<string>? Textures { get; set; }

        public int TrianglesCount
        {
            get
            {
                if (this.Faces is null)
                {
                    return 0;
                }

                int count = 0;

                for (int i = 0; i < this.Faces.Count; i++)
                {
                    count += this.Faces[i].VerticesIndex.D < 0 ? 1 : 2;
                }

                return count;
            }
        }

        public int VerticesCount
        {
            get
            {
                if (this.Faces is null)
                {
                    return 0;
                }

                int count = 0;

                for (int i = 0; i < this.Faces.Count; i++)
                {
                    count += this.Faces[i].VerticesIndex.D < 0 ? 3 : 4;
                }

                return count;
            }
        }

        public int EdgesCount
        {
            get
            {
                if (this.Faces is null)
                {
                    return 0;
                }

                var distinctIndices = new int[this.VerticesCount];
                int distinctIndicesCount = 0;

                for (int faceIndex = 0; faceIndex < this.Faces.Count; faceIndex++)
                {
                    Face face = this.Faces[faceIndex];
                    Indices index = face.EdgesIndex;

                    if (index.A >= 0)
                    {
                        bool found = false;

                        for (int i = 0; i < distinctIndicesCount; i++)
                        {
                            if (distinctIndices[i] == index.A)
                            {
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            distinctIndices[distinctIndicesCount] = index.A;
                            distinctIndicesCount++;
                        }
                    }

                    if (index.B >= 0)
                    {
                        bool found = false;

                        for (int i = 0; i < distinctIndicesCount; i++)
                        {
                            if (distinctIndices[i] == index.B)
                            {
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            distinctIndices[distinctIndicesCount] = index.B;
                            distinctIndicesCount++;
                        }
                    }

                    if (index.C >= 0)
                    {
                        bool found = false;

                        for (int i = 0; i < distinctIndicesCount; i++)
                        {
                            if (distinctIndices[i] == index.C)
                            {
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            distinctIndices[distinctIndicesCount] = index.C;
                            distinctIndicesCount++;
                        }
                    }

                    if (index.D >= 0)
                    {
                        bool found = false;

                        for (int i = 0; i < distinctIndicesCount; i++)
                        {
                            if (distinctIndices[i] == index.D)
                            {
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            distinctIndices[distinctIndicesCount] = index.D;
                            distinctIndicesCount++;
                        }
                    }
                }

                return distinctIndicesCount;
            }
        }

        public FaceGroup Clone()
        {
            var faceGroup = new FaceGroup(false);

            if (this.Faces is not null)
            {
                faceGroup.Faces = new List<Face>(this.Faces.Count);

                foreach (var face in this.Faces)
                {
                    faceGroup.Faces.Add(face.Clone());
                }
            }

            if (this.Textures is not null)
            {
                faceGroup.Textures = new List<string>(this.Textures.Count);

                foreach (string texture in this.Textures)
                {
                    faceGroup.Textures.Add(texture);
                }
            }

            return faceGroup;
        }

        public void ComputeEdges()
        {
            if (this.Faces is null)
            {
                return;
            }

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
