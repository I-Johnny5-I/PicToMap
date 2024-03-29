﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using PicToMap.Models.Drawing;

namespace PicToMap.Models
{
    public struct Assets
    {
        public string[] _blockIds;
        public Color[] _mapColors;
        public Color[] _pixels;
        public int _width;
        public int _height;

        public Assets(string imagePath, bool highQualitySelected, int widthInMaps, int heightInMaps)
        {
            (_blockIds, _mapColors) = ReadAssetsJson();
            var source = new Bitmap(imagePath);
            (_width, _height) = ScaledDimensions(source.Width, source.Height, widthInMaps * 128, heightInMaps * 128);
            var resizeMethod = highQualitySelected ?
            Bitmaps.ResizeMethod.HighQuality :
            Bitmaps.ResizeMethod.NearestNeighbour;
            var resizedBitmap = Bitmaps.Resize(source, _width, _height, resizeMethod);
            source.Dispose();
            var bgraValues = Bitmaps.GetPixels(resizedBitmap, Bitmaps.Channels.ARGB);
            resizedBitmap.Dispose();
            _pixels = Color.FromByteArrayARGB(bgraValues);
            _height++;

        }
        private static (string[], Color[]) ReadAssetsJson()
        {
            var json = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory() + "/assets.json"));
            var idsAndColors = JsonSerializer.Deserialize<Dictionary<string, string[]>>(json);
            if (idsAndColors == null) throw new NullReferenceException();
            var blockIds = new string[idsAndColors.Count];
            idsAndColors.Keys.CopyTo(blockIds, 0);
            var mapColors = new Color[blockIds.Length * 3];
            for (var i = 0; i < blockIds.Length; i++)
                for (var j = 0; j < 3; j++)
                {
                    mapColors[i * 3 + j] = Color.Parse(idsAndColors[blockIds[i]][j]);
                }
            return (blockIds, mapColors);
        }
        private static (int _width, int _height) ScaledDimensions(int sourceWidth, int sourceHeight, int boundariesWidth, int boundariesHeight)
        {
            if ((double)sourceWidth / sourceHeight > (double)boundariesWidth / boundariesHeight)
            {
                return (boundariesWidth, (int)Math.Round((double)boundariesWidth * sourceHeight / sourceWidth));
            }
            return ((int)Math.Round((double)boundariesHeight * sourceWidth / sourceHeight), boundariesHeight);
        }
    }
}
