// -----------------------------------------------------------------------
// <copyright file="OptFile.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class OptFileNodes
    {
        public OptFileNodes(int nodesCount = -1)
        {
            this.Version = 5;

            if (nodesCount <= 0)
            {
                this.Nodes = new List<Node>();
            }
            else
            {
                this.Nodes = new List<Node>(nodesCount);
            }
        }

        public string FileName { get; private set; }

        public int Version { get; private set; }

        public int FileSize
        {
            get
            {
                int size = (this.Version == 0 ? 4 : 8) + 14;

                size += this.Nodes.Count * 4;

                for (int i = 0; i < this.Nodes.Count; i++)
                {
                    size += this.Nodes[i].SizeInFile;
                }

                return size;
            }
        }

        public IList<Node> Nodes { get; private set; }

        public static OptFileNodes FromFile(string path)
        {
            OptFileNodes opt;
            FileStream filestream = null;

            try
            {
                filestream = new FileStream(path, FileMode.Open, FileAccess.Read);

                using (BinaryReader file = new BinaryReader(filestream, Encoding.ASCII))
                {
                    filestream = null;

                    opt = ReadOpt(file);
                }
            }
            finally
            {
                if (filestream != null)
                {
                    filestream.Dispose();
                }
            }

            opt.FileName = path;
            return opt;
        }

        public static OptFileNodes FromStream(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            OptFileNodes opt;

            using (BinaryReader file = new BinaryReader(stream, Encoding.ASCII))
            {
                opt = ReadOpt(file);
            }

            return opt;
        }

        [SuppressMessage("Globalization", "CA1303:Ne pas passer de littéraux en paramètres localisés", Justification = "Reviewed.")]
        private static OptFileNodes ReadOpt(BinaryReader file)
        {
            var opt = new OptFileNodes();

            int version;
            int filesize;

            version = file.ReadInt32();
            if (version > 0)
            {
                filesize = version;
                version = 0;
            }
            else
            {
                version = -version;
                filesize = file.ReadInt32();
            }

            opt.Version = version;

            if (file.BaseStream.Length - file.BaseStream.Position != filesize)
            {
                throw new InvalidDataException("invalid file size");
            }

            opt.Parse(file);
            return opt;
        }

        public void Save(string path)
        {
            FileStream filestream = null;

            try
            {
                filestream = new FileStream(path, FileMode.Create, FileAccess.Write);

                using (BinaryWriter file = new BinaryWriter(filestream, Encoding.ASCII))
                {
                    filestream = null;

                    this.WriteOpt(file);
                    this.FileName = path;
                }
            }
            finally
            {
                if (filestream != null)
                {
                    filestream.Dispose();
                }
            }
        }

        public void Save(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using (BinaryWriter file = new BinaryWriter(stream, Encoding.ASCII))
            {
                this.WriteOpt(file);
            }
        }

        private void WriteOpt(BinaryWriter file)
        {
            if (this.Version != 0)
            {
                file.Write(-this.Version);
            }

            file.Write(this.FileSize - (this.Version == 0 ? 4 : 8));

            file.Write((int)0);
            file.Write((short)0);

            file.Write(this.Nodes.Count);
            file.Write((int)14);

            int offset = 14 + (this.Nodes.Count * 4);

            for (int i = 0; i < this.Nodes.Count; i++)
            {
                Node node = this.Nodes[i];
                file.Write(offset);
                offset += node.SizeInFile;
            }

            offset = 14 + (this.Nodes.Count * 4);

            for (int i = 0; i < this.Nodes.Count; i++)
            {
                Node node = this.Nodes[i];
                node.Write(file, offset);
                offset += node.SizeInFile;
            }
        }

        private void Parse(BinaryReader file)
        {
            int globalOffset = file.ReadInt32();
            file.BaseStream.Position += 2;
            int nodesCount = file.ReadInt32();
            int nodesOffset = file.ReadInt32();

            globalOffset -= this.Version == 0 ? 4 : 8;

            if (nodesOffset == 0)
            {
                return;
            }

            this.Nodes = new List<Node>(nodesCount);

            nodesOffset -= globalOffset;

            for (int i = 0; i < nodesCount; i++)
            {
                file.BaseStream.Position = nodesOffset + (i * 4);
                int nodeOffset = file.ReadInt32();

                if (nodeOffset == 0)
                {
                    this.Nodes.Add(Node.Null);
                    continue;
                }

                nodeOffset -= globalOffset;

                this.Nodes.Add(Node.ParseNode(file, globalOffset, nodeOffset));
            }
        }
    }
}
