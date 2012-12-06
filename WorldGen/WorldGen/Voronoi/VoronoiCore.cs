using System;
using System.Collections.Generic;

namespace WorldGen.Voronoi
{
	public class VoronoiCore
	{
		/// <summary>
		/// EPSILON is: 1e-9
		/// </summary>
		private const double EPSILON = 1e-9;

		#region Fields
		private RBTree beachLine;
		private RBTree circleEvents;

		private Stack<BeachSection> beachSectionJunkyard;
		private Stack<CircleEvent> circleEventJunkyard;

		private CircleEvent firstCircleEvent;

		private List<Edge> edges;
		private List<Cell> cells;
		#endregion

		#region Properties
		public List<Edge> Edges
		{
			get { return edges; }
		}

		public List<Cell> Cells
		{
			get { return cells; }
		}
		#endregion

		public VoronoiCore()
		{
			beachLine = new RBTree();
			circleEvents = new RBTree();

			beachSectionJunkyard = new Stack<BeachSection>();
			circleEventJunkyard = new Stack<CircleEvent>();
			
			edges = new List<Edge>();
			cells = new List<Cell>();
		}

		#region EPSILON Comparisons
		/// <summary>
		/// Math.Abs(a - b) &lt; EPSILON
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		private bool equalWithEpsilon(double a, double b)
		{
			return Math.Abs(a - b) < EPSILON;
		}

		/// <summary>
		/// a - b &gt; EPSILON
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		private bool greaterThanEpsilon(double a, double b)
		{
			return a - b > EPSILON;
		}

		/// <summary>
		/// b - a &lt; EPSILON
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		private bool greaterThanOrEqualWithEpsilon(double a, double b)
		{
			return b - a < EPSILON;
		}

		/// <summary>
		/// b - a &gt; EPSILON
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		private bool lessThanEpsilon(double a, double b)
		{
			return b - a > EPSILON;
		}

		/// <summary>
		/// a - b &lt; EPSILON
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		private bool lessThanOrEqualWithEpsilon(double a, double b)
		{
			return a - b < EPSILON;
		}
		#endregion

		public void reset()
		{
			if (beachLine.Root != null)
			{
				BeachSection beachSection = (BeachSection) beachLine.getFirst(beachLine.Root);

				while (beachSection != null)
				{
					beachSectionJunkyard.Push(beachSection);
					beachSection = (BeachSection) beachSection.Next;
				}

				beachLine.Root = null;
			}

			circleEvents.Root = null;
			firstCircleEvent = null;

			edges.Clear();
			cells.Clear();
		}

		private Edge createEdge(Cell leftCell, Cell rightCell, Vertex va, Vertex vb)
		{
			Edge edge = new Edge(leftCell, rightCell);

			edges.Add(edge);

			if (va != null)
			{
				setEdgeStartPoint(edge, leftCell, rightCell, va);
			}

			if (vb != null)
			{
				setEdgeEndPoint(edge, leftCell, rightCell, vb);
			}

			cells[leftCell.VoronoiID].HalfEdges.Add(new HalfEdge(edge, leftCell, rightCell));
			cells[rightCell.VoronoiID].HalfEdges.Add(new HalfEdge(edge, rightCell, leftCell));

			return edge;
		}

		private Edge createBorderEdge(Cell leftCell, Vertex va, Vertex vb)
		{
			Edge edge = new Edge(leftCell, null)
			{
				VertexA = va,
				VertexB = vb
			};

			edges.Add(edge);

			return edge;
		}

		private void setEdgeStartPoint(Edge edge, Cell leftCell, Cell rightCell, Vertex v)
		{
			if (edge.VertexA == null && edge.VertexB == null)
			{
				edge.VertexA = v;
				edge.LeftCell = leftCell;
				edge.RightCell = rightCell;
			}
			else if (edge.LeftCell == rightCell)
			{
				edge.VertexB = v;
			}
			else
			{
				edge.VertexA = v;
			}
		}

		private void setEdgeEndPoint(Edge edge, Cell leftCell, Cell rightCell, Vertex v)
		{
			setEdgeStartPoint(edge, rightCell, leftCell, v);
		}

		private BeachSection createBeachSection(Cell cell)
		{
			BeachSection beachSection = beachSectionJunkyard.Count > 0 ? beachSectionJunkyard.Pop() : new BeachSection();

			beachSection.Cell = cell;

			return beachSection;
		}

		private double leftBreakPoint(BeachSection arc, double directrix)
		{
			Cell cell = arc.Cell;
			double rfocx = cell.X;
			double rfocy = cell.Y;
			double pby2 = rfocy - directrix;

			if (pby2 == 0)
			{
				return rfocx;
			}

			BeachSection lArc = (BeachSection) arc.Previous;

			if (lArc == null)
			{
				return double.NegativeInfinity;
			}

			cell = lArc.Cell;
			double lfocx = cell.X;
			double lfocy = cell.Y;
			double plby2 = lfocy - directrix;

			if (plby2 == 0)
			{
				return lfocx;
			}

			double hl = lfocx - rfocx;
			double aby2 = 1 / pby2 - 1 / plby2;
			double b = hl / plby2;

			if (aby2 != 0)
			{
				return (-b + Math.Sqrt(b * b - 2 * aby2 * (hl * hl / (-2 * plby2) - lfocy + plby2 / 2 + rfocy - pby2 / 2))) / aby2 + rfocx;
			}

			return (rfocx + lfocx) / 2;
		}

		private double rightBreakPoint(BeachSection arc, double directrix)
		{
			BeachSection rArc = (BeachSection) arc.Next;

			if (rArc != null)
			{
				return leftBreakPoint(rArc, directrix);
			}

			Cell cell = arc.Cell;
			return cell.Y == directrix ? cell.X : double.PositiveInfinity;
		}

		private void detachBeachSection(BeachSection beachSection)
		{
			detachCircleEvent(beachSection);
			beachLine.removeNode(beachSection);
			beachSectionJunkyard.Push(beachSection);
		}

		private void removeBeachSection(BeachSection beachSection)
		{
			CircleEvent circle = beachSection.CircleEvent;
			double x = circle.X;
			double y = circle.YCenter;
			Vertex v = new Vertex(x, y);

			BeachSection previous = (BeachSection) beachSection.Previous;
			BeachSection next = (BeachSection) beachSection.Next;
			List<BeachSection> disappearingTransitions = new List<BeachSection>();
			disappearingTransitions.Add(beachSection);

			detachBeachSection(beachSection);

			BeachSection lArc = previous;

			while (lArc.CircleEvent != null && equalWithEpsilon(x, lArc.CircleEvent.X) && equalWithEpsilon(y, lArc.CircleEvent.YCenter))
			{
				previous = (BeachSection) lArc.Previous;
				disappearingTransitions.Insert(0, lArc);
				detachBeachSection(lArc);
				lArc = previous;
			}

			disappearingTransitions.Insert(0, lArc);
			detachCircleEvent(lArc);


			BeachSection rArc = next;

			while (rArc.CircleEvent != null && equalWithEpsilon(x, rArc.CircleEvent.X) && equalWithEpsilon(y, rArc.CircleEvent.YCenter))
			{
				next = (BeachSection) rArc.Next;
				disappearingTransitions.Add(rArc);
				detachBeachSection(rArc);
				rArc = next;
			}

			disappearingTransitions.Add(rArc);
			detachCircleEvent(rArc);


			int nArcs = disappearingTransitions.Count;

			for (int i = 1; i < nArcs; i++)
			{
				rArc = disappearingTransitions[i];
				lArc = disappearingTransitions[i - 1];

				setEdgeStartPoint(rArc.Edge, lArc.Cell, rArc.Cell, v);
			}


			lArc = disappearingTransitions[0];
			rArc = disappearingTransitions[nArcs - 1];
			rArc.Edge = createEdge(lArc.Cell, rArc.Cell, null, v);

			attachCircleEvent(lArc);
			attachCircleEvent(rArc);
		}

		private void addBeachSection(Cell cell)
		{
			double x = cell.X;
			double directrix = cell.Y;

			BeachSection lArc = null;
			BeachSection rArc = null;

			BeachSection node = (BeachSection) beachLine.Root;

			while (node != null)
			{
				double dxl = leftBreakPoint(node, directrix) - x;

				if (dxl > EPSILON)
				{
					node = (BeachSection) node.Left;
				}
				else
				{
					double dxr = x - rightBreakPoint(node, directrix);

					if (dxr > EPSILON)
					{
						if (node.Right == null)
						{
							lArc = node;
							break;
						}
						node = (BeachSection) node.Right;
					}
					else
					{
						if (dxl > -EPSILON)
						{
							lArc = (BeachSection) node.Previous;
							rArc = node;
						}
						else if (dxr > -EPSILON)
						{
							lArc = node;
							rArc = (BeachSection) node.Next;
						}
						else
						{
							lArc = node;
							rArc = node;
						}
						break;
					}
				}
			}

			BeachSection newArc = createBeachSection(cell);
			beachLine.insertSuccessor(lArc, newArc);

			if (lArc == null && rArc == null)
			{
				return;
			}

			if (lArc == rArc)
			{
				detachCircleEvent(lArc);

				rArc = createBeachSection(lArc.Cell);
				beachLine.insertSuccessor(newArc, rArc);

				newArc.Edge = rArc.Edge = createEdge(lArc.Cell, newArc.Cell, null, null);

				attachCircleEvent(lArc);
				attachCircleEvent(rArc);

				return;
			}

			if (lArc != null && rArc == null)
			{
				newArc.Edge = createEdge(lArc.Cell, newArc.Cell, null, null);

				return;
			}

			if (lArc != rArc)
			{
				detachCircleEvent(lArc);
				detachCircleEvent(rArc);

				Cell leftCell = lArc.Cell;
				double ax = leftCell.X;
				double ay = leftCell.Y;
				double bx = cell.X - ax;
				double by = cell.Y - ay;

				Cell rightCell = rArc.Cell;
				double cx = rightCell.X - ax;
				double cy = rightCell.Y - ay;
				double d = 2 * (bx * cy - by * cx);
				double hb = bx * bx + by * by;
				double hc = cx * cx + cy * cy;

				Vertex v = new Vertex((cy * hb - by * hc) / d + ax, (bx * hc - cx * hb) / d + ay);

				setEdgeStartPoint(rArc.Edge, leftCell, rightCell, v);

				newArc.Edge = createEdge(leftCell, cell, null, v);
				rArc.Edge = createEdge(cell, rightCell, null, v);

				attachCircleEvent(lArc);
				attachCircleEvent(rArc);

				return;
			}
		}

		private void attachCircleEvent(BeachSection arc)
		{
			BeachSection lArc = (BeachSection) arc.Previous;
			BeachSection rArc = (BeachSection) arc.Next;

			if (lArc == null || rArc == null)
			{
				return;
			}

			Cell leftCell = lArc.Cell;
			Cell centerCell = arc.Cell;
			Cell rightCell = rArc.Cell;

			if (leftCell == rightCell)
			{
				return;
			}

			double bx = centerCell.X;
			double by = centerCell.Y;
			double ax = leftCell.X - bx;
			double ay = leftCell.Y - by;
			double cx = rightCell.X - bx;
			double cy = rightCell.Y - by;

			double d = 2 * (ax * cy - ay * cx);

			if (d >= -2e-12)
			{
				return;
			}

			double ha = ax * ax + ay * ay;
			double hc = cx * cx + cy * cy;
			double x = (cy * ha - ay * hc) / d;
			double y = (ax * hc - cx * ha) / d;
			double ycenter = y + by;

			CircleEvent circleEvent = circleEventJunkyard.Count > 0 ? circleEventJunkyard.Pop() : new CircleEvent();

			circleEvent.Arc = arc;
			circleEvent.X = x + bx;
			circleEvent.Y = ycenter + Math.Sqrt(x * x + y * y);
			circleEvent.YCenter = ycenter;
			arc.CircleEvent = circleEvent;

			CircleEvent predecessor = null;
			CircleEvent node = (CircleEvent) circleEvents.Root;

			while (node != null)
			{
				if (circleEvent.Y < node.Y || (circleEvent.Y == node.Y && circleEvent.X <= node.X))
				{
					if (node.Left != null)
					{
						node = (CircleEvent) node.Left;
					}
					else
					{
						predecessor = (CircleEvent) node.Previous;
						break;
					}
				}
				else
				{
					if (node.Right != null)
					{
						node = (CircleEvent) node.Right;
					}
					else
					{
						predecessor = node;
						break;
					}
				}
			}

			circleEvents.insertSuccessor(predecessor, circleEvent);

			if (predecessor == null)
			{
				firstCircleEvent = circleEvent;
			}
		}

		private void detachCircleEvent(BeachSection arc)
		{
			CircleEvent circle = arc.CircleEvent;

			if (circle != null)
			{
				if (circle.Previous == null)
				{
					firstCircleEvent = (CircleEvent) circle.Next;
				}

				circleEvents.removeNode(circle);
				circleEventJunkyard.Push(circle);
				arc.CircleEvent = null;
			}
		}

		private bool connectEdge(Edge edge, Boundary bounds)
		{
			Vertex vb = edge.VertexB;

			if (vb != null)
			{
				return true;
			}

			Vertex va = edge.VertexA;
			int xl = bounds.Left;
			int xr = bounds.Right;
			int yt = bounds.Top;
			int yb = bounds.Bottom;
			Cell leftCell = edge.LeftCell;
			Cell rightCell = edge.RightCell;
			double lx = leftCell.X;
			double ly = leftCell.Y;
			double rx = rightCell.X;
			double ry = rightCell.Y;
			double fx = (lx + rx) / 2;
			double fy = (ly + ry) / 2;

			double fm = double.NaN;
			double fb = 0;

			if (ly != ry)
			{
				fm = (lx - rx) / (ry - ly);
				fb = fy - fm * fx;
			}

			if (double.IsNaN(fm))
			{
				if (fx < xl || fx >= xr)
				{
					return false;
				}

				if (lx > rx)
				{
					if (va == null)
					{
						va = new Vertex(fx, yt);
					}
					else if (va.Y >= yb)
					{
						return false;
					}

					vb = new Vertex(fx, yb);
				}
				else
				{
					if (va == null)
					{
						va = new Vertex(fx, yb);
					}
					else if (va.Y < yt)
					{
						return false;
					}

					vb = new Vertex(fx, yt);
				}
			}
			else if (fm < -1 || fm > 1)
			{
				if (lx > rx)
				{
					if (va == null)
					{
						va = new Vertex((yt - fb) / fm, yt);
					}
					else if (va.Y >= yb)
					{
						return false;
					}

					vb = new Vertex((yb - fb) / fm, yb);
				}
				else
				{
					if (va == null)
					{
						va = new Vertex((yb - fb) / fm, yb);
					}
					else if (va.Y < yt)
					{
						return false;
					}

					vb = new Vertex((yt - fb) / fm, yt);
				}
			}
			else
			{
				if (ly < ry)
				{
					if (va == null)
					{
						va = new Vertex(xl, fm * xl + fb);
					}
					else if (va.X >= xr)
					{
						return false;
					}

					vb = new Vertex(xr, fm * xr + fb);
				}
				else
				{
					if (va == null)
					{
						va = new Vertex(xr, fm * xr + fb);
					}
					else if (va.X < xl)
					{
						return false;
					}

					vb = new Vertex(xl, fm * xl + fb);
				}
			}

			edge.VertexA = va;
			edge.VertexB = vb;

			return true;
		}

		private bool clipEdge(Edge edge, Boundary bounds)
		{
			double ax = edge.VertexA.X;
			double ay = edge.VertexA.Y;
			double bx = edge.VertexB.X;
			double by = edge.VertexB.Y;
			double t0 = 0;
			double t1 = 1;
			double dx = bx - ax;
			double dy = by - ay;

			double q = ax - bounds.Left;

			if (dx == 0 && q < 0)
			{
				return false;
			}

			double r = -q / dx;

			if (dx < 0)
			{
				if (r < t0)
				{
					return false;
				}
				else if (r < t1)
				{
					t1 = r;
				}
			}
			else if (dx > 0)
			{
				if (r > t1)
				{
					return false;
				}
				else if (r > t0)
				{
					t0 = r;
				}
			}


			q = bounds.Right - ax;

			if (dx == 0 && q < 0)
			{
				return false;
			}

			r = q / dx;

			if (dx < 0)
			{
				if (r > t1)
				{
					return false;
				}
				else if (r > t0)
				{
					t0 = r;
				}
			}
			else if (dx > 0)
			{
				if (r < t0)
				{
					return false;
				}
				else if (r < t1)
				{
					t1 = r;
				}
			}


			q = ay - bounds.Top;

			if (dx == 0 && q < 0)
			{
				return false;
			}

			r = -q / dy;

			if (dy < 0)
			{
				if (r < t0)
				{
					return false;
				}
				else if (r < t1)
				{
					t1 = r;
				}
			}
			else if (dy > 0)
			{
				if (r > t1)
				{
					return false;
				}
				else if (r > t0)
				{
					t0 = r;
				}
			}


			q = bounds.Bottom - ay;

			if (dx == 0 && q < 0)
			{
				return false;
			}

			r = q / dy;

			if (dy < 0)
			{
				if (r > t1)
				{
					return false;
				}
				else if (r > t0)
				{
					t0 = r;
				}
			}
			else if (dy > 0)
			{
				if (r < t0)
				{
					return false;
				}
				else if (r < t1)
				{
					t1 = r;
				}
			}


			if (t0 > 0)
			{
				edge.VertexA = new Vertex(ax + t0 * dx, ay + t0 * dy);
			}

			if (t1 < 1)
			{
				edge.VertexB = new Vertex(ax + t1 * dx, ay + t1 * dy);
			}

			return true;
		}

		private void clipEdges(Boundary bounds)
		{
			int iEdge = edges.Count;

			while (iEdge-- > 0)
			{
				Edge edge = edges[iEdge];

				if (!connectEdge(edge, bounds) || !clipEdge(edge, bounds) || (Math.Abs(edge.VertexA.X - edge.VertexB.X) < EPSILON && Math.Abs(edge.VertexA.Y - edge.VertexB.Y) < EPSILON))
				{
					edge.VertexA = null;
					edge.VertexB = null;
					edges.RemoveAt(iEdge);
				}
			}
		}

		private void closeCells(Boundary bounds)
		{
			int xl = bounds.Left;
			int xr = bounds.Right;
			int yt = bounds.Top;
			int yb = bounds.Bottom;

			int iCell = cells.Count;

			while (iCell-- > 0)
			{
				Cell cell = cells[iCell];

				if (cell.prepare() == 0)
				{
					continue;
				}

				List<HalfEdge> halfEdges = cell.HalfEdges;
				int nHalfEdges = halfEdges.Count;

				for (int iLeft = 0, iRight = 1 % nHalfEdges; //Initialize 2 indices
					iLeft < nHalfEdges; //Check
					iLeft++, iRight = (iLeft + 1) % nHalfEdges) //Update both indices
				{
					Vertex endPoint = halfEdges[iLeft].EndPoint;
					Vertex startPoint = halfEdges[iRight].StartPoint;

					//HalfEdge crossing bounds somewhere
					if (Math.Abs(endPoint.X - startPoint.X) >= EPSILON || Math.Abs(endPoint.Y - startPoint.Y) >= EPSILON)
					{
						Vertex va = endPoint;
						Vertex vb = null;

						if (equalWithEpsilon(endPoint.X, xl) && lessThanEpsilon(endPoint.Y, yb))
						{
							vb = new Vertex(xl, equalWithEpsilon(startPoint.X, xl) ? startPoint.Y : yb);

							cell.CellType = CellType.WestEdge;
						}
						else if (equalWithEpsilon(endPoint.Y, yb) && lessThanEpsilon(endPoint.X, xr))
						{
							vb = new Vertex(equalWithEpsilon(startPoint.Y, yb) ? startPoint.X : xr, yb);

							cell.CellType = CellType.SouthEdge;
						}
						else if (equalWithEpsilon(endPoint.X, xr) && greaterThanEpsilon(endPoint.Y, yt))
						{
							vb = new Vertex(xr, equalWithEpsilon(startPoint.X, xr) ? startPoint.Y : yt);

							cell.CellType = CellType.EastEdge;
						}
						else if (equalWithEpsilon(endPoint.Y, yt) && greaterThanEpsilon(endPoint.X, xl))
						{
							vb = new Vertex(equalWithEpsilon(startPoint.Y, yt) ? startPoint.X : xl, yt);

							cell.CellType = CellType.NorthEdge;
						}

						if (vb == null)
						{
							Console.WriteLine("VB should be set now! LINE 903");
						}

						Edge edge = createBorderEdge(cell, va, vb);
						halfEdges.Insert(iLeft + 1, new HalfEdge(edge, cell, null));
						nHalfEdges = halfEdges.Count;
					}
				}
			}
		}

		public void compute(List<Vertex> unsortedPoints, Boundary bounds)
		{
			if (unsortedPoints.Count <= 1)
			{
				return;
			}
			unsortedPoints.Sort();
			
			Stack<Vertex> points = new Stack<Vertex>(unsortedPoints);

			Vertex point = points.Pop();
			int pointID = 0;
			double xPointX = double.MinValue;
			double xPointY = double.MinValue;

			while (true)
			{
				CircleEvent circle = firstCircleEvent;

				if (point != null && (circle == null || point.Y < circle.Y || (point.Y == circle.Y && point.X < circle.X)))
				{
					if (point.X != xPointX || point.Y != xPointY)
					{
						Cell cell = new Cell(point) { VoronoiID = pointID++ };
						cells.Add(cell);

						addBeachSection(cell);

						xPointX = point.X;
						xPointY = point.Y;
					}

					point = points.Count > 0 ? points.Pop() : null;
				}
				else if (circle != null)
				{
					removeBeachSection(circle.Arc);
				}
				else
				{
					break;
				}
			}

			clipEdges(bounds);

			closeCells(bounds);
		}
	}
}
