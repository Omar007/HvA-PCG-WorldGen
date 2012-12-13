using System.Collections.Generic;
using System.Linq;

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

	public class Cell
	{
		#region Fields
		private Vertex vertex;

		private List<HalfEdge> halfEdges;
		private List<Vertex> cellPoints;

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

		public List<Vertex> CellPoints
		{
			get
			{
				if (cellPoints.Count != halfEdges.Count)
				{
					cellPoints.Clear();
					foreach (HalfEdge edge in halfEdges)
					{
						cellPoints.Add(edge.StartPoint);
					}
				}
				return cellPoints;
			}
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
			cellPoints = new List<Vertex>();

			cellEdgeType = CellEdgeType.NoEdge;
			cellLandType = CellLandType.Undefined;
			cellElevationLevel = float.NaN;
		}

		public int prepare()
		{
			for (int i = halfEdges.Count - 1; i >= 0; i--)
			{
				HalfEdge edge = halfEdges[i];

				if (edge.StartPoint == null || edge.EndPoint == null)
				{
					halfEdges.RemoveAt(i);
				}
			}

			halfEdges.Sort();

			return halfEdges.Count;
		}

		public bool IsVertexInCell(Vertex vertex)
		{
			List<double> coefficients = CellPoints.Skip(1).Select((p, i) =>
				(vertex.Y - CellPoints[i].Y) * (p.X - CellPoints[i].X)
				- (vertex.X - CellPoints[i].X) * (p.Y - CellPoints[i].Y)).ToList();

			if (coefficients.Any(p => p == 0))
			{
				return true;
			}

			for (int i = 1; i < coefficients.Count(); i++)
			{
				if (coefficients[i] * coefficients[i - 1] < 0)
				{
					return false;
				}
			}

			return true;
		}
	}
}
