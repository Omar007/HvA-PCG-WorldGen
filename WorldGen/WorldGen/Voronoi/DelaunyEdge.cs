
namespace WorldGen.Voronoi
{
	public class DelaunyEdge
	{
		#region Fields
		private Site lSite;
		private Site rSite;
		#endregion

		#region Properties
		public Site LeftSite
		{
			get { return lSite; }
		}

		public Site RightSite
		{
			get { return rSite; }
		}

		public Vertex VertexA
		{
			get { return lSite.Vertex; }
		}

		public Vertex VertexB
		{
			get { return rSite.Vertex; }
		}
		#endregion

		public DelaunyEdge(Site lSite, Site rSite)
		{
			this.lSite = lSite;
			this.rSite = rSite;
		}
	}
}
