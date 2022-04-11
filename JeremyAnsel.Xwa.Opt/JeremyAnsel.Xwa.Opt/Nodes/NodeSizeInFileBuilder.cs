
namespace JeremyAnsel.Xwa.Opt.Nodes
{
    public static class NodeSizeInFileBuilder
    {
        public static int OptFileNodes(int nodesCount, bool includeHeader = true)
        {
            int version = 5;
            int size = 0;

            if (includeHeader)
            {
                size += version == 0 ? 4 : 8;
            }

            size += 14;
            size += nodesCount * 4;
            // Nodes.Sum(t => t.SizeInFile)
            return size;
        }

        public static int Node(string name, int nodesCount)
        {
            int nameSize = name == null || name.Length == 0 ? 0 : (name.Length + 1);
            int nodesOffsetsSize = nodesCount * 4;
            return 24 + nameSize + nodesOffsetsSize; // + dataSize + nodesSize;
        }

        public static int NullNode()
        {
            return 0;
        }

        public static int EngineGlowNode(string name, int nodesCount)
        {
            return Node(name, nodesCount) + 72;
        }

        public static int FaceDataNode(string name, int nodesCount, int facesCount)
        {
            return Node(name, nodesCount) + 4 + (facesCount * 100);
        }

        public static int FaceGroupingNode(string name, int nodesCount, int distancesCount)
        {
            return Node(name, nodesCount) + distancesCount * 4;
        }

        public static int HardpointNode(string name, int nodesCount)
        {
            return Node(name, nodesCount) + 16;
        }

        public static int MeshDescriptorNode(string name, int nodesCount)
        {
            return Node(name, nodesCount) + 72;
        }

        public static int MeshVerticesNode(string name, int nodesCount, int verticesCount)
        {
            return Node(name, nodesCount) + verticesCount * 12;
        }

        public static int NodeGroupNode(string name, int nodesCount)
        {
            return Node(name, nodesCount);
        }

        public static int NodeReferenceNode(string name, int nodesCount, string reference)
        {
            int referenceSize = reference == null || reference.Length == 0 ? 0 : (reference.Length + 1);
            return Node(name, nodesCount) + referenceSize;
        }

        public static int NodeSwitchNode(string name, int nodesCount)
        {
            return Node(name, nodesCount);
        }

        public static int RotationScaleNode(string name, int nodesCount)
        {
            return Node(name, nodesCount) + 48;
        }

        public static int TextureAlphaNode(string name, int nodesCount, int bytesLength)
        {
            return Node(name, nodesCount) + bytesLength;
        }

        public static int TextureCoordinatesNode(string name, int nodesCount, int textureVerticesCount)
        {
            return Node(name, nodesCount) + textureVerticesCount * 8;
        }

        public static int TextureNode(string name, int nodesCount, int palettesLength, int bytesLength)
        {
            int dataSize = bytesLength == 0 ? 0 : (24 + bytesLength + palettesLength);
            return Node(name, nodesCount) + dataSize;
        }

        public static int VertexNormalsNode(string name, int nodesCount, int normalsCount)
        {
            return Node(name, nodesCount) + normalsCount * 12;
        }
    }
}
