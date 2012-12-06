
namespace WorldGen.Voronoi
{
	public class DelaunyEdge
	{
		#region Fields
		private Cell leftCell;
		private Cell rightCell;
		#endregion

		#region Properties
		public Cell LeftCell
		{
			get { return leftCell; }
		}

		public Cell RightCell
		{
			get { return rightCell; }
		}

		public Vertex VertexA
		{
			get { return leftCell.Vertex; }
		}

		public Vertex VertexB
		{
			get { return rightCell.Vertex; }
		}
		#endregion

		public DelaunyEdge(Cell leftCell, Cell rightCell)
		{
			this.leftCell = leftCell;
			this.rightCell = rightCell;
		}
	}
}
