using System;

namespace PicToMap.Models.Structures
{
    public struct Color
	{
		public float _r;
		public float _g;
		public float _b;
		public float _a;
		public bool IsTranslucent => _a < 128;

		public Color(float r, float g, float b, float a = 255)
		{	
			_r = r;
			_g = g;
			_b = b;
			_a = a;
		}

		public void Fix()
		{
			_r = _r < 0f ? 0f : (_r > 255 ? 255f : _r);
			_g = _g < 0f ? 0f : (_g > 255 ? 255f : _g);
			_b = _b < 0f ? 0f : (_b > 255 ? 255f : _b);
		}

		public void Add(Color color)
		{
			_r += color._r;
			_g += color._g;
			_b += color._b;
		}
	
		public static Color operator -(Color left, Color right)
		{
			return new Color(
				left._r - right._r,
				left._g - right._g,
				left._b - right._b);
		}

		public static Color operator *(Color left, float right)
		{
			return new Color(
				left._r * right,
				left._g * right,
				left._b * right);
		}

		public static float Distance(Color a, Color b)
		{
			return (float)Math.Sqrt(
				(a._r - b._r) * (a._r - b._r) +
				(a._g - b._g) * (a._g - b._g) +
				(a._b - b._b) * (a._b - b._b));
		}

		public static Color[] FromByteArrayARGB(byte[] bgraValues)
		{
			var newArray = new Color[bgraValues.Length / 4];
			for (int i = 0; i < bgraValues.Length; i += 4)
			{
				newArray[i / 4] = new Color(
					bgraValues[i + 2],
					bgraValues[i + 1],
					bgraValues[i],
					bgraValues[i + 3]);
			}
			return newArray;
		}

		public static Color Parse(string inputString)
        {
			var rgbValues = inputString.Split(' ');
			if (rgbValues.Length != 3)
				throw new ArgumentException();
			return new Color(
				float.Parse(rgbValues[0]),
				float.Parse(rgbValues[1]),
				float.Parse(rgbValues[2])
				);
        }
	}
}
