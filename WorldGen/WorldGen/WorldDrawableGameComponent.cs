using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using WorldGen.HelperFunctions;
using WorldGen.Voronoi;
using WorldGen.WorldGenerator;

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

		private TimeSpan landComputeTime;
		private TimeSpan elevationComputeTime;

		private VoronoiCore vc;
		private LandGenerator landGenerator;
		private ElevationGenerator elevationGenerator;
		private int landCount;
		private int waterCount;

		private Texture2D texture;

		public WorldDrawableGameComponent(Game game, VoronoiCore vc)
			: base(game)
		{
			this.vc = vc;
			vc.OnVoronoiChanged += () => clear();
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

			landGenerator = new LandGenerator();
			elevationGenerator = new ElevationGenerator(6, 1);

			base.LoadContent();
		}

		/// <summary>
		/// Allows the game component to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Update(GameTime gameTime)
		{
			// TODO: Add your update code here
			if (Keyboard.GetState().IsKeyDown(Keys.F2) && (lastState.IsKeyUp(Keys.F2) || Keyboard.GetState().IsKeyDown(Keys.LeftAlt)))
			{
				generate();
			}

			if (Keyboard.GetState().IsKeyDown(Keys.C) && lastState.IsKeyUp(Keys.C))
			{
				clear();
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

			foreach (Cell cell in vc.Cells)
			{
				HelperFunctions.PrimitivesBatch.DrawPoint(spriteBatch, Color.Black, cell.Vertex.ToVector2(), 5);
				spriteBatch.DrawString(sFont, cell.CellElevationLevel.ToString(), cell.Vertex.ToVector2(), Color.Black);
			}

			spriteBatch.DrawString(sFont, landComputeTime.ToString(), new Vector2(0, 100), Color.Brown);
			spriteBatch.DrawString(sFont, elevationComputeTime.ToString(), new Vector2(0, 120), Color.Brown);

			spriteBatch.DrawString(sFont, "Water Cells: " + waterCount.ToString(), new Vector2(0, 140), Color.Brown);
			spriteBatch.DrawString(sFont, "Land Cells: " + landCount.ToString(), new Vector2(0, 160), Color.Brown);

			spriteBatch.End();

			base.Draw(gameTime);
		}

		public void clear()
		{
			landCount = 0;
			waterCount = 0;

			texture = null;
		}

		public void generate()
		{
			clear();

			Stopwatch sw = Stopwatch.StartNew();
			landGenerator.generate(vc, true);
			sw.Stop();
			landComputeTime = sw.Elapsed;

			sw.Restart();
			elevationGenerator.generate(vc, true);
			sw.Stop();
			elevationComputeTime = sw.Elapsed;

			System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
			System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap);
			graphics.Clear(System.Drawing.Color.Transparent);

			foreach (Cell cell in vc.Cells)
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
						rgbValue = (cell.CellElevationLevel / elevationGenerator.MaxHeight);
						landCount++;
						break;

					case CellLandType.Water:
						color = System.Drawing.Color.Blue;
						gotoColor = System.Drawing.Color.LightBlue;
						rgbValue = (cell.CellElevationLevel / elevationGenerator.MaxHeight);
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
		}
	}
}
