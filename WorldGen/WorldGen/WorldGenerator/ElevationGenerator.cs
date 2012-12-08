using System;
using System.Collections.Generic;
using System.Diagnostics;
using WorldGen.Voronoi;

namespace WorldGen.WorldGenerator
{
	public class ElevationGenerator : WorldGenerator
	{
		private HashSet<Cell> handledWaterCells;
		private HashSet<Cell> handledLandCells;

		public ElevationGenerator()
			: base()
		{
			handledWaterCells = new HashSet<Cell>();
			handledLandCells = new HashSet<Cell>();
		}

		private void setLandElevation(Cell cell)
		{
			float highestNeighbourLevel = CellElevationLevel.SeaLevel;
			float lowestNeighbourLevel = CellElevationLevel.Maximum;

			foreach (HalfEdge halfEdge in cell.HalfEdges)
			{
				Cell otherCell = halfEdge.NeighbourCell;

				if (otherCell != null && otherCell.CellLandType != CellLandType.Water)
				{
					if (otherCell.CellElevationLevel > highestNeighbourLevel)
					{
						highestNeighbourLevel = otherCell.CellElevationLevel;
					}

					if (otherCell.CellElevationLevel < lowestNeighbourLevel)
					{
						lowestNeighbourLevel = otherCell.CellElevationLevel;
					}



					if (lowestNeighbourLevel == CellElevationLevel.SeaLevel)
					{
						if (lowestNeighbourLevel == highestNeighbourLevel) //Only SeaLevel elevation level found in neighbours
						{
							lowestNeighbourLevel = highestNeighbourLevel = CellElevationLevel.GroundLevel;
						}
						else
						{
							lowestNeighbourLevel = CellElevationLevel.GroundLevel;
						}
					}

					//lowestNeighbourLevel += 0.5f;

					if (lowestNeighbourLevel > CellElevationLevel.Maximum)
					{
						lowestNeighbourLevel = highestNeighbourLevel = CellElevationLevel.Maximum;
					}
					else if (lowestNeighbourLevel > highestNeighbourLevel)
					{
						highestNeighbourLevel = lowestNeighbourLevel;
					}

					float elevationDiff = highestNeighbourLevel - lowestNeighbourLevel;

					if (elevationDiff > 2 || elevationDiff == 0)
					{
						cell.CellElevationLevel = (float)Math.Round(lowestNeighbourLevel + (Random.NextDouble() * 2));
					}
					else
					{
						cell.CellElevationLevel = (float)Math.Round(lowestNeighbourLevel + (Random.NextDouble() * elevationDiff));
					}

					if (cell.CellElevationLevel > CellElevationLevel.Maximum)
					{
						cell.CellElevationLevel = CellElevationLevel.Maximum;
					}



					if (otherCell.CellLandType == CellLandType.Land && !handledLandCells.Contains(otherCell))
					{
						handledLandCells.Add(cell);
						setLandElevation(otherCell);
					}
				}
			}
		}

		private void setWaterElevation(Cell cell, ref float lowestLand)
		{
			foreach (HalfEdge halfEdge in cell.HalfEdges)
			{
				Cell otherCell = halfEdge.NeighbourCell;

				if (otherCell != null)
				{
					if (otherCell.CellLandType == CellLandType.Land && otherCell.CellElevationLevel < lowestLand)
					{
						lowestLand = otherCell.CellElevationLevel;
					}
					else if (otherCell.CellLandType == CellLandType.Water && !handledWaterCells.Contains(otherCell))
					{
						handledWaterCells.Add(cell);
						setWaterElevation(otherCell, ref lowestLand);
					}
				}
			}

			cell.CellElevationLevel = lowestLand;
			handledWaterCells.Remove(cell);
		}

		protected override void reset(VoronoiCore vc)
		{
			handledWaterCells.Clear();
			handledLandCells.Clear();

			foreach (Cell cell in vc.Cells)
			{
				switch (cell.CellLandType)
				{
					case CellLandType.Land:
						cell.CellElevationLevel = CellElevationLevel.GroundLevel;
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
			Stopwatch sw = Stopwatch.StartNew();

			foreach (Cell cell in vc.Cells)
			{
				if (cell.CellLandType == CellLandType.Land && !handledLandCells.Contains(cell))
				{
					setLandElevation(cell);

					//if (handledLandCells.Count > 0) throw new Exception();
				}
			}

			sw.Stop();

			TimeSpan land = sw.Elapsed;

			sw.Restart();

			foreach (Cell cell in vc.Cells)
			{
				if (cell.CellLandType == CellLandType.Water && float.IsNaN(cell.CellElevationLevel))
				{
					float lowestRef = CellElevationLevel.Maximum;
					setWaterElevation(cell, ref lowestRef);

					if (handledWaterCells.Count > 0) throw new Exception(); //All water cells should have been handled here
				}
			}

			sw.Stop();

			TimeSpan water = sw.Elapsed;

			if (water > land)
			{
				Console.WriteLine();
			}
		}
	}
}
