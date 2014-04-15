using System;
using System.Collections.Generic;
using System.IO;
using RDBMS.DataStructure;
using RDBMS.SpaceManager;
using RDBMS.Util;

namespace RDBMS.FileManager
{
	/**
	 * Handles queries related to the database
	 */
	class DatabaseManager
	{
		public Database db;
		public StorageManager storageManager = new StorageManager();

		/**
		 * Not initializing db object as that will
		 * be done after he has used "use database" query
		 */
		public void CreateDatabase(String dbName)
		{
			String path = GetFilePath.Database(dbName);
			String conf = GetFilePath.DatabaseConf(dbName);
			if (Directory.Exists(path))
			{
				throw new Exception("Database already exists");
			}
			else
			{
				//create folder and conf file
				//if needed add entry to conf of database
				storageManager.CreateFolder(path);
				//storageManager.CreateFile(conf, Converter.CharToBytes(new char[Constants.MaxStringSize]).Length, false);
			}
		}

		public void UseDatabase(String dbName)
		{
			String path = GetFilePath.Database(dbName);
			String conf = GetFilePath.DatabaseConf(dbName);
			if (!Directory.Exists(path))
			{
				throw new Exception("Database does not exist");
			}
			else
			{
				db = new Database(dbName);

				//right now not using conf file
				/*byte[] tableBytes = storageManager.GetCompleteFile(File.OpenRead(conf));
				List<String> tables = Converter.BytesToStringList(tableBytes);*/
			}
		}

		public void DropDatabase(String dbName)
		{
			String path = GetFilePath.Database(dbName);
			if (!Directory.Exists(path))
			{
				throw new Exception("Database does not exist");
			}
			else
			{
				storageManager.DropFolder(path);
			}
		}

		public List<String> ShowTables()
		{
			String path = GetFilePath.Database(db.Name);
			DirectoryInfo dirInfo = new DirectoryInfo(path);
			DirectoryInfo[] subdirInfo = dirInfo.GetDirectories();
			
			List<String> subdirNames = new List<string>();
			foreach (DirectoryInfo subdir in subdirInfo)
			{
				subdirNames.Add(subdir.Name);
			}
			return subdirNames;
		}
	}
}
