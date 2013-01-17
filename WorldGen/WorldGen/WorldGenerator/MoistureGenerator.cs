using Microsoft.Xna.Framework;
using System;
using System.Linq;
using WorldGen.Voronoi;

namespace WorldGen.WorldGenerator
{
	public class MoistureGenerator
	{
		private Vertex windDirection;

		#region Properties
		public Vertex WindDirection
		{
			get { return windDirection; }
		}
		#endregion

		public MoistureGenerator(Vertex windDirection)
		{
			this.windDirection = windDirection;
		}

		public MoistureGenerator(double windDirX, double windDirY)
			: this(new Vertex(windDirX, windDirY))
		{
		}

		private void calcMoisture(Cell cell)
		{
			float highestMoistureNeighbour = cell.MoistureLevel;

			foreach (HalfEdge hEdge in cell.HalfEdges)
			{
				Cell otherCell = hEdge.NeighbourCell;
				
				if (otherCell != null)
				{
					Vertex direction = otherCell.Vertex - cell.Vertex;
					double dotProd = Vector2.Dot(direction.ToVector2(), windDirection.ToVector2());

					if (dotProd > 0)
					{
						if (otherCell.LandType != CellLandType.Land)
						{
							highestMoistureNeighbour = float.MaxValue;

							break;
						}
						else if (otherCell.MoistureLevel > highestMoistureNeighbour)
						{
							float moistureLevelAdapted = otherCell.MoistureLevel;
							//If the angle differs more from the windDirection, less moisture is reaching the otherCell.
							moistureLevelAdapted -= (float)Math.Round(dotProd * 1000000);
							//If the cells are further away, less moisture reaches the otherCell.
							moistureLevelAdapted -= (float)Math.Round(direction.ToVector2().LengthSquared() * 1000000);
							
							if (otherCell.ElevationLevel < cell.ElevationLevel)
							{
								//If we are going down, some moisture is added back.
								moistureLevelAdapted += (float)Math.Round(cell.ElevationLevel - otherCell.ElevationLevel);
							}

							if (moistureLevelAdapted > highestMoistureNeighbour)
							{
								highestMoistureNeighbour = moistureLevelAdapted;
							}
						}
					}
				}
			}

			cell.MoistureLevel = highestMoistureNeighbour;
		}

		private void reset(Cell cell)
		{
			switch (cell.LandType)
			{
				case CellLandType.Land:
					cell.MoistureLevel = 0;
					break;

				case CellLandType.Water:
					cell.MoistureLevel = float.MaxValue;
					break;

				case CellLandType.Ocean:
					cell.MoistureLevel = float.MaxValue;
					break;
			}
		}

		public void generate(VoronoiCore vc)
		{
			foreach (Cell cell in vc.Cells)
			{
				reset(cell);
			}

			foreach (Cell cell in vc.Cells)
			{
				if (cell.LandType != CellLandType.Land)
				{
					calcMoisture(cell);
				}
			}
		}
	}
}
