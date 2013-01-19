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

		private TimeSpan groupCellsComputeTime;
		private GroupedCell rootGC;

		private TimeSpan landComputeTime;
		private LandGenerator landGenerator;

		private TimeSpan elevationComputeTime;
		private ElevationGenerator elevationGenerator;

		private TimeSpan moistureComputeTime;
		private MoistureGenerator moistureGenerator;

		private Random random;
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

		public TimeSpan GroupCellsComputeTime
		{
			get { return groupCellsComputeTime; }
		}

		public TimeSpan LandComputeTime
		{
			get { return landComputeTime; }
		}

		public TimeSpan ElevationComputeTime
		{
			get { return elevationComputeTime; }
		}

		public TimeSpan MoistureComputeTime
		{
			get { return moistureComputeTime; }
		}

		public float MaxHeight
		{
			get { return elevationGenerator.MaxHeight; }
		}

		public Vertex WindDirection
		{
			get { return moistureGenerator.WindDirection; }
		}
		#endregion

		public WorldManager(List<VoronoiCore> voronoiDiagrams)
		{
			this.voronoiDiagrams = voronoiDiagrams;

			random = new Random();

			Stopwatch sw = Stopwatch.StartNew();
			rootGC = GroupedCell.groupCells(voronoiDiagrams);
			sw.Stop();
			groupCellsComputeTime = sw.Elapsed;

			landGenerator = new LandGenerator();
			elevationGenerator = new ElevationGenerator(6, 2);
			moistureGenerator = new MoistureGenerator(new Vertex(10 - random.NextDouble() * 20, 10 - random.NextDouble() * 20));
		}

		public void generateLand()
		{
			//Reset all cells, otherwise land will be added instead of completely regenerated!
			foreach (VoronoiCore vc in voronoiDiagrams)
			{
				foreach (Cell cell in vc.Cells)
				{
					cell.LandType = CellLandType.Undefined;
				}
			}

			Stopwatch sw = Stopwatch.StartNew();
			landGenerator.generate(rootGC, DeepestVoronoi);
			sw.Stop();
			landComputeTime = sw.Elapsed;
		}

		public void generateElevation()
		{
			Stopwatch sw = Stopwatch.StartNew();
			elevationGenerator.generate(DeepestVoronoi);
			sw.Stop();
			elevationComputeTime = sw.Elapsed;
		}

		public void generateMoisture()
		{
			Stopwatch sw = Stopwatch.StartNew();
			//moistureGenerator.generate(DeepestVoronoi);
			moistureGenerator.generate(DeepestVoronoi, new Vertex(10 - random.NextDouble() * 20, 10 - random.NextDouble() * 20));
			sw.Stop();
			moistureComputeTime = sw.Elapsed;
		}

		public void generate()
		{
			generateLand();
			generateElevation();
			generateMoisture();
		}
	}
}
