using Microsoft.Xna.Framework;
using System;

namespace WorldGen.Voronoi
{
	public class Vertex : IComparable<Vertex>
	{
		private double x;
		private double y;

		#region Properties
		public double X
		{
			get { return x; }
			set { x = value; }
		}

		public double Y
		{
			get { return y; }
			set { y = value; }
		}
		#endregion

		public Vertex(double x, double y)
		{
			this.x = x;
			this.y = y;
		}

		public Vertex(Vertex vertex)
			: this(vertex.x, vertex.y)
		{
		}

		public Vertex(Vector2 vector)
			: this(vector.X, vector.Y)
		{
		}

		public double DistanceTo(Vertex other)
		{
			double dx = x - other.x;
			double dy = y - other.y;

			return dx * dx + dy * dy;
		}

		public Vector2 ToVector2()
		{
			return new Vector2((float) x, (float) y);
		}

		public Vector2 ToVector2Normalized()
		{
			Vector2 normalizedVector = new Vector2((float)x, (float)y);
			normalizedVector.Normalize();
			return normalizedVector;
		}

		public int CompareTo(Vertex other)
		{
			double r = other.Y - Y;

			if (r != 0)
			{
				return (int)Math.Round(r * 1000000);
			}

			return (int)Math.Round((other.X - X) * 1000000);
		}

		public static Vertex operator-(Vertex v1, Vertex v2)
		{
			return new Vertex(v1.X - v2.X, v1.Y - v2.Y);
		}

		public static Vertex operator +(Vertex v1, Vertex v2)
		{
			return new Vertex(v1.X + v2.X, v1.Y + v2.Y);
		}
	}
}
