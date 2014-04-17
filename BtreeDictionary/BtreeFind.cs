//
// Library: KwData
// File:    BtreeFind.cs
// Purpose: Define BtreeDictionary seek functions without a TreePath.
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

		/// <summary>Get the leftmost leaf.</summary>
		/// <remarks>Used by iteration.</remarks>
		internal Leaf<TKey, TValue> GetFirstLeaf()
		{
			for (Node<TKey> node = root;; node = ((Branch<TKey>) node).FirstChild)
			{
				Leaf<TKey, TValue> leaf = node as Leaf<TKey, TValue>;
				if (leaf != null)
					return leaf;
			}
		}


		/// <summary>Perform lite search for key.</summary>
		/// <param name="key">Target of search.</param>
		/// <param name="index">When found, holds index of returned Leaf; else ~index of nearest greater key.</param>
		/// <returns>Leaf holding target (found or not).</returns>
		internal Leaf<TKey, TValue> Find(TKey key, out int index)
		{
			//  Method is unfolded on comparer to improve speed 5%.
			if (comparer == Comparer<TKey>.Default)
				for (Node<TKey> node = root;;)
				{
					int nodeIndex = node.Search(key);

					Branch<TKey> branch = node as Branch<TKey>;
					if (branch == null)
					{
						index = nodeIndex;
						return (Leaf<TKey, TValue>) node;
					}

					node = branch.GetChild(nodeIndex < 0 ? ~nodeIndex : nodeIndex + 1);
				}
			else
			{
				for (Node<TKey> node = root;;)
				{
					int nodeIndex = node.Search(key, comparer);

					Branch<TKey> branch = node as Branch<TKey>;
					if (branch == null)
					{
						index = nodeIndex;
						return (Leaf<TKey, TValue>) node;
					}

					node = branch.GetChild(nodeIndex < 0 ? ~nodeIndex : nodeIndex + 1);
				}
			}
		}

		#endregion
	}
}