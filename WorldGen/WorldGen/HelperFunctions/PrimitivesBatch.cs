#if DEBUG

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WorldGen.HelperFunctions
{
	/// <summary>
	/// Class containing draw functions for basic 2D primitives.
	/// </summary>
	public static class PrimitivesBatch
	{
		private static Texture2D emptyTexture;

		/// <summary>
		/// Initializes the texture used to draw the primitives.
		/// </summary>
		/// <param name="device">GraphicsDevice to use for the texture.</param>
		public static void Init(GraphicsDevice device)
		{
			emptyTexture = new Texture2D(device, 1, 1, false, SurfaceFormat.Color);
			emptyTexture.SetData(new[] { Color.White });
		}

		/// <summary>
		/// Draws a point in the given color with the given size.
		/// </summary>
		/// <param name="batch">The SpriteBatch to draw in.</param>
		/// <param name="color">The color to draw in.</param>
		/// <param name="point">Position of the point.</param>
		/// <param name="pointSize">Size of the point.</param>
		public static void DrawPoint(SpriteBatch batch, Color color, Vector2 point, int pointSize)
		{
			DrawPoint(batch, color, point, pointSize, 0);
		}

		/// <summary>
		/// Draws a point in the given color with the given size on the given layer.
		/// </summary>
		/// <param name="batch">The SpriteBatch to draw in.</param>
		/// <param name="color">The color to draw in.</param>
		/// <param name="point">Position of the point.</param>
		/// <param name="pointSize">Size of the point.</param>
		/// <param name="layer">The depth of a layer. By default, 0 represents the front layer and 1 represents a back layer.</param>
		public static void DrawPoint(SpriteBatch batch, Color color, Vector2 point, int pointSize, float layer)
		{
			batch.Draw(emptyTexture, point, null, color, 0, new Vector2(0.5f), new Vector2(pointSize), SpriteEffects.None, layer);
		}

		/// <summary>
		/// Draws a line in the given color.
		/// </summary>
		/// <param name="batch">The SpriteBatch to draw in.</param>
		/// <param name="color">The color to draw in.</param>
		/// <param name="start">Starting point.</param>
		/// <param name="end">End point.</param>
		public static void DrawLine(SpriteBatch batch, Color color, Vector2 start, Vector2 end)
		{
			DrawLine(batch, color, start, end, 0);
		}

		/// <summary>
		/// Draws a line in the given color.
		/// </summary>
		/// <param name="batch">The SpriteBatch to draw in.</param>
		/// <param name="color">The color to draw in.</param>
		/// <param name="start">Starting point.</param>
		/// <param name="end">End point.</param>
		/// <param name="layer">The depth of a layer. By default, 0 represents the front layer and 1 represents a back layer.</param>
		public static void DrawLine(SpriteBatch batch, Color color, Vector2 start, Vector2 end, float layer)
		{
			Vector2 directionVector = end - start;
			float angle = (float)Math.Atan2(directionVector.Y, directionVector.X);
			float length = directionVector.Length();

			batch.Draw(emptyTexture, start, null, color, angle, Vector2.Zero, new Vector2(length, 1), SpriteEffects.None, layer);
		}

		/// <summary>
		/// Draws a rectangle using the given color.
		/// </summary>
		/// <param name="batch">The SpriteBatch to draw in.</param>
		/// <param name="color">The color to draw in.</param>
		/// <param name="rect">The Rectangle to draw.</param>
		public static void DrawRectangle(SpriteBatch batch, Color color, Rectangle rect)
		{
			DrawRectangle(batch, color, rect, 0);
		}

		/// <summary>
		/// Draws a rectangle using the given color.
		/// </summary>
		/// <param name="batch">The SpriteBatch to draw in.</param>
		/// <param name="color">The color to draw in.</param>
		/// <param name="rect">The Rectangle to draw.</param>
		/// <param name="layer">The depth of a layer. By default, 0 represents the front layer and 1 represents a back layer.</param>
		public static void DrawRectangle(SpriteBatch batch, Color color, Rectangle rect, int layer)
		{
			Vector2 topLeft = new Vector2(rect.Left, rect.Top);
			Vector2 topRight = new Vector2(rect.Right, rect.Top);
			Vector2 bottomLeft = new Vector2(rect.Left, rect.Bottom);
			Vector2 bottomRight = new Vector2(rect.Right, rect.Bottom);

			DrawLine(batch, color, topLeft, topRight, layer);
			DrawLine(batch, color, topRight, bottomRight, layer);
			DrawLine(batch, color, bottomLeft, bottomRight, layer);
			DrawLine(batch, color, topLeft, bottomLeft, layer);
		}

		/// <summary>
		/// Draws a triangle in the given color.
		/// </summary>
		/// <param name="batch">The SpriteBatch to draw in.</param>
		/// <param name="color">The color to draw in.</param>
		/// <param name="point0">Point 1.</param>
		/// <param name="point1">Point 2.</param>
		/// <param name="point2">Point 3.</param>
		public static void DrawTriangle(SpriteBatch batch, Color color, Vector2 point0, Vector2 point1, Vector2 point2)
		{
			DrawTriangle(batch, color, point0, point1, point2, 0);
		}

		/// <summary>
		/// Draws a triangle in the given color.
		/// </summary>
		/// <param name="batch">The SpriteBatch to draw in.</param>
		/// <param name="color">The color to draw in.</param>
		/// <param name="point0">Point 1.</param>
		/// <param name="point1">Point 2.</param>
		/// <param name="point2">Point 3.</param>
		/// <param name="layer">The depth of a layer. By default, 0 represents the front layer and 1 represents a back layer.</param>
		public static void DrawTriangle(SpriteBatch batch, Color color, Vector2 point0, Vector2 point1, Vector2 point2, int layer)
		{
			DrawLine(batch, color, point0, point1, layer);
			DrawLine(batch, color, point1, point2, layer);
			DrawLine(batch, color, point2, point0, layer);
		}

		/// <summary>
		/// Draws a circle at the given point with the given radius and color.
		/// </summary>
		/// <param name="batch">The SpriteBatch to draw in.</param>
		/// <param name="color">The color to draw in.</param>
		/// <param name="center">Position of the circle.</param>
		/// <param name="radius">Radius of the circle.</param>
		public static void DrawCircle(SpriteBatch batch, Color color, Vector2 center, int radius)
		{
			DrawCircle(batch, color, center, radius, 0);
		}

		/// <summary>
		/// Draws a circle at the given point with the given radius and color.
		/// </summary>
		/// <param name="batch">The SpriteBatch to draw in.</param>
		/// <param name="color">The color to draw in.</param>
		/// <param name="center">Position of the circle.</param>
		/// <param name="radius">Radius of the circle.</param>
		/// <param name="layer">The depth of a layer. By default, 0 represents the front layer and 1 represents a back layer.</param>
		public static void DrawCircle(SpriteBatch batch, Color color, Vector2 center, int radius, int layer)
		{
			float thetaStep = 0.1F;
			float theta = 0;

			Vector2 PointToDraw;
			while (theta < Math.PI * 2)
			{
				PointToDraw = new Vector2() { X = (float)(radius * Math.Cos(theta)), Y = (float)(radius * Math.Sin(theta)) } + center;
				batch.Draw(emptyTexture, PointToDraw, Color.White);
				theta += thetaStep;
			}
		}
	}
}

#endif