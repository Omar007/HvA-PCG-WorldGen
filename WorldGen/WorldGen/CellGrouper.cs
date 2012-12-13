using System.Collections.Generic;
using System.Linq;
using WorldGen.Voronoi;

namespace WorldGen
{
	public class CellGrouper
	{
		#region Static
		public static CellGrouper GroupCells(List<VoronoiCore> voronoiDiagrams)
		{
			CellGrouper cg = new CellGrouper(null);

			groupDiagram(cg, voronoiDiagrams, 0);

			return cg;
		}

		private static void groupDiagram(CellGrouper cg, List<VoronoiCore> voronoiDiagrams, int index)
		{
			if (index >= voronoiDiagrams.Count)
			{
				return;
			}

			foreach (Cell cell in voronoiDiagrams[index].Cells)
			{
				if (cg.current == null || IsInPolygon(cg.current, cell.Vertex))
				{
					CellGrouper child = new CellGrouper(cell);
					cg.AddChild(child);

					groupDiagram(child, voronoiDiagrams, index + 1);
				}
			}
		}

		private static bool IsInPolygon(Cell cell, Vertex vertex)
		{
			List<double> coef = cell.CellPoints.Skip(1).Select((p, i) =>
				(vertex.Y - cell.CellPoints[i].Y) * (p.X - cell.CellPoints[i].X)
				- (vertex.X - cell.CellPoints[i].X) * (p.Y - cell.CellPoints[i].Y)).ToList();

			if (coef.Any(p => p == 0))
			{
				return true;
			}

			for (int i = 1; i < coef.Count(); i++)
			{
				if (coef[i] * coef[i - 1] < 0)
				{
					return false;
				}
			}
			return true;
		}
		#endregion

		#region Fields
		private Cell current;
		private List<CellGrouper> children;
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

		private CellGrouper(Cell current)
		{
			this.current = current;
		}

		public void AddChild(CellGrouper child)
		{
			if (children == null)
			{
				children = new List<CellGrouper>();
			}

			children.Add(child);
		}

		public CellGrouper this[int index]
		{
			get { return children != null ? children[index] : null; }
		}
	}
}
