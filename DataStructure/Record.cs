using System;
using System.Collections.Generic;
using RDBMS.Util;

namespace RDBMS.DataStructure
{
	[Serializable]
	class Record
	{
		public List<String> Fields;
		//TODO change to char[] as want to serialize in fixed size

		public Record(List<String> fields)
		{
			Fields = fields;
		}
	}
}
