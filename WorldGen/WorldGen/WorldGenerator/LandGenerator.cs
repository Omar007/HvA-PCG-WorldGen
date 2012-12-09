using System;
using WorldGen.Voronoi;

namespace WorldGen.WorldGenerator
{
	public class LandGenerator : WorldGenerator
	{
		public LandGenerator()
			: base()
		{
		}

		private void setLandCells(Cell cell)
		{
			double halfLandMark = cell.HalfEdges.Count * 0.5;
			int landNeighbourCount = 0;

			foreach (HalfEdge halfEdge in cell.HalfEdges)
			{
				Cell otherCell = halfEdge.NeighbourCell;

				if (otherCell != null && otherCell.CellLandType == CellLandType.Land)
				{
					landNeighbourCount++;

					if (landNeighbourCount > halfLandMark) //Surrounded by land for more than 50%; make myself land.
					{
						cell.CellLandType = CellLandType.Land;

						if (otherCell.CellLandType != CellLandType.Land)
						{
							setLandCells(otherCell);
						}
					}
				}
			}
		}

		private void setOceanCells(Cell cell)
		{
			foreach (HalfEdge halfEdge in cell.HalfEdges)
			{
				Cell otherCell = halfEdge.NeighbourCell;

				if (otherCell != null && otherCell.CellLandType == CellLandType.Water)
				{
					otherCell.CellLandType = CellLandType.Ocean;
					setOceanCells(otherCell);
				}
			}
		}

		protected override void reset(VoronoiCore vc)
		{
			foreach (Cell cell in vc.Cells)
			{
				if (cell.CellEdgeType == CellEdgeType.NoEdge && Random.NextDouble() > 0.6) //Not a cell on the edge of the map and land 'threshold' reached.
				{
					cell.CellLandType = CellLandType.Land;
				}
				else if (cell.CellEdgeType != CellEdgeType.NoEdge) //Cell on the edge of the map
				{
					cell.CellLandType = CellLandType.Ocean;
				}
				else //Everything else is water
				{
					cell.CellLandType = CellLandType.Water;
				}
			}
		}

		protected override void generate(VoronoiCore vc)
		{
			foreach (Cell cell in vc.Cells)
			{
				if (cell.CellEdgeType == CellEdgeType.NoEdge && cell.CellLandType != CellLandType.Land)
				{
					setLandCells(cell);
				}
			}

			foreach (Cell cell in vc.Cells)
			{
				if (cell.CellLandType == CellLandType.Ocean)
				{
					setOceanCells(cell);
				}
			}
		}
	}
}
