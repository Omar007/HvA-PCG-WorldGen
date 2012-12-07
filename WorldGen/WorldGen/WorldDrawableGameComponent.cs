using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Linq;
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
		private TimeSpan computeTime;

		private LandGenerator landGenerator;
		private int landCount;
		private int waterCount;

		private Texture2D texture;

		public WorldDrawableGameComponent(Game game)
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

			base.Initialize();
		}

		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);
			sFont = Game.Content.Load<SpriteFont>("Fonts/Arial12");

			landGenerator = new LandGenerator();

			base.LoadContent();
		}

		/// <summary>
		/// Allows the game component to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Update(GameTime gameTime)
		{
			// TODO: Add your update code here
			if (Keyboard.GetState().IsKeyDown(Keys.C) && lastState.IsKeyUp(Keys.C))
			{
				clear();
			}

			lastState = Keyboard.GetState();

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			if (texture == null || landGenerator.WorldCells == null) //No land generated yet
			{
				return;
			}

			spriteBatch.Begin();

			spriteBatch.Draw(texture, Vector2.Zero, Color.White);

			foreach (Cell cell in landGenerator.WorldCells)
			{
				HelperFunctions.PrimitivesBatch.DrawPoint(spriteBatch, cell.CellLandType == CellLandType.Water ? Color.LightBlue : Color.SandyBrown,
					cell.Vertex.ToVector2(), 10);
			}

			spriteBatch.DrawString(sFont, computeTime.ToString(), new Vector2(0, 100), Color.Brown);

			spriteBatch.DrawString(sFont, "Water Cells: " + waterCount.ToString(), new Vector2(0, 120), Color.Brown);
			spriteBatch.DrawString(sFont, "Land Cells: " + landCount.ToString(), new Vector2(0, 140), Color.Brown);

			spriteBatch.End();

			base.Draw(gameTime);
		}

		public void clear()
		{
			texture = null;
		}

		public void generate(VoronoiCore vc)
		{
			clear();

			Stopwatch sw = Stopwatch.StartNew();
			landGenerator.generate(vc, true);
			sw.Stop();
			computeTime = sw.Elapsed;

			landCount = landGenerator.WorldCells.Count(x => x.CellLandType == CellLandType.Land);
			waterCount = landGenerator.WorldCells.Count - landCount;


			System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
			System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap);
			graphics.Clear(System.Drawing.Color.Transparent);

			foreach (Cell cell in landGenerator.WorldCells)
			{
				System.Drawing.Drawing2D.GraphicsPath gPath = new System.Drawing.Drawing2D.GraphicsPath();
				foreach (HalfEdge he in cell.HalfEdges)
				{
					Vector2 sp = he.StartPoint.ToVector2();
					Vector2 ep = he.EndPoint.ToVector2();

					gPath.AddLine(sp.X, sp.Y, ep.X, ep.Y);
				}
				gPath.CloseFigure();

				graphics.FillRegion(cell.CellLandType == CellLandType.Land ? System.Drawing.Brushes.LightBlue : System.Drawing.Brushes.SandyBrown,
					new System.Drawing.Region(gPath));
			}

			graphics.Dispose();

			System.Drawing.Imaging.BitmapData bData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
			byte[] bytes = new byte[bData.Height * bData.Stride];
			System.Runtime.InteropServices.Marshal.Copy(bData.Scan0, bytes, 0, bytes.Length);
			texture = new Texture2D(GraphicsDevice, bitmap.Width, bitmap.Height);
			texture.SetData(bytes);
		}
	}
}
