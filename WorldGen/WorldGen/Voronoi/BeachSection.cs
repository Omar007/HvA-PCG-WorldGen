
namespace WorldGen.Voronoi
{
	public class BeachSection : RBNode
	{
		#region Fields
		private Site site;
		private Edge edge;
		private CircleEvent circleEvent;
		#endregion

		#region Properties
		public Site Site
		{
			get { return site; }
			set { site = value; }
		}

		public Edge Edge
		{
			get { return edge; }
			set { edge = value; }
		}

		public CircleEvent CircleEvent
		{
			get { return circleEvent; }
			set { circleEvent = value;}
		}
		#endregion

		public BeachSection()
		{
		}
	}
}
