using Microsoft.Xna.Framework;
using System.Collections.Generic;
using WorldGen.Voronoi;

namespace WorldGen.Pathfinding
{
	public class Pathfinder
	{
		private HashSet<Cell> cellsPartOfRoute;

		public Pathfinder()
		{
			cellsPartOfRoute = new HashSet<Cell>();
		}

		public PathNode findPath(Cell startCell, Cell endCell)
		{
			if (startCell == null || endCell == null)
			{
				return null;
			}

			List<GroupedCell> startParents = new List<GroupedCell>();
			List<GroupedCell> endParents = new List<GroupedCell>();
			cellsPartOfRoute.Clear();

			GroupedCell startParent = startCell.GroupedCellInfo;
			GroupedCell endParent = endCell.GroupedCellInfo;

			while (true)
			{
				startParents.Add(startParent);
				endParents.Add(endParent);

				if (startParent == endParent)
				{
					break;
				}
				else if (startParent.Parent != null && startParent.Parent.Parent != null
					&& endParent.Parent != null && endParent.Parent.Parent != null)
				{
					startParent = startParent.Parent;
					endParent = endParent.Parent;
				}
				else
				{
					break;
				}
			}

			while (true)
			{
				PathNode route = null;

				if (startParent.Cell != endParent.Cell)
				{
					route = doFindPath(startParent.Cell, endParent.Cell);
				}
				else
				{
					route = new PathNode(startParent.Cell, 0, 0, null);
				}

				if (route != null)
				{
					cellsPartOfRoute.Clear();

					PathNode cursor = route;
					while (cursor != null)
					{
						for (int i = 0; i < cursor.Cell.GroupedCellInfo.Count; i++)
						{
							cellsPartOfRoute.Add(cursor.Cell.GroupedCellInfo[i].Cell);

							foreach (HalfEdge hEdge in cursor.Cell.HalfEdges)
							{
								Cell otherCell = hEdge.NeighbourCell;

								if (otherCell != null)
								{
									for (int j = 0; j < otherCell.GroupedCellInfo.Count; j++)
									{
										cellsPartOfRoute.Add(otherCell.GroupedCellInfo[j].Cell);
									}
								}
							}
						}

						cursor = cursor.Next;
					}

					startParents.RemoveAt(startParents.Count - 1);
					endParents.RemoveAt(endParents.Count - 1);

					if (startParents.Count > 0 && endParents.Count > 0)
					{
						startParent = startParents[startParents.Count - 1];
						endParent = endParents[endParents.Count - 1];
					}
					else
					{
						return route;
					}
				}
				else
				{
					return null;
				}
			}
		}

		private PathNode doFindPath(Cell startCell, Cell endCell)
		{
			HashSet<Cell> closedList = new HashSet<Cell>();
			Heap openList = new Heap();

			openList.add(new PathNode(startCell, 0, getHCost(startCell, endCell), null));

			while (openList.HasNext)
			{
				PathNode node = openList.Pop();

				if (closedList.Contains(node.Cell))
				{
					continue;
				}

				if (node.Cell == endCell)
				{
					return node;
				}

				closedList.Add(node.Cell);

				foreach (HalfEdge hEdge in node.Cell.HalfEdges)
				{
					Cell otherCell = hEdge.NeighbourCell;

					if (otherCell != null)// && !closedList.Contains(otherCell))
					{
						if ((otherCell == endCell || otherCell.LandType == CellLandType.Land)
							&& (cellsPartOfRoute.Count == 0 || cellsPartOfRoute.Contains(otherCell)))
						{
							PathNode toNode = new PathNode(otherCell, getGCost(node.Cell, otherCell), getHCost(otherCell, endCell), node);
							openList.add(toNode);
						}
					}
				}
			}

			return null;
		}

		private double getGCost(Cell cell, Cell toCell)
		{
			//double gCost = 0;

			//gCost = cell.Vertex.DistanceTo(toCell.Vertex);
			//gCost -= cell.ElevationLevel - toCell.ElevationLevel;

			//return gCost;
			
			return Vector3.DistanceSquared(new Vector3(cell.Vertex.ToVector2(), float.IsNaN(cell.ElevationLevel) ? 0 : cell.ElevationLevel),
				new Vector3(toCell.Vertex.ToVector2(), float.IsNaN(toCell.ElevationLevel) ? 0 : toCell.ElevationLevel));
		}

		private double getHCost(Cell cell, Cell endCell)
		{
			return getGCost(cell, endCell);
		}
	}
}
