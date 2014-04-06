using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDBMS.DataStructure
{
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
