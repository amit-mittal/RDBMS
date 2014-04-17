using System;
using System.Collections.Generic;
using RDBMS.DataStructure;

namespace RDBMS.FileManager
{
	/**
	 * Handles all the smaller queries
	 * coming from the top layer and returns
	 * data accordingly or throws exception
	 * TODO testing of this class pending
	 */

	//TODO implement show databases also
	//TODO drop index also
	internal class SubQueryHandler
	{
		//TODO make sure consisitency in null and empty string in record object
		public DatabaseManager DbManager = null;

		//handles database storageManager and table storageManager acc 2 the task

		#region Constructors

		public SubQueryHandler()
		{
			//dbManager is only init when use database query used
			//Init by QueryHandler class in the starting - only 1 instance made
		}

		#endregion

		#region Database Methods

		private void CheckIfDatabaseSelected()
		{
			if (DbManager == null || DbManager.db == null)
				throw new Exception("No database selected");
		}

		public void UseDatabase(String dbName)
		{
			if (DbManager == null)
				DbManager = new DatabaseManager();
			DbManager.UseDatabase(dbName);
		}

		public void CreateDatabase(String dbName)
		{
			if (DbManager == null)
				DbManager = new DatabaseManager();
			DbManager.CreateDatabase(dbName);
		}

		public void DropDatabase(String dbName)
		{
			if (DbManager == null)
				DbManager = new DatabaseManager();
			DbManager.DropDatabase(dbName);
			DbManager = null;
		}

		public List<String> ShowTables()
		{
			CheckIfDatabaseSelected();
			return DbManager.ShowTables();
		}

		#endregion

		#region Table Methods

		/**
		 * Initially no index can be added, have to write
		 * explicit command
		 */

		public void CreateTable(String tableName, List<Column> columns)
		{
			CheckIfDatabaseSelected();
			TableManager tableManager = new TableManager();
			tableManager.CreateTable(DbManager.db.Name, tableName, columns);
		}

		public void DropTable(String tableName)
		{
			CheckIfDatabaseSelected();
			TableManager tableManager = new TableManager();
			tableManager.DropTable(DbManager.db.Name, tableName);
		}

		public Table DescribeTable(String tableName)
		{
			CheckIfDatabaseSelected();
			TableManager tableManager = new TableManager(DbManager.db.Name, tableName);
			return tableManager.DescribeTable();
		}

		/**
		 * Add that column to index
		 */

		public void CreateIndex(String tableName, Column column)
		{
			CheckIfDatabaseSelected();
			TableManager tableManager = new TableManager(DbManager.db.Name, tableName);

			//checking if index on it already exists
			if (tableManager.table.CheckIfColumnIndexed(column))
				throw new Exception("Column has already been indexed");

			tableManager.AddIndex(column);
		}

		/**
		 * After checking for error if any, inserts the record into
		 * the table and index file
		 */

		public void InsertRecordToTable(String tableName, Record record)
		{
			CheckIfDatabaseSelected();
			TableManager tableManager = new TableManager(DbManager.db.Name, tableName);
			tableManager.CheckRecord(record); //checks for any error in the record
			int address = tableManager.InsertRecord(record);
			tableManager.InsertRecordToIndices(record, address);
		}

		/**
		 * Delete records from table which satisifies the @param name="condition"
		 * 
		 * If condition null that means deleting all the records
		 */

		public void DeleteRecordsFromTable(String tableName, Condition condition)
		{
			CheckIfDatabaseSelected();
			TableManager tableManager = new TableManager(DbManager.db.Name, tableName);

			//check if condition is valid or not
			if (!tableManager.table.CheckIfConditionValid(condition))
			{
				throw new Exception("Condition is not valid");
			}

			Dictionary<int, Record> records = SelectRecordsFromTable(tableName, condition);

			tableManager.DeleteRecords(records);
			tableManager.DeleteRecordsFromIndices(records);
		}

		/**
		 * @param name="condition" updates records which satisfy the condition
		 * if null that means update all
		 * 
		 * @param name="updatedColumns" has those entries set which have been
		 * changed and rest of the entries are null
		 * 
		 * Check for error if there in updatedColumns
		 * If no error combine with old records to get new and updated records
		 */

		public void UpdateRecordToTable(String tableName, Record updatedColumns, Condition condition)
		{
			CheckIfDatabaseSelected();
			TableManager tableManager = new TableManager(DbManager.db.Name, tableName);

			//checking for errors in objects
			tableManager.CheckRecord(updatedColumns);
			if (!tableManager.table.CheckIfConditionValid(condition))
			{
				throw new Exception("Condition is not valid");
			}

			Dictionary<int, Record> oldRecords = SelectRecordsFromTable(tableName, condition);
			Dictionary<int, Record> newRecords = new Dictionary<int, Record>(oldRecords);

			for (int i = 0; i < updatedColumns.Fields.Count; i++) //updating new records
			{
				String value = updatedColumns.Fields[i];
				if (value != null)
				{
					foreach (KeyValuePair<int, Record> pair in newRecords)
					{
						pair.Value.Fields[i] = value;
					}
				}
			}

			tableManager.UpdateRecord(newRecords);
			tableManager.UpdateRecordToIndices(oldRecords, newRecords);
		}

		/**
		 * @returns Dictionart<Address, Record> accordingly using the index or linearly
		 */

		public Dictionary<int, Record> SelectRecordsFromTable(String tableName, Condition condition)
		{
			CheckIfDatabaseSelected();
			TableManager tableManager = new TableManager(DbManager.db.Name, tableName);
			Dictionary<int, Record> records;

			if (!tableManager.table.CheckIfConditionValid(condition))
			{
				throw new Exception("Condition is not valid");
			}

			if (condition != null)
			{
				if (tableManager.table.CheckIfColumnIndexed(condition.Attribute))
					records = tableManager.GetAddressRecordDictOnIndex(condition);
				else
					records = tableManager.GetAddressRecordDict(condition);
			}
			else
			{
				records = tableManager.GetAddressRecordDict(condition);
			}

			return records;
		}

		#endregion
	}
}