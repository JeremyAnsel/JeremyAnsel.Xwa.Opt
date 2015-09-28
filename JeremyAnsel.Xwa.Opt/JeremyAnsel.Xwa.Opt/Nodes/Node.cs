﻿// -----------------------------------------------------------------------
// <copyright file="Node.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public abstract class Node
    {
        private static readonly Node nullNode = new NullNode();

        internal Node(NodeTypes type)
        {
            this.NodeType = type;
            this.Nodes = new List<Node>();
        }

        public static Node Null
        {
            get { return Node.nullNode; }
        }

        public int Offset { get; private set; }

        public NodeTypes NodeType { get; private set; }

        public string Name { get; set; }

        public IList<Node> Nodes { get; private set; }

        protected int Parameter1 { get; private set; }

        protected int Parameter2 { get; private set; }

        public int SizeInFile
        {
            get
            {
                if (this.NodeType == NodeTypes.NullNode)
                {
                    return 0;
                }

                return 24 + this.NameSize + this.NodesOffsetsSize + this.DataSize + this.NodesSize;
            }
        }

        protected int NameSize
        {
            get
            {
                return this.Name == null || this.Name.Length == 0 ? 0 : (this.Name.Length + 1);
            }
        }

        protected int NodesOffsetsSize
        {
            get
            {
                return this.Nodes.Count * 4;
            }
        }

        protected virtual int DataSize
        {
            get
            {
                return 0;
            }
        }

        protected int NodesSize
        {
            get
            {
                return this.Nodes.Sum(t => t.SizeInFile);
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Node {0} {1}", this.NodeType, this.Name);
        }

        internal static Node ParseNode(byte[] buffer, int globalOffset, int offset)
        {
            NodeTypes type = (NodeTypes)BitConverter.ToInt32(buffer, offset + 4);

            Node node = null;

            switch (type)
            {
                case NodeTypes.NullNode:
                    node = new NullNode();
                    break;
                case NodeTypes.NodeGroup:
                    node = new NodeGroupNode();
                    break;
                case NodeTypes.FaceData:
                    node = new FaceDataNode();
                    break;
                case NodeTypes.MeshVertices:
                    node = new MeshVerticesNode();
                    break;
                case NodeTypes.NodeReference:
                    node = new NodeReferenceNode();
                    break;
                case NodeTypes.VertexNormals:
                    node = new VertexNormalsNode();
                    break;
                case NodeTypes.TextureCoordinates:
                    node = new TextureCoordinatesNode();
                    break;
                case NodeTypes.Texture:
                    node = new TextureNode();
                    break;
                case NodeTypes.FaceGrouping:
                    node = new FaceGroupingNode();
                    break;
                case NodeTypes.Hardpoint:
                    node = new HardpointNode();
                    break;
                case NodeTypes.RotationScale:
                    node = new RotationScaleNode();
                    break;
                case NodeTypes.NodeSwitch:
                    node = new NodeSwitchNode();
                    break;
                case NodeTypes.MeshDescriptor:
                    node = new MeshDescriptorNode();
                    break;
                case NodeTypes.TextureAlpha:
                    node = new TextureAlphaNode();
                    break;
                case NodeTypes.EngineGlow:
                    node = new EngineGlowNode();
                    break;
            }

            if (node == null)
            {
                throw new InvalidDataException("invalid node found: " + type);
            }

            node.Parse(buffer, globalOffset, offset);

            return node;
        }

        internal virtual void Parse(byte[] buffer, int globalOffset, int offset)
        {
            this.Offset = offset;

            int nameOffset = BitConverter.ToInt32(buffer, offset);
            if (nameOffset != 0)
            {
                nameOffset -= globalOffset;

                this.Name = Utils.GetNullTerminatedString(buffer, nameOffset);
            }

            this.Parameter1 = BitConverter.ToInt32(buffer, offset + 16);

            this.Parameter2 = BitConverter.ToInt32(buffer, offset + 20);
            if (this.Parameter2 != 0)
            {
                this.Parameter2 -= globalOffset;
            }

            int nodesCount = BitConverter.ToInt32(buffer, offset + 8);
            int nodesOffset = BitConverter.ToInt32(buffer, offset + 12);

            this.Nodes = new List<Node>(nodesCount);

            if (nodesCount != 0 && nodesOffset != 0)
            {
                nodesOffset -= globalOffset;

                for (int i = 0; i < nodesCount; i++)
                {
                    int nodeOffset = BitConverter.ToInt32(buffer, nodesOffset + (i * 4));

                    if (nodeOffset == 0)
                    {
                        this.Nodes.Add(Node.Null);
                        continue;
                    }

                    nodeOffset -= globalOffset;

                    this.Nodes.Add(Node.ParseNode(buffer, globalOffset, nodeOffset));
                }
            }
        }

        internal virtual void Write(BinaryWriter file, int offset)
        {
            this.Offset = offset;

            int nameOffset = this.NameSize == 0 ? 0 : (offset + 24);
            int nodesOffsetsOffset = this.NodesOffsetsSize == 0 ? 0 : (offset + 24 + this.NameSize);

            file.Write(nameOffset);
            file.Write((int)this.NodeType);
            file.Write(this.Nodes.Count);
            file.Write(nodesOffsetsOffset);
        }

        protected void WriteName(BinaryWriter file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            if (this.NameSize == 0)
            {
                return;
            }

            file.Write(Encoding.ASCII.GetBytes(this.Name));
            file.Write((byte)0);
        }

        protected void WriteNodesOffsets(BinaryWriter file, int offset)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            int nodeOffset;

            checked
            {
                nodeOffset = offset + 24 + this.NameSize + this.NodesOffsetsSize + this.DataSize;
            }

            foreach (Node node in this.Nodes)
            {
                if (node.NodeType == Opt.NodeTypes.NullNode)
                {
                    file.Write((int)0);
                    continue;
                }

                file.Write(nodeOffset);
                nodeOffset += node.SizeInFile;
            }
        }

        protected void WriteNodes(BinaryWriter file, int offset)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            int nodeOffset;

            checked
            {
                nodeOffset = offset + 24 + this.NameSize + this.NodesOffsetsSize + this.DataSize;
            }

            foreach (Node node in this.Nodes)
            {
                if (node.NodeType == NodeTypes.NullNode)
                {
                    continue;
                }

                node.Write(file, nodeOffset);
                nodeOffset += node.SizeInFile;
            }
        }
    }
}
