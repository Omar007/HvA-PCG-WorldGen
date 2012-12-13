using System;
using System.Collections.Generic;
using System.Diagnostics;
using WorldGen.Voronoi;
using WorldGen.WorldGenerator;

namespace WorldGen
{
	public class WorldManager
	{
		#region Fields
		private List<VoronoiCore> voronoiDiagrams;

		private CellGrouper cg;

		private TimeSpan landComputeTime;
		private LandGenerator landGenerator;

		private TimeSpan elevationComputeTime;
		private ElevationGenerator elevationGenerator;
		#endregion

		#region Properties
		public List<VoronoiCore> VoronoiDiagrams
		{
			get { return voronoiDiagrams; }
		}

		public VoronoiCore DeepestVoronoi
		{
			get { return voronoiDiagrams[voronoiDiagrams.Count - 1]; }
		}

		public TimeSpan LandComputeTime
		{
			get { return landComputeTime; }
		}

		public TimeSpan ElevationComputeTime
		{
			get { return elevationComputeTime; }
		}

		public float MaxHeight
		{
			get { return elevationGenerator.MaxHeight; }
		}
		#endregion

		public WorldManager(List<VoronoiCore> voronoiDiagrams)
		{
			this.voronoiDiagrams = voronoiDiagrams;

			cg = CellGrouper.GroupCells(voronoiDiagrams);

			landGenerator = new LandGenerator();
			elevationGenerator = new ElevationGenerator(6, 2);
		}

		public void generate()
		{
			Stopwatch sw = Stopwatch.StartNew();
			landGenerator.generate(cg, DeepestVoronoi);
			sw.Stop();
			landComputeTime = sw.Elapsed;

			sw.Restart();
			elevationGenerator.generate(DeepestVoronoi);
			sw.Stop();
			elevationComputeTime = sw.Elapsed;
		}
	}
}
