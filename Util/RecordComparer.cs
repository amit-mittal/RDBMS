using System;
using System.Collections.Generic;
using RDBMS.DataStructure;

namespace RDBMS.Util
{
	class RecordComparer : IComparer<Record>
	{
		private int index;
		private Column.DataType type;

		public RecordComparer(int index, Column.DataType type)
		{
			this.type = type;
			this.index = index;
		}

		/**
		 * Compare according to the index set
		 */
		public int Compare(Record x, Record y)
		{
			String s1 = x.Fields[index];
			String s2 = y.Fields[index];

			if (type == Column.DataType.Char)
			{
				return s1.CompareTo(s2);
			}
			if (type == Column.DataType.Double)
			{
				double v1 = double.Parse(s1);
				double v2 = double.Parse(s2);
				return v1.CompareTo(v2);
			}
			if (type == Column.DataType.Int)
			{
				int v1 = int.Parse(s1);
				int v2 = int.Parse(s2);
				return v1.CompareTo(v2);
			}

			throw new Exception("Unsupported type");
		}
	}
}
