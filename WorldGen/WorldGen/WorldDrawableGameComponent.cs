using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using WorldGen.HelperFunctions;
using WorldGen.Pathfinding;
using WorldGen.Voronoi;

namespace WorldGen
{
	/// <summary>
	/// This is a game component that implements IUpdateable.
	/// </summary>
	public class WorldDrawableGameComponent : DrawableGameComponent
	{
		private SpriteBatch spriteBatch;
		private SpriteFont sFont;

		private KeyboardState lastState;

		private Texture2D texture;
		private TimeSpan drawTime;

		private VoronoiManager voronoiManager;
		private WorldManager wm;

		private int landCount;
		private int waterCount;

		private int drawIndex = 0;

		private Pathfinder pathfinder;
		private PathNode lastPath;
		private Cell startCell;
		private Cell endCell;

		public WorldDrawableGameComponent(Game game, VoronoiManager voronoiManager)
			: base(game)
		{
			this.voronoiManager = voronoiManager;
		}

		/// <summary>
		/// Allows the game component to perform any initialization it needs to before starting
		/// to run.  This is where it can query for any required services and load content.
		/// </summary>
		public override void Initialize()
		{
			// TODO: Add your initialization code here

			base.Initialize();
		}

		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);
			sFont = Game.Content.Load<SpriteFont>("Fonts/Arial12");

			base.LoadContent();
		}

		/// <summary>
		/// Allows the game component to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Update(GameTime gameTime)
		{
			// TODO: Add your update code here
			if (Keyboard.GetState().IsKeyDown(Keys.F2) && lastState.IsKeyUp(Keys.F2))
			{
				generate();
			}

			if (Keyboard.GetState().IsKeyDown(Keys.F3) && lastState.IsKeyUp(Keys.F3))
			{
				generateLand();
			}

			if (Keyboard.GetState().IsKeyDown(Keys.F4) && lastState.IsKeyUp(Keys.F4))
			{
				generateElevation();
			}

			if (Keyboard.GetState().IsKeyDown(Keys.C) && lastState.IsKeyUp(Keys.C))
			{
				clear();
			}

			if (Keyboard.GetState().IsKeyDown(Keys.OemPlus) && lastState.IsKeyUp(Keys.OemPlus))
			{
				if (wm != null && wm.VoronoiDiagrams != null)
				{
					drawIndex = (drawIndex + 1) % wm.VoronoiDiagrams.Count;
					createTexture();

					pathfinder = new Pathfinder(wm.VoronoiDiagrams[drawIndex].Cells);
				}
			}

			lastState = Keyboard.GetState();

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			if (texture == null) //No land generated yet
			{
				return;
			}

			spriteBatch.Begin();

			spriteBatch.Draw(texture, Vector2.Zero, Color.White);

			foreach (Cell cell in wm.VoronoiDiagrams[drawIndex].Cells)
			{
				if (cell.ContainsVertex(new Vertex(Mouse.GetState().X, Mouse.GetState().Y)))
				{
					HelperFunctions.PrimitivesBatch.DrawPoint(spriteBatch, Color.Yellow, cell.Vertex.ToVector2(), 3);
					spriteBatch.DrawString(sFont, cell.CellElevationLevel.ToString(), cell.Vertex.ToVector2() - new Vector2(15, 0), Color.Black);


					if (Mouse.GetState().LeftButton == ButtonState.Pressed)
					{
						if (startCell == null)
						{
							startCell = cell;
						}
						else if (endCell == null)
						{
							endCell = cell;
						}
						else
						{
							lastPath = pathfinder.findPath(startCell, endCell);
							startCell = endCell = null;
						}
					}
				}
			}

			if (lastPath != null)
			{
				PathNode path = lastPath;

				while (path.Next != null)
				{
					HelperFunctions.PrimitivesBatch.DrawLine(spriteBatch, Color.White, path.Cell.Vertex.ToVector2(), path.Next.Cell.Vertex.ToVector2());
					path = path.Next;
				}
			}

			HelperFunctions.PrimitivesBatch.DrawPoint(spriteBatch, new Color(0, 0, 0, 192), new Vector2(100, 100), 200);

			spriteBatch.DrawString(sFont, wm.GroupCellsComputeTime.ToString(), Vector2.Zero, Color.Brown);
			spriteBatch.DrawString(sFont, wm.LandComputeTime.ToString(), new Vector2(0, 20), Color.Brown);
			spriteBatch.DrawString(sFont, wm.ElevationComputeTime.ToString(), new Vector2(0, 40), Color.Brown);
			spriteBatch.DrawString(sFont, drawTime.ToString(), new Vector2(0, 60), Color.Brown);

			spriteBatch.DrawString(sFont, "Water Cells: " + waterCount.ToString(), new Vector2(0, 80), Color.Brown);
			spriteBatch.DrawString(sFont, "Land Cells: " + landCount.ToString(), new Vector2(0, 100), Color.Brown);

			spriteBatch.End();

			base.Draw(gameTime);
		}

		public void clear()
		{
			landCount = 0;
			waterCount = 0;

			texture = null;
		}

		private void generateLand()
		{
			if (texture == null)
			{
				wm = new WorldManager(voronoiManager.VoronoiDiagrams);
			}

			clear();
			drawIndex = voronoiManager.VoronoiDiagrams.Count - 1;

			wm.generateLand();

			createTexture();
		}

		private void generateElevation()
		{
			if (texture == null)
			{
				wm = new WorldManager(voronoiManager.VoronoiDiagrams);
			}

			clear();
			drawIndex = voronoiManager.VoronoiDiagrams.Count - 1;

			wm.generateElevation();

			createTexture();
		}

		private void generate()
		{
			if (texture == null)
			{
				wm = new WorldManager(voronoiManager.VoronoiDiagrams);
			}

			clear();
			drawIndex = voronoiManager.VoronoiDiagrams.Count - 1;

			wm.generate();

			createTexture();

			pathfinder = new Pathfinder(wm.VoronoiDiagrams[drawIndex].Cells);
		}

		private void createTexture()
		{
			Stopwatch sw = Stopwatch.StartNew();

			System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
			System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap);
			graphics.Clear(System.Drawing.Color.Transparent);

			foreach (Cell cell in wm.VoronoiDiagrams[drawIndex].Cells)
			{
				System.Drawing.Drawing2D.GraphicsPath gPath = new System.Drawing.Drawing2D.GraphicsPath();
				foreach (HalfEdge he in cell.HalfEdges)
				{
					Vector2 sp = he.StartPoint.ToVector2();
					Vector2 ep = he.EndPoint.ToVector2();

					gPath.AddLine(sp.X, sp.Y, ep.X, ep.Y);
				}
				gPath.CloseFigure();

				System.Drawing.Color color = System.Drawing.Color.Transparent;
				System.Drawing.Color gotoColor = System.Drawing.Color.Transparent;

				float rgbValue = 0;

				switch (cell.CellLandType)
				{
					case CellLandType.Land:
						color = System.Drawing.Color.SaddleBrown;
						gotoColor = System.Drawing.Color.SandyBrown;
						rgbValue = (cell.CellElevationLevel / wm.MaxHeight);
						landCount++;
						break;

					case CellLandType.Water:
						color = System.Drawing.Color.Blue;
						gotoColor = System.Drawing.Color.LightBlue;
						rgbValue = (cell.CellElevationLevel / wm.MaxHeight);
						waterCount++;
						break;

					case CellLandType.Ocean:
						color = System.Drawing.Color.DarkBlue;
						gotoColor = System.Drawing.Color.DarkBlue;
						waterCount++;
						break;
				}

				if (float.IsNaN(cell.CellElevationLevel)) //Prevent 'Undefined' areas from becoming black.
				{
					rgbValue = 0;
				}

				graphics.FillRegion(new System.Drawing.SolidBrush(color.Lerp(gotoColor, rgbValue)), new System.Drawing.Region(gPath));
			}

			graphics.Dispose();

			//Convert Bitmap to XNA texture
			using (System.IO.MemoryStream s = new System.IO.MemoryStream())
			{
				bitmap.Save(s, System.Drawing.Imaging.ImageFormat.Png);
				s.Seek(0, System.IO.SeekOrigin.Begin);
				texture = Texture2D.FromStream(GraphicsDevice, s);
			}

			sw.Stop();
			drawTime = sw.Elapsed;
		}
	}
}
