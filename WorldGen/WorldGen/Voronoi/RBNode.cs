
namespace WorldGen.Voronoi
{
	public abstract class RBNode
	{
		#region Fields
		private RBNode previous;
		private RBNode next;

		private RBNode left;
		private RBNode right;

		private RBNode parent;

		private bool red;
		#endregion

		#region Properties
		public RBNode Previous
		{
			get { return previous; }
			set { previous = value; }
		}

		public RBNode Next
		{
			get { return next; }
			set { next = value; }
		}

		public RBNode Left
		{
			get { return left; }
			set { left = value; }
		}

		public RBNode Right
		{
			get { return right; }
			set { right = value; }
		}

		public RBNode Parent
		{
			get { return parent; }
			set { parent = value; }
		}

		public bool IsRed
		{
			get { return red; }
			set { red = value; }
		}
		#endregion

		public RBNode()
		{
			previous = null;
			next = null;

			left = null;
			right = null;

			parent = null;

			red = false;
		}
	}
}
