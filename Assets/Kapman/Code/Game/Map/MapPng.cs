using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// A PNG READ WITHOUT THE ENGINE, so that it can be read on a thread of its own (the user, 7 Oct 2026, night: "the
    /// map also is extremely buggy with the map not updating on the fly fast enough"). A tile of the chart's picture is
    /// 4096 pixels a side: read, unpacked and recoloured by the engine's own calls it held the game still for the best
    /// part of a second, a tile a frame. This does the reading, the unpacking and the recolouring off the main thread
    /// and hands back the pixels ready to be put on the card.
    ///
    /// It reads what the map's bakers write and what a paint program saves by default: 8 bits a channel, not
    /// interlaced, as colours of a palette (with or without clear ones), grey, grey with alpha, RGB or RGBA. Anything
    /// else (16 bits, interlaced, fewer than 8 bits) it declines, and the caller reads that file the old way.
    /// The rows come out bottom-up, as a Texture2D wants them.
    /// </summary>
    public static class MapPng
    {
        static readonly byte[] Signature = { 137, 80, 78, 71, 13, 10, 26, 10 };

        /// <summary>The picture's pixels (bottom row first), each colour passed through `which` of the world's palettes
        /// (MapPalette.Map; 0: as painted). `into` is used if it is big enough, else a new array is made. Null if the
        /// file is of a kind this does not read.</summary>
        public static Color32[] Read(byte[] file, int which, Color32[] into, out int width, out int height)
        {
            width = height = 0;
            if (file == null || file.Length < 33)
                return null;
            for (int i = 0; i < 8; i++)
                if (file[i] != Signature[i])
                    return null;
            int depth = 0, kind = -1, interlace = 0;
            Color32[] palette = null;
            var data = new MemoryStream(file.Length);
            int at = 8;
            while (at + 12 <= file.Length)
            {
                int length = (file[at] << 24) | (file[at + 1] << 16) | (file[at + 2] << 8) | file[at + 3];
                int type = (file[at + 4] << 24) | (file[at + 5] << 16) | (file[at + 6] << 8) | file[at + 7];
                int body = at + 8;
                if (length < 0 || body + length + 4 > file.Length)
                    return null;
                if (type == 0x49484452)                                   // IHDR
                {
                    width = (file[body] << 24) | (file[body + 1] << 16) | (file[body + 2] << 8) | file[body + 3];
                    height = (file[body + 4] << 24) | (file[body + 5] << 16) | (file[body + 6] << 8) | file[body + 7];
                    depth = file[body + 8];
                    kind = file[body + 9];
                    interlace = file[body + 12];
                }
                else if (type == 0x504C5445)                              // PLTE
                {
                    palette = new Color32[256];
                    for (int i = 0; i < 256; i++)
                        palette[i] = new Color32(0, 0, 0, 255);
                    for (int i = 0; i < length / 3 && i < 256; i++)
                        palette[i] = new Color32(file[body + i * 3], file[body + i * 3 + 1], file[body + i * 3 + 2], 255);
                }
                else if (type == 0x74524E53 && palette != null)           // tRNS (how clear each colour of the palette is)
                {
                    for (int i = 0; i < length && i < 256; i++)
                        palette[i].a = file[body + i];
                }
                else if (type == 0x49444154)                              // IDAT
                    data.Write(file, body, length);
                else if (type == 0x49454E44)                              // IEND
                    break;
                at = body + length + 4;
            }
            if (width < 1 || height < 1 || depth != 8 || interlace != 0 || data.Length < 3)
                return null;
            int bpp = kind == 3 ? 1 : kind == 0 ? 1 : kind == 4 ? 2 : kind == 2 ? 3 : kind == 6 ? 4 : 0;
            if (bpp == 0 || (kind == 3 && palette == null))
                return null;
            long count = (long)width * height;
            if (count > int.MaxValue)
                return null;
            var px = into != null && into.Length == count ? into : new Color32[count];

            // the palette's colours are passed through the world's palette once, not a pixel at a time
            if (kind == 3 && which > 0)
                for (int i = 0; i < 256; i++)
                    palette[i] = MapPalette.Map(palette[i], which);

            int n = width * bpp;
            byte[] cur = new byte[n], prev = new byte[n];
            var seen = which > 0 && kind != 3 ? new Dictionary<int, Color32>(64) : null;
            int lastKey = -1;
            Color32 last = default;
            data.Position = 2;                                            // (past the two bytes that say "zlib")
            using (var z = new DeflateStream(data, CompressionMode.Decompress))
            {
                for (int y = 0; y < height; y++)
                {
                    int filter = z.ReadByte();
                    if (filter < 0)
                        return null;
                    int got = 0;
                    while (got < n)
                    {
                        int r = z.Read(cur, got, n - got);
                        if (r <= 0)
                            return null;
                        got += r;
                    }
                    switch (filter)
                    {
                        case 0:
                            break;
                        case 1:
                            for (int i = bpp; i < n; i++)
                                cur[i] = (byte)(cur[i] + cur[i - bpp]);
                            break;
                        case 2:
                            for (int i = 0; i < n; i++)
                                cur[i] = (byte)(cur[i] + prev[i]);
                            break;
                        case 3:
                            for (int i = 0; i < n; i++)
                                cur[i] = (byte)(cur[i] + (((i >= bpp ? cur[i - bpp] : 0) + prev[i]) >> 1));
                            break;
                        case 4:
                            for (int i = 0; i < n; i++)
                            {
                                int a = i >= bpp ? cur[i - bpp] : 0, b = prev[i], c = i >= bpp ? prev[i - bpp] : 0;
                                int p = a + b - c, pa = Math.Abs(p - a), pb = Math.Abs(p - b), pc = Math.Abs(p - c);
                                cur[i] = (byte)(cur[i] + (pa <= pb && pa <= pc ? a : pb <= pc ? b : c));
                            }
                            break;
                        default:
                            return null;
                    }
                    int o = (height - 1 - y) * width;                     // (the rows of a texture run bottom-up)
                    if (kind == 3)
                    {
                        for (int x = 0; x < width; x++)
                            px[o + x] = palette[cur[x]];
                    }
                    else
                    {
                        for (int x = 0, i = 0; x < width; x++, i += bpp)
                        {
                            byte r, g, b, a = 255;
                            if (kind == 2 || kind == 6)
                            {
                                r = cur[i];
                                g = cur[i + 1];
                                b = cur[i + 2];
                                if (kind == 6)
                                    a = cur[i + 3];
                            }
                            else
                            {
                                r = g = b = cur[i];
                                if (kind == 4)
                                    a = cur[i + 1];
                            }
                            if (seen == null)
                            {
                                px[o + x] = new Color32(r, g, b, a);
                                continue;
                            }
                            int key = (r << 16) | (g << 8) | b;
                            if (key != lastKey)
                            {
                                if (!seen.TryGetValue(key, out last))
                                    seen[key] = last = MapPalette.Map(new Color32(r, g, b, 255), which);
                                lastKey = key;
                            }
                            px[o + x] = new Color32(last.r, last.g, last.b, a);
                        }
                    }
                    var swap = prev;
                    prev = cur;
                    cur = swap;
                }
            }
            return px;
        }
    }
}
