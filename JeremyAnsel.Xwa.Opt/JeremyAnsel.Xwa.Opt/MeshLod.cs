// -----------------------------------------------------------------------
// <copyright file="MeshLod.cs" company="">
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

    public class MeshLod
    {
        public MeshLod()
        {
            this.Distance = 0.0f;
            this.FaceGroups = new List<FaceGroup>();
        }

        public float Distance { get; set; }

        public IList<FaceGroup> FaceGroups { get; private set; }

        public int TrianglesCount
        {
            get
            {
                return this.FaceGroups.Sum(t => t.TrianglesCount);
            }
        }

        public int VerticesCount
        {
            get
            {
                return this.FaceGroups.Sum(t => t.VerticesCount);
            }
        }

        public MeshLod Clone()
        {
            var lod = new MeshLod
            {
                Distance = this.Distance
            };

            foreach(var faceGroup in this.FaceGroups)
            {
                lod.FaceGroups.Add(faceGroup.Clone());
            }

            return lod;
        }

        public void CompactFaceGroups()
        {
            var groups = new List<FaceGroup>(this.FaceGroups.Count);

            foreach (var faceGroup in this.FaceGroups)
            {
                FaceGroup index = null;

                foreach (var group in groups)
                {
                    if (group.Textures.Count != faceGroup.Textures.Count)
                    {
                        continue;
                    }

                    if (group.Textures.Count == 0)
                    {
                        index = group;
                        break;
                    }

                    int t = 0;
                    for (; t < group.Textures.Count; t++)
                    {
                        if (group.Textures[t] != faceGroup.Textures[t])
                        {
                            break;
                        }
                    }

                    if (t == group.Textures.Count)
                    {
                        index = group;
                        break;
                    }
                }

                if (index == null)
                {
                    groups.Add(faceGroup);
                }
                else
                {
                    foreach (var face in faceGroup.Faces)
                    {
                        index.Faces.Add(face);
                    }
                }
            }

            groups.TrimExcess();
            this.FaceGroups = groups;

            this.SplitFaceGroups();

            foreach (var faceGroup in this.FaceGroups)
            {
                faceGroup.ComputeEdges();
            }
        }

        private void SplitFaceGroups()
        {
            const int MaxVerticesCount = 384;

            var createNewGroup = new Func<FaceGroup, FaceGroup>(faceGroup =>
            {
                var group = new FaceGroup();

                foreach (string texture in faceGroup.Textures)
                {
                    group.Textures.Add(texture);
                }

                return group;
            });

            var groups = new List<FaceGroup>(this.FaceGroups.Count);

            foreach (var faceGroup in this.FaceGroups)
            {
                if (faceGroup.VerticesCount <= MaxVerticesCount)
                {
                    groups.Add(faceGroup);
                }
                else
                {
                    int verticesCount = 0;
                    FaceGroup group = createNewGroup(faceGroup);

                    foreach (var face in faceGroup.Faces)
                    {
                        verticesCount += face.VerticesCount;

                        if (verticesCount > MaxVerticesCount)
                        {
                            groups.Add(group);
                            verticesCount = face.VerticesCount;
                            group = createNewGroup(faceGroup);
                        }

                        group.Faces.Add(face);
                    }

                    groups.Add(group);
                }
            }

            groups.TrimExcess();
            this.FaceGroups = groups;
        }
    }
}
