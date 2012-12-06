using System;
using System.Collections.Generic;
using WorldGen.Voronoi;

namespace WorldGen.WorldGenerator
{
	public class LandGenerator
	{
		#region Fields
		private Random random;

		private List<Cell> waterCells;
		private List<Cell> landCells;
		#endregion

		#region Properties
		public List<Cell> WaterCells
		{
			get { return waterCells; }
		}

		public List<Cell> LandCells
		{
			get { return landCells; }
		}
		#endregion

		public LandGenerator()
		{
			random = new Random();

			waterCells = new List<Cell>();
			landCells = new List<Cell>();
		}

		public void reset()
		{
			waterCells.Clear();
			landCells.Clear();
		}

		public void generate(VoronoiCore vc)
		{
			foreach (Cell cell in vc.Cells)
			{
				if (cell.CellType == CellType.NoEdge && random.NextDouble() > 0.5)
				{
					landCells.Add(cell);
				}
				else
				{
					waterCells.Add(cell);
				}
			}

			List<Cell> landNeighbours = new List<Cell>();

			for(int i = waterCells.Count - 1; i >= 0; i--)
			{
				Cell cell = waterCells[i];

				if (cell.CellType == CellType.NoEdge)
				{
					int neighbourCount = 0;

					landNeighbours.Clear();

					foreach (HalfEdge halfEdge in cell.HalfEdges)
					{
						if (halfEdge.Edge.LeftCell == cell && landCells.Contains(halfEdge.Edge.RightCell))
						{
							landNeighbours.Add(halfEdge.Edge.RightCell);
						}
						else if (halfEdge.Edge.RightCell == cell && landCells.Contains(halfEdge.Edge.LeftCell))
						{
							landNeighbours.Add(halfEdge.Edge.LeftCell);
						}

						neighbourCount++;
					}

					if (landNeighbours.Count > neighbourCount * 0.5)
					{
						waterCells.Remove(cell);
						landCells.Add(cell);
					}
				}
			}
		}
	}
}
