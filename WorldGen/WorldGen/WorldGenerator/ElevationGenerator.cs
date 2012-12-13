using System;
using WorldGen.Voronoi;

namespace WorldGen.WorldGenerator
{
	public class ElevationGenerator
	{
		#region Fields
		private const float SeaLevel = -1;
		private const float GroundLevel = 0;

		private Random random;

		private float maxHeight;

		private float maxDeviation;
		#endregion

		#region Properties
		public float MaxHeight
		{
			get { return maxHeight; }
		}
		#endregion

		public ElevationGenerator(float maxHeight, float maxDeviation)
			: base()
		{
			this.maxHeight = maxHeight;
			this.maxDeviation = maxDeviation;

			random = new Random();
		}

		private void setElevation(Cell cell)
		{
			float lowestNeighbour = maxHeight;

			foreach (HalfEdge halfEdge in cell.HalfEdges)
			{
				Cell otherCell = halfEdge.NeighbourCell;

				if (otherCell != null)
				{
					if (otherCell.CellLandType == CellLandType.Ocean)
					{
						cell.CellElevationLevel = (float)Math.Round(random.NextDouble() * maxDeviation);
						return;
					}
					else if (otherCell.CellElevationLevel < lowestNeighbour)
					{
						lowestNeighbour = otherCell.CellElevationLevel;
					}
				}
			}

			float deviation = Math.Abs(cell.CellElevationLevel - lowestNeighbour);

			if(deviation < maxDeviation)
			{
				cell.CellElevationLevel += (float)Math.Round(random.NextDouble() * (maxDeviation - deviation));
			}
		}

		private void correctWater(Cell cell, ref float lowestLand)
		{
			foreach (HalfEdge halfEdge in cell.HalfEdges)
			{
				Cell otherCell = halfEdge.NeighbourCell;

				if (otherCell != null)
				{
					if (otherCell.CellLandType == CellLandType.Land && otherCell.CellElevationLevel < lowestLand)
					{
						lowestLand = otherCell.CellElevationLevel;
						cell.CellElevationLevel = lowestLand;
					}
					else if (otherCell.CellLandType == CellLandType.Water)
					{
						if (otherCell.CellElevationLevel > lowestLand)
						{
							otherCell.CellElevationLevel = lowestLand;
							correctWater(otherCell, ref lowestLand);
						}
						else
						{
							lowestLand = otherCell.CellElevationLevel;
							cell.CellElevationLevel = lowestLand;
						}
					}
				}
			}
		}

		private void reset(VoronoiCore vc)
		{
			foreach (Cell cell in vc.Cells)
			{
				switch (cell.CellLandType)
				{
					case CellLandType.Land:
						cell.CellElevationLevel = GroundLevel;
						break;

					case CellLandType.Ocean: //Ocean always at sea level
						cell.CellElevationLevel = SeaLevel;
						break;

					case CellLandType.Water:
						cell.CellElevationLevel = GroundLevel;
						break;
				}
			}
		}

		public void generate(VoronoiCore vc)
		{
			reset(vc);

			for (int i = 0; i <= maxHeight; i++)
			{
				foreach (Cell cell in vc.Cells)
				{
					if (cell.CellLandType != CellLandType.Ocean)
					{
						setElevation(cell);
					}
				}
			}

			float lowestLand = float.MaxValue;

			foreach (Cell cell in vc.Cells)
			{
				if (cell.CellLandType == CellLandType.Water)
				{
					lowestLand = maxHeight;
					correctWater(cell, ref lowestLand);
					cell.CellElevationLevel = lowestLand;
				}
			}
		}
	}
}
