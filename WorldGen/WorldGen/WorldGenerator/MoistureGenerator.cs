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
					if (cell.MoistureLevel > 0)
					{
						Vertex direction = otherCell.Vertex - cell.Vertex;
						float angle = MathHelper.ToDegrees((float)Math.Acos(Vector2.Dot(direction.ToVector2Normalized(), windDirection.ToVector2Normalized())));

						//Set base moisture to our moisture level.
						double moistureLevelAdapted = cell.MoistureLevel;
						//If the angle differs more from the windDirection, less moisture is reaching the otherCell.
						moistureLevelAdapted *= 1 - (angle / 360);
						//If the cells are further away, less moisture reaches the otherCell.
						moistureLevelAdapted -= (direction - windDirection).ToVector2().LengthSquared() * 30;
						//If we are going down, some moisture is added back else some more is subtracted
						moistureLevelAdapted += (cell.ElevationLevel - otherCell.ElevationLevel) * 10;
						moistureLevelAdapted = Math.Max(0, Math.Round(moistureLevelAdapted));

						//Not assigned yet or we can supply a higher moisture level
						if (otherCell.MoistureLevel < 0 || otherCell.MoistureLevel < moistureLevelAdapted)
						{
							otherCell.MoistureLevel = (short)moistureLevelAdapted;

							calcMoisture(otherCell);
						}
					}
					else if (cell.MoistureLevel == 0 && otherCell.MoistureLevel < 0)
					{
						otherCell.MoistureLevel = 0;
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

			//While we still have unassigned cells.
			while (vc.Cells.Where(c => c.MoistureLevel < 0).Count() > 0)
			{
				//Loop through all assigned cells.
				foreach (Cell cell in vc.Cells)
				{
					if (cell.MoistureLevel >= 0)
					{
						calcMoisture(cell);
					}
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
