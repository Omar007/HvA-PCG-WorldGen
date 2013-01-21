
namespace WorldGen.Pathfinding
{
	public class Heap
	{
		#region Fields
		private uint routeCount;
		private PathNode headNode;
		#endregion

		#region Properties
		public uint RouteCount
		{
			get { return routeCount; }
		}

		public bool HasNext
		{
			get { return headNode != null; }
		}
		#endregion

		public Heap()
		{
			routeCount = 0;
		}

		public void add(PathNode node)
		{
			routeCount++;

			if (headNode == null)
			{
				headNode = node;
			}
			else if (node.FCost < headNode.FCost)
			{
				node.NextListNode = headNode;
				headNode = node;
			}
			else
			{
				PathNode current = headNode;
				while (current.NextListNode != null && current.NextListNode.FCost < node.FCost)
				{
					current = current.NextListNode;
				}
				node.NextListNode = current.NextListNode;
				current.NextListNode = node;
			}
		}

		public PathNode Pop()
		{
			routeCount--;

			PathNode first = headNode;
			headNode = headNode.NextListNode;
			first.NextListNode = null;
			return first;
		}
	}
}
