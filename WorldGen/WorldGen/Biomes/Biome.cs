using System.Collections.Generic;

namespace WorldGen.Biomes
{
	public class Biome
	{
		#region Fields
		private Latitude latitude;
		private Humidity humidity;
		private int elevation;

		private List<Biome> relatedBiomes;
		#endregion

		#region Properties
		public Latitude Latitude
		{
			get { return latitude; }
		}

		public Humidity Humidity
		{
			get { return humidity; }
		}

		public int Elevation
		{
			get { return elevation; }
		}

		public List<Biome> RelatedBiomes
		{
			get { return relatedBiomes; }
		}
		#endregion

		public Biome(Latitude latitude, Humidity humidity, int elevation)
		{
			this.latitude = latitude;
			this.humidity = humidity;
			this.elevation = elevation;

			relatedBiomes = new List<Biome>();
		}
	}
}
