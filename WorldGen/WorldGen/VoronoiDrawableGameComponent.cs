using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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

		private VoronoiManager vManager;

		private int drawIndex = 0;

		public VoronoiManager VoronoiManager
		{
			get { return vManager; }
		}

		public VoronoiDrawableGameComponent(Game game)
			: base(game)
		{
			vManager = new VoronoiManager(1280, 720);
			vManager.generate();

			drawIndex = vManager.VoronoiDiagrams.Count - 1;
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

			HelperFunctions.PrimitivesBatch.Init(GraphicsDevice);
			
			base.LoadContent();
		}

		/// <summary>
		/// Allows the game component to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Update(GameTime gameTime)
		{
			if (Keyboard.GetState().IsKeyDown(Keys.F1) && (lastState.IsKeyUp(Keys.F1) || Keyboard.GetState().IsKeyDown(Keys.LeftAlt)))
			{
				vManager.generate();
				((WorldDrawableGameComponent)Game.Components[1]).clear();

				drawIndex = vManager.VoronoiDiagrams.Count - 1;
			}

			if (Keyboard.GetState().IsKeyDown(Keys.LeftControl) && (lastState.IsKeyUp(Keys.LeftControl) || Keyboard.GetState().IsKeyDown(Keys.LeftAlt)))
			{
				vManager.relax();
				((WorldDrawableGameComponent)Game.Components[1]).clear();
			}

			if (Keyboard.GetState().IsKeyDown(Keys.OemPlus) && lastState.IsKeyUp(Keys.OemPlus))
			{
				if (vManager != null && vManager.VoronoiDiagrams != null)
				{
					drawIndex = (drawIndex + 1) % vManager.VoronoiDiagrams.Count;
				}
			}

			lastState = Keyboard.GetState();

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			spriteBatch.Begin();

			int delaunyEdgeCount = 0;

			foreach (Edge edge in vManager.VoronoiDiagrams[drawIndex].Edges)
			{
				HelperFunctions.PrimitivesBatch.DrawLine(spriteBatch, Color.White, edge.VertexA.ToVector2(), edge.VertexB.ToVector2());

				if (edge.HasDelaunyEdge)
				{
					HelperFunctions.PrimitivesBatch.DrawLine(spriteBatch, Color.Green, edge.DelaunyVertexA.ToVector2(), edge.DelaunyVertexB.ToVector2());

					delaunyEdgeCount++;
				}
			}

			foreach (Cell cell in vManager.VoronoiDiagrams[drawIndex].Cells)
			{
				Color color = Color.Yellow;
				int size = 4;

				switch (cell.CellEdgeType)
				{
					case CellEdgeType.WestEdge:
						color = Color.Red;
						size = 6;
						break;

					case CellEdgeType.EastEdge:
						color = Color.Red;
						size = 6;
						break;

					case CellEdgeType.NorthEdge:
						color = Color.White;
						size = 6;
						break;

					case CellEdgeType.SouthEdge:
						color = Color.White;
						size = 6;
						break;
				}

				HelperFunctions.PrimitivesBatch.DrawPoint(spriteBatch, color, cell.Vertex.ToVector2(), size);
			}

			spriteBatch.DrawString(sFont, vManager.timeFor(vManager.VoronoiDiagrams[drawIndex]).ToString(), Vector2.Zero, Color.Brown);

			HelperFunctions.PrimitivesBatch.DrawRectangle(spriteBatch, Color.Red,
				new Rectangle(vManager.Bounds.Left, vManager.Bounds.Top,
					(vManager.Bounds.Right - vManager.Bounds.Left), (vManager.Bounds.Bottom - vManager.Bounds.Top)));

			spriteBatch.DrawString(sFont, "Points: " + vManager.LastLevelPointCount.ToString(), new Vector2(0, 20), Color.Brown);
			spriteBatch.DrawString(sFont, "Cells: " + vManager.LastLevel.Cells.Count.ToString(), new Vector2(0, 40), Color.Brown);
			spriteBatch.DrawString(sFont, "Edges: " + vManager.LastLevel.Edges.Count.ToString(), new Vector2(0, 60), Color.Brown);
			spriteBatch.DrawString(sFont, "Delauny Edges: " + delaunyEdgeCount.ToString(), new Vector2(0, 80), Color.Brown);

			spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
