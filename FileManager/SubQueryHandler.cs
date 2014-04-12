using System;
using System.Collections.Generic;
using RDBMS.DataStructure;

namespace RDBMS.FileManager
{
	/**
	 * Handles all the smaller queries
	 * coming from the top layer and returns
	 * data accordingly or throws exception
	 */
	class SubQueryHandler
	{
		public DatabaseManager DbManager = null;
		
		//handles database storageManager and table storageManager acc 2 the task
		
		public SubQueryHandler()
		{
			//dbManager is only init when use database query used
			//Init by QueryHandler class in the starting - only 1 instance made
		}

		private void CheckIfDatabaseSelected()
		{
			if (DbManager == null || DbManager.db == null)
				throw new Exception("No database selected");
		}

		public void UseDatabase(String dbName)
		{
			if(DbManager == null)
				DbManager = new DatabaseManager();
			DbManager.UseDatabase(dbName);
		}

		public void CreateDatabase(String dbName)
		{
			if (DbManager == null)
				DbManager = new DatabaseManager();
			DbManager.CreateDatabase(dbName);
		}

		public void CreateTable(String tableName, List<Column> columns)
		{
			CheckIfDatabaseSelected();
			TableManager tableManager = new TableManager();
			tableManager.CreateTable(DbManager.db.Name, tableName, columns);
		}

		public void InsertRecordToTable(String tableName, Record record)
		{
			CheckIfDatabaseSelected();
			TableManager tableManager = new TableManager(DbManager.db.Name, tableName);
			tableManager.InsertRecord(record);
		}

		public void DeleteRecordFromTable(String tableName, List<Column> columns)
		{
			CheckIfDatabaseSelected();
			TableManager tableManager = new TableManager(DbManager.db.Name, tableName);
			tableManager.DeleteRecord();
		}
	}
}
