using System;
using System.Collections.Generic;
using RDBMS.Util;

namespace RDBMS.DataStructure
{
	[Serializable]
	internal class Record
	{
		public List<String> Fields;

		public Record(List<String> fields)
		{
			Fields = fields;
		}
	}
}