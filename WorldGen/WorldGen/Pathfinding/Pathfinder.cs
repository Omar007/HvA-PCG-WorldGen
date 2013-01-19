using Microsoft.Xna.Framework;
using System.Collections.Generic;
using WorldGen.Voronoi;

namespace WorldGen.Pathfinding
{
	public class Pathfinder
	{
		private Dictionary<Cell, PathNode> cells;

		public Pathfinder(HashSet<Cell> cells)
		{
			this.cells = new Dictionary<Cell, PathNode>();

			foreach (Cell cell in cells)
			{
				this.cells.Add(cell, new PathNode(cell, 0, 0, null));
			}
		}

		public void reset()
		{
			foreach (PathNode node in cells.Values)
			{
				node.GCost = 0;
				node.FCost = 0;
				node.Next = null;
			}
		}

		public PathNode findPath(Cell startCell, Cell endCell)
		{
			if (startCell == null || endCell == null)
			{
				return null;
			}

			HashSet<PathNode> closedList = new HashSet<PathNode>();
			SortedList<double, PathNode> openList = new SortedList<double, PathNode>();

			PathNode node = cells[startCell];
			openList.Add(node.FCost, node);

			while (openList.Count > 0)
			{
				node = openList.Values[0];
				openList.RemoveAt(0);

				if (node.Cell == endCell)
				{
					return node;
				}

				closedList.Add(node);

				foreach (HalfEdge hEdge in node.Cell.HalfEdges)
				{
					if (hEdge.NeighbourCell == null || hEdge.NeighbourCell.LandType != CellLandType.Land)
					{
						continue;
					}

					PathNode toNode = cells[hEdge.NeighbourCell];

					if (closedList.Contains(toNode))
					{
						continue;
					}

					double newGCost = node.GCost + getGCost(node.Cell, toNode.Cell);
					double newFCost = newGCost + getHCost(node.Cell, endCell);

					bool openContainsToNode = openList.ContainsValue(toNode);

					if (!openContainsToNode || (openContainsToNode && newFCost < toNode.FCost))
					{
						if (openContainsToNode)
						{
							openList.Remove(toNode.FCost);
						}

						toNode.GCost = newGCost;
						toNode.FCost = newFCost;
						toNode.Next = node;
						openList.Add(toNode.FCost, toNode);
					}
				}
			}

			return null;
		}

		private double getGCost(Cell cell, Cell toCell)
		{
			double gCost = 0;

			//gCost = cell.Vertex.DistanceTo(toCell.Vertex);
			//gCost += Math.Abs(cell.CellElevationLevel - toCell.CellElevationLevel);
			gCost = getHCost(cell, toCell);

			return gCost;
		}

		private double getHCost(Cell cell, Cell endCell)
		{
			double hCost = 0;

			//hCost = cell.Vertex.DistanceTo(endCell.Vertex);
			//hCost += Math.Abs(cell.CellElevationLevel - endCell.CellElevationLevel);
			hCost = Vector3.Distance(new Vector3(cell.Vertex.ToVector2(), cell.ElevationLevel), new Vector3(endCell.Vertex.ToVector2(), endCell.ElevationLevel));

			return hCost;
		}
	}
}
