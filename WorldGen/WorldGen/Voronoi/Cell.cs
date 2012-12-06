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
		private int voronoiID;

		private Vertex vertex;

		private List<HalfEdge> halfEdges;

		private CellType cellType;
		#endregion

		#region Properties
		public int VoronoiID
		{
			get { return voronoiID; }
			set { voronoiID = value; }
		}

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

		public Cell(double x, double y)
			: this(new Vertex(x, y))
		{
		}

		public Cell(Vertex v)
		{
			this.vertex = v;

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
