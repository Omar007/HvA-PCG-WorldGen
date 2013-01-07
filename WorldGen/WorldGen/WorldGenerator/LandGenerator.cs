using System;
using WorldGen.Voronoi;

namespace WorldGen.WorldGenerator
{
	public class LandGenerator
	{
		private const double RESET_AS_LAND_MIN = 0.6;
		private const double CHANGE_TO_LAND_THRESHOLD = 0.5;

		private Random random;

		private double resetAsLandMinimum = RESET_AS_LAND_MIN;
		private double changeToLandThreshold = CHANGE_TO_LAND_THRESHOLD;

		public LandGenerator()
			: base()
		{
			random = new Random();
		}

		private void setLandCells(Cell cell)
		{
			double becomesLandMark = cell.HalfEdges.Count * changeToLandThreshold;
			int landNeighbourCount = 0;

			foreach (HalfEdge halfEdge in cell.HalfEdges)
			{
				Cell otherCell = halfEdge.NeighbourCell;

				if (otherCell != null && otherCell.CellLandType == CellLandType.Land)
				{
					landNeighbourCount++;

					if (landNeighbourCount > becomesLandMark) //Surrounded by land for more than the marked value; make myself land.
					{
						cell.CellLandType = CellLandType.Land;
						break;
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

		private void reset(GroupedCell gc)
		{
			resetAsLandMinimum = RESET_AS_LAND_MIN;
			changeToLandThreshold = CHANGE_TO_LAND_THRESHOLD;

			if (gc.Parent != null && gc.Parent.Cell != null)
			{
				switch (gc.Parent.Cell.CellLandType)
				{
					case CellLandType.Land:
						resetAsLandMinimum = 0.5; //0.2;
						changeToLandThreshold = 0.4; //0.1;
						break;

					case CellLandType.Water:
						resetAsLandMinimum = 0.8; //0.7;
						changeToLandThreshold = 0.7; //0.6;
						break;

					case CellLandType.Ocean:
						resetAsLandMinimum = 0.9; //0.85;
						changeToLandThreshold = 0.8; //0.75;
						break;
				}

				gc.Cell.CellLandType = gc.Parent.Cell.CellLandType;

				if (gc.Cell.CellEdgeType != CellEdgeType.NoEdge)
				{
					gc.Cell.CellLandType = CellLandType.Ocean;
				}
				else if (gc.Cell.CellLandType == CellLandType.Ocean)
				{
					gc.Cell.CellLandType = CellLandType.Water;
				}

				return;
			}

			if (gc.Cell.CellEdgeType != CellEdgeType.NoEdge) //Cell on the edge of the map is always ocean
			{
				gc.Cell.CellLandType = CellLandType.Ocean;
			}
			else if (random.NextDouble() > resetAsLandMinimum) //Not a cell on the edge of the map and land 'threshold' reached.
			{
				gc.Cell.CellLandType = CellLandType.Land;
			}
			else //Everything else is water
			{
				gc.Cell.CellLandType = CellLandType.Water;
			}
		}

		public void generate(GroupedCell rootGC, VoronoiCore vc)
		{
			if (rootGC != null)
			{
				generate(rootGC);
			}

			if (vc != null)
			{
				correctOceans(vc);
			}
		}

		private void generate(GroupedCell gc)
		{
			for (int i = 0; i < gc.Count; i++)
			{
				reset(gc[i]);
			}

			for (int i = 0; i < gc.Count; i++)
			{
				GroupedCell child = gc[i];

				//reset(child);

				if (child.Cell.CellLandType == CellLandType.Water)
				{
					setLandCells(child.Cell);
				}

				generate(child);
			}
		}

		private void correctOceans(VoronoiCore vc)
		{
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
