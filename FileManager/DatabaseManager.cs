using System;
using System.Collections.Generic;
using System.IO;
using RDBMS.DataStructure;
using RDBMS.SpaceManager;
using RDBMS.Util;

namespace RDBMS.FileManager
{
	class DatabaseManager
	{
		public Database db;
		public StorageManager manager;

		public void CreateDatabase(String dbName)
		{
			manager = new StorageManager();
			db = new Database(dbName, new List<Table>());
			String path = GetFilePath.Database(dbName);
			if (Directory.Exists(path))
			{
				throw new Exception("Database already exists");
			}
			else
			{
				//create folder and conf file
				manager.CreateFolder(path);
			}
		}

		public void UseDatabase(String dbName)
		{
			List<Table> tables = new List<Table>();
			manager = new StorageManager();
			int tableSize = manager.GetRecordSize(File.OpenRead(GetFilePath.Database(dbName)));
			byte[] tableBytes = manager.GetCompleteFile(File.OpenRead(GetFilePath.Database(dbName)));
			List<object> objects = Converter.FromBytes(tableBytes, tableSize);
			foreach (object obj in objects)
			{
				tables.Add((Table)obj);
			}
			db = new Database(dbName, tables);
		}
	}
}
