//
// Library: KwData
// File:    BtreeDictionaryObject.cs
// Purpose: Defines nongeneric API for BtreeDictionary and its Keys and Values sublcasses.
//
// Copyright © 2009-2012 Kasey Osborn (Kasewick@gmail.com)
// Ms-PL - Use and redistribute freely
//

using System;
using System.Collections;
using System.Diagnostics;

namespace Kw.Data
{
	public partial class BtreeDictionary<TKey, TValue>
	{
		#region Explicit object properties and methods

		void IDictionary.Add(object key, object value)
		{
			var genCol = (System.Collections.Generic.IDictionary<TKey, TValue>) this;
			genCol.Add((TKey) key, (TValue) value);
		}


		bool IDictionary.Contains(object jKey)
		{
			var path = new TreePath<TKey, TValue>(this, (TKey) jKey);
			return path.IsFound;
		}


		void ICollection.CopyTo(Array array, int index)
		{
			if (array == null)
				throw new ArgumentNullException("array");

			if (array.Rank > 1)
				throw new ArgumentException("Multi dimension array is not supported on this operation.");

			if (index < 0)
				throw new ArgumentOutOfRangeException("index", "Index is less than zero.");

			if (index + Count > array.Length)
				throw new ArgumentException(
					"Destination array is not long enough to copy all the items in the collection. Check array index and length.");

			if (!(array is System.Collections.Generic.KeyValuePair<TKey, TValue>[])
				& array.GetType() != typeof (object[]))
				throw new ArgumentException("Target array type is not compatible with the type of items in the collection.");

			foreach (System.Collections.Generic.KeyValuePair<TKey, TValue> pair in this)
			{
				array.SetValue(pair, index);
				++index;
			}
		}

		/// <summary>Gets an enumerator that iterates thru the collection.</summary>
		/// <returns>An enumerator for the collection.</returns>
		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return new BtreeObjectEnumerator(this);
		}


		/// <summary>Gets an enumerator that iterates thru the collection.</summary>
		/// <returns>An enumerator for the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IDictionary) this).GetEnumerator();
		}


		/// <summary>Remove the supplied key and its associated value from the collection.</summary>
		/// <param name="key">Key to remove.</param>
		/// <exception cref="ArgumentNullException">When <em>key</em> is <b>null</b>.</exception>
		void IDictionary.Remove(object key)
		{
			if (key == null)
				throw new ArgumentNullException("key");

			var path = new TreePath<TKey, TValue>(this, (TKey) key);
			if (path.IsFound)
				Delete(path);
		}


		/// <summary>Deprecated.</summary>
		object ICollection.SyncRoot
		{
			get { return new System.Object(); }
		}

		#endregion

		/// <summary>
		/// Represents an iterator for the collection.
		/// </summary>
		public class BtreeObjectEnumerator : IDictionaryEnumerator
		{
			private BtreeDictionary<TKey, TValue> target;
			private Leaf<TKey, TValue> currentLeaf;
			private int leafIndex;

			#region Constructors

			/// <summary>
			/// Make an enumerator that will loop thru the collection in order.
			/// </summary>
			/// <param name="dictionary">
			/// <see cref="BtreeDictionary&lt;TKey,TValue&gt;"/>
			/// containing these key/value pairs.
			/// </param>
			public BtreeObjectEnumerator(BtreeDictionary<TKey, TValue> dictionary)
			{
				target = dictionary;
				Reset();
			}

			#endregion

			#region Properties

			/// <summary>
			/// Get the key/value pair at the current location.
			/// </summary>
			public DictionaryEntry Entry
			{
				get { return new DictionaryEntry(Key, Value); }
			}

			/// <summary>
			/// Get the key/value pair at the current location.
			/// </summary>
			public object Current
			{
				get { return Entry; }
			}

			/// <summary>
			/// Get the key at the current location.
			/// </summary>
			public object Key
			{
				get { return currentLeaf.GetKey(leafIndex); }
			}

			/// <summary>
			/// Get the value at the current location.
			/// </summary>
			public object Value
			{
				get { return currentLeaf.GetValue(leafIndex); }
			}

			#endregion

			#region Methods

			/// <summary>
			/// Advance the enumerator to the next location.
			/// </summary>
			/// <returns><b>false</b> if no more data; otherewise <b>true</b></returns>
			public bool MoveNext()
			{
				if (++leafIndex < currentLeaf.KeyCount)
					return true;

				leafIndex = 0;
				currentLeaf = currentLeaf.RightLeaf;
				return currentLeaf != null;
			}

			/// <summary>
			/// Move the enumerator back to its initial location.
			/// </summary>
			public void Reset()
			{
				leafIndex = -1;
				currentLeaf = target.GetFirstLeaf();
			}

			#endregion
		}

		#region Explicit properties and methods

		/// <summary>Get or set the value associated with the supplied key.</summary>
		public object this[object key]
		{
			get
			{
				if (key == null)
					throw new ArgumentNullException("key");

				int index;
				Leaf<TKey, TValue> leaf = Find((TKey) key, out index);
				if (index < 0)
					return null;
				return leaf.GetValue(index);
			}
			set
			{
				if (key == null)
					throw new ArgumentNullException("key");

				var path = new TreePath<TKey, TValue>(this, (TKey) key);
				if (path.IsFound)
					path.LeafValue = (TValue) value;
				else
					Insert(path, (TKey) key, (TValue) value);
			}
		}

		bool IDictionary.IsFixedSize
		{
			get { return false; }
		}

		/// <summary>Indicate that structure may be modified.</summary>
		bool IDictionary.IsReadOnly
		{
			get { return false; }
		}

		bool ICollection.IsSynchronized
		{
			get { return false; }
		}

		ICollection IDictionary.Keys
		{
			get { return (ICollection) keys; }
		}

		ICollection IDictionary.Values
		{
			get { return (ICollection) values; }
		}

		#endregion

		[Serializable]
		public partial class BtreeKeys
		{
			#region Explicit object properties and methods

			void System.Collections.ICollection.CopyTo(Array array, int index)
			{
				if (array == null)
					throw new ArgumentNullException("array");

				if (array.Rank > 1)
					throw new ArgumentException("Multi dimension array is not supported on this operation.");

				if (index < 0)
					throw new System.ArgumentOutOfRangeException("index", "Index is less than zero.");

				if (index + Count > array.Length)
					throw new ArgumentException(
						"Destination array is not long enough to copy all the items in the collection. Check array index and length.");

				foreach (TKey key in this)
				{
					array.SetValue(key, index);
					++index;
				}
			}


			/// <summary>Gets an enumerator that iterates thru the collection.</summary>
			/// <returns>An enumerator for the collection.</returns>
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			bool System.Collections.ICollection.IsSynchronized
			{
				get { return false; }
			}

			object System.Collections.ICollection.SyncRoot
			{
				get { return ((System.Collections.ICollection) tree).SyncRoot; }
			}

			#endregion
		}


		public partial class BtreeValues
		{
			#region Explicit object properties and methods

			void System.Collections.ICollection.CopyTo(Array array, int index)
			{
				if (array == null)
					throw new ArgumentNullException("array");

				if (array.Rank > 1)
					throw new ArgumentException("Multi dimension array is not supported on this operation.");

				if (index < 0)
					throw new System.ArgumentOutOfRangeException("index", "Index is less than zero.");

				if (index + Count > array.Length)
					throw new ArgumentException(
						"Destination array is not long enough to copy all the items in the collection. Check array index and length.");

				foreach (TValue value in this)
				{
					array.SetValue(value, index);
					++index;
				}
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			bool System.Collections.ICollection.IsSynchronized
			{
				get { return false; }
			}

			object System.Collections.ICollection.SyncRoot
			{
				get { return ((System.Collections.ICollection) tree).SyncRoot; }
			}

			#endregion
		}
	}
}