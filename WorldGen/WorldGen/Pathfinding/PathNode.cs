using WorldGen.Voronoi;

namespace WorldGen.Pathfinding
{
	public class PathNode
	{
		#region Fields
		private Cell cell;
		private double gCost;
		private double fCost;
		private PathNode next;
		#endregion

		#region Properties
		public Cell Cell
		{
			get { return cell; }
		}

		public double GCost
		{
			get { return gCost; }
			set { gCost = value; }
		}

		public double FCost
		{
			get { return fCost; }
			set { fCost = value; }
		}

		public PathNode Next
		{
			get { return next; }
			set { next = value; }
		}
		#endregion

		public PathNode(Cell cell, double gCost, double hCost, PathNode next)
		{
			this.cell = cell;
			this.gCost = (next != null ? next.gCost + gCost : gCost);
			this.fCost = this.gCost + hCost;
			this.next = next;
		}
	}
}
