using System.Collections.Generic;
using WorldGen.Voronoi;

namespace WorldGen.Pathfinding
{
	public class Pathfinder
	{
		public Pathfinder()
		{
		}

		public PathNode findPath(Cell startCell, Cell endCell)
		{
			if (startCell == null || endCell == null || startCell == endCell)
			{
				return null;
			}

			HashSet<Cell> closedList = new HashSet<Cell>();
			Heap openList = new Heap();

			openList.add(new PathNode(startCell, 0, getHCost(startCell, endCell), null));

			while (openList.HasNext)
			{
				PathNode node = openList.Pop();

				if (node.Cell == endCell)
				{
					return node;
				}

				closedList.Add(node.Cell);

				foreach (HalfEdge hEdge in node.Cell.HalfEdges)
				{
					if (hEdge.NeighbourCell == null || hEdge.NeighbourCell.LandType != CellLandType.Land || closedList.Contains(hEdge.NeighbourCell))
					{
						continue;
					}

					PathNode toNode = new PathNode(hEdge.NeighbourCell, getGCost(node.Cell, hEdge.NeighbourCell), getHCost(hEdge.NeighbourCell, endCell), node);
					openList.add(toNode);
				}
			}

			return null;
		}

		private double getGCost(Cell cell, Cell toCell)
		{
			double gCost = 0;

			gCost = cell.Vertex.DistanceTo(toCell.Vertex);
			gCost -= cell.ElevationLevel - toCell.ElevationLevel;
			
			//gCost = Vector3.Distance(new Vector3(cell.Vertex.ToVector2(), cell.ElevationLevel), new Vector3(toCell.Vertex.ToVector2(), toCell.ElevationLevel * 0.5f));

			return gCost;
		}

		private double getHCost(Cell cell, Cell endCell)
		{
			//double hCost = 0;

			//hCost = cell.Vertex.DistanceTo(endCell.Vertex);
			//hCost -= cell.ElevationLevel - endCell.ElevationLevel;
			
			////hCost = Vector3.Distance(new Vector3(cell.Vertex.ToVector2(), cell.ElevationLevel), new Vector3(endCell.Vertex.ToVector2(), endCell.ElevationLevel));

			//return hCost;

			return getGCost(cell, endCell);
		}
	}
}
