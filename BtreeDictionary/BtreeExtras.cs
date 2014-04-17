//
// Library: KwData
// File:    BtreeExtras.cs
// Purpose: Define methods that do not have corresponding definitions in SortedDictionary.
//

using System;
using System.Collections.Generic;

namespace Kw.Data
{
	public partial class BtreeDictionary<TKey, TValue>
	{
		/// <summary>
		/// Get the Last key/value pair without performing a full structure scan.
		/// </summary>
		/// <returns>Key/value pair with largest key in dictionary</returns>
		public KeyValuePair<TKey, TValue> Last()
		{
			if (Count == 0)
				throw new InvalidOperationException("Sequence contains no elements");

			// Take rightmost child until no more.
			for (Node<TKey> node = root;;)
			{
				Branch<TKey> branch = node as Branch<TKey>;
				if (branch == null)
					return new KeyValuePair<TKey, TValue>(node.GetKey(node.KeyCount - 1),
						((Leaf<TKey, TValue>) node).GetValue(node.KeyCount - 1));

				node = branch.GetChild(node.KeyCount);
			}
		}


		/// <summary>
		/// This iterator provides range query support with ordered results.
		/// </summary>
		/// <param name="key">Minimum value of range.</param>
		/// <returns>An enumerator for the collection for key values greater than or equal to <em>key</em>.</returns>
		public IEnumerable<KeyValuePair<TKey, TValue>> SkipUntilKey(TKey key)
		{
			int index;
			Leaf<TKey, TValue> leaf = Find(key, out index);

			// When the supplied start key is not be found, start with the next highest key.
			if (index < 0)
				index = ~index;

			for (;;)
			{
				if (index < leaf.KeyCount)
				{
					yield return leaf.GetPair(index);
					++index;
					continue;
				}

				leaf = leaf.RightLeaf;
				if (leaf == null)
					yield break;

				index = 0;
			}
		}


		/// <summary>
		/// This iterator provides range query support.
		/// </summary>
		/// <param name="startKey">Minimum inclusive key value of range.</param>
		/// <param name="endKey">Maximum inclusive key value of range.</param>
		/// <returns>An enumerator for all key/value pairs between startKey and endKey.</returns>
		/// <remarks>
		/// Neither <em>startKey</em> or <em>endKey</em> need to be present in the collection.
		/// </remarks>
		/// <example>
		/// <code source="BtreeExamples\BtreeExample03\BtreeExample03.cs" lang="cs" />
		/// </example>
		public IEnumerable<KeyValuePair<TKey, TValue>> BetweenKeys(TKey startKey, TKey endKey)
		{
			int index;
			Leaf<TKey, TValue> leaf = Find(startKey, out index);

			// When the supplied start key is not be found, start with the next highest key.
			if (index < 0)
				index = ~index;

			for (;;)
			{
				if (index < leaf.KeyCount)
				{
					if (leaf.GetKey(index).CompareTo(endKey) > 0)
						yield break;

					yield return leaf.GetPair(index);
					++index;
					continue;
				}

				leaf = leaf.RightLeaf;
				if (leaf == null)
					yield break;

				index = 0;
			}
		}
	}
}