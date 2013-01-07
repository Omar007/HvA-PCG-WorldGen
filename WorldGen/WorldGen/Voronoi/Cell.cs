using System;
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

		public bool ContainsVertex(Vertex vertex)
		{
			double sign = 0;

			foreach (HalfEdge hEdge in HalfEdges)
			{
				Vertex affineSegment = hEdge.EndPoint - hEdge.StartPoint;
				Vertex affinePoint = vertex - hEdge.StartPoint;

				double crossProd = affineSegment.X * affinePoint.Y - affineSegment.Y * affinePoint.X;
				if (crossProd != 0)
				{
					crossProd /= Math.Abs(crossProd);
				}

				if (sign == 0)
				{
					sign = crossProd;
				}
				else if (sign != crossProd)
				{
					return false;
				}
			}

			return true;
		}
	}
}
