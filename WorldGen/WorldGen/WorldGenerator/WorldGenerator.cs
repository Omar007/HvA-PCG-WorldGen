using System;
using WorldGen.Voronoi;

namespace WorldGen.WorldGenerator
{
	public abstract class WorldGenerator
	{
		#region Fields
		private Random random;
		#endregion

		#region Properties
		protected Random Random
		{
			get { return random; }
		}
		#endregion

		public WorldGenerator()
		{
			random = new Random();
		}

		public void generate(VoronoiCore vc, bool regenerate)
		{
			if (regenerate)
			{
				reset(vc);
			}

			generate(vc);
		}

		protected abstract void reset(VoronoiCore vc);

		protected abstract void generate(VoronoiCore vc);
	}
}
