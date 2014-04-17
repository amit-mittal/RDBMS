//
// Library: KwData
// File:    BtreeInsert.cs
// Purpose: Define internal BtreeDictionary insert operations.
//
// Copyright © 2009-2012 Kasey Osborn (Kasewick@gmail.com)
// Ms-PL - Use and redistribute freely
//

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Kw.Data
{
	public partial class BtreeDictionary<TKey, TValue>
	{
		#region Internal methods

		// Insert element at preseeked path.
		private void Insert(TreePath<TKey, TValue> path, TKey key, TValue value)
		{
			Leaf<TKey, TValue> leaf = (Leaf<TKey, TValue>) path.TopNode;
			int leafIndex = path.TopNodeIndex;

			if (leaf.NotFull)
			{
				leaf.Insert(leafIndex, key, value);
				++Count;
				return;
			}

			// Leaf overflow.  Right split a new leaf.
			Leaf<TKey, TValue> newLeaf = new Leaf<TKey, TValue>(leaf);

			if (newLeaf.RightLeaf == null && leafIndex == leaf.KeyCount)
				// Densify sequential loads.
				newLeaf.Add(key, value);
			else
			{
				int halfway = leaf.KeyCount/2 + 1;

				if (leafIndex < halfway)
				{
					// Left-side insert: Copy right side to the split leaf.
					newLeaf.Add(leaf, halfway - 1, leaf.KeyCount);
					leaf.Truncate(halfway - 1);
					leaf.Insert(leafIndex, key, value);
				}
				else
				{
					// Right-side insert: Copy split leaf parts and new key.
					newLeaf.Add(leaf, halfway, leafIndex);
					newLeaf.Add(key, value);
					newLeaf.Add(leaf, leafIndex, leaf.KeyCount);
					leaf.Truncate(halfway);
				}
			}

			// Promote anchor of split leaf.
			++Count;
			Promote(path, newLeaf.GetKey(0), (Node<TKey>) newLeaf);
		}


		// Leaf has been split so insert the new anchor into a branch.
		private void Promote(TreePath<TKey, TValue> path, TKey key, Node<TKey> newNode)
		{
			for (;;)
			{
				if (path.Height == 1)
				{
					Debug.Assert(root == path.TopNode);

					// Graft new root.
					root = new Branch<TKey>(path.TopNode, Order);
					root.Add(key, newNode);
					++height;
					break;
				}

				path.Pop();
				Branch<TKey> branch = (Branch<TKey>) path.TopNode;
				int branchIndex = path.TopNodeIndex;

				if (branch.NotFull)
				{
					// Typical case where branch has room.
					branch.InsertKey(branchIndex, key);
					branch.Insert(branchIndex + 1, newNode);
					break;
				}

				// Right split an overflowing branch.
				Branch<TKey> newBranch = new Branch<TKey>(branch);
				int halfway = (branch.KeyCount + 1)/2;

				if (branchIndex < halfway)
				{
					// Split with left-side insert.
					for (int i = halfway;; ++i)
					{
						if (i >= branch.KeyCount)
						{
							newBranch.Add(branch.GetChild(i));
							break;
						}
						newBranch.Add(branch.GetKey(i), branch.GetChild(i));
					}

					TKey newPromotion = branch.GetKey(halfway - 1);
					branch.Truncate(halfway - 1);
					branch.InsertKey(branchIndex, key);
					branch.Insert(branchIndex + 1, newNode);
					key = newPromotion;
				}
				else
				{
					// Split branch with right-side insert (or cascade promote).
					int moveIndex = halfway;

					if (branchIndex > halfway)
					{
						for (;;)
						{
							++moveIndex;
							newBranch.Add(branch.GetChild(moveIndex));
							if (moveIndex >= branchIndex)
								break;
							newBranch.AddKey(branch.GetKey(moveIndex));
						}
						newBranch.AddKey(key);
						key = branch.GetKey(halfway);
					}

					newBranch.Add(newNode);

					while (moveIndex < branch.KeyCount)
					{
						newBranch.AddKey(branch.GetKey(moveIndex));
						++moveIndex;
						newBranch.Add(branch.GetChild(moveIndex));
					}

					branch.Truncate(halfway);
				}

				newNode = newBranch;
			}
		}

		#endregion
	}
}