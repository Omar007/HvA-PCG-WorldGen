﻿using Microsoft.Xna.Framework;
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

		public Vertex(Vector2 vector)
			: this(vector.X, vector.Y)
		{
		}

		public Vector2 ToVector2()
		{
			return new Vector2((float) x, (float) y);
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
	}
}
