//
// Library: KwData
// File:    BtreeNodes.cs
// Purpose: Define internal tree structure and its basic operations.
//
// Copyright © 2009-2012 Kasey Osborn (Kasewick@gmail.com)
// Ms-PL - Use and redistribute freely
//

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Kw.Data
{
	/// <summary>Common functionality of a branch and leaf - a list of ordered keys.
	/// </summary>
	/// <typeparam name="TKey">Type of ordered field.</typeparam>
	/// <remarks>In a leaf, this is a contiguous block of keys from the dictionary.
	/// In a branch, this contains the first keys that are contained within every
	/// leaf, except the leftmost leaf.</remarks>
	[Serializable]
	internal abstract partial class Node<TKey>
	{
		internal List<TKey> keys { get; private set; }

		protected Node(int newOrder)
		{
			keys = new List<TKey>(newOrder - 1);
		}

		internal int KeyCount
		{
			get { return keys.Count; }
		}

		internal int KeyCapacity
		{
			get { return keys.Capacity; }
		}

		internal bool NotFull
		{
			get { return keys.Count < keys.Capacity; }
		}

		internal void AddKey(TKey key)
		{
			keys.Add(key);
		}

		internal TKey GetKey(int i)
		{
			return keys[i];
		}

		internal int Search(TKey key)
		{
			return keys.BinarySearch(key);
		}

		internal int Search(TKey key, IComparer<TKey> c)
		{
			return keys.BinarySearch(key, c);
		}

		internal void SetKey(int i, TKey key)
		{
			keys[i] = key;
		}

		internal void RemoveKey(int i)
		{
			keys.RemoveAt(i);
		}

		internal void RemoveKeys(int i, int count)
		{
			keys.RemoveRange(i, count);
		}

		internal void TruncateKeys(int i)
		{
			keys.RemoveRange(i, keys.Count - i);
		}

		internal void InsertKey(int i, TKey newKey)
		{
			keys.Insert(i, newKey);
		}
	}


	/// <summary>Any page internal to the tree. Provides subtree functionality.
	/// </summary>
	/// <typeparam name="TKey">Type of ordered field.</typeparam>
	[Serializable]
	internal class Branch<TKey> : Node<TKey>
	{
		private List<Node<TKey>> childNodes;

		internal Branch(Branch<TKey> leftBranch)
			: base(leftBranch.ChildCount)
		{
			Init(leftBranch.ChildCount);
		}

		internal Branch(Node<TKey> child, int newOrder)
			: base(newOrder)
		{
			Init(newOrder);
			Add(child);
		}

		private void Init(int newOrder)
		{
			childNodes = new List<Node<TKey>>(newOrder);
		}

		internal int ChildCount
		{
			get { return childNodes.Count; }
		}

		internal Node<TKey> GetChild(int i)
		{
			return childNodes[i];
		}

		internal Node<TKey> FirstChild
		{
			get { return childNodes[0]; }
		}

		internal void RemoveChild(int i)
		{
			childNodes.RemoveAt(i);
		}

		internal void Truncate(int index)
		{
			TruncateKeys(index);
			childNodes.RemoveRange(index + 1, childNodes.Count - (index + 1));
		}

		internal void Add(Node<TKey> newBlock)
		{
			childNodes.Add(newBlock);
		}

		internal void Add(TKey newKey, Node<TKey> newBlock)
		{
			AddKey(newKey);
			childNodes.Add(newBlock);
		}

		internal void Insert(int index, Node<TKey> newItem)
		{
			childNodes.Insert(index, newItem);
		}

		internal void Remove(int index, int count)
		{
			RemoveKeys(index, count);
			childNodes.RemoveRange(index, count);
		}
	}


	/// <summary>Terminal node giving the value for each key in the keys list.
	/// </summary>
	/// <typeparam name="TKey">Type of ordered field.</typeparam>
	/// <typeparam name="TValue">Type of field associated with TKey.</typeparam>
	[Serializable]
	internal class Leaf<TKey, TValue> : Node<TKey>
	{
		private Leaf<TKey, TValue> rightLeaf; // For the linked leaf list.
		private List<TValue> values; // Payload.

		internal Leaf(int newOrder)
			: base(newOrder)
		{
			values = new List<TValue>(newOrder - 1);
			rightLeaf = null;
		}

		/// <summary>Splice a leaf to right of <paramref name="leftLeaf"/>.</summary>
		/// <param name="leftLeaf">Provides linked list insert point.</param>
		internal Leaf(Leaf<TKey, TValue> leftLeaf)
			: base(leftLeaf.KeyCapacity + 1)
		{
			values = new List<TValue>(leftLeaf.KeyCapacity);

			// Linked list insertion.
			rightLeaf = leftLeaf.rightLeaf;
			leftLeaf.rightLeaf = this;
		}


		/// <summary>Give next leaf in linked list.</summary>
		internal Leaf<TKey, TValue> RightLeaf
		{
			get { return rightLeaf; }
			set { rightLeaf = value; }
		}


		internal int ValueCount
		{
			get { return values.Count; }
		}


		internal KeyValuePair<TKey, TValue> GetPair(int i)
		{
			return new KeyValuePair<TKey, TValue>(keys[i], values[i]);
		}


		internal TValue GetValue(int i)
		{
			return values[i];
		}


		internal void SetValue(int i, TValue newValue)
		{
			values[i] = newValue;
		}


		internal void Add(TKey key, TValue value)
		{
			AddKey(key);
			values.Add(value);
		}

		internal void Add(Leaf<TKey, TValue> source, int sourceStart, int sourceStop)
		{
			for (int i = sourceStart; i < sourceStop; ++i)
				Add(source.GetKey(i), source.GetValue(i));
		}

		internal void Insert(int index, TKey key, TValue value)
		{
			Debug.Assert(index >= 0 && index <= ValueCount);
			InsertKey(index, key);
			values.Insert(index, value);
		}

		internal void Remove(int index)
		{
			Debug.Assert(index >= 0 && index <= ValueCount);
			RemoveKey(index);
			values.RemoveAt(index);
		}

		internal void Remove(int index, int count)
		{
			Debug.Assert(index >= 0 && index + count <= ValueCount);
			RemoveKeys(index, count);
			values.RemoveRange(index, count);
		}

		internal void Truncate(int index)
		{
			Debug.Assert(index >= 0 && index < ValueCount);
			RemoveKeys(index, KeyCount - index);
			values.RemoveRange(index, ValueCount - index);
		}
	}
}