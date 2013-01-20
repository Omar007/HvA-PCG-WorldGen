using System.Collections.Generic;
using WorldGen.HelperFunctions;
using WorldGen.Voronoi;

namespace WorldGen.WorldGenerator
{
	public class Biome
	{
		#region Fields
		private System.Drawing.Color color;

		private float elevationMin;
		private float elevationMax;

		private short moistureMin;
		private short moistureMax;

		private List<Cell> cellsInBiome;
		#endregion

		#region Properties
		public System.Drawing.Color Color
		{
			get { return color; }
		}

		public float ElevationMin
		{
			get { return elevationMin; }
		}

		public float ElevationMax
		{
			get { return elevationMax; }
		}

		public short MoistureMin
		{
			get { return moistureMin; }
		}

		public short MoistureMax
		{
			get { return moistureMax; }
		}

		public List<Cell> CellsInBiome
		{
			get { return cellsInBiome; }
		}
		#endregion

		public Biome(System.Drawing.Color color, float elevationMin, float elevationMax, short moistureMin, short moistureMax)
		{
			this.color = color;

			this.elevationMin = elevationMin;
			this.elevationMax = elevationMax;
			
			this.moistureMin = moistureMin;
			this.moistureMax = moistureMax;

			this.cellsInBiome = new List<Cell>();
		}
	}

	public class BiomeMapper
	{
		private List<Biome> biomes;

		public List<Biome> Biomes
		{
			get { return biomes; }
		}

		public BiomeMapper(WorldManager wm)
		{
			biomes = new List<Biome>();

			setBiomes(wm);
		}

		public void setBiomes(WorldManager wm)
		{
			biomes.Clear();

			//All the biomes. Make sure none overlap.
			//If any biomes do overlap, the cell will be put into the first in the list.
			biomes.Add(new Biome(0xFFF8F8F8.ToColor(), wm.MaxHeight * 0.75f, wm.MaxHeight, (short)(short.MaxValue * 0.5f), short.MaxValue)); //Snow
			biomes.Add(new Biome(0xFFDDDDBB.ToColor(), wm.MaxHeight * 0.75f, wm.MaxHeight, (short)(short.MaxValue / 3.0f), (short)(short.MaxValue * 0.5f))); //Tundra
			biomes.Add(new Biome(0xFFBBBBBB.ToColor(), wm.MaxHeight * 0.75f, wm.MaxHeight, (short)(short.MaxValue / 6.0f), (short)(short.MaxValue / 3.0f))); //Bare
			biomes.Add(new Biome(0xFF999999.ToColor(), wm.MaxHeight * 0.75f, wm.MaxHeight, 0, (short)(short.MaxValue / 6.0f))); //Scorched
			biomes.Add(new Biome(0xFFCCD4BB.ToColor(), wm.MaxHeight * 0.5f, wm.MaxHeight * 0.75f, (short)(short.MaxValue / 1.5f), short.MaxValue)); //Taiga
			biomes.Add(new Biome(0xFFC4CCBB.ToColor(), wm.MaxHeight * 0.5f, wm.MaxHeight * 0.75f, (short)(short.MaxValue / 3.0f), (short)(short.MaxValue / 1.5f))); //Shrubland
			biomes.Add(new Biome(0xFFE4E8CA.ToColor(), wm.MaxHeight * 0.5f, wm.MaxHeight * 0.75f, 0, (short)(short.MaxValue / 3.0f))); //Temperate Desert
			biomes.Add(new Biome(0xFFA4C4A8.ToColor(), wm.MaxHeight * 0.25f, wm.MaxHeight * 0.5f, (short)(short.MaxValue / 1.2f), short.MaxValue)); //Temperate Rain Forest
			biomes.Add(new Biome(0xFFB4C9A9.ToColor(), wm.MaxHeight * 0.25f, wm.MaxHeight * 0.5f, (short)(short.MaxValue * 0.5f), (short)(short.MaxValue / 1.2f))); //Temperate Deciduous Forest
			biomes.Add(new Biome(0xFFC4D4AA.ToColor(), wm.MaxHeight * 0.25f, wm.MaxHeight * 0.5f, (short)(short.MaxValue / 6.0f), (short)(short.MaxValue * 0.5f))); //Grassland
			biomes.Add(new Biome(0xFFE4E8CA.ToColor(), wm.MaxHeight * 0.25f, wm.MaxHeight * 0.5f, 0, (short)(short.MaxValue / 6.0f))); //Temperate Desert
			biomes.Add(new Biome(0xFF9CBBA9.ToColor(), 0, wm.MaxHeight * 0.25f, (short)(short.MaxValue / 1.5f), short.MaxValue)); //Tropical Rain Forest
			biomes.Add(new Biome(0xFFA9CCA4.ToColor(), 0, wm.MaxHeight * 0.25f, (short)(short.MaxValue / 3.0f), (short)(short.MaxValue / 1.5f))); //Tropical Seasonal Forest
			biomes.Add(new Biome(0xFFC4D4AA.ToColor(), 0, wm.MaxHeight * 0.25f, (short)(short.MaxValue / 6.0f), (short)(short.MaxValue / 3.0f))); //Grassland
			biomes.Add(new Biome(0xFFE9DDC7.ToColor(), 0, wm.MaxHeight * 0.25f, 0, (short)(short.MaxValue / 6.0f))); //Subtropical Desert
		}

		private void resetBiomes()
		{
			foreach (Biome biome in biomes)
			{
				biome.CellsInBiome.Clear();
			}
		}

		public void assignBiomes(VoronoiCore vc)
		{
			resetBiomes();

			foreach (Cell cell in vc.Cells)
			{
				if (cell.LandType == CellLandType.Land)
				{
					foreach (Biome biome in biomes)
					{
						if (cell.ElevationLevel >= biome.ElevationMin
							&& cell.ElevationLevel <= biome.ElevationMax
							&& cell.MoistureLevel >= biome.MoistureMin
							&& cell.MoistureLevel <= biome.MoistureMax)
						{
							biome.CellsInBiome.Add(cell);
							break;
						}
					}
				}
			}
		}
	}
}
