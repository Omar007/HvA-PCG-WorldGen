
namespace WorldGen.Voronoi
{
	public class BeachSection : RBNode
	{
		#region Fields
		private Cell cell;
		private Edge edge;
		private CircleEvent circleEvent;
		#endregion

		#region Properties
		public Cell Cell
		{
			get { return cell; }
			set { cell = value; }
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
