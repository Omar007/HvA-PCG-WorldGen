using System.Collections.Generic;
using WorldGen.Voronoi;

namespace WorldGen
{
	public class GroupedCell
	{
		#region Static
		#region NEW - Stores all Voronoi Cells on LinkedLists and uses that list to keep track of unsorted cells. HUGE speed improvement!
		public static GroupedCell groupCells(List<VoronoiCore> voronoiDiagrams)
		{
			List<LinkedList<Cell>> ungroupedCells = new List<LinkedList<Cell>>();

			foreach (VoronoiCore vc in voronoiDiagrams)
			{
				ungroupedCells.Add(new LinkedList<Cell>(vc.Cells));
			}

			GroupedCell gc = new GroupedCell(null);

			groupCells(gc, ungroupedCells, 0);

			return gc;
		}

		private static void groupCells(GroupedCell gc, List<LinkedList<Cell>> ungroupedCells, int index)
		{
			LinkedListNode<Cell> currentNode = ungroupedCells[index].First;

			while (currentNode != null)
			{
				Cell cell = currentNode.Value;
				currentNode = currentNode.Next;

				if (gc.Current == null || gc.Current.IsVertexInCell(cell.Vertex))
				{
					ungroupedCells[index].Remove(cell);

					GroupedCell child = new GroupedCell(cell);
					gc.AddChild(child);

					if (index + 1 < ungroupedCells.Count)
					{
						groupCells(child, ungroupedCells, index + 1);
					}
				}
			}
		}
		#endregion

		#region OLD - Loops through all cells all the time, even already grouped ones.
		public static GroupedCell groupCells_OLD(List<VoronoiCore> voronoiDiagrams)
		{
			GroupedCell gc = new GroupedCell(null);

			groupCells_OLD(gc, voronoiDiagrams, 0);

			return gc;
		}

		private static void groupCells_OLD(GroupedCell gc, List<VoronoiCore> voronoiDiagrams, int index)
		{
			foreach (Cell cell in voronoiDiagrams[index].Cells)
			{
				if (gc.Current == null || gc.Current.IsVertexInCell(cell.Vertex))
				{
					GroupedCell child = new GroupedCell(cell);
					gc.AddChild(child);

					if (index + 1 < voronoiDiagrams.Count)
					{
						groupCells_OLD(child, voronoiDiagrams, index + 1);
					}
				}
			}
		}
		#endregion
		#endregion

		#region Fields
		private Cell current;
		private List<GroupedCell> children;
		#endregion

		#region Properties
		public Cell Current
		{
			get { return current; }
		}

		public int Count
		{
			get { return children != null ? children.Count : 0; }
		}
		#endregion

		private GroupedCell(Cell current)
		{
			this.current = current;
		}

		public void AddChild(GroupedCell child)
		{
			if (children == null)
			{
				children = new List<GroupedCell>();
			}

			children.Add(child);
		}

		public GroupedCell this[int index]
		{
			get { return children != null ? children[index] : null; }
		}
	}
}
