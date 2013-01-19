using System.Drawing;

namespace WorldGen.HelperFunctions
{
	public static class ColorHelpers
	{
		public static float Lerp(this float start, float end, float amount)
		{
			float difference = end - start;
			float adjusted = difference * amount;
			return start + adjusted;
		}

		public static Color Lerp(this Color colour, Color to, float amount)
		{
			// start colours as lerp-able floats
			float sa = colour.A;
			float sr = colour.R;
			float sg = colour.G;
			float sb = colour.B;

			// end colours as lerp-able floats
			float ea = to.A;
			float er = to.R;
			float eg = to.G;
			float eb = to.B;

			// lerp the colours to get the difference
			byte a = (byte)sa.Lerp(ea, amount);
			byte r = (byte)sr.Lerp(er, amount);
			byte g = (byte)sg.Lerp(eg, amount);
			byte b = (byte)sb.Lerp(eb, amount);

			// return the new colour
			return Color.FromArgb(a, r, g, b);
		}
	}
}
