using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using WorldGen.Voronoi;

namespace WorldGen
{
	/// <summary>
	/// This is a game component that implements IUpdateable.
	/// </summary>
	public class VoronoiDrawableGameComponent : DrawableGameComponent
	{
		private SpriteBatch spriteBatch;
		private SpriteFont sFont;

		private KeyboardState lastState;
		private TimeSpan computeTime;

		private const int pointCount = 500;
		private Boundary bounds;
		private VoronoiCore vc;

		private WorldDrawableGameComponent wdgc;

		public VoronoiDrawableGameComponent(Game game)
			: base(game)
		{
			// TODO: Construct any child components here
		}

		/// <summary>
		/// Allows the game component to perform any initialization it needs to before starting
		/// to run.  This is where it can query for any required services and load content.
		/// </summary>
		public override void Initialize()
		{
			// TODO: Add your initialization code here
			wdgc = new WorldDrawableGameComponent(Game);
			Game.Components.Add(wdgc);

			base.Initialize();
		}

		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);
			sFont = Game.Content.Load<SpriteFont>("Fonts/Arial12");

			HelperFunctions.PrimitivesBatch.Init(GraphicsDevice);

			bounds = new Boundary(0, GraphicsDevice.Viewport.Width, 0, GraphicsDevice.Viewport.Height);

			Random r = new Random();
			double margin = 0.025;

			double xLeftBound = GraphicsDevice.Viewport.Width * margin;
			double xRightBound = GraphicsDevice.Viewport.Width - xLeftBound * 2;
			double yUpBound = GraphicsDevice.Viewport.Height * margin;
			double yBottomBound = GraphicsDevice.Viewport.Height - yUpBound * 2;

			List<Vertex> points = new List<Vertex>();

			for (int i = 0; i < pointCount; i++)
			{
				points.Add(new Vertex(xLeftBound + r.NextDouble() * xRightBound,
					yUpBound + r.NextDouble() * yBottomBound));
			}

			vc = new VoronoiCore();
			Stopwatch sw = Stopwatch.StartNew();
			vc.compute(points, bounds);
			sw.Stop();
			computeTime = sw.Elapsed;
			
			base.LoadContent();
		}

		/// <summary>
		/// Allows the game component to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Update(GameTime gameTime)
		{
			if (Keyboard.GetState().IsKeyDown(Keys.LeftControl) && (lastState.IsKeyUp(Keys.LeftControl) || Keyboard.GetState().IsKeyDown(Keys.LeftAlt)))
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

				vc.reset();
				Stopwatch sw = Stopwatch.StartNew();
				vc.compute(points, bounds);
				sw.Stop();
				computeTime = sw.Elapsed;

				wdgc.clear();
			}

			if (Keyboard.GetState().IsKeyDown(Keys.F1) && lastState.IsKeyUp(Keys.F1))
			{
				wdgc.generate(vc);
			}

			lastState = Keyboard.GetState();

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			spriteBatch.Begin();

			int delaunyEdgeCount = 0;

			foreach (Edge edge in vc.Edges)
			{
				HelperFunctions.PrimitivesBatch.DrawLine(spriteBatch, Color.White, edge.VertexA.ToVector2(), edge.VertexB.ToVector2());

				if (edge.HasDelaunyEdge)
				{
					DelaunyEdge delaunyEdge = edge.DelaunyEdge;
					HelperFunctions.PrimitivesBatch.DrawLine(spriteBatch, Color.Green, delaunyEdge.VertexA.ToVector2(), delaunyEdge.VertexB.ToVector2());

					delaunyEdgeCount++;
				}
			}

			foreach (Cell cell in vc.Cells)
			{
				Color color = Color.Yellow;
				int size = 4;

				switch (cell.CellType)
				{
					case CellType.WestEdge:
						color = Color.RosyBrown;
						size = 6;
						break;

					case CellType.EastEdge:
						color = Color.MediumVioletRed;
						size = 6;
						break;
						
					case CellType.NorthEdge:
						color = Color.SaddleBrown;
						size = 6;
						break;

					case CellType.SouthEdge:
						color = Color.Maroon;
						size = 6;
						break;
				}

				HelperFunctions.PrimitivesBatch.DrawPoint(spriteBatch, color, cell.Vertex.ToVector2(), size);
			}

			HelperFunctions.PrimitivesBatch.DrawRectangle(spriteBatch, Color.Red,
				new Rectangle((int)bounds.Left, (int)bounds.Top, (int)(bounds.Right - bounds.Left), (int)(bounds.Bottom - bounds.Top)));

			spriteBatch.DrawString(sFont, computeTime.ToString(), Vector2.Zero, Color.Brown);

			spriteBatch.DrawString(sFont, "Points: " + pointCount.ToString(), new Vector2(0, 20), Color.Brown);
			spriteBatch.DrawString(sFont, "Cells: " + vc.Cells.Count.ToString(), new Vector2(0, 40), Color.Brown);
			spriteBatch.DrawString(sFont, "Edges: " + vc.Edges.Count.ToString(), new Vector2(0, 60), Color.Brown);
			spriteBatch.DrawString(sFont, "Delauny Edges: " + delaunyEdgeCount.ToString(), new Vector2(0, 80), Color.Brown);

			spriteBatch.End();
			
			base.Draw(gameTime);
		}
	}
}
