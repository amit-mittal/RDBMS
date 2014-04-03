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

		public Record(List<String> fields)
		{
			Fields = fields;
		}
	}
}
