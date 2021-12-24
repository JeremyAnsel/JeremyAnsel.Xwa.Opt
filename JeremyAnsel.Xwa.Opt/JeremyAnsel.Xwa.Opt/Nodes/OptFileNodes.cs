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
        public OptFileNodes()
        {
            this.Version = 5;
        }

        public string FileName { get; private set; }

        public int Version { get; private set; }

        public int FileSize
        {
            get
            {
                int size = (this.Version == 0 ? 4 : 8) + 14;

                size += this.Nodes.Count * 4;
                size += this.Nodes.Sum(t => t.SizeInFile);

                return size;
            }
        }

        public IList<Node> Nodes { get; private set; } = new List<Node>();

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

            byte[] buffer = file.ReadBytes(filesize);

            opt.Parse(buffer);
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

            foreach (Node node in this.Nodes)
            {
                file.Write(offset);
                offset += node.SizeInFile;
            }

            offset = 14 + (this.Nodes.Count * 4);

            foreach (Node node in this.Nodes)
            {
                node.Write(file, offset);
                offset += node.SizeInFile;
            }
        }

        private void Parse(byte[] buffer)
        {
            int globalOffset = BitConverter.ToInt32(buffer, 0);
            int nodesCount = BitConverter.ToInt32(buffer, 6);
            int nodesOffset = BitConverter.ToInt32(buffer, 10);

            if (nodesOffset == 0)
            {
                return;
            }

            this.Nodes = new List<Node>(nodesCount);

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
}
