using System.Collections.Generic;

namespace WorldGen.Voronoi
{
	public enum CellType
	{
		NoEdge,
		WestEdge,
		EastEdge,
		NorthEdge,
		SouthEdge
	}

	public class Cell
	{
		#region Fields
		private Site site;
		private List<HalfEdge> halfEdges;

		private CellType cellType;
		#endregion

		#region Properties
		public Site Site
		{
			get { return site; }
		}

		public List<HalfEdge> HalfEdges
		{
			get { return halfEdges; }
		}

		public CellType CellType
		{
			get { return cellType; }
			set { cellType = value; }
		}
		#endregion

		public Cell(Site site)
		{
			this.site = site;

			halfEdges = new List<HalfEdge>();

			cellType = CellType.NoEdge;
		}

		public int prepare()
		{
			int iHalfEdge = halfEdges.Count;

			while (iHalfEdge-- > 0)
			{
				Edge edge = halfEdges[iHalfEdge].Edge;

				if (edge.VertexB == null || edge.VertexA == null)
				{
					halfEdges.RemoveAt(iHalfEdge);
				}
			}

			halfEdges.Sort();

			return halfEdges.Count;
		}
	}
}
