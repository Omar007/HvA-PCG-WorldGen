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
		Water,
		Ocean
	}

	public class CellElevationLevel
	{
		public static readonly float SeaLevel = -1;
		public static readonly float GroundLevel = 0;
		public static readonly float Low = 1;
		public static readonly float Medium = 2;
		public static readonly float High = 3;
		public static readonly float Maximum = 4;
	}

	public class Cell
	{
		#region Fields
		private Vertex vertex;

		private List<HalfEdge> halfEdges;

		private CellEdgeType cellEdgeType;
		private CellLandType cellLandType;
		private float cellElevationLevel;
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

		public float CellElevationLevel
		{
			get { return cellElevationLevel; }
			set { cellElevationLevel = value; }
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
			cellElevationLevel = float.NaN;
		}

		public int prepare()
		{
			int iHalfEdge = halfEdges.Count;

			while (iHalfEdge-- > 0)
			{
				HalfEdge edge = halfEdges[iHalfEdge];

				if (edge.StartPoint == null || edge.EndPoint == null)
				{
					halfEdges.RemoveAt(iHalfEdge);
				}
			}

			halfEdges.Sort();

			return halfEdges.Count;
		}
	}
}
