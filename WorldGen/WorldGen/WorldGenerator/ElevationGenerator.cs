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
					if (otherCell.LandType == CellLandType.Ocean)
					{
						//Set height, clamped to maxDeviation
						cell.ElevationLevel = (float)Math.Round(random.NextDouble() * maxDeviation);

						return;
					}

					if (cell.LandType != CellLandType.Water && otherCell.LandType == CellLandType.Water)
					{
						correctWater(otherCell, ref lowestNeighbour);
					}
					else if (otherCell.ElevationLevel < lowestNeighbour)
					{
						lowestNeighbour = otherCell.ElevationLevel;
					}
				}
			}

			float deviation = Math.Abs(cell.ElevationLevel - lowestNeighbour);

			//We can only increase height if we won't cross the maxDeviation
			if (deviation <= maxDeviation)
			{
				cell.ElevationLevel += (float)Math.Round(random.NextDouble() * (maxDeviation - deviation));

				//Clamp to the maxHeight if we exceeded it
				if (cell.ElevationLevel > maxHeight)
				{
					cell.ElevationLevel = maxHeight;
				}
			}
		}

		private void correctWater(Cell cell, ref float lowestLand)
		{
			foreach (HalfEdge halfEdge in cell.HalfEdges)
			{
				Cell otherCell = halfEdge.NeighbourCell;

				if (otherCell != null)
				{
					if (otherCell.ElevationLevel < lowestLand && otherCell.LandType == CellLandType.Land)
					{
						lowestLand = otherCell.ElevationLevel;
						cell.ElevationLevel = lowestLand;
					}
					else if(otherCell.ElevationLevel != lowestLand && otherCell.LandType == CellLandType.Water)
					{
						otherCell.ElevationLevel = lowestLand;
						correctWater(otherCell, ref lowestLand);
					}
				}

				cell.ElevationLevel = lowestLand;
			}
		}

		private void reset(VoronoiCore vc)
		{
			foreach (Cell cell in vc.Cells)
			{
				switch (cell.LandType)
				{
					case CellLandType.Land:
						cell.ElevationLevel = GroundLevel;
						break;

					case CellLandType.Ocean: //Ocean always at sea level
						cell.ElevationLevel = SeaLevel;
						break;

					case CellLandType.Water:
						cell.ElevationLevel = GroundLevel;
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
					if (cell.LandType != CellLandType.Ocean)
					{
						setElevation(cell);
					}
				}
			}

			foreach (Cell cell in vc.Cells)
			{
				if (cell.LandType == CellLandType.Water)
				{
					float lowestLand = maxHeight;
					correctWater(cell, ref lowestLand);
				}
			}
		}
	}
}
