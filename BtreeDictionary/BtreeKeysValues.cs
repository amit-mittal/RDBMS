//
// Library: KwData
// File:    BtreeKeysValues.cs
// Purpose: Define BtreeDictionary Keys and Values nested classes.
//
// Copyright © 2009-2012 Kasey Osborn (Kasewick@gmail.com)
// Ms-PL - Use and redistribute freely
//

using System;
using System.Collections.Generic;

namespace Kw.Data
{
	public partial class BtreeDictionary<TKey, TValue>
	{
		/// <summary>
		/// Represents a collection of keys of a <see cref="BtreeDictionary&lt;TKey,TValue&gt;"/>.
		/// </summary>
		public sealed partial class BtreeKeys :
			System.Collections.Generic.ICollection<TKey>,
			System.Collections.Generic.IEnumerable<TKey>,
			System.Collections.ICollection,
			System.Collections.IEnumerable
		{
			private BtreeDictionary<TKey, TValue> tree;

			#region Constructors

			/// <summary>
			/// Make a new <b>"BtreeDictionary&lt;TKey,TValue&gt;.KeyCollection</b> that
			/// holds the keys of a <see cref="BtreeDictionary&lt;TKey,TValue&gt;"/>.
			/// </summary>
			/// <param name="dictionary">
			/// <see cref="BtreeDictionary&lt;TKey,TValue&gt;"/> containing these keys.
			/// </param>
			public BtreeKeys(BtreeDictionary<TKey, TValue> dictionary)
			{
				this.tree = dictionary;
			}

			#endregion

			#region Properties

			// Implements ICollection<TKey> and object ICollection.
			/// <summary>
			/// Get the number of keys in the collection.
			/// </summary>
			public int Count
			{
				get { return tree.Count; }
			}

			#endregion

			#region Methods

			/// <summary>
			/// Copy keys to a target array starting as position <em>arrayIndex</em> in the target.
			/// </summary>
			/// <param name="array">Array to modify.</param>
			/// <param name="arrayIndex">Starting position in <em>array</em>.</param>
			public void CopyTo(TKey[] array, int arrayIndex)
			{
				if (array == null)
					throw new ArgumentNullException("array");

				if (arrayIndex < 0)
					throw new ArgumentOutOfRangeException("arrayIndex", "Specified argument was out of the range of valid values.");

				if (arrayIndex + Count > array.Length)
					throw new ArgumentException(
						"Destination array is not long enough to copy all the items in the collection. Check array index and length.",
						"arrayIndex");

				foreach (TKey key in this)
				{
					array[arrayIndex] = key;
					++arrayIndex;
				}
			}

			#endregion

			#region Iteration

			/// <summary>
			/// Get an iterator that will loop thru the collection in order.
			/// </summary>
			public IEnumerator<TKey> GetEnumerator()
			{
				return new BtreeKeysEnumerator(tree);
			}


			/// <summary>
			/// Get an enumerator that will loop thru the collection in order.
			/// </summary>
			private class BtreeKeysEnumerator : IEnumerator<TKey>
			{
				private BtreeDictionary<TKey, TValue> target;
				private Leaf<TKey, TValue> currentLeaf;
				private int leafIndex;

				// Long form used for 5% performance increase.
				public BtreeKeysEnumerator(BtreeDictionary<TKey, TValue> tree)
				{
					target = tree;
					Reset();
				}

				object System.Collections.IEnumerator.Current
				{
					get { return (object) Current; }
				}

				public TKey Current
				{
					get { return currentLeaf.GetKey(leafIndex); }
				}

				public bool MoveNext()
				{
					if (++leafIndex < currentLeaf.KeyCount)
						return true;

					leafIndex = 0;
					currentLeaf = currentLeaf.RightLeaf;
					return currentLeaf != null;
				}

				public void Reset()
				{
					leafIndex = -1;
					currentLeaf = target.GetFirstLeaf();
				}

				public void Dispose()
				{
					Dispose(true);
					GC.SuppressFinalize(this);
				}

				protected virtual void Dispose(bool disposing)
				{
				}
			}

			#endregion

			#region Explicit properties and methods

			void ICollection<TKey>.Add(TKey key)
			{
				throw new NotSupportedException();
			}

			void ICollection<TKey>.Clear()
			{
				throw new NotSupportedException();
			}

			bool ICollection<TKey>.Contains(TKey key)
			{
				return tree.ContainsKey(key);
			}

			bool ICollection<TKey>.IsReadOnly
			{
				get { return true; }
			}

			bool ICollection<TKey>.Remove(TKey key)
			{
				throw new NotSupportedException();
			}

			#endregion
		}


		/// <summary>
		/// Represents a collection of values of a <see cref="BtreeDictionary&lt;TKey,TValue&gt;"/>.
		/// </summary>
		[Serializable]
		public sealed partial class BtreeValues :
			System.Collections.Generic.ICollection<TValue>,
			System.Collections.Generic.IEnumerable<TValue>,
			System.Collections.ICollection,
			System.Collections.IEnumerable
		{
			private BtreeDictionary<TKey, TValue> tree;

			#region Constructors

			/// <summary>
			/// Make a new <b>"BtreeDictionary&lt;TKey,TValue&gt;.ValueCollection</b> that
			/// holds the values of a <see cref="BtreeDictionary&lt;TKey,TValue&gt;"/>.
			/// </summary>
			/// <param name="dictionary">
			/// <see cref="BtreeDictionary&lt;TKey,TValue&gt;"/> containing these keys.
			/// </param>
			public BtreeValues(BtreeDictionary<TKey, TValue> dictionary)
			{
				this.tree = dictionary;
			}

			#endregion

			#region Properties

			/// <summary>
			/// Get the number of values in the collection.
			/// </summary>
			public int Count
			{
				get { return tree.Count; }
			}

			#endregion

			#region Methods

			/// <summary>
			/// Copy values to a target array starting as position <em>arrayIndex</em> in the target.
			/// </summary>
			/// <param name="array">Array to modify.</param>
			/// <param name="arrayIndex">Starting position in <em>array</em>.</param>
			public void CopyTo(TValue[] array, int arrayIndex)
			{
				if (array == null)
					throw new ArgumentNullException("array");

				if (arrayIndex < 0)
					throw new ArgumentOutOfRangeException("arrayIndex", "Specified argument was out of the range of valid values.");

				if (arrayIndex + Count > array.Length)
					throw new ArgumentException(
						"Destination array is not long enough to copy all the items in the collection. Check array index and length.",
						"arrayIndex");

				foreach (TValue value in this)
				{
					array[arrayIndex] = value;
					++arrayIndex;
				}
			}

			#endregion

			#region Iteration

			/// <summary>
			/// Get an enumerator that will loop thru the collection of values.
			/// </summary>
			/// <returns>An enumerator for the collection.</returns>
			public IEnumerator<TValue> GetEnumerator()
			{
				for (Leaf<TKey, TValue> currentLeaf = tree.GetFirstLeaf();;)
				{
					for (int leafIndex = 0; leafIndex < currentLeaf.KeyCount; ++leafIndex)
						yield return currentLeaf.GetValue(leafIndex);

					currentLeaf = currentLeaf.RightLeaf;
					if (currentLeaf == null)
						break;
				}
			}

			#endregion

			#region Explicit properties and methods

			/// <exclude />
			void ICollection<TValue>.Add(TValue value)
			{
				throw new NotSupportedException();
			}

			void ICollection<TValue>.Clear()
			{
				throw new NotSupportedException();
			}

			bool ICollection<TValue>.Contains(TValue value)
			{
				return tree.ContainsValue(value);
			}

			bool ICollection<TValue>.IsReadOnly
			{
				get { return true; }
			}

			bool ICollection<TValue>.Remove(TValue val)
			{
				throw new NotSupportedException();
			}

			#endregion
		}
	}
}