using System;
using System.Collections.Generic;
using System.Linq;
using WorldGen.Pathfinding;
using WorldGen.Voronoi;

namespace WorldGen.WorldGenerator
{
	public class City
	{
		#region Fields
		public static readonly System.Drawing.Color Color = System.Drawing.Color.Gray;

		private Cell centerCell;
		private List<Cell> cells;

		private Dictionary<City, PathNode> routesToCities;
		#endregion

		#region Properties
		public Cell CenterCell
		{
			get { return centerCell; }
		}

		public List<Cell> Cells
		{
			get { return cells; }
		}

		public Dictionary<City, PathNode> RoutesToCities
		{
			get { return routesToCities; }
		}
		#endregion

		public City(Cell centerCell, List<Cell> cells)
		{
			this.centerCell = centerCell;
			this.cells = cells;

			routesToCities = new Dictionary<City, PathNode>();
		}
	}

	public class CityGenerator
	{
		private WorldManager wm;

		private Pathfinder pathfinder;
		private List<City> cities;

		public List<City> Cities
		{
			get { return cities; }
		}

		public CityGenerator(WorldManager wm)
		{
			this.wm = wm;

			pathfinder = new Pathfinder();
			cities = new List<City>();
		}

		private List<Cell> checkCityPossible(Cell cell)
		{
			List<Cell> cells = null;

			if (cell.ElevationLevel >= 0
				&& cell.ElevationLevel <= wm.MaxHeight * 0.5f
				&& cell.MoistureLevel >= short.MaxValue / 3.0f
				&& cell.MoistureLevel <= short.MaxValue)
			{
				cells = new List<Cell>();

				cells.Add(cell);
				checkNeighboursForCity(cell, cells);
			}

			if (cells != null && cells.Count > 0)
			{
				return cells;
			}

			return null;
		}

		private void checkNeighboursForCity(Cell cell, List<Cell> cells)
		{
			if (cells == null || cells.Count > 15)
			{
				return;
			}

			foreach (HalfEdge hEdge in cell.HalfEdges)
			{
				Cell otherCell = hEdge.NeighbourCell;

				if (otherCell != null && otherCell.LandType == CellLandType.Land)
				{
					if (cities.Where(c => c.Cells.Contains(cell)).Count() == 0)
					{
						if (otherCell.ElevationLevel >= 0
							&& otherCell.ElevationLevel <= wm.MaxHeight * 0.5f
							&& otherCell.MoistureLevel >= short.MaxValue / 3.0f
							&& otherCell.MoistureLevel <= short.MaxValue
							&& Math.Abs(otherCell.ElevationLevel - cell.ElevationLevel) <= 1)
						{
							cells.Add(otherCell);
							checkNeighboursForCity(otherCell, cells);
						}
					}
					else
					{
						//Touching other city; abort!
						cells = null;
						return;
					}
				}
			}
		}

		public void generate()
		{
			cities.Clear();

			int counter = 0;

			foreach (Cell cell in wm.DeepestVoronoi.Cells)
			{
				if (cell.LandType == CellLandType.Land && cities.Where(c => c.Cells.Contains(cell)).Count() == 0)
				{
					List<Cell> cityCells = checkCityPossible(cell);

					if (cityCells != null && cityCells.Count > 5)
					{
						//Only keep every 20th city
						if (counter % 20 == 0)
						{
							cities.Add(new City(cell, cityCells));
						}
						counter++;
					}
				}
			}

			createRoads();
		}

		private void createRoads()
		{
			for (int i = 0; i < cities.Count; i++)
			{
				for (int j = i + 1; j < cities.Count; j++)
				{
					PathNode route = pathfinder.findPath(cities[i].CenterCell, cities[j].CenterCell);
					
					if (route != null)
					{
						PathNode cursor = route;

						while (cursor != null)
						{
							if (cities.Where(c => (
								(c != cities[i] && c != cities[j]) //Not part of start/end city
								&& c.Cells.Contains(cursor.Cell) //Part of another city
								&& (cities[i].RoutesToCities.ContainsKey(c) && cities[j].RoutesToCities.ContainsKey(c)) //We have routes to that city
								)).Count() > 0)
							{
								return;
							}

							cursor = cursor.Next;
						}

						cities[i].RoutesToCities.Add(cities[j], route);
						cities[j].RoutesToCities.Add(cities[i], route);
					}
				}
			}
		}
	}
}
