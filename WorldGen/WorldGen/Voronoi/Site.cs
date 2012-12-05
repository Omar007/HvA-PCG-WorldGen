
namespace WorldGen.Voronoi
{
	public class Site
	{
		#region Fields
		private Vertex vertex;

		private int voronoiID;
		#endregion

		#region Properties
		public double X
		{
			get { return vertex.X; }
		}

		public double Y
		{
			get { return vertex.Y; }
		}

		public Vertex Vertex
		{
			get { return vertex; }
		}

		public int VoronoiID
		{
			get { return voronoiID; }
			set { voronoiID = value; }
		}
		#endregion

		public Site(double x, double y)
			: this(new Vertex(x, y))
		{
		}

		public Site(Vertex v)
		{
			this.vertex = v;
		}
	}
}
