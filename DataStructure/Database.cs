using System;
using System.Collections.Generic;

namespace RDBMS.DataStructure
{
	[Serializable]
	internal class Database
	{
		public String Name;

		public Database(String name)
		{
			Name = name;
		}
	}
}