// -----------------------------------------------------------------------
// <copyright file="Mesh.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    public class Mesh
    {
        public IList<Vector> Vertices { get; private set; } = new List<Vector>();

        public IList<TextureCoordinates> TextureCoordinates { get; private set; } = new List<TextureCoordinates>();

        public IList<Vector> VertexNormals { get; private set; } = new List<Vector>();

        public MeshDescriptor Descriptor { get; set; } = new MeshDescriptor();

        public RotationScale RotationScale { get; set; } = new RotationScale();

        public IList<MeshLod> Lods { get; } = new List<MeshLod>();

        public IList<Hardpoint> Hardpoints { get; } = new List<Hardpoint>();

        public IList<EngineGlow> EngineGlows { get; } = new List<EngineGlow>();

        public Mesh Clone()
        {
            var mesh = new Mesh();

            foreach (var vertex in this.Vertices)
            {
                mesh.Vertices.Add(vertex);
            }

            foreach (var textureCoordinates in this.TextureCoordinates)
            {
                mesh.TextureCoordinates.Add(textureCoordinates);
            }

            foreach (var normal in this.VertexNormals)
            {
                mesh.VertexNormals.Add(normal);
            }

            mesh.Descriptor = this.Descriptor?.Clone();
            mesh.RotationScale = this.RotationScale?.Clone();

            foreach (var lod in this.Lods)
            {
                mesh.Lods.Add(lod.Clone());
            }

            foreach (var hardpoint in this.Hardpoints)
            {
                mesh.Hardpoints.Add(hardpoint.Clone());
            }

            foreach (var engineGlow in this.EngineGlows)
            {
                mesh.EngineGlows.Add(engineGlow.Clone());
            }

            return mesh;
        }

        public void SortLods()
        {
            var lods = this.Lods.OrderByDescending(t => t.Distance).ToList();
            this.Lods.Clear();

            foreach (var lod in lods)
            {
                this.Lods.Add(lod);
            }
        }

        public void CompactBuffers()
        {
            this.CompactVerticesBuffer();
            this.AddHitzoneToVerticesBuffer();
            this.CompactTextureCoordinatesBuffer();
            this.CompactVertexNormalsBuffer();

            foreach (var lod in this.Lods)
            {
                lod.CompactFaceGroups();
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void CompactVerticesBuffer()
        {
            if (this.Vertices.Count == 0)
            {
                return;
            }

            bool[] isUsed = new bool[this.Vertices.Count];

            foreach (int i in this.Lods
                .SelectMany(t => t.FaceGroups)
                .SelectMany(t => t.Faces)
                .Select(t => t.VerticesIndex)
                .SelectMany(t => new int[] { t.A, t.B, t.C, t.D }))
            {
                if (i >= 0 && i < this.Vertices.Count)
                {
                    isUsed[i] = true;
                }
            }

            List<Vector> newValues = new List<Vector>(this.Vertices.Count + 10);
            int[] newIndices = new int[this.Vertices.Count];

            for (int i = 0; i < this.Vertices.Count; i++)
            {
                if (!isUsed[i])
                {
                    continue;
                }

                Vector value = this.Vertices[i];
                int index = -1;

                for (int j = 0; j < newValues.Count; j++)
                {
                    if (newValues[j] == value)
                    {
                        index = j;
                        break;
                    }
                }

                if (index == -1)
                {
                    newIndices[i] = newValues.Count;
                    newValues.Add(value);
                }
                else
                {
                    newIndices[i] = index;
                }
            }

            newValues.TrimExcess();

            this.Vertices = newValues;

            foreach (var face in this.Lods
                .SelectMany(t => t.FaceGroups)
                .SelectMany(t => t.Faces))
            {
                Indices index = face.VerticesIndex;

                face.VerticesIndex = new Indices(
                    index.A < 0 ? -1 : newIndices[index.A],
                    index.B < 0 ? -1 : newIndices[index.B],
                    index.C < 0 ? -1 : newIndices[index.C],
                    index.D < 0 ? -1 : newIndices[index.D]);
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void CompactTextureCoordinatesBuffer()
        {
            if (this.TextureCoordinates.Count == 0)
            {
                return;
            }

            bool[] isUsed = new bool[this.TextureCoordinates.Count];

            foreach (int i in this.Lods
                .SelectMany(t => t.FaceGroups)
                .SelectMany(t => t.Faces)
                .Select(t => t.TextureCoordinatesIndex)
                .SelectMany(t => new int[] { t.A, t.B, t.C, t.D }))
            {
                if (i >= 0 && i < this.TextureCoordinates.Count)
                {
                    isUsed[i] = true;
                }
            }

            List<TextureCoordinates> newValues = new List<TextureCoordinates>(this.TextureCoordinates.Count);
            int[] newIndices = new int[this.TextureCoordinates.Count];

            for (int i = 0; i < this.TextureCoordinates.Count; i++)
            {
                if (!isUsed[i])
                {
                    continue;
                }

                TextureCoordinates value = this.TextureCoordinates[i];
                int index = -1;

                for (int j = 0; j < newValues.Count; j++)
                {
                    if (newValues[j] == value)
                    {
                        index = j;
                        break;
                    }
                }

                if (index == -1)
                {
                    newIndices[i] = newValues.Count;
                    newValues.Add(value);
                }
                else
                {
                    newIndices[i] = index;
                }
            }

            newValues.TrimExcess();

            this.TextureCoordinates = newValues;

            foreach (var face in this.Lods
                .SelectMany(t => t.FaceGroups)
                .SelectMany(t => t.Faces))
            {
                Indices index = face.TextureCoordinatesIndex;

                face.TextureCoordinatesIndex = new Indices(
                    index.A < 0 ? -1 : newIndices[index.A],
                    index.B < 0 ? -1 : newIndices[index.B],
                    index.C < 0 ? -1 : newIndices[index.C],
                    index.D < 0 ? -1 : newIndices[index.D]);
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void CompactVertexNormalsBuffer()
        {
            if (this.VertexNormals.Count == 0)
            {
                return;
            }

            bool[] isUsed = new bool[this.VertexNormals.Count];

            foreach (int i in this.Lods
                .SelectMany(t => t.FaceGroups)
                .SelectMany(t => t.Faces)
                .Select(t => t.VertexNormalsIndex)
                .SelectMany(t => new int[] { t.A, t.B, t.C, t.D }))
            {
                if (i >= 0 && i < this.VertexNormals.Count)
                {
                    isUsed[i] = true;
                }
            }

            List<Vector> newValues = new List<Vector>(this.VertexNormals.Count);
            int[] newIndices = new int[this.VertexNormals.Count];

            for (int i = 0; i < this.VertexNormals.Count; i++)
            {
                if (!isUsed[i])
                {
                    continue;
                }

                Vector value = this.VertexNormals[i];
                int index = -1;

                for (int j = 0; j < newValues.Count; j++)
                {
                    if (newValues[j] == value)
                    {
                        index = j;
                        break;
                    }
                }

                if (index == -1)
                {
                    newIndices[i] = newValues.Count;
                    newValues.Add(value);
                }
                else
                {
                    newIndices[i] = index;
                }
            }

            newValues.TrimExcess();

            this.VertexNormals = newValues;

            foreach (var face in this.Lods
                .SelectMany(t => t.FaceGroups)
                .SelectMany(t => t.Faces))
            {
                Indices index = face.VertexNormalsIndex;

                face.VertexNormalsIndex = new Indices(
                    index.A < 0 ? -1 : newIndices[index.A],
                    index.B < 0 ? -1 : newIndices[index.B],
                    index.C < 0 ? -1 : newIndices[index.C],
                    index.D < 0 ? -1 : newIndices[index.D]);
            }
        }

        private void AddHitzoneToVerticesBuffer()
        {
            //this.Vertices.Add(new Vector(this.Descriptor.Min.X, this.Descriptor.Min.Y, this.Descriptor.Min.Z));
            //this.Vertices.Add(new Vector(this.Descriptor.Min.X, this.Descriptor.Min.Y, this.Descriptor.Max.Z));
            //this.Vertices.Add(new Vector(this.Descriptor.Min.X, this.Descriptor.Max.Y, this.Descriptor.Min.Z));
            //this.Vertices.Add(new Vector(this.Descriptor.Min.X, this.Descriptor.Max.Y, this.Descriptor.Max.Z));
            //this.Vertices.Add(new Vector(this.Descriptor.Max.X, this.Descriptor.Min.Y, this.Descriptor.Min.Z));
            //this.Vertices.Add(new Vector(this.Descriptor.Max.X, this.Descriptor.Min.Y, this.Descriptor.Max.Z));
            //this.Vertices.Add(new Vector(this.Descriptor.Max.X, this.Descriptor.Max.Y, this.Descriptor.Min.Z));
            //this.Vertices.Add(new Vector(this.Descriptor.Max.X, this.Descriptor.Max.Y, this.Descriptor.Max.Z));

            this.Vertices.Add(this.Descriptor.Min);
            this.Vertices.Add(this.Descriptor.Max);
        }

        public void ComputeHitzone()
        {
            this.SortLods();

            this.CompactVerticesBuffer();

            if (this.Vertices.Count == 0 || this.Lods.Count == 0)
            {
                this.Descriptor.Min = Vector.Empty;
                this.Descriptor.Max = Vector.Empty;
                this.Descriptor.Center = Vector.Empty;
                this.Descriptor.Span = Vector.Empty;
                this.Descriptor.Target = Vector.Empty;
            }
            else
            {
                var vertices = this.Lods[0]
                    .FaceGroups
                    .SelectMany(t => t.Faces)
                    .SelectMany(t => new[] { t.VerticesIndex.A, t.VerticesIndex.B, t.VerticesIndex.C, t.VerticesIndex.D })
                    .Where(t => t >= 0)
                    .Distinct()
                    .Select(t => t < this.Vertices.Count ? this.Vertices[t] : this.Vertices[0])
                    .ToArray();

                Vector min;
                Vector max;

                if (vertices.Length == 0)
                {
                    min = Vector.Empty;
                    max = Vector.Empty;
                }
                else
                {
                    min = new Vector
                    (
                        vertices.Min(t => t.X),
                        vertices.Min(t => t.Y),
                        vertices.Min(t => t.Z)
                    );

                    max = new Vector
                    (
                        vertices.Max(t => t.X),
                        vertices.Max(t => t.Y),
                        vertices.Max(t => t.Z)
                    );
                }

                this.Descriptor.Min = min;
                this.Descriptor.Max = max;

                this.Descriptor.Center = new Vector
                (
                    (min.X + max.X) * 0.5f,
                    (min.Y + max.Y) * 0.5f,
                    (min.Z + max.Z) * 0.5f
                );

                this.Descriptor.Span = new Vector
                (
                    max.X - min.X,
                    max.Y - min.Y,
                    max.Z - min.Z
                );

                this.Descriptor.Target = this.Descriptor.Center;
            }

            this.AddHitzoneToVerticesBuffer();
        }

        public void SplitLod(MeshLod lod)
        {
            if (lod == null)
            {
                throw new ArgumentNullException(nameof(lod));
            }

            if (!this.Lods.Contains(lod))
            {
                throw new ArgumentOutOfRangeException(nameof(lod));
            }

            this.Lods.Remove(lod);

            foreach (var faceGroup in lod.FaceGroups)
            {
                var groupLod = new MeshLod
                {
                    Distance = lod.Distance
                };

                groupLod.FaceGroups.Add(faceGroup);

                this.Lods.Add(groupLod);
            }
        }

        public MeshLod MergeLods(IEnumerable<MeshLod> lods)
        {
            if (lods == null)
            {
                throw new ArgumentNullException(nameof(lods));
            }

            if (!lods.All(t => this.Lods.Contains(t)))
            {
                throw new ArgumentOutOfRangeException(nameof(lods));
            }

            if (!lods.Any())
            {
                return null;
            }

            var merge = new MeshLod
            {
                Distance = lods.Min(t => t.Distance)
            };

            foreach (var lod in lods)
            {
                this.Lods.Remove(lod);

                foreach (var faceGroup in lod.FaceGroups)
                {
                    merge.FaceGroups.Add(faceGroup);
                }
            }

            merge.CompactFaceGroups();

            this.Lods.Add(merge);

            this.SortLods();
            this.ComputeHitzone();

            return merge;
        }

        public void Move(float moveX, float moveY, float moveZ)
        {
            for (int i = 0; i < this.Vertices.Count; i++)
            {
                this.Vertices[i] = this.Vertices[i].Move(moveX, moveY, moveZ);
            }

            this.Descriptor.Min = this.Descriptor.Min.Move(moveX, moveY, moveZ);
            this.Descriptor.Max = this.Descriptor.Max.Move(moveX, moveY, moveZ);
            this.Descriptor.Center = this.Descriptor.Center.Move(moveX, moveY, moveZ);
            this.Descriptor.Target = this.Descriptor.Target.Move(moveX, moveY, moveZ);

            this.RotationScale.Pivot = this.RotationScale.Pivot.Move(moveX, moveY, moveZ);

            foreach (var hardpoint in this.Hardpoints)
            {
                hardpoint.Position = hardpoint.Position.Move(moveX, moveY, moveZ);
            }

            foreach (var engineGlow in this.EngineGlows)
            {
                engineGlow.Position = engineGlow.Position.Move(moveX, moveY, moveZ);
            }
        }

        public Mesh Duplicate()
        {
            var newMesh = this.Clone();

            if (newMesh.Descriptor != null)
            {
                var center = newMesh.Descriptor.Center;
                newMesh.Move(-center.X, -center.Y, -center.Z);
            }

            return newMesh;
        }
    }
}
