using Microsoft.Xna.Framework;
using System;

namespace WorldGen.Voronoi
{
	public class HalfEdge : IComparable<HalfEdge>
	{
		#region Fields
		private Edge edge;
		private Site site;

		private double angle;
		#endregion

		#region Properties
		public Edge Edge
		{
			get { return edge; }
		}

		public Site Site
		{
			get { return site; }
		}

		public Vertex StartPoint
		{
			get { return edge.LeftSite == site ? edge.VertexA : edge.VertexB; }
		}

		public Vertex EndPoint
		{
			get { return edge.LeftSite == site ? edge.VertexB : edge.VertexA; }
		}
		#endregion

		public HalfEdge(Edge edge, Site lSite, Site rSite)
		{
			this.edge = edge;

			this.site = lSite;

			if (rSite != null)
			{
				this.angle = Math.Atan2(rSite.Y - lSite.Y, rSite.X - lSite.X);
			}
			else
			{
				Vertex va = edge.VertexA;
				Vertex vb = edge.VertexB;

				this.angle = edge.LeftSite == lSite ? Math.Atan2(vb.X - va.X, va.Y - vb.Y) : Math.Atan2(va.X - vb.X, vb.Y - va.Y);
			}
		}

		public int CompareTo(HalfEdge other)
		{
			return (int)Math.Round((other.angle - angle) * 1000000);
		}
	}
}
