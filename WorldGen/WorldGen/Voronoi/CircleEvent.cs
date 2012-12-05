
namespace WorldGen.Voronoi
{
	public class CircleEvent : RBNode
	{
		#region Fields
		private double x;
		private double y;
		private double yCenter;

		private BeachSection arc;
		#endregion

		#region Properties
		public double X
		{
			get { return x; }
			set { x = value; }
		}

		public double Y
		{
			get { return y; }
			set { y = value; }
		}

		public double YCenter
		{
			get { return yCenter; }
			set { yCenter = value; }
		}

		public BeachSection Arc
		{
			get { return arc; }
			set { arc = value; }
		}
		#endregion

		public CircleEvent()
		{
		}
	}
}
