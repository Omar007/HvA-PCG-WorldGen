using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using WorldGen.HelperFunctions;
using WorldGen.Pathfinding;
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
		private MouseState lastMouseState;

		private Texture2D texture;
		private Texture2D moistureTexture;
		private Texture2D biomeTexture;
		private Texture2D cityTexture;

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

			pathfinder = new Pathfinder();
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
			//if (Keyboard.GetState().IsKeyDown(Keys.F2) && lastState.IsKeyUp(Keys.F2))
			//{
			//	generate();
			//}

			if (Keyboard.GetState().IsKeyDown(Keys.F3) && lastState.IsKeyUp(Keys.F3))
			{
				generateLand();
			}

			if (Keyboard.GetState().IsKeyDown(Keys.F4) && lastState.IsKeyUp(Keys.F4))
			{
				generateElevation();
			}

			if (Keyboard.GetState().IsKeyDown(Keys.F5) && lastState.IsKeyUp(Keys.F5))
			{
				generateMoisture();
			}

			if (Keyboard.GetState().IsKeyDown(Keys.F6) && lastState.IsKeyUp(Keys.F6))
			{
				mapBiomes();
			}

			if (Keyboard.GetState().IsKeyDown(Keys.F7) && lastState.IsKeyUp(Keys.F7))
			{
				generateCities();
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
				}
			}

			lastState = Keyboard.GetState();
			lastMouseState = Mouse.GetState();

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			if (texture == null) //No land generated yet
			{
				return;
			}

			spriteBatch.Begin();

			if (drawIndex != wm.VoronoiDiagrams.Count - 1 || (Keyboard.GetState().IsKeyDown(Keys.L) || (Keyboard.GetState().IsKeyUp(Keys.M) && Keyboard.GetState().IsKeyUp(Keys.B))))
			{
				spriteBatch.Draw(texture, Vector2.Zero, Color.White);
			}

			if (drawIndex == wm.VoronoiDiagrams.Count - 1)
			{
				if (biomeTexture != null && Keyboard.GetState().IsKeyDown(Keys.B))
				{
					spriteBatch.Draw(biomeTexture, Vector2.Zero, Color.White);
				}

				if (moistureTexture != null && Keyboard.GetState().IsKeyDown(Keys.M))
				{
					spriteBatch.Draw(moistureTexture, Vector2.Zero, Color.White);
				}

				if (cityTexture != null && (Keyboard.GetState().IsKeyDown(Keys.R) || (Keyboard.GetState().IsKeyUp(Keys.M) && Keyboard.GetState().IsKeyUp(Keys.B) && Keyboard.GetState().IsKeyUp(Keys.L))))
				{
					spriteBatch.Draw(cityTexture, Vector2.Zero, Color.White);
				}
			}

			if (startCell != null)
			{
				HelperFunctions.PrimitivesBatch.DrawPoint(spriteBatch, Color.Yellow, startCell.Vertex.ToVector2(), 3);
				spriteBatch.DrawString(sFont, startCell.ElevationLevel.ToString(), startCell.Vertex.ToVector2() - new Vector2(15, 0), Color.Black);
				spriteBatch.DrawString(sFont, startCell.MoistureLevel.ToString(), startCell.Vertex.ToVector2() - new Vector2(15, 20), Color.Black);
			}

			if (endCell != null)
			{
				HelperFunctions.PrimitivesBatch.DrawPoint(spriteBatch, Color.Yellow, endCell.Vertex.ToVector2(), 3);
				spriteBatch.DrawString(sFont, endCell.ElevationLevel.ToString(), endCell.Vertex.ToVector2() - new Vector2(15, 0), Color.Black);
				spriteBatch.DrawString(sFont, endCell.MoistureLevel.ToString(), endCell.Vertex.ToVector2() - new Vector2(15, 20), Color.Black);
			}

			foreach (Cell cell in wm.VoronoiDiagrams[drawIndex].Cells)
			{
				if (cell.ContainsVertex(new Vertex(Mouse.GetState().X, Mouse.GetState().Y)))
				{
					HelperFunctions.PrimitivesBatch.DrawPoint(spriteBatch, Color.Yellow, cell.Vertex.ToVector2(), 3);
					spriteBatch.DrawString(sFont, cell.ElevationLevel.ToString(), cell.Vertex.ToVector2() - new Vector2(15, 0), Color.Black);
					spriteBatch.DrawString(sFont, cell.MoistureLevel.ToString(), cell.Vertex.ToVector2() - new Vector2(15, 20), Color.Black);

					if (cell.LandType == CellLandType.Land
						&& (Mouse.GetState().LeftButton == ButtonState.Pressed
						&& lastMouseState.LeftButton == ButtonState.Released))
					{
						if (lastPath != null)
						{
							lastPath = null;
							startCell = endCell = null;
						}

						if (startCell == null)
						{
							startCell = cell;
						}
						else if (endCell == null && cell != startCell)
						{
							endCell = cell;
						}
						else if (startCell != null && endCell != null)
						{
							lastPath = pathfinder.findPath(startCell, endCell);

							if (lastPath == null)
							{
								startCell = endCell = null;
							}
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
			spriteBatch.DrawString(sFont, wm.MoistureComputeTime.ToString(), new Vector2(0, 60), Color.Brown);
			spriteBatch.DrawString(sFont, wm.BiomeComputeTime.ToString(), new Vector2(0, 80), Color.Brown);
			spriteBatch.DrawString(sFont, wm.CityComputeTime.ToString(), new Vector2(0, 100), Color.Brown);

			spriteBatch.DrawString(sFont, "Water Cells: " + waterCount.ToString(), new Vector2(0, 120), Color.Brown);
			spriteBatch.DrawString(sFont, "Land Cells: " + landCount.ToString(), new Vector2(0, 140), Color.Brown);

			spriteBatch.DrawString(sFont, "Wind Dir: ", new Vector2(0, 160), Color.Brown);
			Vector2 windDirVector = wm.WindDirection.ToVector2();
			spriteBatch.DrawString(sFont, windDirVector.LengthSquared().ToString(), new Vector2(0, 180), Color.Brown);
			Vector2 start = new Vector2(100, 180);
			windDirVector.Normalize();
			Vector2 end = windDirVector * 20;
			HelperFunctions.PrimitivesBatch.DrawLine(spriteBatch, Color.White, start, start + end);

			spriteBatch.End();

			base.Draw(gameTime);
		}

		public void clear()
		{
			landCount = 0;
			waterCount = 0;

			texture = null;
			moistureTexture = null;
			biomeTexture = null;
			cityTexture = null;
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

				//We need land to generate elevation.
				generateLand();
			}

			clear();
			drawIndex = voronoiManager.VoronoiDiagrams.Count - 1;

			wm.generateElevation();

			createTexture();
		}

		private void generateMoisture()
		{
			if (texture == null)
			{
				wm = new WorldManager(voronoiManager.VoronoiDiagrams);

				//We need land and elevation to generate moisture.
				generateLand();
				generateElevation();
			}

			moistureTexture = null;
			biomeTexture = null;
			cityTexture = null;

			drawIndex = voronoiManager.VoronoiDiagrams.Count - 1;

			wm.generateMoisture();

			createMoistureTexture();
		}

		private void mapBiomes()
		{
			if (texture == null)
			{
				wm = new WorldManager(voronoiManager.VoronoiDiagrams);

				//We need land, moisture and elevation to generate biomes.
				generateLand();
				generateElevation();
				generateMoisture();
			}

			biomeTexture = null;
			//cityTexture = null;

			drawIndex = voronoiManager.VoronoiDiagrams.Count - 1;

			wm.mapBiomes();

			createBiomeTexture();
		}

		private void generateCities()
		{
			if (texture == null)
			{
				wm = new WorldManager(voronoiManager.VoronoiDiagrams);

				//We need land, moisture, elevation and biomes to generate cities.
				generateLand();
				generateElevation();
				generateMoisture();
				//mapBiomes();
			}

			cityTexture = null;

			drawIndex = voronoiManager.VoronoiDiagrams.Count - 1;

			wm.generateCities();

			createCityTexture();
		}

		public void generate()
		{
			if (texture == null)
			{
				wm = new WorldManager(voronoiManager.VoronoiDiagrams);
			}

			clear();
			drawIndex = voronoiManager.VoronoiDiagrams.Count - 1;

			wm.generate();

			createTexture();
			createMoistureTexture();
			createBiomeTexture();
			createCityTexture();
		}

		private void createTexture()
		{
			System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
			System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap);
			graphics.Clear(System.Drawing.Color.Transparent);

			foreach (Cell cell in wm.VoronoiDiagrams[drawIndex].Cells)
			{
				System.Drawing.Drawing2D.GraphicsPath gPath = new System.Drawing.Drawing2D.GraphicsPath();
				foreach (HalfEdge he in cell.HalfEdges)
				{
					Vertex sp = he.StartPoint;
					Vertex ep = he.EndPoint;

					gPath.AddLine((float)sp.X, (float)sp.Y, (float)ep.X, (float)ep.Y);
				}
				gPath.CloseFigure();

				System.Drawing.Color baseColor = System.Drawing.Color.Transparent;
				System.Drawing.Color terrainColor = System.Drawing.Color.Transparent;

				float terrainRGBValue = 0;

				switch (cell.LandType)
				{
					case CellLandType.Land:
						baseColor = System.Drawing.Color.SaddleBrown;
						terrainColor = System.Drawing.Color.SandyBrown;
						terrainRGBValue = (cell.ElevationLevel / wm.MaxHeight);
						landCount++;
						break;

					case CellLandType.Water:
						baseColor = System.Drawing.Color.Blue;
						terrainColor = System.Drawing.Color.LightBlue;
						terrainRGBValue = (cell.ElevationLevel / wm.MaxHeight);
						waterCount++;
						break;

					case CellLandType.Ocean:
						baseColor = System.Drawing.Color.DarkBlue;
						terrainColor = System.Drawing.Color.DarkBlue;
						waterCount++;
						break;
				}

				if (float.IsNaN(terrainRGBValue))
				{
					terrainRGBValue = 0;
				}

				graphics.FillRegion(new System.Drawing.SolidBrush(baseColor.Lerp(terrainColor, terrainRGBValue)), new System.Drawing.Region(gPath));

				if (drawIndex != wm.VoronoiDiagrams.Count - 1)
				{
					graphics.DrawPath(System.Drawing.Pens.Black, gPath);
				}
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

		private void createMoistureTexture()
		{
			if (drawIndex != voronoiManager.VoronoiDiagrams.Count - 1)
			{
				return;
			}

			System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
			System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap);
			graphics.Clear(System.Drawing.Color.Transparent);

			foreach (Cell cell in wm.VoronoiDiagrams[drawIndex].Cells)
			{
				//Skip water as that is always 100% moisture
				if (cell.LandType != CellLandType.Land)
				{
					continue;
				}

				System.Drawing.Drawing2D.GraphicsPath gPath = new System.Drawing.Drawing2D.GraphicsPath();
				foreach (HalfEdge he in cell.HalfEdges)
				{
					Vertex sp = he.StartPoint;
					Vertex ep = he.EndPoint;

					gPath.AddLine((float)sp.X, (float)sp.Y, (float)ep.X, (float)ep.Y);
				}
				gPath.CloseFigure();

				System.Drawing.Color baseColor = System.Drawing.Color.FromArgb(16, System.Drawing.Color.Blue);
				System.Drawing.Color moistureColor = System.Drawing.Color.FromArgb(128, System.Drawing.Color.Blue);

				float moistureRGBValue = Math.Max(0, ((float)cell.MoistureLevel / (float)short.MaxValue));
				System.Drawing.Color finalColor = baseColor.Lerp(moistureColor, moistureRGBValue);

				if (moistureRGBValue == 0)
				{
					finalColor = System.Drawing.Color.Transparent;
				}

				graphics.FillRegion(new System.Drawing.SolidBrush(finalColor), new System.Drawing.Region(gPath));
			}

			graphics.Dispose();

			//Convert Bitmap to XNA texture
			using (System.IO.MemoryStream s = new System.IO.MemoryStream())
			{
				bitmap.Save(s, System.Drawing.Imaging.ImageFormat.Png);
				s.Seek(0, System.IO.SeekOrigin.Begin);
				moistureTexture = Texture2D.FromStream(GraphicsDevice, s);
			}
		}

		private void createBiomeTexture()
		{
			if (drawIndex != voronoiManager.VoronoiDiagrams.Count - 1)
			{
				return;
			}

			System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
			System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap);
			graphics.Clear(System.Drawing.Color.Transparent);

			foreach (Biome biome in wm.Biomes)
			{
				foreach (Cell cell in biome.CellsInBiome)
				{
					System.Drawing.Drawing2D.GraphicsPath gPath = new System.Drawing.Drawing2D.GraphicsPath();
					foreach (HalfEdge he in cell.HalfEdges)
					{
						Vertex sp = he.StartPoint;
						Vertex ep = he.EndPoint;

						gPath.AddLine((float)sp.X, (float)sp.Y, (float)ep.X, (float)ep.Y);
					}
					gPath.CloseFigure();

					graphics.FillRegion(new System.Drawing.SolidBrush(biome.Color), new System.Drawing.Region(gPath));
				}
			}

			graphics.Dispose();

			//Convert Bitmap to XNA texture
			using (System.IO.MemoryStream s = new System.IO.MemoryStream())
			{
				bitmap.Save(s, System.Drawing.Imaging.ImageFormat.Png);
				s.Seek(0, System.IO.SeekOrigin.Begin);
				biomeTexture = Texture2D.FromStream(GraphicsDevice, s);
			}
		}

		private void createCityTexture()
		{
			if (drawIndex != voronoiManager.VoronoiDiagrams.Count - 1)
			{
				return;
			}

			System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
			System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap);
			graphics.Clear(System.Drawing.Color.Transparent);

			System.Drawing.SolidBrush cityBrush = new System.Drawing.SolidBrush(City.Color);

			foreach (City city in wm.Cities)
			{
				foreach (Cell cell in city.Cells)
				{
					System.Drawing.Drawing2D.GraphicsPath gPath = new System.Drawing.Drawing2D.GraphicsPath();
					foreach (HalfEdge he in cell.HalfEdges)
					{
						Vertex sp = he.StartPoint;
						Vertex ep = he.EndPoint;

						gPath.AddLine((float)sp.X, (float)sp.Y, (float)ep.X, (float)ep.Y);
					}
					gPath.CloseFigure();

					graphics.FillRegion(cityBrush, new System.Drawing.Region(gPath));
				}
			}

			//Seperate loop to draw roads always on top of every city!
			foreach (City city in wm.Cities)
			{
				foreach (PathNode route in city.RoutesToCities.Values)
				{
					PathNode path = route;

					while (path.Next != null)
					{
						graphics.DrawLine(System.Drawing.Pens.White, (float)path.Cell.Vertex.X, (float)path.Cell.Vertex.Y, (float)path.Next.Cell.Vertex.X, (float)path.Next.Cell.Vertex.Y);
						path = path.Next;
					}
				}
			}

			graphics.Dispose();

			//Convert Bitmap to XNA texture
			using (System.IO.MemoryStream s = new System.IO.MemoryStream())
			{
				bitmap.Save(s, System.Drawing.Imaging.ImageFormat.Png);
				s.Seek(0, System.IO.SeekOrigin.Begin);
				cityTexture = Texture2D.FromStream(GraphicsDevice, s);
			}
		}
	}
}
