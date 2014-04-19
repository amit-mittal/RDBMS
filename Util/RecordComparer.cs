using System;
using System.Collections.Generic;
using RDBMS.DataStructure;

namespace RDBMS.Util
{
	class RecordComparer : IComparer<Record>
	{
		private int index;

		public RecordComparer(int index)
		{
			this.index = index;
		}

		/**
		 * Compare according to the index set
		 */
		public int Compare(Record x, Record y)
		{
			String s1 = x.Fields[index];
			String s2 = y.Fields[index];

			return s1.CompareTo(s2);
		}
	}
}
