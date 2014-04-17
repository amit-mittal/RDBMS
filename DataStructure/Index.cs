using System;

namespace RDBMS.DataStructure
{
	[Serializable]
	internal class Index<T> : IComparable where T : IComparable
	{
		public T Key;
		public int Address;

		public Index(T key, int address)
		{
			Key = key;
			Address = address;
		}

		public int CompareTo(object obj)
		{
			Index<T> other = (Index<T>) obj;
			if (Key.CompareTo(other.Key) == 1)
				return 1;
			else if (Key.CompareTo(other.Key) == -1)
				return -1;
			else
			{
				return Address.CompareTo(other.Address);
			}
		}
	}
}