//
// Library: KwData
// File     BtreePath.cs
// Purpose: Defines internal class that stores a element location path.
//
// Copyright © 2009-2012 Kasey Osborn (Kasewick@gmail.com)
// Ms-PL - Use and redistribute freely
//

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Kw.Data
{
	/// <summary>Stack trace from root to leaf of a key/value pair.</summary>
	/// <remarks>Performs search function for key. Provides directions to existing key
	/// or insertion point for non-existing key.
	/// </remarks>
	/// <typeparam name="TKey">Key type.</typeparam>
	/// <typeparam name="TValue">Value type.</typeparam>
	[Serializable]
	internal partial class TreePath<TKey, TValue>
		where TKey : IComparable
	{
		private List<int> indexStack;
		private List<Node<TKey>> nodeStack;

		/// <summary>
		/// <b>true</b> if leaf key is an exact match; otherwise <b>false</b>.</summary>
		internal bool IsFound { get; private set; }

		#region Constructors

		/// <summary>Perform search and store each level of path on the stack.</summary>
		/// <param name="tree">Tree to search.</param>
		/// <param name="key">Value to find.</param>
		internal TreePath(BtreeDictionary<TKey, TValue> tree, TKey key)
		{
			indexStack = new List<int>();
			nodeStack = new List<Node<TKey>>();

			Node<TKey> node = tree.root;

			for (;;)
			{
				Debug.Assert(node != null);

				nodeStack.Add(node);
				int i = node.Search(key, tree.comparer);

				if (node is Leaf<TKey, TValue>)
				{
					IsFound = i >= 0;
					if (!IsFound)
						i = ~i;
					indexStack.Add(i);
					return;
				}

				if (i < 0)
					i = ~i;
				else
					++i;

				indexStack.Add(i);
				node = ((Branch<TKey>) node).GetChild(i);
			}
		}

		#endregion

		#region Properties

		internal Node<TKey> TopNode
		{
			get { return nodeStack[indexStack.Count - 1]; }
		}

		internal int TopNodeIndex
		{
			get { return indexStack[indexStack.Count - 1]; }
		}

		internal int Height
		{
			get { return indexStack.Count; }
		}


		internal TValue LeafValue
		{
			get
			{
				int leafIndex = indexStack.Count - 1;
				return ((Leaf<TKey, TValue>) nodeStack[leafIndex]).GetValue(indexStack[leafIndex]);
			}
			set
			{
				int leafIndex = indexStack.Count - 1;
				((Leaf<TKey, TValue>) nodeStack[leafIndex]).SetValue(indexStack[leafIndex], value);
			}
		}


		/// <summary>
		/// Get the node to the immediate left of the node at TreePath.
		/// </summary>
		internal Node<TKey> GetLeftNode()
		{
			Debug.Assert(indexStack.Count == nodeStack.Count);

			for (int depth = indexStack.Count - 2; depth >= 0; --depth)
			{
				if (indexStack[depth] > 0)
				{
					Node<TKey> result = ((Branch<TKey>) nodeStack[depth]).GetChild(indexStack[depth] - 1);
					for (; depth < indexStack.Count - 2; ++depth)
						result = ((Branch<TKey>) result).GetChild(result.KeyCount);
					return result;
				}
			}
			return null;
		}


		/// <summary>Get nearest key where left child path taken.</summary>
		/// <remarks>On entry, top of path refers to a branch.</remarks>
		internal TKey GetPivot()
		{
			Debug.Assert(TopNode is Branch<TKey>);
			for (int depth = indexStack.Count - 2; depth >= 0; --depth)
			{
				if (indexStack[depth] > 0)
					return nodeStack[depth].GetKey(indexStack[depth] - 1);
			}

			Debug.Fail("no left pivot");
			return default (TKey);
		}


		/// <summary>Set nearest key where left child path taken.</summary>
		/// <remarks>On entry, top of path refers to a branch.</remarks>
		internal void SetPivot(TKey newPivot)
		{
			for (int depth = indexStack.Count - 2; depth >= 0; --depth)
				if (indexStack[depth] > 0)
				{
					nodeStack[depth].SetKey(indexStack[depth] - 1, newPivot);
					return;
				}
		}

		#endregion

		#region Methods

		internal void Clear()
		{
			indexStack.Clear();
			nodeStack.Clear();
		}

		internal void Pop()
		{
			nodeStack.RemoveAt(nodeStack.Count - 1);
			indexStack.RemoveAt(indexStack.Count - 1);
		}

		internal void Push(Node<TKey> newNode, int newNodeIndex)
		{
			nodeStack.Add(newNode);
			indexStack.Add(newNodeIndex);
		}


		/// <summary>Adjust tree path to node to the right.</summary>
		/// <returns>Node to immediate right of current path; <b>null</b> if current path
		/// at rightmost node.</returns>
		internal Node<TKey> TraverseRight()
		{
			Node<TKey> node = null;
			int height = indexStack.Count;
			for (;;)
			{
				if (indexStack.Count < 2)
				{
					Clear();
					node = null;
					break;
				}

				Pop();
				node = TopNode;
				int newIndex = TopNodeIndex + 1;

				if (newIndex < ((Branch<TKey>) node).ChildCount)
				{
					indexStack[indexStack.Count - 1] = newIndex;
					node = ((Branch<TKey>) node).GetChild(newIndex);
					for (;;)
					{
						Push(node, 0);
						if (indexStack.Count >= height)
							break;
						node = ((Branch<TKey>) node).FirstChild;
					}
					break;
				}
			}

			return node;
		}

		#endregion
	}
}