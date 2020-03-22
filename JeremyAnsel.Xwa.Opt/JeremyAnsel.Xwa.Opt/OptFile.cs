// -----------------------------------------------------------------------
// <copyright file="OptFile.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using JeremyAnsel.Xwa.Opt.Nodes;

    public class OptFile
    {
        public const float ScaleFactor = 1600.0f * 1.52587890625E-05f;

        public string FileName { get; private set; }

        public IList<Mesh> Meshes { get; } = new List<Mesh>();

        public IDictionary<string, Texture> Textures { get; } = new SortedDictionary<string, Texture>();

        public Vector MinSize
        {
            get
            {
                if (this.Meshes.Count == 0)
                {
                    return Vector.Empty;
                }

                return new Vector()
                {
                    X = this.Meshes.Min(t => Math.Min(t.Descriptor.Min.X, t.Descriptor.Max.X)),
                    Y = this.Meshes.Min(t => Math.Min(t.Descriptor.Min.Y, t.Descriptor.Max.Y)),
                    Z = this.Meshes.Min(t => Math.Min(t.Descriptor.Min.Z, t.Descriptor.Max.Z))
                };
            }
        }

        public Vector MaxSize
        {
            get
            {
                if (this.Meshes.Count == 0)
                {
                    return Vector.Empty;
                }

                return new Vector()
                {
                    X = this.Meshes.Max(t => Math.Max(t.Descriptor.Min.X, t.Descriptor.Max.X)),
                    Y = this.Meshes.Max(t => Math.Max(t.Descriptor.Min.Y, t.Descriptor.Max.Y)),
                    Z = this.Meshes.Max(t => Math.Max(t.Descriptor.Min.Z, t.Descriptor.Max.Z))
                };
            }
        }

        public Vector SpanSize
        {
            get
            {
                Vector max = this.MaxSize;
                Vector min = this.MinSize;

                return new Vector()
                {
                    X = max.X - min.X,
                    Y = max.Y - min.Y,
                    Z = max.Z - min.Z
                };
            }
        }

        public float Size
        {
            get
            {
                Vector span = this.SpanSize;

                return Math.Max(Math.Max(span.X, span.Y), span.Z);
            }
        }

        public int MaxTextureVersion
        {
            get
            {
                if (this.Meshes.Count == 0)
                {
                    return 0;
                }

                return this.Meshes.Max(mesh =>
                    mesh.Lods.Count == 0 ?
                    0 :
                    mesh.Lods.Max(lod =>
                        lod.FaceGroups.Count == 0 ?
                        0 :
                        lod.FaceGroups.Max(group =>
                            group.Textures.Count)));
            }
        }

        public int HardpointsCount
        {
            get
            {
                return this.Meshes.Sum(t => t.Hardpoints.Count);
            }
        }

        public int EngineGlowsCount
        {
            get
            {
                return this.Meshes.Sum(t => t.EngineGlows.Count);
            }
        }

        public int TexturesBitsPerPixel
        {
            get
            {
                if (this.Textures.Count == 0)
                {
                    return 0;
                }

                int bpp = this.Textures.ElementAt(0).Value.BitsPerPixel;

                if (this.Textures.Any(t => t.Value.BitsPerPixel != bpp))
                {
                    return 0;
                }

                return bpp;
            }
        }

        public OptFile Clone()
        {
            var opt = new OptFile
            {
                FileName = this.FileName
            };

            foreach (var mesh in this.Meshes)
            {
                opt.Meshes.Add(mesh.Clone());
            }

            foreach (var texture in this.Textures)
            {
                opt.Textures.Add(texture.Key, texture.Value.Clone());
            }

            return opt;
        }

        [SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        [SuppressMessage("Globalization", "CA1303:Ne pas passer de littéraux en paramètres localisés", Justification = "Reviewed.")]
        public static OptFile FromFile(string path)
        {
            OptFile opt = new OptFile
            {
                FileName = path
            };

            OptFileNodes optNodes = OptFileNodes.FromFile(path);

            List<string> globalTexture = null;

            for (int meshId = 0; meshId < optNodes.Nodes.Count; meshId++)
            {
                if (meshId == 0 && optNodes.Nodes[meshId].NodeType == NodeType.Texture)
                {
                    TextureNode textureNode = (TextureNode)optNodes.Nodes[meshId];
                    opt.CreateTexture(textureNode);
                    globalTexture = new List<string>() { textureNode.Name };
                    continue;
                }

                if (optNodes.Nodes[meshId].NodeType != NodeType.NodeGroup)
                {
                    throw new InvalidDataException("invalid mesh");
                }

                NodeGroupNode meshNode = (NodeGroupNode)optNodes.Nodes[meshId];

                var meshNodes = meshNode.Nodes.Union((meshNode.Nodes.Where(t => t.NodeType == NodeType.NodeGroup).FirstOrDefault() ?? new NodeGroupNode()).Nodes).ToList();

                RotationScaleNode rotationScaleNode = (RotationScaleNode)meshNodes.FirstOrDefault(t => t.NodeType == NodeType.RotationScale);
                MeshDescriptorNode descriptorNode = (MeshDescriptorNode)meshNodes.FirstOrDefault(t => t.NodeType == NodeType.MeshDescriptor);
                MeshVerticesNode verticesNode = (MeshVerticesNode)meshNodes.First(t => t.NodeType == NodeType.MeshVertices);
                TextureCoordinatesNode textureVerticesNode = (TextureCoordinatesNode)meshNodes.First(t => t.NodeType == NodeType.TextureCoordinates);
                VertexNormalsNode vertexNormalsNode = (VertexNormalsNode)meshNodes.First(t => t.NodeType == NodeType.VertexNormals);

                var hardpointsNodes = meshNodes.Where(t => t.NodeType == NodeType.Hardpoint).Select(t => (HardpointNode)t).ToList();
                var engineGlowsNodes = meshNodes.Where(t => t.NodeType == NodeType.EngineGlow).Select(t => (EngineGlowNode)t).ToList();

                FaceGroupingNode faceGroupingNode = (FaceGroupingNode)meshNodes.First(t => t.NodeType == NodeType.FaceGrouping);

                Mesh mesh = new Mesh();

                if (rotationScaleNode != null)
                {
                    mesh.RotationScale.Pivot = rotationScaleNode.Pivot;
                    mesh.RotationScale.Look = rotationScaleNode.Look;
                    mesh.RotationScale.Up = rotationScaleNode.Up;
                    mesh.RotationScale.Right = rotationScaleNode.Right;
                }

                if (descriptorNode != null)
                {
                    mesh.Descriptor.MeshType = descriptorNode.MeshType;
                    mesh.Descriptor.ExplosionType = descriptorNode.ExplosionType;
                    mesh.Descriptor.Span = descriptorNode.Span;
                    mesh.Descriptor.Center = descriptorNode.Center;
                    mesh.Descriptor.Min = descriptorNode.Min;
                    mesh.Descriptor.Max = descriptorNode.Max;
                    mesh.Descriptor.TargetId = descriptorNode.TargetId;
                    mesh.Descriptor.Target = descriptorNode.Target;
                }

                foreach (Vector vertex in verticesNode.Vertices)
                {
                    mesh.Vertices.Add(vertex);
                }

                foreach (TextureCoordinates textureVertex in textureVerticesNode.TextureVertices)
                {
                    mesh.TextureCoordinates.Add(textureVertex);
                }

                foreach (Vector normal in vertexNormalsNode.Normals)
                {
                    mesh.VertexNormals.Add(normal);
                }

                foreach (var hardpoint in hardpointsNodes)
                {
                    mesh.Hardpoints.Add(new Hardpoint()
                    {
                        HardpointType = hardpoint.HardpointType,
                        Position = hardpoint.Position
                    });
                }

                foreach (var engineGlow in engineGlowsNodes)
                {
                    mesh.EngineGlows.Add(new EngineGlow()
                    {
                        IsDisabled = engineGlow.IsDisabled,
                        CoreColor = engineGlow.CoreColor,
                        OuterColor = engineGlow.OuterColor,
                        Position = engineGlow.Position,
                        Format = engineGlow.Format,
                        Look = engineGlow.Look,
                        Up = engineGlow.Up,
                        Right = engineGlow.Right
                    });
                }

                if (faceGroupingNode.Distances.Count != faceGroupingNode.Nodes.Count)
                {
                    throw new InvalidDataException("invalid face groups count in face grouping");
                }

                for (int lodId = 0; lodId < faceGroupingNode.Distances.Count; lodId++)
                {
                    List<string> texture = globalTexture;

                    MeshLod lod = new MeshLod
                    {
                        Distance = faceGroupingNode.Distances[lodId]
                    };

                    foreach (Node node in EnumerateNodesInNodeGroupNodes(faceGroupingNode.Nodes[lodId].Nodes))
                    {
                        switch (node.NodeType)
                        {
                            case NodeType.Texture:
                                {
                                    TextureNode textureNode = (TextureNode)node;

                                    opt.CreateTexture(textureNode);
                                    texture = new List<string>() { textureNode.Name };
                                    break;
                                }

                            case NodeType.NodeReference:
                                texture = new List<string>() { ((NodeReferenceNode)node).Reference };
                                break;

                            case NodeType.NodeSwitch:
                                {
                                    NodeSwitchNode switchNode = (NodeSwitchNode)node;
                                    texture = new List<string>();

                                    foreach (Node nodeSwitch in switchNode.Nodes)
                                    {
                                        switch (nodeSwitch.NodeType)
                                        {
                                            case NodeType.Texture:
                                                {
                                                    TextureNode textureNode = (TextureNode)nodeSwitch;

                                                    opt.CreateTexture(textureNode);
                                                    texture.Add(textureNode.Name);
                                                    break;
                                                }

                                            case NodeType.NodeReference:
                                                texture.Add(((NodeReferenceNode)nodeSwitch).Reference);
                                                break;
                                        }
                                    }

                                    break;
                                }

                            case NodeType.FaceData:
                                {
                                    FaceDataNode faceDataNode = (FaceDataNode)node;

                                    FaceGroup faceGroup = new FaceGroup();

                                    if (texture != null)
                                    {
                                        foreach (var name in texture)
                                        {
                                            faceGroup.Textures.Add(name);
                                        }
                                    }

                                    foreach (var face in faceDataNode.Faces)
                                    {
                                        faceGroup.Faces.Add(new Face()
                                        {
                                            VerticesIndex = face.VerticesIndex,
                                            EdgesIndex = face.EdgesIndex,
                                            TextureCoordinatesIndex = face.TextureCoordinatesIndex,
                                            VertexNormalsIndex = face.VertexNormalsIndex,
                                            Normal = face.Normal,
                                            TexturingDirection = face.TexturingDirection,
                                            TexturingMagniture = face.TexturingMagniture
                                        });
                                    }

                                    foreach (var face in faceGroup.Faces)
                                    {
                                        if (face.VertexNormalsIndex.A >= mesh.VertexNormals.Count)
                                        {
                                            face.VertexNormalsIndex = face.VertexNormalsIndex.SetA(0);
                                        }

                                        if (face.VertexNormalsIndex.B >= mesh.VertexNormals.Count)
                                        {
                                            face.VertexNormalsIndex = face.VertexNormalsIndex.SetB(0);
                                        }

                                        if (face.VertexNormalsIndex.C >= mesh.VertexNormals.Count)
                                        {
                                            face.VertexNormalsIndex = face.VertexNormalsIndex.SetC(0);
                                        }

                                        if (face.VertexNormalsIndex.D >= mesh.VertexNormals.Count)
                                        {
                                            face.VertexNormalsIndex = face.VertexNormalsIndex.SetD(0);
                                        }

                                        if (face.TextureCoordinatesIndex.A >= mesh.TextureCoordinates.Count)
                                        {
                                            face.TextureCoordinatesIndex = face.TextureCoordinatesIndex.SetA(0);
                                        }

                                        if (face.TextureCoordinatesIndex.B >= mesh.TextureCoordinates.Count)
                                        {
                                            face.TextureCoordinatesIndex = face.TextureCoordinatesIndex.SetB(0);
                                        }

                                        if (face.TextureCoordinatesIndex.C >= mesh.TextureCoordinates.Count)
                                        {
                                            face.TextureCoordinatesIndex = face.TextureCoordinatesIndex.SetC(0);
                                        }

                                        if (face.TextureCoordinatesIndex.D >= mesh.TextureCoordinates.Count)
                                        {
                                            face.TextureCoordinatesIndex = face.TextureCoordinatesIndex.SetD(0);
                                        }

                                        if (face.VerticesIndex.A >= 0 && face.TextureCoordinatesIndex.A < 0)
                                        {
                                            face.TextureCoordinatesIndex = face.TextureCoordinatesIndex.SetA(0);
                                        }

                                        if (face.VerticesIndex.B >= 0 && face.TextureCoordinatesIndex.B < 0)
                                        {
                                            face.TextureCoordinatesIndex = face.TextureCoordinatesIndex.SetB(0);
                                        }

                                        if (face.VerticesIndex.C >= 0 && face.TextureCoordinatesIndex.C < 0)
                                        {
                                            face.TextureCoordinatesIndex = face.TextureCoordinatesIndex.SetC(0);
                                        }

                                        if (face.VerticesIndex.D >= 0 && face.TextureCoordinatesIndex.D < 0)
                                        {
                                            face.TextureCoordinatesIndex = face.TextureCoordinatesIndex.SetD(0);
                                        }
                                    }

                                    lod.FaceGroups.Add(faceGroup);

                                    texture = null;
                                    break;
                                }
                        }
                    }

                    mesh.Lods.Add(lod);
                }

                opt.Meshes.Add(mesh);
            }

            opt.SetFaceGroupTextureWhenEmpty();

            return opt;
        }

        private static IEnumerable<Node> EnumerateNodesInNodeGroupNodes(IEnumerable<Node> nodes)
        {
            foreach (Node node in nodes)
            {
                if (node.NodeType == NodeType.NodeGroup)
                {
                    foreach (Node sub in EnumerateNodesInNodeGroupNodes(node.Nodes))
                    {
                        yield return sub;
                    }
                }
                else
                {
                    yield return node;
                }
            }
        }

        private void SetFaceGroupTextureWhenEmpty()
        {
            foreach (var mesh in this.Meshes)
            {
                foreach (var lod in mesh.Lods)
                {
                    foreach (var faceGroup in lod.FaceGroups.Where(t => t.Textures.Count == 0))
                    {
                        var texturesFaceGroup = this.Meshes
                            .SelectMany(t => t.Lods)
                            .Where(t => t.Distance <= lod.Distance)
                            .SelectMany(t => t.FaceGroups)
                            .LastOrDefault(t => t.Textures.Count != 0);

                        if (texturesFaceGroup == null)
                        {
                            continue;
                        }

                        foreach (var texture in texturesFaceGroup.Textures)
                        {
                            faceGroup.Textures.Add(texture);
                        }
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public void Save(string path)
        {
            this.CompactBuffers();

            OptFileNodes optNodes = new OptFileNodes();

            Dictionary<string, bool> texturesWriten = this.Textures.Keys.ToDictionary(t => t, t => false);

            foreach (Mesh mesh in this.Meshes)
            {
                mesh.SortLods();

                NodeGroupNode meshNode = new NodeGroupNode();

                RotationScaleNode rotationScaleNode = new RotationScaleNode();
                MeshDescriptorNode descriptorNode = new MeshDescriptorNode();
                MeshVerticesNode verticesNode = new MeshVerticesNode();
                TextureCoordinatesNode textureVerticesNode = new TextureCoordinatesNode();
                VertexNormalsNode vertexNormalsNode = new VertexNormalsNode();

                rotationScaleNode.Pivot = mesh.RotationScale.Pivot;
                rotationScaleNode.Look = mesh.RotationScale.Look;
                rotationScaleNode.Up = mesh.RotationScale.Up;
                rotationScaleNode.Right = mesh.RotationScale.Right;

                descriptorNode.MeshType = mesh.Descriptor.MeshType;
                descriptorNode.ExplosionType = mesh.Descriptor.ExplosionType;
                descriptorNode.Span = mesh.Descriptor.Span;
                descriptorNode.Center = mesh.Descriptor.Center;
                descriptorNode.Min = mesh.Descriptor.Min;
                descriptorNode.Max = mesh.Descriptor.Max;
                descriptorNode.TargetId = mesh.Descriptor.TargetId;
                descriptorNode.Target = mesh.Descriptor.Target;

                foreach (Vector vertex in mesh.Vertices)
                {
                    verticesNode.Vertices.Add(vertex);
                }

                foreach (TextureCoordinates textureVertex in mesh.TextureCoordinates)
                {
                    textureVerticesNode.TextureVertices.Add(textureVertex);
                }

                foreach (Vector vertexNormal in mesh.VertexNormals)
                {
                    vertexNormalsNode.Normals.Add(vertexNormal);
                }

                meshNode.Nodes.Add(verticesNode);
                meshNode.Nodes.Add(textureVerticesNode);
                meshNode.Nodes.Add(vertexNormalsNode);
                meshNode.Nodes.Add(descriptorNode);
                meshNode.Nodes.Add(rotationScaleNode);

                FaceGroupingNode faceGroupingNode = new FaceGroupingNode();

                foreach (var lod in mesh.Lods)
                {
                    NodeGroupNode lodNode = new NodeGroupNode();

                    foreach (var faceGroup in lod.FaceGroups)
                    {
                        if (faceGroup.Textures.Count != 0)
                        {
                            List<Node> texturesNodes = new List<Node>();

                            foreach (var textureName in faceGroup.Textures)
                            {
                                if (!texturesWriten.ContainsKey(textureName) || texturesWriten[textureName])
                                {
                                    NodeReferenceNode textureNode = new NodeReferenceNode()
                                    {
                                        Reference = textureName
                                    };

                                    texturesNodes.Add(textureNode);
                                }
                                else
                                {
                                    var texture = this.Textures[textureName];

                                    TextureNode textureNode = new TextureNode
                                    {
                                        Name = texture.Name,
                                        UniqueId = 0, // texture.Id
                                        Width = texture.Width,
                                        Height = texture.Height,
                                        Palettes = texture.Palette,
                                        Bytes = texture.ImageData
                                    };

                                    if (textureNode.Bytes != null)
                                    {
                                        int size = textureNode.Width * textureNode.Height;
                                        int bpp;

                                        if (textureNode.Bytes.Length >= size && textureNode.Bytes.Length < size * 2)
                                        {
                                            bpp = 8;
                                        }
                                        else if (textureNode.Bytes.Length >= size * 4 && textureNode.Bytes.Length < size * 8)
                                        {
                                            bpp = 32;
                                        }
                                        else
                                        {
                                            bpp = 0;
                                        }

                                        if (bpp != 0)
                                        {
                                            textureNode.Bytes = OptFile.FlipPixels(textureNode.Bytes, textureNode.Width, textureNode.Height, bpp);
                                        }
                                    }

                                    if (texture.AlphaIllumData != null)
                                    {
                                        TextureAlphaNode alphaNode = new TextureAlphaNode
                                        {
                                            Bytes = OptFile.FlipPixels(texture.AlphaIllumData, textureNode.Width, textureNode.Height, 8)
                                        };

                                        textureNode.Nodes.Add(alphaNode);
                                    }

                                    texturesNodes.Add(textureNode);

                                    texturesWriten[textureName] = true;
                                }
                            }

                            if (texturesNodes.Count == 1)
                            {
                                lodNode.Nodes.Add(texturesNodes[0]);
                            }
                            else
                            {
                                NodeSwitchNode switchNode = new NodeSwitchNode();

                                foreach (var textureNode in texturesNodes)
                                {
                                    switchNode.Nodes.Add(textureNode);
                                }

                                lodNode.Nodes.Add(switchNode);
                            }
                        }

                        FaceDataNode faceDataNode = new FaceDataNode
                        {
                            EdgesCount = faceGroup.EdgesCount
                        };

                        foreach (var face in faceGroup.Faces)
                        {
                            FaceDataNodeData faceData = new FaceDataNodeData
                            {
                                VerticesIndex = face.VerticesIndex,
                                EdgesIndex = face.EdgesIndex,
                                TextureCoordinatesIndex = face.TextureCoordinatesIndex,
                                VertexNormalsIndex = face.VertexNormalsIndex,
                                Normal = face.Normal,
                                TexturingDirection = face.TexturingDirection,
                                TexturingMagniture = face.TexturingMagniture
                            };

                            faceDataNode.Faces.Add(faceData);
                        }

                        lodNode.Nodes.Add(faceDataNode);
                    }

                    faceGroupingNode.Distances.Add(lod.Distance);
                    faceGroupingNode.Nodes.Add(lodNode);
                }

                NodeGroupNode faceGroupingNodeGroup = new NodeGroupNode();
                faceGroupingNodeGroup.Nodes.Add(Node.Null);
                faceGroupingNodeGroup.Nodes.Add(Node.Null);
                faceGroupingNodeGroup.Nodes.Add(Node.Null);
                faceGroupingNodeGroup.Nodes.Add(faceGroupingNode);

                meshNode.Nodes.Add(faceGroupingNodeGroup);

                foreach (var hardpoint in mesh.Hardpoints)
                {
                    meshNode.Nodes.Add(new HardpointNode()
                    {
                        HardpointType = hardpoint.HardpointType,
                        Position = hardpoint.Position
                    });
                }

                foreach (var engineGlow in mesh.EngineGlows)
                {
                    meshNode.Nodes.Add(new EngineGlowNode()
                    {
                        IsDisabled = engineGlow.IsDisabled,
                        CoreColor = engineGlow.CoreColor,
                        OuterColor = engineGlow.OuterColor,
                        Position = engineGlow.Position,
                        Format = engineGlow.Format,
                        Look = engineGlow.Look,
                        Up = engineGlow.Up,
                        Right = engineGlow.Right
                    });
                }

                optNodes.Nodes.Add(meshNode);
            }

            foreach (var texture in texturesWriten.Where(t => !t.Value))
            {
                this.Textures.Remove(texture.Key);
            }

            optNodes.Save(path);
            this.FileName = path;
        }

        [SuppressMessage("Globalization", "CA1303:Ne pas passer de littéraux en paramètres localisés", Justification = "Reviewed.")]
        private void CreateTexture(TextureNode textureNode)
        {
            if (textureNode.Name == null)
            {
                textureNode.Name = string.Format(CultureInfo.InvariantCulture, "Tex{0}", textureNode.UniqueId);
            }

            Texture texture = new Texture
            {
                Name = textureNode.Name
            };

            if (textureNode.Width == 0 || textureNode.Height == 0)
            {
                Texture id = this.Textures.Values
                    .FirstOrDefault(t => t.Id == textureNode.UniqueId);

                if (id == null && this.Textures.Count != 0)
                {
                    id = this.Textures.ElementAt(this.Textures.Count - 1).Value;
                }

                if (id == null)
                {
                    throw new InvalidDataException("invalid 0x0 texture");
                }

                texture.Id = id.Id;
                texture.Width = id.Width;
                texture.Height = id.Height;
                texture.Palette = id.Palette;
                texture.ImageData = id.ImageData;
                texture.AlphaIllumData = id.AlphaIllumData;
            }
            else
            {
                texture.Id = textureNode.UniqueId;
                texture.Width = textureNode.Width;
                texture.Height = textureNode.Height;
                texture.Palette = textureNode.Palettes;
                texture.ImageData = textureNode.Bytes;

                TextureAlphaNode alphaNode = (TextureAlphaNode)textureNode.Nodes
                    .FirstOrDefault(t => t.NodeType == NodeType.TextureAlpha);

                if (alphaNode != null)
                {
                    texture.AlphaIllumData = alphaNode.Bytes;
                }

                if (texture.ImageData != null)
                {
                    int size = texture.Width * texture.Height;
                    int bpp;

                    if (texture.ImageData.Length >= size && texture.ImageData.Length < size * 2)
                    {
                        bpp = 8;
                    }
                    else if (texture.ImageData.Length >= size * 4 && texture.ImageData.Length < size * 8)
                    {
                        bpp = 32;
                    }
                    else
                    {
                        bpp = 0;
                    }

                    if (bpp != 0)
                    {
                        texture.ImageData = OptFile.FlipPixels(texture.ImageData, texture.Width, texture.Height, bpp);
                    }
                }

                if (texture.AlphaIllumData != null)
                {
                    texture.AlphaIllumData = OptFile.FlipPixels(texture.AlphaIllumData, texture.Width, texture.Height, 8);
                }
            }

            this.Textures.Add(texture.Name, texture);
        }

        public void CompactBuffers()
        {
            this.Meshes
                .AsParallel()
                .ForAll(mesh => mesh.CompactBuffers());
        }

        public void CompactTextures()
        {
            var uniqueTextures = new List<string>(this.Textures.Count);

            var duplicateTextures = new Dictionary<string, string>(this.Textures.Count);

            foreach (var texture in this.Textures.Values)
            {
                var texName = uniqueTextures
                    .Where(t => Texture.AreEquals(texture, this.Textures[t]))
                    .FirstOrDefault();

                if (texName == null)
                {
                    uniqueTextures.Add(texture.Name);
                }
                else
                {
                    duplicateTextures.Add(texture.Name, texName);
                }
            }

            foreach (var faceGroup in this.Meshes
                .SelectMany(t => t.Lods)
                .SelectMany(t => t.FaceGroups))
            {
                foreach (var texture in duplicateTextures)
                {
                    for (int i = 0; i < faceGroup.Textures.Count; i++)
                    {
                        if (string.Equals(faceGroup.Textures[i], texture.Key, StringComparison.Ordinal))
                        {
                            faceGroup.Textures[i] = texture.Value;
                        }
                    }

                    this.Textures.Remove(texture.Key);
                }
            }
        }

        public void GenerateTexturesNames()
        {
            var map = new Dictionary<string, string>(this.Textures.Count);
            var mapTextures = new List<Texture>(this.Textures.Count);

            for (int index = 0; index < this.Textures.Count; index++)
            {
                var texture = this.Textures.ElementAt(index).Value;

                string newName = string.Format(CultureInfo.InvariantCulture, "Tex{0:D5}", index);

                map.Add(texture.Name, newName);

                texture.Name = newName;
                mapTextures.Add(texture);
            }

            this.Textures.Clear();

            foreach (var texture in mapTextures)
            {
                this.Textures.Add(texture.Name, texture);
            }

            foreach (var faceGroup in this.Meshes
                .SelectMany(t => t.Lods)
                .SelectMany(t => t.FaceGroups))
            {
                for (int i = 0; i < faceGroup.Textures.Count; i++)
                {
                    string key = faceGroup.Textures[i];

                    if (map.TryGetValue(key, out string name))
                    {
                        faceGroup.Textures[i] = name;
                    }
                }
            }
        }

        public void ComputeHitzones()
        {
            this.Meshes
                .AsParallel()
                .ForAll(mesh => mesh.ComputeHitzone());
        }

        public bool CanTexturesBeConvertedWithoutLoss()
        {
            if (this.Textures.Count == 0)
            {
                return true;
            }

            return this.Textures.Values
                .AsParallel()
                .All(t => t.CanBeConvertedWithoutLoss());
        }

        public void ConvertTextures8To32()
        {
            this.Textures.Values
                .AsParallel()
                .Where(t => t.BitsPerPixel == 8)
                .ForAll(t => t.Convert8To32());
        }

        public void ConvertTextures32To8()
        {
            this.Textures.Values
                .AsParallel()
                .Where(t => t.BitsPerPixel == 32)
                .ForAll(t => t.Convert32To8());
        }

        public void GenerateTexturesMipmaps()
        {
            this.Textures.Values
                .AsParallel()
                .ForAll(t => t.GenerateMipmaps());
        }

        public void RemoveTexturesMipmaps()
        {
            this.Textures.Values
                .AsParallel()
                .ForAll(t => t.RemoveMipmaps());
        }

        public void SplitMesh(Mesh mesh)
        {
            if (mesh == null)
            {
                throw new ArgumentNullException(nameof(mesh));
            }

            if (!this.Meshes.Contains(mesh))
            {
                throw new ArgumentOutOfRangeException(nameof(mesh));
            }

            this.Meshes.Remove(mesh);

            foreach (var lod in mesh.Lods)
            {
                var lodMesh = new Mesh();

                lodMesh.Descriptor.MeshType = mesh.Descriptor.MeshType;
                lodMesh.Descriptor.ExplosionType = mesh.Descriptor.ExplosionType;
                lodMesh.Descriptor.TargetId = mesh.Descriptor.TargetId;

                lodMesh.RotationScale.Pivot = mesh.RotationScale.Pivot;
                lodMesh.RotationScale.Look = mesh.RotationScale.Look;
                lodMesh.RotationScale.Up = mesh.RotationScale.Up;
                lodMesh.RotationScale.Right = mesh.RotationScale.Right;

                foreach (var v in mesh.Vertices)
                {
                    lodMesh.Vertices.Add(v);
                }

                foreach (var v in mesh.TextureCoordinates)
                {
                    lodMesh.TextureCoordinates.Add(v);
                }

                foreach (var v in mesh.VertexNormals)
                {
                    lodMesh.VertexNormals.Add(v);
                }

                lodMesh.Lods.Add(lod);

                foreach (var hardpoint in mesh.Hardpoints)
                {
                    lodMesh.Hardpoints
                        .Add(new Hardpoint
                        {
                            HardpointType = hardpoint.HardpointType,
                            Position = hardpoint.Position
                        });
                }

                foreach (var engineGlow in mesh.EngineGlows)
                {
                    lodMesh.EngineGlows
                        .Add(new EngineGlow
                        {
                            IsDisabled = engineGlow.IsDisabled,
                            CoreColor = engineGlow.CoreColor,
                            OuterColor = engineGlow.OuterColor,
                            Format = engineGlow.Format,
                            Position = engineGlow.Position,
                            Look = engineGlow.Look,
                            Up = engineGlow.Up,
                            Right = engineGlow.Right
                        });
                }

                lodMesh.ComputeHitzone();
                lodMesh.CompactBuffers();

                this.Meshes.Add(lodMesh);
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public Mesh MergeMeshes(IEnumerable<Mesh> meshes)
        {
            if (meshes == null)
            {
                throw new ArgumentNullException(nameof(meshes));
            }

            if (!meshes.All(t => this.Meshes.Contains(t)))
            {
                throw new ArgumentOutOfRangeException(nameof(meshes));
            }

            if (!meshes.Any())
            {
                return null;
            }

            Indices indexAdd(Indices index, int offset) => new Indices(
                index.A >= 0 ? index.A + offset : -1,
                index.B >= 0 ? index.B + offset : -1,
                index.C >= 0 ? index.C + offset : -1,
                index.D >= 0 ? index.D + offset : -1);

            var merge = new Mesh();

            int verticesIndex = 0;
            int textureCoordinatesIndex = 0;
            int vertexNormalsIndex = 0;

            foreach (var mesh in meshes)
            {
                this.Meshes.Remove(mesh);

                foreach (var v in mesh.Vertices)
                {
                    merge.Vertices.Add(v);
                }

                foreach (var v in mesh.TextureCoordinates)
                {
                    merge.TextureCoordinates.Add(v);
                }

                foreach (var v in mesh.VertexNormals)
                {
                    merge.VertexNormals.Add(v);
                }

                foreach (var lod in mesh.Lods)
                {
                    foreach (var face in lod.FaceGroups.SelectMany(t => t.Faces))
                    {
                        face.VerticesIndex = indexAdd(face.VerticesIndex, verticesIndex);
                        face.TextureCoordinatesIndex = indexAdd(face.TextureCoordinatesIndex, textureCoordinatesIndex);
                        face.VertexNormalsIndex = indexAdd(face.VertexNormalsIndex, vertexNormalsIndex);
                    }

                    merge.Lods.Add(lod);
                }

                verticesIndex += mesh.Vertices.Count;
                textureCoordinatesIndex += mesh.TextureCoordinates.Count;
                vertexNormalsIndex += mesh.VertexNormals.Count;

                foreach (var hardpoint in mesh.Hardpoints)
                {
                    merge.Hardpoints
                        .Add(new Hardpoint
                        {
                            HardpointType = hardpoint.HardpointType,
                            Position = hardpoint.Position
                        });
                }

                foreach (var engineGlow in mesh.EngineGlows)
                {
                    merge.EngineGlows
                        .Add(new EngineGlow
                        {
                            IsDisabled = engineGlow.IsDisabled,
                            CoreColor = engineGlow.CoreColor,
                            OuterColor = engineGlow.OuterColor,
                            Format = engineGlow.Format,
                            Position = engineGlow.Position,
                            Look = engineGlow.Look,
                            Up = engineGlow.Up,
                            Right = engineGlow.Right
                        });
                }
            }

            merge.SortLods();
            merge.ComputeHitzone();
            merge.CompactBuffers();

            this.Meshes.Add(merge);

            return merge;
        }

        public void Scale(float scaleFactor)
        {
            this.Scale(scaleFactor, scaleFactor, scaleFactor);
        }

        public void Scale(float scaleX, float scaleY, float scaleZ)
        {
            if (scaleX == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(scaleX));
            }

            if (scaleY == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(scaleY));
            }

            if (scaleZ == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(scaleZ));
            }

            bool invertFaceOrder = (Math.Sign(scaleX) * Math.Sign(scaleY) * Math.Sign(scaleZ)) < 0;

            this.Meshes
                .AsParallel()
                .ForAll(mesh =>
                {
                    for (int i = 0; i < mesh.Vertices.Count; i++)
                    {
                        mesh.Vertices[i] = mesh.Vertices[i].Scale(scaleX, scaleY, scaleZ);
                    }

                    for (int i = 0; i < mesh.VertexNormals.Count; i++)
                    {
                        mesh.VertexNormals[i] = mesh.VertexNormals[i].Scale(Math.Sign(scaleX), Math.Sign(scaleY), Math.Sign(scaleZ));
                    }

                    Vector min = mesh.Descriptor.Min.Scale(scaleX, scaleY, scaleZ);
                    Vector max = mesh.Descriptor.Max.Scale(scaleX, scaleY, scaleZ);

                    mesh.Descriptor.Min = new Vector(Math.Min(min.X, max.X), Math.Min(min.Y, max.Y), Math.Min(min.Z, max.Z));
                    mesh.Descriptor.Max = new Vector(Math.Max(min.X, max.X), Math.Max(min.Y, max.Y), Math.Max(min.Z, max.Z));

                    mesh.Descriptor.Center = mesh.Descriptor.Center.Scale(scaleX, scaleY, scaleZ);
                    mesh.Descriptor.Span = mesh.Descriptor.Span.Scale(scaleX, scaleY, scaleZ).Abs();
                    mesh.Descriptor.Target = mesh.Descriptor.Target.Scale(scaleX, scaleY, scaleZ);

                    mesh.RotationScale.Pivot = mesh.RotationScale.Pivot.Scale(scaleX, scaleY, scaleZ);

                    foreach (var lod in mesh.Lods)
                    {
                        lod.Distance /= Math.Max(Math.Max(Math.Abs(scaleX), Math.Abs(scaleY)), Math.Abs(scaleZ));

                        foreach (var face in lod.FaceGroups.SelectMany(t => t.Faces))
                        {
                            face.Normal = face.Normal.Scale(Math.Sign(scaleX), Math.Sign(scaleY), Math.Sign(scaleZ));
                            face.TexturingDirection = face.TexturingDirection.Scale(scaleX, scaleY, scaleZ);
                            face.TexturingMagniture = face.TexturingMagniture.Scale(scaleX, scaleY, scaleZ);

                            if (invertFaceOrder)
                            {
                                face.VerticesIndex = face.VerticesIndex.InvertOrder();
                                face.EdgesIndex = face.EdgesIndex.InvertOrder();
                                face.TextureCoordinatesIndex = face.TextureCoordinatesIndex.InvertOrder();
                                face.VertexNormalsIndex = face.VertexNormalsIndex.InvertOrder();
                            }
                        }
                    }

                    foreach (var hardpoint in mesh.Hardpoints)
                    {
                        hardpoint.Position = hardpoint.Position.Scale(scaleX, scaleY, scaleZ);
                    }

                    foreach (var engineGlow in mesh.EngineGlows)
                    {
                        engineGlow.Position = engineGlow.Position.Scale(scaleX, scaleY, scaleZ);
                    }
                });
        }

        public void Move(float moveX, float moveY, float moveZ)
        {
            this.Meshes
                .AsParallel()
                .ForAll(mesh => mesh.Move(moveX, moveY, moveZ));
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("Globalization", "CA1303:Ne pas passer de littéraux en paramètres localisés", Justification = "Reviewed.")]
        public void ChangeAxes(int axisX, int axisY, int axisZ)
        {
            if (axisX < -3 || axisX == 0 || axisX > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(axisX));
            }

            if (axisY < -3 || axisY == 0 || axisY > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(axisY));
            }

            if (axisZ < -3 || axisZ == 0 || axisZ > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(axisZ));
            }

            if (Math.Abs(axisX) == Math.Abs(axisY) || Math.Abs(axisY) == Math.Abs(axisZ) || Math.Abs(axisZ) == Math.Abs(axisX))
            {
                throw new InvalidOperationException("X Y Z must be different axes.");
            }

            bool invertFaceOrder = (Math.Sign(axisX) * Math.Sign(axisY) * Math.Sign(axisZ)) < 0;

            if (((Math.Abs(axisX) == 1 ? 1 : 0) + (Math.Abs(axisY) == 2 ? 1 : 0) + (Math.Abs(axisZ) == 3 ? 1 : 0)) == 1)
            {
                invertFaceOrder = !invertFaceOrder;
            }

            float selectAxis(Vector v, int axis)
            {
                switch (axis)
                {
                    case 1:
                        return v.X;

                    case 2:
                        return v.Y;

                    case 3:
                        return v.Z;

                    case -1:
                        return -v.X;

                    case -2:
                        return -v.Y;

                    case -3:
                        return -v.Z;
                }

                return 0;
            }

            Vector selectVector(Vector v) => new Vector(
                selectAxis(v, axisX),
                selectAxis(v, axisY),
                selectAxis(v, axisZ));

            this.Meshes
                .AsParallel()
                .ForAll(mesh =>
                {
                    for (int i = 0; i < mesh.Vertices.Count; i++)
                    {
                        mesh.Vertices[i] = selectVector(mesh.Vertices[i]);
                    }

                    for (int i = 0; i < mesh.VertexNormals.Count; i++)
                    {
                        mesh.VertexNormals[i] = selectVector(mesh.VertexNormals[i]);
                    }

                    Vector min = selectVector(mesh.Descriptor.Min);
                    Vector max = selectVector(mesh.Descriptor.Max);

                    mesh.Descriptor.Min = new Vector(Math.Min(min.X, max.X), Math.Min(min.Y, max.Y), Math.Min(min.Z, max.Z));
                    mesh.Descriptor.Max = new Vector(Math.Max(min.X, max.X), Math.Max(min.Y, max.Y), Math.Max(min.Z, max.Z));

                    mesh.Descriptor.Center = selectVector(mesh.Descriptor.Center);
                    mesh.Descriptor.Span = selectVector(mesh.Descriptor.Span).Abs();
                    mesh.Descriptor.Target = selectVector(mesh.Descriptor.Target);

                    mesh.RotationScale.Pivot = selectVector(mesh.RotationScale.Pivot);

                    foreach (var lod in mesh.Lods)
                    {
                        foreach (var face in lod.FaceGroups.SelectMany(t => t.Faces))
                        {
                            face.Normal = selectVector(face.Normal);
                            face.TexturingDirection = selectVector(face.TexturingDirection);
                            face.TexturingMagniture = selectVector(face.TexturingMagniture);

                            if (invertFaceOrder)
                            {
                                face.VerticesIndex = face.VerticesIndex.InvertOrder();
                                face.EdgesIndex = face.EdgesIndex.InvertOrder();
                                face.TextureCoordinatesIndex = face.TextureCoordinatesIndex.InvertOrder();
                                face.VertexNormalsIndex = face.VertexNormalsIndex.InvertOrder();
                            }
                        }
                    }

                    foreach (var hardpoint in mesh.Hardpoints)
                    {
                        hardpoint.Position = selectVector(hardpoint.Position);
                    }

                    foreach (var engineGlow in mesh.EngineGlows)
                    {
                        engineGlow.Position = selectVector(engineGlow.Position);
                    }
                });
        }

        public IList<PlayabilityMessage> CheckPlayability()
        {
            return PlayabilityChecker.CheckPlayability(this);
        }

        public IList<string> CheckFlatTextures(bool removeFlatTextures)
        {
            var flatTextures = new List<string>();

            for (int meshIndex = 0; meshIndex < this.Meshes.Count; meshIndex++)
            {
                var mesh = this.Meshes[meshIndex];

                for (int lodIndex = 0; lodIndex < mesh.Lods.Count; lodIndex++)
                {
                    var lod = mesh.Lods[lodIndex];

                    for (int faceGroupIndex = 0; faceGroupIndex < lod.FaceGroups.Count; faceGroupIndex++)
                    {
                        var faceGroup = lod.FaceGroups[faceGroupIndex];

                        List<int> removeFacesIndexes = null;

                        if (removeFlatTextures)
                        {
                            removeFacesIndexes = new List<int>(faceGroup.Faces.Count);
                        }

                        for (int faceIndex = 0; faceIndex < faceGroup.Faces.Count; faceIndex++)
                        {
                            var face = faceGroup.Faces[faceIndex];

                            if (face.HasFlatTexture(mesh))
                            {
                                string text = "MESH " + (meshIndex + 1).ToString(CultureInfo.InvariantCulture)
                                    + ", LOD " + (lodIndex + 1).ToString(CultureInfo.InvariantCulture)
                                    + ", GROUP " + (faceGroupIndex + 1).ToString(CultureInfo.InvariantCulture)
                                    + ", FACE " + (faceIndex + 1).ToString(CultureInfo.InvariantCulture)
                                    + " has flat texture.";

                                flatTextures.Add(text);

                                if (removeFlatTextures)
                                {
                                    removeFacesIndexes.Add(faceIndex);
                                }
                            }
                        }

                        if (removeFlatTextures)
                        {
                            for (int faceIndex = removeFacesIndexes.Count - 1; faceIndex >= 0; faceIndex--)
                            {
                                faceGroup.Faces.RemoveAt(removeFacesIndexes[faceIndex]);
                            }
                        }
                    }
                }
            }

            return flatTextures;
        }

        private static byte[] FlipPixels(byte[] pixels, int width, int height, int bpp)
        {
            int length = pixels.Length;
            byte[] data = new byte[length];
            int offset = 0;
            int w = width;
            int h = height;

            while (offset < length)
            {
                int stride = w * bpp / 8;

                for (int i = 0; i < h; i++)
                {
                    Array.Copy(pixels, offset + i * stride, data, offset + (h - 1 - i) * stride, stride);
                }

                offset += h * stride;

                w = w > 1 ? w / 2 : 1;
                h = h > 1 ? h / 2 : 1;
            }

            return data;
        }
    }
}
