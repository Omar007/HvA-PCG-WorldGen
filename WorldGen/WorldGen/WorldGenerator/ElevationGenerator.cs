using System;
using WorldGen.Voronoi;

namespace WorldGen.WorldGenerator
{
	public class ElevationGenerator : WorldGenerator
	{
		#region Fields
		private const float SeaLevel = -1;
		private const float GroundLevel = 0;

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
		}

		private void setLandElevation(Cell cell)
		{
			if (cell.CellLandType != CellLandType.Land)
			{
				return;
			}

			float averageHeight = 0;
			int averageParticipants = 0;

			foreach (HalfEdge halfEdge in cell.HalfEdges)
			{
				Cell otherCell = halfEdge.NeighbourCell;

				if (otherCell != null)
				{
					if (otherCell.CellLandType == CellLandType.Ocean)
					{
						cell.CellElevationLevel = (float)Math.Round(Random.NextDouble() * maxDeviation);
						return;
					}
					else
					{
						if (otherCell.CellLandType == CellLandType.Land)
						{
							averageHeight += cell.CellElevationLevel;
						}
						averageParticipants++;
					}
				}
			}

			if (averageParticipants > 0)
			{
				averageHeight /= averageParticipants;

				double heightRandom = Random.NextDouble() * maxDeviation;

				if (averageHeight - maxDeviation >= 0)
				{
					averageHeight -= maxDeviation;
					heightRandom *= 2;
				}

				if (averageHeight + heightRandom > maxHeight)
				{
					cell.CellElevationLevel = maxHeight;
				}
				else
				{
					cell.CellElevationLevel = (float)Math.Round(averageHeight + heightRandom);
				}
			}
			else
			{
				cell.CellElevationLevel = (float)Math.Round(Random.NextDouble() * maxDeviation);
			}
		}

		private void setWaterElevation(Cell cell, ref float lowestLand)
		{
			if (cell.CellLandType != CellLandType.Water)
			{
				return;
			}

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
						lowestLand = otherCell.CellElevationLevel;
						cell.CellElevationLevel = lowestLand;
					}
				}
			}
		}

		protected override void reset(VoronoiCore vc)
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

		protected override void generate(VoronoiCore vc)
		{
			float lowestLand = float.MaxValue;

			for (int i = 0; i <= maxHeight; i++)
			{
				foreach (Cell cell in vc.Cells)
				{
					if (cell.CellLandType == CellLandType.Land)
					{
						setLandElevation(cell);
					}
					else if (cell.CellLandType == CellLandType.Water)
					{
						lowestLand = maxHeight;
						setWaterElevation(cell, ref lowestLand);
						cell.CellElevationLevel = lowestLand;
					}
				}
			}
		}
	}
}
