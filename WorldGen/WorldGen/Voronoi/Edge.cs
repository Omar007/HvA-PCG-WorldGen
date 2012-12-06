using Microsoft.Xna.Framework;

namespace WorldGen.Voronoi
{
	public class Edge
	{
		#region Fields
		private Cell leftCell;
		private Cell rightCell;

		private Vertex va;
		private Vertex vb;
		#endregion

		#region Properties
		public Cell LeftCell
		{
			get { return leftCell; }
			set { leftCell = value; }
		}

		public Cell RightCell
		{
			get { return rightCell; }
			set { rightCell = value; }
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

		public Edge(Cell leftCell, Cell rightCell)
		{
			this.leftCell = leftCell;
			this.rightCell = rightCell;

			va = null;
			vb = null;
		}

		public bool HasDelaunyEdge
		{
			get { return leftCell != null && rightCell != null;}
		}

		public DelaunyEdge DelaunyEdge
		{
			get { return new DelaunyEdge(leftCell, rightCell); }
		}
	}
}
