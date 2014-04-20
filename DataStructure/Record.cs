using System;
using System.Collections;
using System.Collections.Generic;

namespace RDBMS.DataStructure
{
	[Serializable]
	internal class Record : IComparable
	{
		public List<String> Fields;

		public Record(List<String> fields)
		{
			Fields = fields;
		}

		public int CompareTo(object obj)
		{
			Record r = (Record) obj;
			if (Fields.Count > r.Fields.Count)
				return 1;
			
			if (Fields.Count < r.Fields.Count)
				return -1;

			for (int i = 0; i < Fields.Count; i++)
			{
				String s1 = Fields[i];
				String s2 = r.Fields[i];
				if(s1.Equals(s2))
					continue;
				return s1.CompareTo(s2);
			}
			return 0;
		}
	}
}