using System.Collections.Generic;
using System.Linq;
using WorldGen.Voronoi;

namespace WorldGen
{
	public class GroupedCell
	{
		#region Static
		public static GroupedCell groupCells(List<VoronoiCore> voronoiDiagrams)
		{
			List<List<Cell>> ungroupedCells = new List<List<Cell>>();

			foreach (VoronoiCore vc in voronoiDiagrams)
			{
				ungroupedCells.Add(new List<Cell>(vc.Cells));
			}

			GroupedCell rootGC = new GroupedCell(null);

			groupCells(rootGC, ungroupedCells, 0);

			return rootGC;
		}

		private static void groupCells(GroupedCell gc, List<List<Cell>> ungroupedCells, int index)
		{
			for (int i = ungroupedCells[index].Count - 1; i >= 0; i--)
			{
				Cell cell = ungroupedCells[index][i];

				if (gc.Cell == null || gc.Cell.ContainsVertex(cell.Vertex))
				{
					ungroupedCells[index].RemoveAt(i);

					GroupedCell child = new GroupedCell(cell);
					cell.GroupedCellInfo = child;
					gc.AddChild(child);

					if (index + 1 < ungroupedCells.Count)
					{
						groupCells(child, ungroupedCells, index + 1);
					}
				}
			}
		}
		#endregion

		#region Fields
		private Cell cell;
		private GroupedCell parent;
		private List<GroupedCell> children;
		#endregion

		#region Properties
		public Cell Cell
		{
			get { return cell; }
		}

		public GroupedCell Parent
		{
			get { return parent; }
		}

		public int Count
		{
			get { return children != null ? children.Count : 0; }
		}
		#endregion

		private GroupedCell(Cell cell)
		{
			this.cell = cell;
			this.parent = null;
		}

		public void AddChild(GroupedCell child)
		{
			if (children == null)
			{
				children = new List<GroupedCell>();
			}

			child.parent = this;

			children.Add(child);
		}

		public GroupedCell this[int index]
		{
			get { return children != null ? children[index] : null; }
		}
	}
}
