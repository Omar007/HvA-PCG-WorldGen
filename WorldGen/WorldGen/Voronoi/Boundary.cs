
namespace WorldGen.Voronoi
{
	public class Boundary
	{
		#region Fields
		private int left;
		private int right;
		private int top;
		private int bottom;
		#endregion

		#region Properties
		public int Top
		{
			get { return top; }
		}

		public int Bottom
		{
			get { return bottom; }
		}

		public int Left
		{
			get { return left; }
		}

		public int Right
		{
			get { return right; }
		}
		#endregion

		public Boundary(int left, int right, int top, int bottom)
		{
			this.left = left;
			this.right = right;
			this.top = top;
			this.bottom = bottom;
		}
	}
}
