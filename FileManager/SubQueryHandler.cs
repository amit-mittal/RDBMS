using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDBMS.FileManager
{
	class SubQueryHandler
	{
		public DatabaseManager DbManager = null;
		
		//handles database manager and table manager acc 2 the task
		
		public SubQueryHandler()
		{
			//dbManager is only init when use database query used
			//Init by QueryHandler class in the starting - only 1 instance made
		}

		public void UseDatabase(String dbName)
		{
			DbManager = new DatabaseManager(dbName);
		}
	}
}
