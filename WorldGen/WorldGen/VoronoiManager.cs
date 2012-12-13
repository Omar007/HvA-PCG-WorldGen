using System;
using System.Collections.Generic;
using System.Diagnostics;
using WorldGen.Voronoi;

namespace WorldGen
{
	public class VoronoiManager
	{
		#region Fields
		private Boundary bounds;

		private List<VoronoiCore> voronoiDiagrams;
		private Dictionary<VoronoiCore, int> pointCounts;
		private Dictionary<VoronoiCore, TimeSpan> computeTimes;
		#endregion

		#region Properties
		public Boundary Bounds
		{
			get { return bounds; }
		}

		public List<VoronoiCore> VoronoiDiagrams
		{
			get { return voronoiDiagrams; }
		}
		#endregion

		public VoronoiManager(int width, int height)
		{
			bounds = new Boundary(0, width, 0, height);

			voronoiDiagrams = new List<VoronoiCore>();
			pointCounts = new Dictionary<VoronoiCore, int>();
			computeTimes = new Dictionary<VoronoiCore, TimeSpan>();

			VoronoiCore vc0 = new VoronoiCore();
			voronoiDiagrams.Add(vc0);
			pointCounts.Add(vc0, 100);

			VoronoiCore vc1 = new VoronoiCore();
			voronoiDiagrams.Add(vc1);
			pointCounts.Add(vc1, 1000);

			VoronoiCore vc2 = new VoronoiCore();
			voronoiDiagrams.Add(vc2);
			pointCounts.Add(vc2, 10000);
		}

		private void compute(VoronoiCore vc, List<Vertex> points)
		{
			vc.reset();
			Stopwatch sw = Stopwatch.StartNew();
			vc.compute(points, bounds);
			sw.Stop();
			computeTimes[vc] = sw.Elapsed;
		}

		public void generate()
		{
			foreach(VoronoiCore vc in voronoiDiagrams)
			{
				generate(vc);
			}
		}

		private void generate(VoronoiCore vc)
		{
			Random r = new Random();
			double margin = 0.025;

			double xLeftBound = bounds.Right * margin;
			double xRightBound = bounds.Right - xLeftBound * 2;
			double yUpBound = bounds.Bottom * margin;
			double yBottomBound = bounds.Bottom - yUpBound * 2;

			List<Vertex> points = new List<Vertex>();

			for (int i = 0; i < pointCounts[vc]; i++)
			{
				points.Add(new Vertex(xLeftBound + r.NextDouble() * xRightBound,
					yUpBound + r.NextDouble() * yBottomBound));
			}

			compute(vc, points);
		}

		public void relax()
		{
			foreach (VoronoiCore vc in voronoiDiagrams)
			{
				relax(vc);
			}
		}

		private void relax(VoronoiCore vc)
		{
			List<Vertex> points = new List<Vertex>();

			foreach (Cell cell in vc.Cells)
			{
				List<Vertex> avrg = new List<Vertex>();

				foreach (HalfEdge hEdge in cell.HalfEdges)
				{
					Vertex va = hEdge.StartPoint;
					Vertex vb = hEdge.EndPoint;

					if (va != null && !avrg.Contains(va))
					{
						avrg.Add(va);
					}

					if (vb != null && !avrg.Contains(vb))
					{
						avrg.Add(vb);
					}
				}

				double xAvrg = 0;
				double yAvrg = 0;

				foreach (Vertex v in avrg)
				{
					xAvrg += v.X;
					yAvrg += v.Y;
				}

				xAvrg /= avrg.Count;
				yAvrg /= avrg.Count;

				points.Add(new Vertex(xAvrg, yAvrg));
			}

			compute(vc, points);
		}

		public TimeSpan timeFor(VoronoiCore vc)
		{
			return computeTimes[vc];
		}

		public int pointCountFor(VoronoiCore vc)
		{
			return pointCounts[vc];
		}
	}
}
