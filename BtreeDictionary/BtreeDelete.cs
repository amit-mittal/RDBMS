//
// Library: KwData
// File:    BtreeDelete.cs
// Purpose: Define internal BtreeDictionary delete operations.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Kw.Data
{
	public partial class BtreeDictionary<TKey, TValue>
	{
		#region Internal methods

		// Delete element already found at path.
		private void Delete(TreePath<TKey, TValue> path)
		{
			int leafIndex = path.TopNodeIndex;
			Leaf<TKey, TValue> leaf = (Leaf<TKey, TValue>) path.TopNode;

			leaf.Remove(leafIndex);
			--Count;

			if (leafIndex == 0)
				if (leaf.KeyCount != 0)
					path.SetPivot(path.TopNode.GetKey(0));
				else
				{
					Debug.Assert(leaf.RightLeaf == null, "only the rightmost leaf should ever be emptied");

					// Leaf is empty.  Prune it unless it is the only leaf in the tree.
					Leaf<TKey, TValue> leftLeaf = (Leaf<TKey, TValue>) path.GetLeftNode();
					if (leftLeaf != null)
					{
						leftLeaf.RightLeaf = leaf.RightLeaf;
						Demote(path);
					}

					return;
				}

			// Leaf underflow?
			if (leaf.KeyCount < leaf.KeyCapacity/2)
			{
				Leaf<TKey, TValue> rightLeaf = leaf.RightLeaf;
				if (rightLeaf != null)
					if (leaf.KeyCount + rightLeaf.KeyCount > leaf.KeyCapacity)
					{
						// Balance leaves by shifting pairs from right leaf.
						int shifts = leaf.KeyCapacity - (leaf.KeyCount + rightLeaf.KeyCount - 1)/2;
						leaf.Add(rightLeaf, 0, shifts);
						rightLeaf.Remove(0, shifts);
						path.TraverseRight();
						path.SetPivot(path.TopNode.GetKey(0));
					}
					else
					{
						// Coalesce right leaf to current leaf and prune right leaf.
						leaf.Add(rightLeaf, 0, rightLeaf.KeyCount);
						leaf.RightLeaf = rightLeaf.RightLeaf;
						path.TraverseRight();
						Demote(path);
					}
			}
		}


		// Leaf has been emptied, now non-lazy delete its pivot.
		private void Demote(TreePath<TKey, TValue> path)
		{
			for (;;)
			{
				Debug.Assert(path.Height > 0);
				path.Pop();

				Branch<TKey> branch = (Branch<TKey>) path.TopNode;
				if (path.TopNodeIndex == 0)
				{
					if (branch.KeyCount == 0)
						// Cascade when rightmost branch is empty.
						continue;

					// Rotate pivot for first child.
					TKey pivot0 = branch.GetKey(0);
					branch.RemoveKey(0);
					branch.RemoveChild(0);
					path.SetPivot(pivot0);
				}
				else
				{
					// Typical branch pivot delete.
					branch.RemoveKey(path.TopNodeIndex - 1);
					branch.RemoveChild(path.TopNodeIndex);
				}

				Branch<TKey> right = (Branch<TKey>) path.TraverseRight();
				if (right == null)
				{
					if (branch == root && branch.KeyCount == 0)
					{
						// Prune the empty root.
						Branch<TKey> newRoot = branch.FirstChild as Branch<TKey>;
						if (newRoot != null)
						{
							root = (Branch<TKey>) branch.FirstChild;
							--height;
						}
					}
					return;
				}

				if (branch.KeyCount + right.KeyCount < branch.KeyCapacity)
				{
					// Coalesce left: move pivot and right sibling nodes.
					branch.AddKey(path.GetPivot());

					for (int i = 0;; ++i)
					{
						branch.Add(right.GetChild(i));
						if (i >= right.KeyCount)
							break;
						branch.AddKey(right.GetKey(i));
					}

					// Cascade demotion.
					continue;
				}

				// Branch underflow?
				if (branch.KeyCount < branch.KeyCapacity/2)
				{
					int shifts = (right.KeyCount - branch.KeyCount)/2 - 1;

					// Balance branches to keep ratio.  Rotate thru the pivot.
					branch.AddKey(path.GetPivot());

					// Shift pairs from right sibling.
					for (int rightIndex = 0;; ++rightIndex)
					{
						branch.Add(right.GetChild(rightIndex));

						if (rightIndex >= shifts)
							break;

						branch.AddKey(right.GetKey(rightIndex));
					}

					path.SetPivot(right.GetKey(shifts));
					right.Remove(0, shifts + 1);
				}

				return;
			}
		}

		#endregion
	}
}