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
			if (cell.ElevationLevel >= 0
				&& cell.ElevationLevel <= wm.MaxHeight * 0.5f
				&& cell.MoistureLevel >= short.MaxValue / 3.0f
				&& cell.MoistureLevel <= short.MaxValue)
			{
				List<Cell> cells = new List<Cell>();

				cells.Add(cell);

				int lastIndex = 0;
				int curCount = 0;

				while (true)
				{
					lastIndex = curCount;
					curCount = cells.Count;

					if (lastIndex == curCount)
					{
						return cells;
					}

					for (int i = lastIndex; i < curCount; i++)
					{
						checkNeighboursForCity(cells[i], cells);

						if (cells.Count == 0 || cells.Count >= 15)
						{
							return cells;
						}
					}
				}
			}

			return null;
		}

		private void checkNeighboursForCity(Cell cell, List<Cell> cells)
		{
			foreach (HalfEdge hEdge in cell.HalfEdges)
			{
				Cell otherCell = hEdge.NeighbourCell;

				if (otherCell != null && otherCell.LandType == CellLandType.Land)
				{
					if (cities.Where(c => c.Cells.Contains(otherCell)).Count() == 0)
					{
						if (otherCell.ElevationLevel >= 0
							&& otherCell.ElevationLevel <= wm.MaxHeight * 0.5f
							&& otherCell.MoistureLevel >= short.MaxValue / 3.0f
							&& otherCell.MoistureLevel <= short.MaxValue
							&& Math.Abs(otherCell.ElevationLevel - cell.ElevationLevel) <= 1)
						{
							cells.Add(otherCell);
						}
					}
					else
					{
						//Touching other city; abort!
						cells.Clear();
						return;
					}
				}
			}
		}

		public void generate()
		{
			cities.Clear();

			byte counter = 0;

			foreach (Cell cell in wm.DeepestVoronoi.Cells)
			{
				if (cell.LandType == CellLandType.Land && cities.Where(c => c.Cells.Contains(cell)).Count() == 0)
				{
					List<Cell> cityCells = checkCityPossible(cell);

					if (cityCells != null && cityCells.Count > 5)
					{
						if (counter == 0)
						{
							cities.Add(new City(cell, cityCells));
						}
						counter = (byte)((counter + 1) % 20);
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
								&& (cities[i].RoutesToCities.ContainsKey(c) && (c.RoutesToCities.ContainsKey(cities[j]) || cities[j].RoutesToCities.ContainsKey(c))) //We have routes to that city
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

				List<KeyValuePair<City, double>> closestCities = new List<KeyValuePair<City, double>>();

				foreach (KeyValuePair<City, PathNode> route in cities[i].RoutesToCities)
				{
					closestCities.Add(new KeyValuePair<City, double>(cities[i], route.Value.FCost));
				}

				closestCities.Sort((kvp, otherKvp) => (int)(kvp.Value - otherKvp.Value));

				for(int j = 3; j < closestCities.Count; j++)
				{
					cities[i].RoutesToCities.Remove(closestCities[j].Key);
					closestCities[j].Key.RoutesToCities.Remove(cities[i]);
				}
			}
		}
	}
}
