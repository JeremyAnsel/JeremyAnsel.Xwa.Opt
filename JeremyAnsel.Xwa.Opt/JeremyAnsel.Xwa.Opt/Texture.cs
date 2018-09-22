﻿// -----------------------------------------------------------------------
// <copyright file="Texture.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;

    public class Texture
    {
        public Texture()
        {
            this.Id = 0;
            this.Name = null;
            this.Width = 0;
            this.Height = 0;
            this.Palette = new byte[8192];
            this.ImageData = null;
            this.AlphaData = null;
        }

        internal int Id { get; set; }

        public string Name { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public byte[] Palette { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public byte[] ImageData { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public byte[] AlphaData { get; set; }

        public bool HasAlpha
        {
            get { return this.AlphaData != null; }
        }

        public int BitsPerPixel
        {
            get
            {
                if (this.ImageData == null || this.Width == 0 || this.Height == 0)
                {
                    return 0;
                }

                int size = this.Width * this.Height;

                if (this.ImageData.Length >= size && this.ImageData.Length < size * 2)
                {
                    if (this.Palette == null)
                    {
                        return 0;
                    }

                    return 8;
                }

                if (this.ImageData.Length >= size * 4 && this.ImageData.Length < size * 8)
                {
                    return 32;
                }

                return 0;
            }
        }

        public bool IsIlluminated
        {
            get
            {
                if (this.BitsPerPixel != 8)
                {
                    return false;
                }

                if (this.Palette == null)
                {
                    return false;
                }

                for (int c = 0; c < 256; c++)
                {
                    byte color0 = this.Palette[8 * 512 + c * 2];
                    byte color1 = this.Palette[8 * 512 + c * 2 + 1];

                    if (color0 == 0 && color1 == 0)
                    {
                        continue;
                    }

                    bool isIlluminated = true;

                    for (int i = 0; i < 8; i++)
                    {
                        byte c0 = this.Palette[i * 512 + c * 2];
                        byte c1 = this.Palette[i * 512 + c * 2 + 1];

                        if (c0 != color0 || c1 != color1)
                        {
                            isIlluminated = false;
                            break;
                        }
                    }

                    if (isIlluminated)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public int MipmapsCount
        {
            get
            {
                int bpp = this.BitsPerPixel;

                if (bpp == 0)
                {
                    return 0;
                }

                int count = 1;
                int length = this.ImageData.Length / (bpp / 8);
                int w = this.Width;
                int h = this.Height;

                while (length > w * h && (w > 1 || h > 1))
                {
                    count++;
                    length -= w * h;
                    w = w > 1 ? w / 2 : 1;
                    h = h > 1 ? h / 2 : 1;
                }

                return count;
            }
        }

        public int MipmapsLength
        {
            get
            {
                int length = 0;
                int count = this.MipmapsCount;
                int w = this.Width;
                int h = this.Height;

                for (int i = 0; i < count; i++)
                {
                    length += w * h;
                    w = w > 1 ? w / 2 : 1;
                    h = h > 1 ? h / 2 : 1;
                }

                return length;
            }
        }

        public int MaximumMipmapsCount
        {
            get
            {
                if (this.Width == 0 || this.Height == 0)
                {
                    return 0;
                }

                int count = 1;
                int size = Math.Max(this.Width, this.Height);

                while (size > 1)
                {
                    count++;
                    size /= 2;
                }

                return count;
            }
        }

        public int MaximumMipmapsLength
        {
            get
            {
                if (this.Width == 0 || this.Height == 0)
                {
                    return 0;
                }

                int length = 1;

                int w = this.Width;
                int h = this.Height;

                while (w > 1 || h > 1)
                {
                    length += w * h;
                    w = w > 1 ? w / 2 : 1;
                    h = h > 1 ? h / 2 : 1;
                }

                return length;
            }
        }

        public Texture Clone()
        {
            var texture = new Texture
            {
                Id = this.Id,
                Name = this.Name,
                Width = this.Width,
                Height = this.Height,
                Palette = (byte[])this.Palette.Clone(),
                ImageData = this.ImageData == null ? null : (byte[])this.ImageData.Clone(),
                AlphaData = this.AlphaData == null ? null : (byte[])this.AlphaData.Clone()
            };

            return texture;
        }

        public void SetPaletteColors(byte[] colors)
        {
            if (colors == null)
            {
                throw new ArgumentNullException("colors");
            }

            if (colors.Length > 256 * 3)
            {
                throw new ArgumentOutOfRangeException("colors");
            }

            byte[] palette = new byte[8192];

            Array.Clear(palette, 0, 512);

            for (int c = 0; c < 256; c++)
            {
                uint cr;
                uint cg;
                uint cb;

                if (c < colors.Length / 3)
                {
                    cr = colors[c * 3 + 0];
                    cg = colors[c * 3 + 1];
                    cb = colors[c * 3 + 2];
                }
                else
                {
                    cr = 0;
                    cg = 0;
                    cb = 0;
                }

                for (int i = 1; i < 16; i++)
                {
                    uint r;
                    uint g;
                    uint b;

                    if (i == 8)
                    {
                        r = cr;
                        g = cg;
                        b = cb;
                    }
                    else if (i < 8)
                    {
                        uint d = (uint)i;

                        r = (cr * 128 * d / 8 + cr * 128) / 256;
                        g = (cg * 128 * d / 8 + cg * 128) / 256;
                        b = (cb * 128 * d / 8 + cb * 128) / 256;
                    }
                    else
                    {
                        uint d = (uint)i - 8;

                        r = ((255 - cr) * 256 * d / 8 + cr * 256) / 256;
                        g = ((255 - cg) * 256 * d / 8 + cg * 256) / 256;
                        b = ((255 - cb) * 256 * d / 8 + cb * 256) / 256;
                    }

                    r = (r * (0x1fU * 2) + 0xffU) / (0xffU * 2);
                    g = (g * (0x3fU * 2) + 0xffU) / (0xffU * 2);
                    b = (b * (0x1fU * 2) + 0xffU) / (0xffU * 2);

                    ushort color = (ushort)((r << 11) | (g << 5) | b);

                    BitConverter.GetBytes(color).CopyTo(palette, i * 512 + c * 2);
                }
            }

            this.Palette = palette;
        }

        public void ResetPaletteColors()
        {
            if (this.Palette == null || this.Palette.Length != 8192)
            {
                return;
            }

            Array.Clear(this.Palette, 0, 512);

            for (int c = 0; c < 256; c++)
            {
                uint cr;
                uint cg;
                uint cb;

                ushort color16 = BitConverter.ToUInt16(this.Palette, 8 * 512 + c * 2);

                cr = (byte)((color16 & 0xF800) >> 11);
                cg = (byte)((color16 & 0x7E0) >> 5);
                cb = (byte)(color16 & 0x1F);

                cr = (byte)((cr * (0xffU * 2) + 0x1fU) / (0x1fU * 2));
                cg = (byte)((cg * (0xffU * 2) + 0x3fU) / (0x3fU * 2));
                cb = (byte)((cb * (0xffU * 2) + 0x1fU) / (0x1fU * 2));

                for (int i = 1; i < 16; i++)
                {
                    if (i == 8)
                    {
                        continue;
                    }

                    uint r;
                    uint g;
                    uint b;

                    if (i < 8)
                    {
                        uint d = (uint)i;

                        r = (cr * 128 * d / 8 + cr * 128) / 256;
                        g = (cg * 128 * d / 8 + cg * 128) / 256;
                        b = (cb * 128 * d / 8 + cb * 128) / 256;
                    }
                    else
                    {
                        uint d = (uint)i - 8;

                        r = ((255 - cr) * 256 * d / 8 + cr * 256) / 256;
                        g = ((255 - cg) * 256 * d / 8 + cg * 256) / 256;
                        b = ((255 - cb) * 256 * d / 8 + cb * 256) / 256;
                    }

                    r = (r * (0x1fU * 2) + 0xffU) / (0xffU * 2);
                    g = (g * (0x3fU * 2) + 0xffU) / (0xffU * 2);
                    b = (b * (0x1fU * 2) + 0xffU) / (0xffU * 2);

                    ushort color = (ushort)((r << 11) | (g << 5) | b);

                    BitConverter.GetBytes(color).CopyTo(this.Palette, i * 512 + c * 2);
                }
            }
        }

        public byte[] GetMipmapImageData()
        {
            int w;
            int h;

            return this.GetMipmapImageData(0, out w, out h);
        }

        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
        public byte[] GetMipmapImageData(int level, out int width, out int height)
        {
            int count = this.MipmapsCount;

            if (count == 0)
            {
                width = 0;
                height = 0;
                return null;
            }

            if (level >= count)
            {
                throw new ArgumentOutOfRangeException("level");
            }

            int bpp = this.BitsPerPixel / 8;
            int w = this.Width;
            int h = this.Height;
            int offset = 0;

            for (int i = 0; i < level; i++)
            {
                offset += w * h * bpp;
                w = w > 1 ? w / 2 : 1;
                h = h > 1 ? h / 2 : 1;
            }

            byte[] imageData = new byte[w * h * bpp];
            Array.Copy(this.ImageData, offset, imageData, 0, w * h * bpp);

            width = w;
            height = h;

            return imageData;
        }

        public byte[] GetMipmapAlphaData()
        {
            int w;
            int h;

            return this.GetMipmapAlphaData(0, out w, out h);
        }

        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
        public byte[] GetMipmapAlphaData(int level, out int width, out int height)
        {
            int count = this.MipmapsCount;

            if (this.AlphaData == null || count == 0)
            {
                width = 0;
                height = 0;
                return null;
            }

            if (level >= count)
            {
                throw new ArgumentOutOfRangeException("level");
            }

            int w = this.Width;
            int h = this.Height;
            int offset = 0;

            for (int i = 0; i < level; i++)
            {
                offset += w * h;
                w = w > 1 ? w / 2 : 1;
                h = h > 1 ? h / 2 : 1;
            }

            byte[] alphaData = new byte[w * h];
            Array.Copy(this.AlphaData, offset, alphaData, 0, w * h);

            width = w;
            height = h;

            return alphaData;
        }

        public byte[] GetColorMap()
        {
            int w;
            int h;

            return this.GetColorMap(0, out w, out h);
        }

        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
        public byte[] GetColorMap(int level, out int width, out int height)
        {
            int bpp = this.BitsPerPixel;

            if (bpp == 8)
            {
                return this.GetMipmapImageData(level, out width, out height);
            }
            else if (bpp == 32)
            {
                byte[] map = this.GetMipmapImageData(level, out width, out height);

                int size = width * height;

                for (int i = 0; i < size; i++)
                {
                    map[i * 4 + 3] = 255;
                }

                return map;
            }
            else
            {
                width = 0;
                height = 0;
                return null;
            }
        }

        public byte[] GetAlphaMap()
        {
            int w;
            int h;

            return this.GetAlphaMap(0, out w, out h);
        }

        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
        public byte[] GetAlphaMap(int level, out int width, out int height)
        {
            int bpp = this.BitsPerPixel;

            if (bpp == 8)
            {
                return this.GetMipmapAlphaData(level, out width, out height);
            }
            else if (bpp == 32)
            {
                return this.GetMipmapAlphaData(level, out width, out height);
            }
            else
            {
                width = 0;
                height = 0;
                return null;
            }
        }

        public void Save(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToUpperInvariant();

            switch (ext)
            {
                case ".BMP":
                    using (var bitmap = this.GetSaveBitmap())
                    {
                        bitmap.Save(fileName, ImageFormat.Bmp);
                    }
                    break;

                case ".PNG":
                    using (var bitmap = this.GetSaveBitmap())
                    {
                        bitmap.Save(fileName, ImageFormat.Png);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException("fileName");
            }
        }

        public void SaveColorMap(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToUpperInvariant();

            switch (ext)
            {
                case ".BMP":
                    using (var bitmap = this.GetSaveColorBitmap())
                    {
                        bitmap.Save(fileName, ImageFormat.Bmp);
                    }
                    break;

                case ".PNG":
                    using (var bitmap = this.GetSaveColorBitmap())
                    {
                        bitmap.Save(fileName, ImageFormat.Png);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException("fileName");
            }
        }

        public void SaveAlphaMap(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToUpperInvariant();

            switch (ext)
            {
                case ".BMP":
                    using (var bitmap = this.GetSaveAlphaBitmap())
                    {
                        bitmap.Save(fileName, ImageFormat.Bmp);
                    }
                    break;

                case ".PNG":
                    using (var bitmap = this.GetSaveAlphaBitmap())
                    {
                        bitmap.Save(fileName, ImageFormat.Png);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException("fileName");
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Supprimer les objets avant la mise hors de portée")]
        private Bitmap GetSaveBitmap()
        {
            byte[] imageData = this.GetMipmapImageData();

            if (imageData == null)
            {
                return null;
            }

            int size = this.Width * this.Height;
            int bpp = this.BitsPerPixel;

            if (bpp == 8)
            {
                if (this.Palette == null)
                {
                    return null;
                }

                var palette = Enumerable.Range(0, 256)
                    .Select(i =>
                    {
                        ushort c = BitConverter.ToUInt16(this.Palette, 8 * 512 + i * 2);

                        byte r = (byte)((c & 0xF800) >> 11);
                        byte g = (byte)((c & 0x7E0) >> 5);
                        byte b = (byte)(c & 0x1F);

                        r = (byte)((r * (0xffU * 2) + 0x1fU) / (0x1fU * 2));
                        g = (byte)((g * (0xffU * 2) + 0x3fU) / (0x3fU * 2));
                        b = (byte)((b * (0xffU * 2) + 0x1fU) / (0x1fU * 2));

                        return new Tuple<byte, byte, byte>(r, g, b);
                    })
                    .ToList();

                byte[] alphaData = this.GetMipmapAlphaData();

                if (alphaData == null)
                {
                    var bitmap = GetBitmap8bpp(this.Width, this.Height, imageData);

                    var pal = bitmap.Palette;

                    for (int i = 0; i < 256; i++)
                    {
                        pal.Entries[i] = Color.FromArgb(palette[i].Item1, palette[i].Item2, palette[i].Item3);
                    }

                    bitmap.Palette = pal;

                    return bitmap;
                }
                else
                {
                    byte[] data = new byte[size * 4];

                    for (int i = 0; i < size; i++)
                    {
                        int colorIndex = imageData[i];

                        data[i * 4 + 0] = palette[colorIndex].Item3;
                        data[i * 4 + 1] = palette[colorIndex].Item2;
                        data[i * 4 + 2] = palette[colorIndex].Item1;
                        data[i * 4 + 3] = alphaData[i];
                    }

                    var handle = GCHandle.Alloc(data, GCHandleType.Pinned);

                    try
                    {
                        return new Bitmap(this.Width, this.Height, this.Width * 4, PixelFormat.Format32bppArgb, handle.AddrOfPinnedObject());
                    }
                    finally
                    {
                        handle.Free();
                    }
                }
            }
            else if (bpp == 32)
            {
                var handle = GCHandle.Alloc(imageData, GCHandleType.Pinned);

                try
                {
                    return new Bitmap(this.Width, this.Height, this.Width * 4, PixelFormat.Format32bppArgb, handle.AddrOfPinnedObject());
                }
                finally
                {
                    handle.Free();
                }
            }
            else
            {
                return null;
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Supprimer les objets avant la mise hors de portée")]
        private Bitmap GetSaveColorBitmap()
        {
            byte[] colorMap = this.GetColorMap();

            if (colorMap == null)
            {
                return null;
            }

            int bpp = this.BitsPerPixel;

            if (bpp == 8)
            {
                if (this.Palette == null)
                {
                    return null;
                }

                var palette = Enumerable.Range(0, 256)
                    .Select(i =>
                    {
                        ushort c = BitConverter.ToUInt16(this.Palette, 8 * 512 + i * 2);

                        byte r = (byte)((c & 0xF800) >> 11);
                        byte g = (byte)((c & 0x7E0) >> 5);
                        byte b = (byte)(c & 0x1F);

                        r = (byte)((r * (0xffU * 2) + 0x1fU) / (0x1fU * 2));
                        g = (byte)((g * (0xffU * 2) + 0x3fU) / (0x3fU * 2));
                        b = (byte)((b * (0xffU * 2) + 0x1fU) / (0x1fU * 2));

                        return new Tuple<byte, byte, byte>(r, g, b);
                    })
                    .ToList();

                var bitmap = GetBitmap8bpp(this.Width, this.Height, colorMap);

                var pal = bitmap.Palette;

                for (int i = 0; i < 256; i++)
                {
                    pal.Entries[i] = Color.FromArgb(palette[i].Item1, palette[i].Item2, palette[i].Item3);
                }

                bitmap.Palette = pal;

                return bitmap;
            }
            else if (bpp == 32)
            {
                var handle = GCHandle.Alloc(colorMap, GCHandleType.Pinned);

                try
                {
                    return new Bitmap(this.Width, this.Height, this.Width * 4, PixelFormat.Format32bppRgb, handle.AddrOfPinnedObject());
                }
                finally
                {
                    handle.Free();
                }
            }
            else
            {
                return null;
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Supprimer les objets avant la mise hors de portée")]
        private Bitmap GetSaveAlphaBitmap()
        {
            byte[] alphaMap = this.GetAlphaMap();

            if (alphaMap == null)
            {
                return null;
            }

            var bitmap = GetBitmap8bpp(this.Width, this.Height, alphaMap);

            var pal = bitmap.Palette;

            for (int i = 0; i < 256; i++)
            {
                pal.Entries[i] = Color.FromArgb(i, i, i, i);
            }

            bitmap.Palette = pal;

            return bitmap;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Supprimer les objets avant la mise hors de portée")]
        private static Bitmap GetBitmap8bpp(int width, int height, byte[] imageData)
        {
            Bitmap bitmap;

            if (width % 4 == 0)
            {
                var handle = GCHandle.Alloc(imageData, GCHandleType.Pinned);

                try
                {
                    bitmap = new Bitmap(width, height, width, PixelFormat.Format8bppIndexed, handle.AddrOfPinnedObject());
                }
                finally
                {
                    handle.Free();
                }
            }
            else
            {
                bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
                var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                var data = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);

                try
                {
                    for (int h = 0; h < data.Height; h++)
                    {
                        Marshal.Copy(imageData, h * data.Width, IntPtr.Add(data.Scan0, h * data.Stride), data.Width);
                    }
                }
                finally
                {
                    bitmap.UnlockBits(data);
                }
            }

            return bitmap;
        }

        public static Texture FromFile(string fileName)
        {
            return FromFile(fileName, null);
        }

        public static Texture FromFile(string fileName, string fileNameAlpha)
        {
            Texture texture;

            string ext = Path.GetExtension(fileName).ToUpperInvariant();

            switch (ext)
            {
                case ".BMP":
                case ".PNG":
                case ".JPG":
                case ".GIF":
                    if (!File.Exists(fileName))
                    {
                        throw new FileNotFoundException(null, fileName);
                    }

                    using (var bitmap = new Bitmap(fileName))
                    {
                        texture = GetFileBitmap(bitmap);
                        texture.Name = Path.GetFileNameWithoutExtension(fileName);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException("fileName");
            }

            if (fileNameAlpha != null)
            {
                texture.SetAlphaMap(fileNameAlpha);
            }

            return texture;
        }

        public void SetAlphaMap(string fileName)
        {
            if (this.ImageData == null)
            {
                throw new InvalidOperationException();
            }

            string extAlpha = Path.GetExtension(fileName).ToUpperInvariant();

            switch (extAlpha)
            {
                case ".BMP":
                case ".PNG":
                case ".JPG":
                case ".GIF":
                    if (!File.Exists(fileName))
                    {
                        throw new FileNotFoundException(null, fileName);
                    }

                    byte[] alphaData = GetFileAlpha(fileName);

                    if (alphaData != null)
                    {
                        int bpp = this.BitsPerPixel;

                        if (bpp == 8)
                        {
                            if (alphaData.Length != this.ImageData.Length)
                            {
                                throw new InvalidDataException();
                            }

                            this.AlphaData = alphaData;
                        }
                        else if (bpp == 32)
                        {
                            if (alphaData.Length * 4 != this.ImageData.Length)
                            {
                                throw new InvalidDataException();
                            }

                            for (int i = 0; i < alphaData.Length; i++)
                            {
                                this.ImageData[i * 4 + 3] = alphaData[i];
                            }

                            this.AlphaData = alphaData;
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException("fileName");
            }
        }

        private static Texture GetFileBitmap(Bitmap image)
        {
            var rect = new Rectangle(0, 0, image.Width, image.Height);
            int length = image.Width * image.Height;

            var texture = new Texture();
            texture.Width = image.Width;
            texture.Height = image.Height;

            if (image.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                byte[] bytes = new byte[length];

                var data = image.LockBits(rect, ImageLockMode.ReadOnly, image.PixelFormat);

                try
                {
                    if (data.Width == data.Stride)
                    {
                        Marshal.Copy(data.Scan0, bytes, 0, length);
                    }
                    else
                    {
                        for (int h = 0; h < data.Height; h++)
                        {
                            Marshal.Copy(IntPtr.Add(data.Scan0, h * data.Stride), bytes, h * data.Width, data.Width);
                        }
                    }
                }
                finally
                {
                    image.UnlockBits(data);
                }

                byte[] palette = new byte[256 * 3];
                var colors = image.Palette.Entries;

                for (int i = 0; i < 256; i++)
                {
                    palette[i * 3 + 0] = colors[i].R;
                    palette[i * 3 + 1] = colors[i].G;
                    palette[i * 3 + 2] = colors[i].B;
                }

                texture.ImageData = bytes;
                texture.SetPaletteColors(palette);
            }
            else
            {
                byte[] bytes = new byte[length * 4];

                using (var bitmap = image.Clone(rect, PixelFormat.Format32bppArgb))
                {
                    var data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);

                    try
                    {
                        Marshal.Copy(data.Scan0, bytes, 0, length * 4);
                    }
                    finally
                    {
                        bitmap.UnlockBits(data);
                    }
                }

                byte[] alphaData = new byte[length];
                bool hasAlpha = false;

                for (int i = 0; i < length; i++)
                {
                    byte a = bytes[i * 4 + 3];

                    alphaData[i] = a;

                    if (a != (byte)255)
                    {
                        hasAlpha = true;
                    }
                }

                texture.ImageData = bytes;
                texture.AlphaData = hasAlpha ? alphaData : null;
                texture.Palette = new byte[8192];
            }

            return texture;
        }

        private static byte[] GetFileAlpha(string fileName)
        {
            using (var image = new Bitmap(fileName))
            {
                var rect = new Rectangle(0, 0, image.Width, image.Height);
                int length = image.Width * image.Height;

                byte[] bytes = new byte[length * 4];

                using (var bitmap = image.Clone(rect, PixelFormat.Format32bppArgb))
                {
                    var data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);

                    try
                    {
                        Marshal.Copy(data.Scan0, bytes, 0, length * 4);
                    }
                    finally
                    {
                        bitmap.UnlockBits(data);
                    }
                }

                byte[] alphaData = new byte[length];
                bool hasAlpha = false;

                for (int i = 0; i < length; i++)
                {
                    byte a = bytes[i * 4 + 3];

                    alphaData[i] = a;

                    if (a != (byte)255)
                    {
                        hasAlpha = true;
                    }
                }

                return hasAlpha ? alphaData : null;
            }
        }

        public bool CanBeConvertedWithoutLoss()
        {
            int bpp = this.BitsPerPixel;

            if (bpp == 8)
            {
                return true;
            }
            else if (bpp == 32)
            {
                var colors = Enumerable.Range(0, this.ImageData.Length / 4)
                    .Select(t => Color.FromArgb(
                        this.ImageData[t * 4 + 2],
                        this.ImageData[t * 4 + 1],
                        this.ImageData[t * 4 + 0]))
                    .Distinct()
                    .ToList();

                if (colors.Count == 0)
                {
                    return true;
                }

                if (colors.Count > 256)
                {
                    return false;
                }

                return colors.All(t =>
                    {
                        byte r = (byte)((t.R * (0x1fU * 2) + 0xffU) / (0xffU * 2));
                        byte g = (byte)((t.G * (0x3fU * 2) + 0xffU) / (0xffU * 2));
                        byte b = (byte)((t.B * (0x1fU * 2) + 0xffU) / (0xffU * 2));

                        r = (byte)((r * (0xffU * 2) + 0x1fU) / (0x1fU * 2));
                        g = (byte)((g * (0xffU * 2) + 0x3fU) / (0x3fU * 2));
                        b = (byte)((b * (0xffU * 2) + 0x1fU) / (0x1fU * 2));

                        return t.R == r && t.G == g && t.B == b;
                    });
            }
            else
            {
                return true;
            }
        }

        public void Convert8To32()
        {
            if (this.BitsPerPixel != 8)
            {
                return;
            }

            if (this.ImageData == null || this.Palette == null)
            {
                return;
            }

            byte[] data = new byte[this.ImageData.Length * 4];

            for (int i = 0; i < this.ImageData.Length; i++)
            {
                int c = this.ImageData[i];

                ushort color = BitConverter.ToUInt16(this.Palette, 8 * 512 + c * 2);

                byte a = this.AlphaData == null ? (byte)255 : this.AlphaData[i];
                byte r = (byte)((color & 0xF800U) >> 11);
                byte g = (byte)((color & 0x7E0U) >> 5);
                byte b = (byte)(color & 0x1FU);

                r = (byte)((r * (0xffU * 2) + 0x1fU) / (0x1fU * 2));
                g = (byte)((g * (0xffU * 2) + 0x3fU) / (0x3fU * 2));
                b = (byte)((b * (0xffU * 2) + 0x1fU) / (0x1fU * 2));

                data[i * 4 + 0] = b;
                data[i * 4 + 1] = g;
                data[i * 4 + 2] = r;
                data[i * 4 + 3] = a;
            }

            this.ImageData = data;

            this.Palette = new byte[8192];

            this.GenerateMipmaps();
        }

        public void Convert32To8()
        {
            if (this.BitsPerPixel != 32)
            {
                return;
            }

            if (this.ImageData == null)
            {
                return;
            }

            int length = this.ImageData.Length / 4;

            byte[] alphaData = new byte[length];
            bool hasAlpha = false;

            for (int i = 0; i < length; i++)
            {
                byte a = this.ImageData[i * 4 + 3];

                alphaData[i] = a;

                if (a != (byte)255)
                {
                    hasAlpha = true;
                }
            }

            this.AlphaData = hasAlpha ? alphaData : null;

            var imageColors = Enumerable.Range(0, length)
                .Select(t =>
                {
                    byte r = (byte)((this.ImageData[t * 4 + 2] * (0x1fU * 2) + 0xffU) / (0xffU * 2));
                    byte g = (byte)((this.ImageData[t * 4 + 1] * (0x3fU * 2) + 0xffU) / (0xffU * 2));
                    byte b = (byte)((this.ImageData[t * 4 + 0] * (0x1fU * 2) + 0xffU) / (0xffU * 2));

                    r = (byte)((r * (0xffU * 2) + 0x1fU) / (0x1fU * 2));
                    g = (byte)((g * (0xffU * 2) + 0x3fU) / (0x3fU * 2));
                    b = (byte)((b * (0xffU * 2) + 0x1fU) / (0x1fU * 2));

                    return Color.FromArgb((int)r, (int)g, (int)b);
                });

            var colors = imageColors
                .Distinct()
                .ToArray();

            if (colors.Length <= 256)
            {
                byte[] palette = new byte[colors.Length * 3];

                for (int i = 0; i < colors.Length; i++)
                {
                    palette[i * 3 + 0] = colors[i].R;
                    palette[i * 3 + 1] = colors[i].G;
                    palette[i * 3 + 2] = colors[i].B;
                }

                var imageData = imageColors
                    .Select(t =>
                    {
                        for (int i = 0; i < colors.Length; i++)
                        {
                            if (colors[i] == t)
                            {
                                return (byte)i;
                            }
                        }

                        return (byte)0;
                    })
                    .ToArray();

                this.SetPaletteColors(palette);
                this.ImageData = imageData;
            }
            else
            {
                byte[] data = new byte[length * 4];

                for (int i = 0; i < length; i++)
                {
                    byte r = (byte)((this.ImageData[i * 4 + 2] * (0x1fU * 2) + 0xffU) / (0xffU * 2));
                    byte g = (byte)((this.ImageData[i * 4 + 1] * (0x3fU * 2) + 0xffU) / (0xffU * 2));
                    byte b = (byte)((this.ImageData[i * 4 + 0] * (0x1fU * 2) + 0xffU) / (0xffU * 2));

                    r = (byte)((r * (0xffU * 2) + 0x1fU) / (0x1fU * 2));
                    g = (byte)((g * (0xffU * 2) + 0x3fU) / (0x3fU * 2));
                    b = (byte)((b * (0xffU * 2) + 0x1fU) / (0x1fU * 2));

                    data[i * 4 + 2] = r;
                    data[i * 4 + 1] = g;
                    data[i * 4 + 0] = b;
                }

                var image = new ColorQuant.WuColorQuantizer().Quantize(data);

                byte[] palette = new byte[768];

                for (int i = 0; i < image.Palette.Length / 4; i++)
                {
                    palette[i * 3 + 0] = image.Palette[i * 4 + 2];
                    palette[i * 3 + 1] = image.Palette[i * 4 + 1];
                    palette[i * 3 + 2] = image.Palette[i * 4 + 0];
                }

                this.SetPaletteColors(palette);
                this.ImageData = image.Bytes;
            }
        }

        public void GenerateMipmaps()
        {
            if (this.ImageData == null)
            {
                return;
            }

            if (this.BitsPerPixel == 8)
            {
                this.Convert8To32();
            }

            if (this.BitsPerPixel != 32)
            {
                return;
            }

            if (this.MipmapsCount == this.MaximumMipmapsCount)
            {
                return;
            }

            int width = this.Width;
            int height = this.Height;

            int mipmapsLength = this.MaximumMipmapsLength;

            byte[] data = new byte[mipmapsLength * 4];
            byte[] alpha = this.HasAlpha ? new byte[mipmapsLength] : null;

            Array.Copy(this.ImageData, data, this.Width * this.Height * 4);

            if (alpha != null)
            {
                Array.Copy(this.AlphaData, alpha, this.Width * this.Height);
            }

            int index = 0;
            int xLength = 1;
            int yLength = 1;

            while (width > 1 || height > 1)
            {
                index += width * height;

                if (width > 1)
                {
                    width /= 2;
                    xLength *= 2;
                }

                if (height > 1)
                {
                    height /= 2;
                    yLength *= 2;
                }

                int length = xLength * yLength;

                for (int h = 0; h < height; h++)
                {
                    for (int w = 0; w < width; w++)
                    {
                        long b = 0;
                        long g = 0;
                        long r = 0;
                        long a = 0;

                        for (int y = 0; y < yLength; y++)
                        {
                            for (int x = 0; x < xLength; x++)
                            {
                                int i = (h * yLength + y) * this.Width + w * xLength + x;

                                b += this.ImageData[i * 4 + 0];
                                g += this.ImageData[i * 4 + 1];
                                r += this.ImageData[i * 4 + 2];
                                a += this.ImageData[i * 4 + 3];
                            }
                        }

                        b /= length;
                        g /= length;
                        r /= length;
                        a /= length;

                        int dataIndex = index + h * width + w;

                        data[dataIndex * 4 + 0] = (byte)b;
                        data[dataIndex * 4 + 1] = (byte)g;
                        data[dataIndex * 4 + 2] = (byte)r;
                        data[dataIndex * 4 + 3] = (byte)a;

                        if (alpha != null)
                        {
                            alpha[dataIndex] = (byte)a;
                        }
                    }
                }
            }

            this.ImageData = data;
            this.AlphaData = alpha;
        }

        public void RemoveMipmaps()
        {
            if (this.ImageData == null)
            {
                return;
            }

            if (this.MipmapsCount == 1)
            {
                return;
            }

            int bpp = this.BitsPerPixel;

            if (bpp == 0)
            {
                return;
            }

            int length = this.Width * this.Height;
            byte[] data = new byte[length * bpp / 8];
            Array.Copy(this.ImageData, 0, data, 0, data.Length);
            this.ImageData = data;

            if (this.AlphaData != null)
            {
                byte[] alphaData = new byte[length];
                Array.Copy(this.AlphaData, 0, alphaData, 0, alphaData.Length);
                this.AlphaData = alphaData;
            }
        }

        public void MakeColorIlluminated(byte red, byte green, byte blue)
        {
            this.MakeColorIlluminated(red, green, blue, red, green, blue);
        }

        public void MakeColorIlluminated(byte red0, byte green0, byte blue0, byte red1, byte green1, byte blue1)
        {
            if (this.BitsPerPixel != 8)
            {
                return;
            }

            if (this.Palette == null)
            {
                return;
            }

            for (int c = 0; c < 256; c++)
            {
                ushort color = BitConverter.ToUInt16(this.Palette, 8 * 512 + c * 2);

                byte r = (byte)((color & 0xF800U) >> 11);
                byte g = (byte)((color & 0x7E0U) >> 5);
                byte b = (byte)(color & 0x1FU);

                r = (byte)((r * (0xffU * 2) + 0x1fU) / (0x1fU * 2));
                g = (byte)((g * (0xffU * 2) + 0x3fU) / (0x3fU * 2));
                b = (byte)((b * (0xffU * 2) + 0x1fU) / (0x1fU * 2));

                if (r >= red0 && r <= red1 && g >= green0 && g <= green1 && b >= blue0 && b <= blue1)
                {
                    byte color0 = this.Palette[8 * 512 + c * 2];
                    byte color1 = this.Palette[8 * 512 + c * 2 + 1];

                    for (int i = 0; i < 16; i++)
                    {
                        if (i == 8)
                        {
                            continue;
                        }

                        this.Palette[i * 512 + c * 2] = color0;
                        this.Palette[i * 512 + c * 2 + 1] = color1;
                    }
                }
            }
        }

        public static bool AreEquals(Texture textureA, Texture textureB)
        {
            if (textureA == null || textureB == null)
            {
                return false;
            }

            if (textureA.Width == 0 || textureA.Height == 0 || textureA.ImageData == null)
            {
                return false;
            }

            if (textureB.Width == 0 || textureB.Height == 0 || textureB.ImageData == null)
            {
                return false;
            }

            if (textureA.Width != textureB.Width)
            {
                return false;
            }

            if (textureA.Height != textureB.Height)
            {
                return false;
            }

            if ((textureA.Palette != null) != (textureB.Palette != null))
            {
                return false;
            }

            if (textureA.Palette != null)
            {
                if (textureA.Palette.Length != textureB.Palette.Length)
                {
                    return false;
                }

                for (int i = 0; i < textureA.Palette.Length; i++)
                {
                    if (textureA.Palette[i] != textureB.Palette[i])
                    {
                        return false;
                    }
                }
            }

            if (textureA.ImageData.Length != textureB.ImageData.Length)
            {
                return false;
            }

            for (int i = 0; i < textureA.ImageData.Length; i++)
            {
                if (textureA.ImageData[i] != textureB.ImageData[i])
                {
                    return false;
                }
            }

            if ((textureA.AlphaData != null) != (textureB.AlphaData != null))
            {
                return false;
            }

            if (textureA.AlphaData != null)
            {
                if (textureA.AlphaData.Length != textureB.AlphaData.Length)
                {
                    return false;
                }

                for (int i = 0; i < textureA.AlphaData.Length; i++)
                {
                    if (textureA.AlphaData[i] != textureB.AlphaData[i])
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
