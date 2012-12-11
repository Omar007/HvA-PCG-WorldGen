
namespace WorldGen.Voronoi
{
	public class RBTree
	{
		private RBNode root;

		public RBNode Root
		{
			get { return root; }
			set { root = value; }
		}

		public RBTree()
		{
			root = null;
		}

		public void insertSuccessor(RBNode node, RBNode successor)
		{
			RBNode parent = null;

			if (node != null)
			{
				successor.Previous = node;
				successor.Next = node.Next;

				if (node.Next != null)
				{
					node.Next.Previous = successor;
				}
				node.Next = successor;

				if (node.Right != null)
				{
					node = node.Right;

					while (node.Left != null)
					{
						node = node.Left;
					}

					node.Left = successor;
				}
				else
				{
					node.Right = successor;
				}

				parent = node;
			}
			else if (root != null)
			{
				node = getFirst(root);
				successor.Previous = null;
				successor.Next = node;
				node.Previous = successor;
				node.Left = successor;
				parent = node;
			}
			else
			{
				successor.Previous = null;
				successor.Next = null;
				root = successor;
				parent = null;
			}

			successor.Left = null;
			successor.Right = null;
			successor.Parent = parent;
			successor.IsRed = true;


			RBNode grandpa = null;
			RBNode uncle = null;

			node = successor;

			while (parent != null && parent.IsRed)
			{
				grandpa = parent.Parent;

				if (parent == grandpa.Left)
				{
					uncle = grandpa.Right;

					if (uncle != null && uncle.IsRed)
					{
						parent.IsRed = false;
						uncle.IsRed = false;
						grandpa.IsRed = true;
						node = grandpa;
					}
					else
					{
						if (node == parent.Right)
						{
							rotateLeft(parent);
							node = parent;
							parent = node.Parent;
						}

						parent.IsRed = false;
						grandpa.IsRed = true;

						rotateRight(grandpa);
					}
				}
				else
				{
					uncle = grandpa.Left;

					if (uncle != null && uncle.IsRed)
					{
						parent.IsRed = false;
						uncle.IsRed = false;
						grandpa.IsRed = true;
						node = grandpa;
					}
					else
					{
						if (node == parent.Left)
						{
							rotateRight(parent);
							node = parent;
							parent = node.Parent;
						}

						parent.IsRed = false;
						grandpa.IsRed = true;

						rotateLeft(grandpa);
					}
				}

				parent = node.Parent;
			}

			root.IsRed = false;
		}

		public void removeNode(RBNode node)
		{
			if (node.Next != null)
			{
				node.Next.Previous = node.Previous;
			}

			if (node.Previous != null)
			{
				node.Previous.Next = node.Next;
			}

			node.Previous = null;
			node.Next = null;

			RBNode parent = node.Parent;
			RBNode left = node.Left;
			RBNode right = node.Right;
			RBNode next = null;

			if (left == null)
			{
				next = right;
			}
			else if (right == null)
			{
				next = left;
			}
			else
			{
				next = getFirst(right);
			}

			if (parent != null)
			{
				if (parent.Left == node)
				{
					parent.Left = next;
				}
				else
				{
					parent.Right = next;
				}
			}
			else
			{
				root = next;
			}


			bool isRed = false;

			if (left != null && right != null)
			{
				isRed = next.IsRed;
				next.IsRed = node.IsRed;
				next.Left = left;
				left.Parent = next;

				if (next != right)
				{
					parent = next.Parent;
					next.Parent = node.Parent;
					node = next.Right;
					parent.Left = node;
					next.Right = right;
					right.Parent = next;
				}
				else
				{
					next.Parent = parent;
					parent = next;
					node = next.Right;
				}
			}
			else
			{
				isRed = node.IsRed;
				node = next;
			}

			if (node != null)
			{
				node.Parent = parent;
			}

			if (isRed)
			{
				return;
			}

			if (node != null && node.IsRed)
			{
				node.IsRed = false;
				return;
			}

			RBNode sibling;

			do
			{
				if (node == root)
				{
					break;
				}

				if (node == parent.Left)
				{
					sibling = parent.Right;

					if (sibling.IsRed)
					{
						sibling.IsRed = false;
						parent.IsRed = true;

						rotateLeft(parent);
						sibling = parent.Right;
					}

					if ((sibling.Left != null && sibling.Left.IsRed) || (sibling.Right != null && sibling.Right.IsRed))
					{
						if (sibling.Right == null || !sibling.Right.IsRed)
						{
							sibling.Left.IsRed = false;
							sibling.IsRed = true;

							rotateRight(sibling);

							sibling = parent.Right;
						}

						sibling.IsRed = parent.IsRed;
						parent.IsRed = false;
						sibling.Right.IsRed = false;

						rotateLeft(parent);

						node = root;
						break;
					}
				}
				else
				{
					sibling = parent.Left;

					if (sibling.IsRed)
					{
						sibling.IsRed = false;
						parent.IsRed = true;

						rotateRight(parent);
						sibling = parent.Left;
					}

					if ((sibling.Left != null && sibling.Left.IsRed) || (sibling.Right != null && sibling.Right.IsRed))
					{
						if (sibling.Left == null || !sibling.Left.IsRed)
						{
							sibling.Right.IsRed = false;
							sibling.IsRed = true;

							rotateLeft(sibling);

							sibling = parent.Left;
						}

						sibling.IsRed = parent.IsRed;
						parent.IsRed = false;
						sibling.Left.IsRed = false;

						rotateRight(parent);

						node = root;
						break;
					}
				}

				sibling.IsRed = true;
				node = parent;
				parent = parent.Parent;
			}
			while (!node.IsRed);

			if (node != null)
			{
				node.IsRed = false;
			}
		}

		private void rotate(RBNode node, RBNode q)
		{
			RBNode parent = node.Parent;

			if (parent != null)
			{
				if (parent.Left == node)
				{
					parent.Left = q;
				}
				else
				{
					parent.Right = q;
				}
			}
			else
			{
				root = q;
			}

			q.Parent = parent;
			node.Parent = q;
		}

		private void rotateLeft(RBNode node)
		{
			RBNode q = node.Right;

			rotate(node, q);
			
			node.Right = q.Left;

			if (node.Right != null)
			{
				node.Right.Parent = node;
			}

			q.Left = node;
		}

		private void rotateRight(RBNode node)
		{
			RBNode q = node.Left;

			rotate(node, q);
			
			node.Left = q.Right;

			if (node.Left != null)
			{
				node.Left.Parent = node;
			}

			q.Right = node;
		}

		public RBNode getFirst(RBNode node)
		{
			while (node.Left != null)
			{
				node = node.Left;
			}
			return node;
		}

		public RBNode getLast(RBNode node)
		{
			while (node.Right != null)
			{
				node = node.Right;
			}
			return node;
		}
	}
}
