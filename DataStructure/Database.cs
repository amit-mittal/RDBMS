using System;
using System.Collections.Generic;

namespace RDBMS.DataStructure
{
	class Database
	{
		public String Name;
		public List<Table> Tables;
		//TODO: Can add list of users also

		public Database(String name, List<Table> tables)
		{
			Name = name;
			Tables = tables;
		}
	}
}
