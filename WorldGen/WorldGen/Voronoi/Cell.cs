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

		private CellEdgeType edgeType;
		private CellLandType landType;
		private float elevationLevel;
		private short moistureLevel;
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

		public CellEdgeType EdgeType
		{
			get { return edgeType; }
			set { edgeType = value; }
		}

		public CellLandType LandType
		{
			get { return landType; }
			set { landType = value; }
		}

		public float ElevationLevel
		{
			get { return elevationLevel; }
			set { elevationLevel = value; }
		}

		public short MoistureLevel
		{
			get { return moistureLevel; }
			set { moistureLevel = value; }
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

			edgeType = CellEdgeType.NoEdge;
			landType = CellLandType.Undefined;
			elevationLevel = float.NaN;
			moistureLevel = short.MinValue;
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
			double sign = double.NaN;

			foreach (HalfEdge hEdge in HalfEdges)
			{
				Vertex affineSegment = hEdge.EndPoint - hEdge.StartPoint;
				Vertex affinePoint = vertex - hEdge.StartPoint;

				double crossProd = affineSegment.X * affinePoint.Y - affineSegment.Y * affinePoint.X;
				if (crossProd != 0)
				{
					crossProd /= Math.Abs(crossProd); //Make it -1 or 1.
				}

				if (double.IsNaN(sign))
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
