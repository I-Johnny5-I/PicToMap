using PicToMap.Models.Drawing;
using PicToMap.Models.Structures;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Text.Json;

namespace PicToMap.Models
{
	public class MapGenerator
	{
		private Settings Settings { get; }
		private Structures.Color[] Pixels { get; }
		private int Width { get; }
		private int Height { get; }
		private string[] Blocks { get; }
		private Structures.Color[] Colors { get; }
		private (int _id, int _y)[] Blueprint { get; }
		public MapGenerator(Settings settings)
		{
			Settings = settings;
			(Pixels, Width, Height) = ReadImage(Settings.ImagePath);
			var (blocks, colorsAsStrings) = ReadJson();
			Blocks = blocks;
			Colors = ParseColors(colorsAsStrings);
			Blueprint = new (int _id, int _y)[Pixels.Length];
		}
		private (string[] _blocks, string[] _colorsAsStrings) ReadJson()
		{
			var jsonContents = File.ReadAllText(Path.Combine(Settings.CurrentDirectory, "block_colors.json"));
			var jsonDictionary = JsonSerializer.Deserialize<Dictionary<string, string[]>>(jsonContents);
			if (jsonDictionary == null) throw new FormatException();
			var keys = new string[jsonDictionary.Count];
			var values = new string[jsonDictionary.Count * 3];
			jsonDictionary.Keys.CopyTo(keys, 0);
			for (var i = 0; i < jsonDictionary.Count; i++)
			{
				jsonDictionary[keys[i]].CopyTo(values, i * 3);
			}
			return (keys, values);
		}
		private Structures.Color[] ParseColors(string[] colorsAsStrings)
		{
			var colors = new Structures.Color[colorsAsStrings.Length];
			for (int i = 0; i < colorsAsStrings.Length; i++)
			{
				colors[i] = Structures.Color.Parse(colorsAsStrings[i]);
			}
			return colors;
		}
		private (Structures.Color[] _pixels, int _width, int _height) ReadImage(string path)
		{
			var source = new Bitmap(path);
			var (width, height) = ScaledDimensions(source.Width, source.Height, Settings.WidthInMaps * 128, Settings.HeightInMaps * 128);
			var resizeMethod = Settings.HighQualitySelected ?
			BitmapUtils.ResizeMethod.HighQuality :
			BitmapUtils.ResizeMethod.NearestNeighbour;
			var resizedBitmap = BitmapUtils.Resize(source, width, height, resizeMethod);
			var bgraValues = BitmapUtils.GetPixels(resizedBitmap, BitmapUtils.Channels.ARGB);
			source.Dispose();
			resizedBitmap.Dispose();
			return (Structures.Color.FromByteArrayARGB(bgraValues), width, height);
		}
		public static (int _width, int _height) ScaledDimensions(int sourceWidth, int sourceHeight, int boundariesWidth, int boundariesHeight)
		{
			if ((float)sourceWidth / sourceHeight > (float)boundariesWidth / boundariesHeight)
			{
				return (boundariesWidth, (int)Math.Round((double)boundariesWidth * sourceHeight / sourceWidth));
			}
			return ((int)Math.Round((double)boundariesHeight * sourceWidth / sourceHeight), boundariesHeight);
		}
		private Structures.Color[] ParseMapColors(string[] colors)
		{
			var result = new Structures.Color[colors.Length];
			for (var i = 0; i < colors.Length; i++)
			{
				result[i] = Structures.Color.Parse(colors[i]);
			}
			return result;
		}
		private void MakeBlueprint()
		{
			for (int y = 0; y < Height; y++)
				for (int x = 0; x < Width; x++)
				{
					var i = Index(x, y, Width);
					if (Pixels[i].IsTranslucent)
					{
						Blueprint[i]._id = -1;
					}
					if (Settings.DitheringChecked)
						Pixels[i].Fix();
					var id = GetClosestColorID(Pixels[i]);
					Blueprint[i]._id = id;
					if (Settings.DitheringChecked)
					{
						var error = Pixels[i] - Colors[id];
						if (x != Width - 1)
						{
							i = Index(x + 1, y, Width);
							Pixels[i].Add(error * 0.4375f);
						}
						if (x != 0 && y != Height - 1)
						{
							i = Index(x - 1, y + 1, Width);
							Pixels[i].Add(error * 0.1875f);
						}
						if (y != Height - 1)
						{
							i = Index(x, y + 1, Width);
							Pixels[i].Add(error * 0.3125f);
						}
						if (x != Width - 1 && y != Height - 1)
						{
							i = Index(x + 1, y + 1, Width);
							Pixels[i].Add(error * 0.0625f);
						}
					}
				}
		}
		private void MakeFlatBlueprint()
        {
			for (var i = 0; i < Pixels.Length; i++)
            {
                if (Pixels[i].IsTranslucent)
                {
					Blueprint[i]._id = -1;
                }
				if (Settings.DitheringChecked)
					Pixels[i].Fix();
			}
        }
		private void WriteDatapack()
		{
			var tempPath = Path.Combine(Settings.CurrentDirectory, "temp");
			if (Directory.Exists(tempPath))
			{
				Directory.Delete(tempPath, true);
			}

			var currentX = Settings.X + datapackX * 1024;
			var currentZ = Settings.Z + datapackZ * 1024;

			var functionsPath = Path.Combine(tempPath, "data", "block_art", "functions");
			if (!Directory.Exists(functionsPath)) Directory.CreateDirectory(functionsPath);
			File.WriteAllText(Path.Combine(tempPath, "pack.mcmeta"),
				"{\"pack\":{\"pack_format\": 7,\"description\": \"\"}}");

			var commands = new List<string>();
			File.WriteAllText(Path.Combine(functionsPath, "teleport.mcfunction"),
				$"tp @s {currentX} {Offset.Y + 1} {currentZ}");

			// forceload command has limits, which is why we will need multiple commands to forceload big areas
			var removeForceloadCommands = new List<string>();
			for (var fz = 0; fz < 1024; fz += 128)
				for (var fx = 0; fx < 1024; fx += 128)
				{
					var command =
						$"forceload add {fx + currentX} {fz + currentZ} {fx + 127 + currentX} {fz + 127 + currentZ}";
					commands.Add(command);
					removeForceloadCommands.Add(command.Replace("add", "remove"));
				}

			File.WriteAllLines(Path.Combine(functionsPath, "forceload_add.mcfunction"), commands);
			File.WriteAllLines(Path.Combine(functionsPath, "forceload_remove.mcfunction"), removeForceloadCommands);
			removeForceloadCommands.Clear();

			commands.Clear();
			var upperBoundX = datapackX * 1024 + 1024;
			var upperBoundZ = datapackZ * 1024 + 1024;
			for (var blockZ = datapackZ * 1024; blockZ < upperBoundZ && blockZ < Height; blockZ++)
			{
				for (int blockX = datapackX * 1024; blockX < upperBoundX && blockX < Width; blockX++)
				{
					var index = Index(blockX, blockZ, Width);
					commands.Add($"setblock {Offset.X + blockX} {Offset.Y} {Offset.Z + blockZ} {Ids[Blueprint[index]]}");
				}
			}
			File.WriteAllLines(Path.Combine(functionsPath, "draw.mcfunction"), commands);
			commands.Clear();

			var zipFile = Path.Combine(DestinationFolder, $"part_{count}.zip");
			if (File.Exists(zipFile)) File.Delete(zipFile);
			ZipFile.CreateFromDirectory(tempPath, zipFile);
			Directory.Delete(tempPath, true);
		}

        public void Generate()
        {
			MakeBlueprint();
			WriteDatapack();
        }
		private int GetClosestColorID(Structures.Color color)
		{
			var result = -1;
			var minDistance = float.MaxValue;
			for (int i = 0; i < Colors.Length; i++)
			{
				var currentDistance = Structures.Color.Distance(color, Colors[i]);
				if (currentDistance < minDistance)
				{
					minDistance = currentDistance;
					result = i;
				}
			}
			return result;
		}
		private static int Index(int x, int y, int width) => y * width + x;
	}
}
