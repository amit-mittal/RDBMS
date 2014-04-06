using System;
using System.IO;
using RDBMS.Util;

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
			String dbPath = GetFilePath.Database(dbName);
			if (Directory.Exists(dbPath))
			{
				DbManager = new DatabaseManager();
				//do other stuff what need to be done
			}
			else
			{
				throw new Exception("Database does not exist");
			}
		}
	}
}
