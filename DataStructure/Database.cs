using System;
using System.Collections.Generic;

namespace RDBMS.DataStructure
{
	[Serializable]
	internal class Database
	{
		public String Name;
		public List<String> Tables; //this attribute is not getting used
		//May happen instead of string, using char[]
		//TODO: Can add list of users also

		public Database(String name)
		{
			Name = name;
		}

		[Obsolete]
		public Database(String name, List<String> tables)
		{
			Name = name;
			Tables = tables;
		}
	}
}