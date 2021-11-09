using PicToMap.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using PicToMap.ViewModels;

namespace PicToMap.Models
{
	public class MapGenerator
	{
		private MainWindowViewModel Settings { get;}
		private Assets Assets { get; }
		private int[] Blueprint { get; }
		private int[]? Heights { get; set; }
		public MapGenerator(MainWindowViewModel settings, Assets assets)
		{
			Settings = settings;
			Assets = assets;
			if (Settings.ImagePath == null) throw new NullReferenceException();
			Blueprint = new int[Assets._width * Assets._height + 1];
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

		private void MakeBlueprint()
		{
			var cobblestone = Array.IndexOf(Blocks, "cobblestone");
			if (cobblestone == -1)
            {
				var blocks = new string[Blocks.Length + 1];
				Blocks.CopyTo(blocks, 0);
				blocks[^1] = "cobblestone"; 
				Blocks = blocks;
				cobblestone = (Blocks.Length - 1) * 3;
            }
			for (var i = 0; i < Width; i++)
			{
				Blueprint[i] = cobblestone;
			}
			for (var i = 0; i < Pixels.Length; i++)
			{
				if (Pixels[i].IsTranslucent)
				{
					Blueprint[i + Width] = -1;
					continue;
				}
				if (Settings.DitheringChecked)
				{
					Pixels[i].Fix();
				}
				var colorIndex = GetClosestColorIndex(Pixels[i]);
				Blueprint[i + Width] = colorIndex;
				if (!Settings.DitheringChecked) continue;
				DistributeError(i, Pixels[i] - Colors[colorIndex]);
			}
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
		private void CalculateHeights()
        {
			Heights = new int[Blueprint.Length];
			for (var i = 0; i < Width; i++)
            {
				Heights[i] = 0;
            }
			for (var i = Width; i < Heights.Length; i++)
            {
				var height = 0;
				switch (Blueprint[i] % 3)
                {
					case 1:
						height++;
						break;
					case 2:
						height--;
						break;
                }
				Heights[i] = Heights[i - Width] + height;
            }
			for (var x = 0; x < Width; x++)
            {
				var minHeight = int.MaxValue;
				for (var y = x; y < Heights.Length; y += Width)
                {
					if (Heights[y] < minHeight)
                    {
						minHeight = Heights[y];
                    }
                }
				for (var y = x; y < Heights.Length; y += Width)
				{
					Heights[y] -= minHeight;
				}
			}
        }

		private void WriteDatapack()
		{
			var tempPath = Path.Combine(Directory.GetCurrentDirectory(), "temp");
			if (Directory.Exists(tempPath))
			{
				Directory.Delete(tempPath, true);
			}
			var functionsPath = Path.Combine(tempPath, "data", "pic_to_map", "functions");
			if (!Directory.Exists(functionsPath)) Directory.CreateDirectory(functionsPath);
			File.WriteAllText(Path.Combine(tempPath, "pack.mcmeta"),
				"{\"pack\":{\"pack_format\": 7,\"description\": \"\"}}");

			var commands = new List<string>();

			// forceload command has limits, which is why we will need multiple commands to forceload big areas
			var removeForceloadCommands = new List<string>();
			for (var fz = Settings.Z; fz < Settings.HeightInMaps * 128 + Settings.Z; fz += 128)
				for (var fx = Settings.X; fx < Settings.WidthInMaps * 128 + Settings.X; fx += 128)
				{
					var command =
						$"forceload add {fx} {fz} {fx + 127} {fz + 127}";
					commands.Add(command);
					removeForceloadCommands.Add(command.Replace("add", "remove"));
				}

			File.WriteAllLines(Path.Combine(functionsPath, "forceload_add.mcfunction"), commands);
			File.WriteAllLines(Path.Combine(functionsPath, "forceload_remove.mcfunction"), removeForceloadCommands);

			commands.Clear();
			removeForceloadCommands.Clear();

			for (var blockZ = 0; blockZ < Height; blockZ++)
			{
				for (int blockX = 0; blockX < Width; blockX++)
				{
					var index = Index(blockX, blockZ, Width);
					if (Settings.StaircaseSelected && Heights != null)
					{
						commands.Add($"setblock {Settings.X + blockX} {Heights[index] + Settings.Y} {Settings.Z + blockZ - 1} {Blocks[Blueprint[index] / 3]}");
					}
					else
                    {
						commands.Add($"setblock {Settings.X + blockX} {Settings.Y} {Settings.Z + blockZ - 1} {Blocks[Blueprint[index]]}");
					}
				}
			}
			File.WriteAllLines(Path.Combine(functionsPath, "draw.mcfunction"), commands);
			commands.Clear();
			if (Settings.DestinationDirectory == null) throw new NullReferenceException();
			var zipFile = Path.Combine(Settings.DestinationDirectory, $"{Settings.Name}.zip");
			if (File.Exists(zipFile)) File.Delete(zipFile);
			ZipFile.CreateFromDirectory(tempPath, zipFile);
			Directory.Delete(tempPath, true);
		}

        public void Generate()
        {
			MakeBlueprint();
			if (Settings.StaircaseSelected)
            {
				CalculateHeights();
            }
			WriteDatapack();
        }
		private int GetClosestColorIndex(Color color)
		{
			var result = -1;
			var minDistance = double.MaxValue;
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
