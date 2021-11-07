using PicToMap.Models.Drawing;
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
		private Color[] Pixels { get; }
		private int Width { get; }
		private int Height { get; }
		private string[] Blocks { get; set; }
		private Color[] Colors { get; }
		private int[] Blueprint { get; }
		private int[] Heights { }
		public MapGenerator(Settings settings)
		{
			Settings = settings;
			(Pixels, Width, Height) = ReadImage(Settings.ImagePath);
			var (blocks, colorsAsStrings) = ReadJson();
			Blocks = blocks;
			Colors = ParseColors(colorsAsStrings);
			var length = Pixels.Length + Width;
			Blueprint = new int[length];
		}
		private (string[] _blocks, string[] _colorsAsStrings) ReadJson()
		{
			var jsonContents = File.ReadAllText(Path.Combine(Settings.CurrentDirectory, "block_colors.json"));
			var jsonDictionary = JsonSerializer.Deserialize<Dictionary<string, string[]>>(jsonContents);
			if (jsonDictionary == null) throw new FormatException();
			var keys = new string[jsonDictionary.Count];
			jsonDictionary.Keys.CopyTo(keys, 0);
			string[] values;		
			values = new string[jsonDictionary.Count];
			for (var i = 0; i < values.Length; i++)
			{
				values[i] = jsonDictionary[keys[i]][0];
			}		
			return (keys, values);
		}
		private static Color[] ParseColors(string[] colorsAsStrings)
		{
			var colors = new Color[colorsAsStrings.Length];
			for (int i = 0; i < colorsAsStrings.Length; i++)
			{
				colors[i] = Color.Parse(colorsAsStrings[i]);
			}
			return colors;
		}
		private (Color[] _pixels, int _width, int _height) ReadImage(string path)
		{
			var source = new Bitmap(path);
			var (width, height) = ScaledDimensions(source.Width, source.Height, Settings.WidthInMaps * 128, Settings.HeightInMaps * 128);
			var resizeMethod = Settings.HighQualitySelected ?
			BitmapUtils.ResizeMethod.HighQuality :
			BitmapUtils.ResizeMethod.NearestNeighbour;
			var resizedBitmap = BitmapUtils.Resize(source, width, height, resizeMethod);
			source.Dispose();
			var bgraValues = BitmapUtils.GetPixels(resizedBitmap, BitmapUtils.Channels.ARGB);			
			resizedBitmap.Dispose();
			return (Color.FromByteArrayARGB(bgraValues), width, height);
		}
		private static (int _width, int _height) ScaledDimensions(int sourceWidth, int sourceHeight, int boundariesWidth, int boundariesHeight)
		{
			if ((float)sourceWidth / sourceHeight > (float)boundariesWidth / boundariesHeight)
			{
				return (boundariesWidth, (int)Math.Round((double)boundariesWidth * sourceHeight / sourceWidth));
			}
			return ((int)Math.Round((double)boundariesHeight * sourceWidth / sourceHeight), boundariesHeight);
		}
		private static Color[] ParseMapColors(string[] colors)
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
			var cobblestone = Array.IndexOf(Blocks, "cobblestone");
			if (cobblestone == -1)
            {
				var blocks = new string[Blocks.Length + 1];
				Blocks.CopyTo(blocks, 0);
				blocks[^0] = "cobblestone"; 
				Blocks = blocks;
				cobblestone = Blocks.Length - 1;
            }
			for (var i = 0; i < Width; i++)
			{
				Blueprint[i] = cobblestone;
			}
			for (var i = Width; i < Blueprint.Length; i++)
			{
				if (Pixels[i - Width].IsTranslucent)
				{
					Blueprint[i] = -1;
					continue;
				}
				if (Settings.DitheringChecked)
				{
					Pixels[i - Width].Fix();
				}
				var colorIndex = GetClosestColorIndex(Pixels[i]);
				Blueprint[i] = colorIndex;
				if (!Settings.DitheringChecked) continue;
				DistributeError(i, Pixels[i] - Colors[colorIndex]);
			}
		}
		private void CalculateHeights()
        {

        }

		private void DistributeError(int index, Color error)
        {
			var right = (index + 1) % Width != 0;
			var leftBottom = index % Width != 0;
			var bottom = index + Width < Pixels.Length;
			var rightBottom = right && bottom;
			if (right)
			{
				Pixels[index].Add(error * 0.4375f);
			}
			if (leftBottom)
			{
				Pixels[index].Add(error * 0.1875f);
			}
			if (bottom)
			{
				Pixels[index].Add(error * 0.3125f);
			}
			if (rightBottom)
			{
				Pixels[index].Add(error * 0.0625f);
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
		private int GetClosestColorIndex(Color color)
		{
			var result = -1;
			var minDistance = float.MaxValue;
			for (int i = 0; i < Colors.Length; i++)
			{
				var currentDistance = Color.Distance(color, Colors[i]);
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
