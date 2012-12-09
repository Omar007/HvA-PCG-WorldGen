using System;
using System.Collections.Generic;
using System.Diagnostics;
using WorldGen.Voronoi;

namespace WorldGen.WorldGenerator
{
	public class ElevationGenerator : WorldGenerator
	{
		private HashSet<Cell> handledCells;

		public ElevationGenerator()
			: base()
		{
			handledCells = new HashSet<Cell>();
		}

		private void setLandElevation(Cell cell)
		{
			if (cell.CellLandType != CellLandType.Land)
			{
				return;
			}

			float highestNeighbourLevel = float.MinValue;
			float lowestNeighbourLevel = float.MaxValue;

			foreach (HalfEdge halfEdge in cell.HalfEdges)
			{
				Cell otherCell = halfEdge.NeighbourCell;

				if (otherCell != null)
				{
					if (otherCell.CellLandType == CellLandType.Ocean)
					{
						//Set land something between ground level and the highest allowed.
						cell.CellElevationLevel = (float)Math.Round(Random.NextDouble() * CellElevationLevel.Low);

						handledCells.Add(cell);
					}
					else if (otherCell.CellLandType == CellLandType.Land && float.IsNaN(otherCell.CellElevationLevel))
					{
						if (!handledCells.Contains(otherCell))
						{
							bool wasNotYetSet = float.IsNaN(cell.CellElevationLevel);

							if (wasNotYetSet)
							{
								cell.CellElevationLevel = 0;
							}

							setLandElevation(otherCell);

							if (wasNotYetSet)
							{
								cell.CellElevationLevel = float.NaN;
							}
						}

						//We are already set so go to the next
						if (handledCells.Contains(cell))
						{
							continue;
						}

						//We can use the neighbour as reference for our height
						if (handledCells.Contains(otherCell))
						{
							if (otherCell.CellElevationLevel > highestNeighbourLevel)
							{
								highestNeighbourLevel = otherCell.CellElevationLevel;
							}

							if (otherCell.CellElevationLevel < lowestNeighbourLevel)
							{
								lowestNeighbourLevel = otherCell.CellElevationLevel;
							}
						}
					}
				}
			}

			if (float.IsNaN(cell.CellElevationLevel))
			{
				float finalElevationLevel = 0;

				if (lowestNeighbourLevel != float.MaxValue && highestNeighbourLevel != float.MinValue)
				{
					float elevationDiff = highestNeighbourLevel - lowestNeighbourLevel;

					if (elevationDiff == 0)
					{
						finalElevationLevel = lowestNeighbourLevel + (float)Math.Round(Random.NextDouble());
					}
					else if (elevationDiff > 2)
					{
						finalElevationLevel = lowestNeighbourLevel + (float)Math.Round(Random.NextDouble() * 2);
					}
					else
					{
						finalElevationLevel = lowestNeighbourLevel + (float)Math.Round(Random.NextDouble() * elevationDiff);
					}

					if (finalElevationLevel > CellElevationLevel.Maximum)
					{
						finalElevationLevel = CellElevationLevel.Maximum;
					}
				}
				//No neighbours found to use as reference.
				//Just take something between GroundLevel and the specified max.
				else
				{
					finalElevationLevel = (float)Math.Round(Random.NextDouble() * CellElevationLevel.Low);
				}

				cell.CellElevationLevel = finalElevationLevel;
			}

			handledCells.Add(cell);
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
					if (otherCell.CellLandType == CellLandType.Land)
					{
						if (!handledCells.Contains(otherCell))
						{
							setLandElevation(otherCell);
						}
						
						if (otherCell.CellElevationLevel < lowestLand)
						{
							lowestLand = otherCell.CellElevationLevel;
							cell.CellElevationLevel = lowestLand;
							handledCells.Add(cell);
						}
					}
					else if (otherCell.CellLandType == CellLandType.Water)
					{
						if (!handledCells.Contains(otherCell) && float.IsNaN(otherCell.CellElevationLevel))
						{
							bool wasNotYetSet = float.IsNaN(cell.CellElevationLevel);

							if (wasNotYetSet)
							{
								cell.CellElevationLevel = 0;
							}

							setWaterElevation(otherCell, ref lowestLand);
							
							if (wasNotYetSet)
							{
								cell.CellElevationLevel = float.NaN;
							}
						}

						if (handledCells.Contains(otherCell))
						{
							if (otherCell.CellElevationLevel > lowestLand)
							{
								otherCell.CellElevationLevel = lowestLand;
								setWaterElevation(otherCell, ref lowestLand);
							}
							else
							{
								lowestLand = otherCell.CellElevationLevel;
							}
							cell.CellElevationLevel = lowestLand;
							handledCells.Add(cell);
						}
					}
				}
			}
		}

		protected override void reset(VoronoiCore vc)
		{
			handledCells.Clear();

			foreach (Cell cell in vc.Cells)
			{
				switch (cell.CellLandType)
				{
					case CellLandType.Land:
						cell.CellElevationLevel = float.NaN;
						break;

					case CellLandType.Ocean: //Ocean always at sea level
						cell.CellElevationLevel = CellElevationLevel.SeaLevel;
						break;

					case CellLandType.Water:
						cell.CellElevationLevel = float.NaN;
						break;
				}
			}
		}

		protected override void generate(VoronoiCore vc)
		{
			float lowestLand = float.MaxValue;

			foreach (Cell cell in vc.Cells)
			{
				if (cell.CellLandType == CellLandType.Land && !handledCells.Contains(cell)) //&& float.IsNaN(cell.CellElevationLevel)
				{
					setLandElevation(cell);
				}
				else if(cell.CellLandType == CellLandType.Water)
				{
					if (handledCells.Contains(cell))
					{
						cell.CellElevationLevel = lowestLand;
					}
					else //float.IsNaN(cell.CellElevationLevel)
					{
						lowestLand = CellElevationLevel.Maximum;
						setWaterElevation(cell, ref lowestLand);
						cell.CellElevationLevel = lowestLand;
					}
				}
			}


			//Stopwatch sw = Stopwatch.StartNew();

			//foreach (Cell cell in vc.Cells)
			//{
			//	if (cell.CellLandType == CellLandType.Land && !handledLandCells.Contains(cell)) //&& float.IsNaN(cell.CellElevationLevel)
			//	{
			//		setLandElevation(cell);
			//	}
			//}

			//sw.Stop();
			//TimeSpan land = sw.Elapsed;
			//sw.Restart();

			//float lowestLand = float.MaxValue;

			//foreach (Cell cell in vc.Cells)
			//{
			//	if (handledWaterCells.Contains(cell))
			//	{
			//		cell.CellElevationLevel = lowestLand;
			//	}
			//	else if (cell.CellLandType == CellLandType.Water) //&& float.IsNaN(cell.CellElevationLevel)
			//	{
			//		lowestLand = CellElevationLevel.Maximum;
			//		setWaterElevation(cell, ref lowestLand);
			//		cell.CellElevationLevel = lowestLand;
			//	}
			//}

			//sw.Stop();
			//TimeSpan water = sw.Elapsed;

			//if (water > land)
			//{
			//	Console.WriteLine();
			//}
		}
	}
}
