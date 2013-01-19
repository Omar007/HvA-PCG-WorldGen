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
			foreach (HalfEdge hEdge in cell.HalfEdges)
			{
				Cell otherCell = hEdge.NeighbourCell;
				
				if (otherCell != null && otherCell.LandType == CellLandType.Land)
				{
					Vertex direction = otherCell.Vertex - cell.Vertex;
					double dotProd = Vector2.Dot(direction.ToVector2Normalized(), windDirection.ToVector2Normalized());

					float angle = MathHelper.ToDegrees((float)Math.Acos(dotProd));

					//if (dotProd > 0)
					if (angle < 67.5)
					{
						if (otherCell.MoistureLevel < 0)
						{
							double moistureLevelAdapted = cell.MoistureLevel;
							//If the angle differs more from the windDirection, less moisture is reaching the otherCell.
							//A maximum of 12.5% will be subtracted.
							moistureLevelAdapted *= 1 - (angle / 720);
							//If the cells are further away, less moisture reaches the otherCell.
							moistureLevelAdapted -= direction.ToVector2().LengthSquared();
							//If we are going down, some moisture is added back else some more is subtracted
							moistureLevelAdapted += (cell.ElevationLevel - otherCell.ElevationLevel) * 10;

							otherCell.MoistureLevel = (short)Math.Max(0, (int)Math.Round(moistureLevelAdapted));

							calcMoisture(otherCell);
						}
						else if (otherCell.MoistureLevel > 0 && otherCell.HalfEdges.Where(he => he.NeighbourCell.MoistureLevel < 0).Count() > 0)
						{
							calcMoisture(otherCell);
						}
					}
				}
			}
		}

		private void reset(Cell cell)
		{
			switch (cell.LandType)
			{
				case CellLandType.Land:
					cell.MoistureLevel = short.MinValue;
					break;

				case CellLandType.Water:
					cell.MoistureLevel = short.MaxValue;
					break;

				case CellLandType.Ocean:
					cell.MoistureLevel = short.MaxValue;
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

		public void generate(VoronoiCore vc, Vertex newWindDir)
		{
			windDirection = newWindDir;

			generate(vc);
		}
	}
}
