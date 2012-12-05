using Microsoft.Xna.Framework;

namespace WorldGen.Voronoi
{
	public class Edge
	{
		#region Fields
		private Site lSite;
		private Site rSite;

		private Vertex va;
		private Vertex vb;
		#endregion

		#region Properties
		public Site LeftSite
		{
			get { return lSite; }
			set { lSite = value; }
		}

		public Site RightSite
		{
			get { return rSite; }
			set { rSite = value; }
		}

		public Vertex VertexA
		{
			get { return va; }
			set { va = value; }
		}

		public Vertex VertexB
		{
			get { return vb; }
			set { vb = value; }
		}
		#endregion

		public Edge(Site lSite, Site rSite)
		{
			this.lSite = lSite;
			this.rSite = rSite;

			va = null;
			vb = null;
		}

		public bool HasDelaunyEdge
		{
			get { return lSite != null && rSite != null;}
		}

		public DelaunyEdge DelaunyEdge
		{
			get { return new DelaunyEdge(lSite, rSite); }
		}
	}
}
