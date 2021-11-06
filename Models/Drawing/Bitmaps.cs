using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace PicToMap.Models.Drawing
{
	public class BitmapUtils
	{
		public enum ResizeMethod { NearestNeighbour, HighQuality }
		public enum Channels { RGB = 3, ARGB = 4 }
		public static byte[] GetPixels(Bitmap source, Channels channels)
		{
			var pixelFormat = channels == Channels.RGB ? PixelFormat.Format24bppRgb : PixelFormat.Format32bppArgb;
			var bitmapData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, pixelFormat);
			var stride = source.Width * ((int)channels);
			var pixels = new byte[stride * source.Height];
			Marshal.Copy(bitmapData.Scan0, pixels, 0, pixels.Length);
			source.UnlockBits(bitmapData);
			return pixels;
		}

		public static Bitmap Resize(Bitmap source, int newWidth, int newHeight, ResizeMethod resizeMethod)
		{
			var resized = new Bitmap(newWidth, newHeight, source.PixelFormat);
			using (var g = Graphics.FromImage(resized))
			{
				if (resizeMethod == ResizeMethod.NearestNeighbour)
				{
					g.SmoothingMode = SmoothingMode.None;
					g.PixelOffsetMode = PixelOffsetMode.HighQuality;
					g.CompositingQuality = CompositingQuality.HighQuality;
					g.InterpolationMode = InterpolationMode.NearestNeighbor;
				}
				if (resizeMethod == ResizeMethod.HighQuality)
				{
					g.SmoothingMode = SmoothingMode.HighQuality;
					g.PixelOffsetMode = PixelOffsetMode.HighQuality;
					g.CompositingQuality = CompositingQuality.HighQuality;
					g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				}
				var destRect = new Rectangle(0, 0, newWidth, newHeight);
				var imageAttributes = new ImageAttributes();
				imageAttributes.SetWrapMode(WrapMode.TileFlipXY);
				g.DrawImage(source, destRect, 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, imageAttributes);
				imageAttributes.Dispose();
			}
			return resized;
		}
	}
}
