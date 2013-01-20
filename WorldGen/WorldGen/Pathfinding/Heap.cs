
namespace WorldGen.Pathfinding
{
	public class Heap
	{
		private PathNode headNode;

		public bool HasNext
		{
			get { return headNode != null; }
		}

		public Heap()
		{
		}

		public void add(PathNode node)
		{
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
			PathNode first = headNode;
			headNode = headNode.Next;
			//first.NextListNode = null;
			return first;
		}
	}
}
