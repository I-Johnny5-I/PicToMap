using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicToMap.Models
{
    public class StaircaseMapGenerator : MapGenerator
    {
        private int[] Heights { get; }
        public StaircaseMapGenerator(Settings settings) : base(settings)
        {
            Heights = new int[Blueprint.Length];
        }

		protected override void MakeBlueprint()
		{
			var cobblestone = Array.IndexOf(Blocks, "cobblestone");
			for (var i = 0; i < Width; i++)
			{
				Blueprint[i] = cobblestone;
				Heights[i] = 0;
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
				var offset = 0;
				switch (colorIndex % 3)
				{
					case 1:
						offset += 1;
						break;
					case 2:
						offset -= 1;
						break;
				}
				Heights[i] = Heights[i - Width] + offset;
				if (!Settings.DitheringChecked)
				{
					continue;
				}
				DistributeError(i, Pixels[i] - Colors[colorIndex]);
			}
		}
	}
}
