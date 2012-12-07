using System;
using System.Collections.Generic;
using WorldGen.Voronoi;

namespace WorldGen.WorldGenerator
{
	public class LandGenerator
	{
		#region Fields
		private Random random;

		private List<Cell> worldCells;
		#endregion

		#region Properties
		public List<Cell> WorldCells
		{
			get { return worldCells; }
		}
		#endregion

		public LandGenerator()
		{
			random = new Random();
		}

		private void setCellLandType(Cell cell)
		{
			if (cell.CellEdgeType == CellEdgeType.NoEdge && random.NextDouble() > 0.55)
			{
				cell.CellLandType = CellLandType.Land;
			}
			else
			{
				cell.CellLandType = CellLandType.Water;
			}
		}

		public void generate(VoronoiCore vc, bool reGenerate)
		{
			if (reGenerate) //Reset all cells
			{
				foreach (Cell cell in vc.Cells)
				{
					cell.CellLandType = CellLandType.Undefined;
				}
			}

			foreach (Cell cell in vc.Cells)
			{
				if (cell.CellLandType == CellLandType.Undefined)
				{
					setCellLandType(cell);
				}
				
				if (cell.CellEdgeType == CellEdgeType.NoEdge && cell.CellLandType == CellLandType.Water)
				{
					int neighbourCount = 0;
					int landNeighbourCount = 0;

					foreach (HalfEdge halfEdge in cell.HalfEdges)
					{
						Cell otherCell = halfEdge.Edge.LeftCell == cell ? halfEdge.Edge.RightCell : halfEdge.Edge.LeftCell;

						if (otherCell.CellLandType == CellLandType.Undefined)
						{
							setCellLandType(otherCell);
						}

						if (otherCell.CellLandType == CellLandType.Land)
						{
							landNeighbourCount++;
						}

						neighbourCount++;
					}

					if (landNeighbourCount > neighbourCount * 0.5)
					{
						cell.CellLandType = CellLandType.Land;
					}
				}
			}

			worldCells = vc.Cells;
		}
	}
}
