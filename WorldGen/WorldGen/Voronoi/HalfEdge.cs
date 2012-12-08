using System;

namespace WorldGen.Voronoi
{
	public class HalfEdge : IComparable<HalfEdge>
	{
		#region Fields
		private Edge edge;
		private Cell cell;
		private Cell neighbourCell;

		private double angle;
		#endregion

		#region Properties
		public Cell NeighbourCell
		{
			get { return neighbourCell; }
		}

		public Vertex StartPoint
		{
			get { return edge.LeftCell == cell ? edge.VertexA : edge.VertexB; }
		}

		public Vertex EndPoint
		{
			get { return edge.LeftCell == cell ? edge.VertexB : edge.VertexA; }
		}
		#endregion

		public HalfEdge(Edge edge, Cell leftCell, Cell rightCell)
		{
			this.edge = edge;

			this.cell = leftCell;
			this.neighbourCell = rightCell;

			if (rightCell != null)
			{
				this.angle = Math.Atan2(rightCell.Y - leftCell.Y, rightCell.X - leftCell.X);
			}
			else
			{
				Vertex va = edge.VertexA;
				Vertex vb = edge.VertexB;

				this.angle = edge.LeftCell == leftCell ? Math.Atan2(vb.X - va.X, va.Y - vb.Y) : Math.Atan2(va.X - vb.X, vb.Y - va.Y);
			}
		}

		public int CompareTo(HalfEdge other)
		{
			return (int)Math.Round((other.angle - angle) * 1000000);
		}
	}
}
