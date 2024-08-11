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

    public class MeshLod
    {
        public MeshLod()
        {
            this.Distance = 0.0f;
        }

        public float Distance { get; set; }

        public IList<FaceGroup> FaceGroups { get; private set; } = new List<FaceGroup>();

        public int TrianglesCount
        {
            get
            {
                int count = 0;

                for (int i = 0; i < this.FaceGroups.Count; i++)
                {
                    count += this.FaceGroups[i].TrianglesCount;
                }

                return count;
            }
        }

        public int VerticesCount
        {
            get
            {
                int count = 0;

                for (int i = 0; i < this.FaceGroups.Count; i++)
                {
                    count += this.FaceGroups[i].VerticesCount;
                }

                return count;
            }
        }

        public MeshLod Clone()
        {
            var lod = new MeshLod
            {
                Distance = this.Distance
            };

            foreach (var faceGroup in this.FaceGroups)
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
                FaceGroup? index = null;

                foreach (var group in groups)
                {
                    int groupTexturesCount = group.Textures is null ? 0 : group.Textures.Count;
                    int faceGroupTexturesCount = faceGroup.Textures is null ? 0 : faceGroup.Textures.Count;

                    if (groupTexturesCount != faceGroupTexturesCount)
                    {
                        continue;
                    }

                    if (groupTexturesCount == 0)
                    {
                        index = group;
                        break;
                    }

                    int t = 0;
                    for (; t < groupTexturesCount; t++)
                    {
                        if (group.Textures![t] != faceGroup.Textures![t])
                        {
                            break;
                        }
                    }

                    if (t == groupTexturesCount)
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
                    if (index.Faces is not null && faceGroup.Faces is not null)
                    {
                        foreach (var face in faceGroup.Faces)
                        {
                            index.Faces.Add(face);
                        }
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

                if (faceGroup.Textures is not null)
                {
                    foreach (string texture in faceGroup.Textures)
                    {
                        group.Textures!.Add(texture);
                    }
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

                    if (faceGroup.Faces is not null)
                    {
                        foreach (var face in faceGroup.Faces)
                        {
                            verticesCount += face.VerticesCount;

                            if (verticesCount > MaxVerticesCount)
                            {
                                groups.Add(group);
                                verticesCount = face.VerticesCount;
                                group = createNewGroup(faceGroup);
                            }

                            group.Faces!.Add(face);
                        }
                    }

                    groups.Add(group);
                }
            }

            groups.TrimExcess();
            this.FaceGroups = groups;
        }

        public void GroupFaceGroups()
        {
            var groups = new List<FaceGroup>(this.FaceGroups.Count);

            foreach (var faceGroup in this.FaceGroups)
            {
                FaceGroup? index = null;

                foreach (var group in groups)
                {
                    int groupTexturesCount = group.Textures is null ? 0 : group.Textures.Count;
                    int faceGroupTexturesCount = faceGroup.Textures is null ? 0 : faceGroup.Textures.Count;

                    if (groupTexturesCount != faceGroupTexturesCount)
                    {
                        continue;
                    }

                    if (groupTexturesCount == 0)
                    {
                        index = group;
                        break;
                    }

                    int t = 0;
                    for (; t < groupTexturesCount; t++)
                    {
                        if (group.Textures![t] != faceGroup.Textures![t])
                        {
                            break;
                        }
                    }

                    if (t == groupTexturesCount)
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
                    if (index.Faces is not null && faceGroup.Faces is not null)
                    {
                        foreach (var face in faceGroup.Faces)
                        {
                            index.Faces.Add(face);
                        }
                    }
                }
            }

            this.FaceGroups = groups;
        }
    }
}
