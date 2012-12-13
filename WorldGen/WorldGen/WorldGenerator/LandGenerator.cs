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

						//if (otherCell.CellLandType != CellLandType.Land)
						//{
						//	setLandCells(otherCell);
						//}
					}
				}
			}

			if (landNeighbourCount > becomesLandMark) //Surrounded by land for more than the marked value; make myself land.
			{
				cell.CellLandType = CellLandType.Land;
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
			for (int i = 0; i < gc.Count; i++)
			{
				GroupedCell child = gc[i];

				resetAsLandMinimum = RESET_AS_LAND_MIN;
				changeToLandThreshold = CHANGE_TO_LAND_THRESHOLD;

				if (gc.Current != null)
				{
					switch (gc.Current.CellLandType)
					{
						case CellLandType.Land:
							resetAsLandMinimum = 0.2;
							changeToLandThreshold = 0.1;
							break;

						case CellLandType.Water:
							resetAsLandMinimum = 0.7;
							changeToLandThreshold = 0.6;
							break;

						case CellLandType.Ocean:
							resetAsLandMinimum = 0.85;
							changeToLandThreshold = 0.75;
							break;
					}
				}

				if (child.Current.CellEdgeType == CellEdgeType.NoEdge && random.NextDouble() > resetAsLandMinimum) //Not a cell on the edge of the map and land 'threshold' reached.
				{
					child.Current.CellLandType = CellLandType.Land;
				}
				else if (child.Current.CellEdgeType != CellEdgeType.NoEdge) //Cell on the edge of the map
				{
					child.Current.CellLandType = CellLandType.Ocean;
				}
				else //Everything else is water
				{
					child.Current.CellLandType = CellLandType.Water;
				}
			}
		}

		public void generate(GroupedCell gc, VoronoiCore vc)
		{
			if (gc != null)
			{
				reset(gc);
				generate(gc);
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
				GroupedCell child = gc[i];

				if (gc.Current != null)
				{
					reset(gc);
				}

				if (child.Current.CellLandType == CellLandType.Water)
				{
					setLandCells(child.Current);
				}

				generate(child, null);
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
