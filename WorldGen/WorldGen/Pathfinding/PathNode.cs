using WorldGen.Voronoi;

namespace WorldGen.Pathfinding
{
	public class PathNode
	{
		#region Fields
		private Cell cell;
		private double gCost;
		private double hCost;
		private double fCost;
		private PathNode next;

		//Used by the heap.
		private PathNode nextListNode;
		#endregion

		#region Properties
		public Cell Cell
		{
			get { return cell; }
		}

		public double GCost
		{
			get { return gCost; }
		}

		public double HCost
		{
			get { return hCost; }
		}

		public double FCost
		{
			get { return fCost; }
		}

		public PathNode Next
		{
			get { return next; }
		}

		internal PathNode NextListNode
		{
			get { return nextListNode; }
			set { nextListNode = value; }
		}
		#endregion

		public PathNode(Cell cell, double gCost, double hCost, PathNode next)
		{
			this.cell = cell;
			this.gCost = (next != null ? next.gCost + gCost : gCost);
			this.hCost = hCost;
			this.fCost = this.gCost + this.hCost;
			this.next = next;
		}
	}
}
