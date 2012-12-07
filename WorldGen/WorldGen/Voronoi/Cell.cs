using System.Collections.Generic;

namespace WorldGen.Voronoi
{
	public enum CellEdgeType
	{
		NoEdge,
		WestEdge,
		EastEdge,
		NorthEdge,
		SouthEdge
	}

	public enum CellLandType
	{
		Undefined,
		Land,
		Water
	}

	public class Cell
	{
		#region Fields
		private int voronoiID;

		private Vertex vertex;

		private List<HalfEdge> halfEdges;

		private CellEdgeType cellEdgeType;

		private CellLandType cellLandType;
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

		public CellEdgeType CellEdgeType
		{
			get { return cellEdgeType; }
			set { cellEdgeType = value; }
		}

		public CellLandType CellLandType
		{
			get { return cellLandType; }
			set { cellLandType = value; }
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

			cellEdgeType = CellEdgeType.NoEdge;

			cellLandType = CellLandType.Undefined;
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
