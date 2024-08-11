// -----------------------------------------------------------------------
// <copyright file="Face.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt
{
    public sealed class Face
    {
        public Face()
        {
            this.VerticesIndex = Indices.Empty;
            this.EdgesIndex = Indices.Empty;
            this.TextureCoordinatesIndex = Indices.Empty;
            this.VertexNormalsIndex = Indices.Empty;
            this.Normal = Vector.Empty;
            this.TexturingDirection = Vector.Empty;
            this.TexturingMagniture = Vector.Empty;
        }

        public Indices VerticesIndex { get; set; }

        public Indices EdgesIndex { get; set; }

        public Indices TextureCoordinatesIndex { get; set; }

        public Indices VertexNormalsIndex { get; set; }

        public Vector Normal { get; set; }

        public Vector TexturingDirection { get; set; }

        public Vector TexturingMagniture { get; set; }

        public int TrianglesCount
        {
            get
            {
                return this.VerticesIndex.D < 0 ? 1 : 2;
            }
        }

        public int VerticesCount
        {
            get
            {
                return this.VerticesIndex.D < 0 ? 3 : 4;
            }
        }

        public Face Clone()
        {
            var face = new Face
            {
                VerticesIndex = this.VerticesIndex,
                EdgesIndex = this.EdgesIndex,
                TextureCoordinatesIndex = this.TextureCoordinatesIndex,
                VertexNormalsIndex = this.VertexNormalsIndex,
                Normal = this.Normal,
                TexturingDirection = this.TexturingDirection,
                TexturingMagniture = this.TexturingMagniture
            };

            return face;
        }

        internal bool HasFlatTexture(Mesh mesh)
        {
            int texCoordCount = 0;
            int texUCount = 0;
            int texVCount = 0;

            int polyVerts = this.VerticesCount;
            Indices texIndex = this.TextureCoordinatesIndex;

            for (int i = 0; i < polyVerts; i++)
            {
                TextureCoordinates vertexI;

                switch (i)
                {
                    case 0:
                        vertexI = mesh.TextureCoordinates![texIndex.A];
                        break;

                    case 1:
                        vertexI = mesh.TextureCoordinates![texIndex.B];
                        break;

                    case 2:
                        vertexI = mesh.TextureCoordinates![texIndex.C];
                        break;

                    case 3:
                        vertexI = mesh.TextureCoordinates![texIndex.D];
                        break;

                    default:
                        vertexI = default;
                        break;
                }

                bool foundUV = false;
                bool foundU = false;
                bool foundV = false;

                for (int j = 0; j < i; j++)
                {
                    TextureCoordinates vertexJ;

                    switch (j)
                    {
                        case 0:
                            vertexJ = mesh.TextureCoordinates![texIndex.A];
                            break;

                        case 1:
                            vertexJ = mesh.TextureCoordinates![texIndex.B];
                            break;

                        case 2:
                            vertexJ = mesh.TextureCoordinates![texIndex.C];
                            break;

                        case 3:
                            vertexJ = mesh.TextureCoordinates![texIndex.D];
                            break;

                        default:
                            vertexJ = default;
                            break;
                    }

                    if (vertexI == vertexJ)
                    {
                        foundUV = true;
                        break;
                    }

                    if (vertexI.U == vertexJ.U)
                    {
                        foundU = true;
                        break;
                    }

                    if (vertexI.V == vertexJ.V)
                    {
                        foundV = true;
                        break;
                    }
                }

                if (!foundUV)
                {
                    texCoordCount++;
                }

                if (!foundU)
                {
                    texUCount++;
                }

                if (!foundV)
                {
                    texVCount++;
                }
            }

            if (texCoordCount < 3 || texUCount < 2 || texVCount < 2)
            {
                return true;
            }

            return false;
        }
    }
}
