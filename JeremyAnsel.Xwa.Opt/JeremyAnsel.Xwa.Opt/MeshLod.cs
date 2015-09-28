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

            foreach (var faceGroup in this.FaceGroups)
            {
                faceGroup.ComputeEdges();
            }
        }
    }
}
