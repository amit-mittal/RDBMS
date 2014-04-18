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
		public void CreateIndex(String tableName, String colName)
		{
			CheckIfDatabaseSelected();
			TableManager tableManager = new TableManager(DbManager.db.Name, tableName);
			Column column = tableManager.table.GetColumnByName(colName);

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

		public void DeleteRecordsFromTable(String tableName, Dictionary<int, Record> records)
		{
			CheckIfDatabaseSelected();
			TableManager tableManager = new TableManager(DbManager.db.Name, tableName);

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
		 * @param name="updatedColumns" has those entries set which have been
		 * changed and rest of the entries are null
		 * 
		 * Check for error if there in updatedColumns
		 * If no error combine with old records to get new and updated records
		 */
		public void UpdateRecordToTable(String tableName, Record updatedColumns, Dictionary<int, Record> oldRecords)
		{
			CheckIfDatabaseSelected();
			TableManager tableManager = new TableManager(DbManager.db.Name, tableName);

			//checking for errors in objects
			tableManager.CheckRecord(updatedColumns);
			
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

		#region Helper Functions

		/**
		 * Makes condition out from the given parameters
		 */
		public Condition GetCondition(String tableName, String colName, String op, String value)
		{
			CheckIfDatabaseSelected();
			TableManager tableManager = new TableManager(DbManager.db.Name, tableName);

			Column col = tableManager.table.GetColumnByName(colName);
			if (col == null)
				throw new Exception("No such column exists");

			Condition.ConditionType conditionType;
			if (op == "=")
				conditionType = Condition.ConditionType.Equal;
			else if (op == "<")
				conditionType = Condition.ConditionType.Less;
			else if (op == ">")
				conditionType = Condition.ConditionType.Greater;
			else if (op == "<=")
				conditionType = Condition.ConditionType.LessEqual;
			else if (op == ">=")
				conditionType = Condition.ConditionType.GreaterEqual;
			else
				throw new Exception("Operation " + op + " not supported");

			if (col.Type == Column.DataType.Int)
			{
				int valueInt;
				if(int.TryParse(value, out valueInt))
					return new Condition(col, conditionType, value);
			}
			else if (col.Type == Column.DataType.Double)
			{
				double valueDouble;
				if (double.TryParse(value, out valueDouble))
					return new Condition(col, conditionType, value);
			}
			else if (col.Type == Column.DataType.Char)
			{
				if(conditionType != Condition.ConditionType.Equal)
					throw new Exception("These type of columns only support equality conditions");

				if (value != null)
					return new Condition(col, conditionType, value);
			}
			else
			{
				throw new Exception("Condition value is not valid");
			}

			return null;
		}

		/**
		 * Makes record compatible with tha table columns 
		 * from the list of column names and values given
		 */
		public Record GetRecordFromValues(String tableName, List<String> colNames, List<String> values)
		{
			CheckIfDatabaseSelected();
			TableManager tableManager = new TableManager(DbManager.db.Name, tableName);
			List<String> allColsValues = new List<string>();

			foreach (var column in tableManager.table.Columns)
			{
				bool found = false;
				for (int i = 0; i < colNames.Count; i++)
				{
					String colName = colNames[i];
					if (colName == column.Name)
					{
						found = true;
						String value = values[i];
						allColsValues.Add(value);
					}
				}
				if (!found)
					allColsValues.Add(null);
			}

			//checking if some column specified left
			foreach (var colName in colNames)
			{
				bool found = false;
				foreach (var column in tableManager.table.Columns)
				{
					if (colName == column.Name)
						found = true;
				}
				if (!found)
					throw new Exception("Column(s) does not exist in the table specified");
			}

			return new Record(allColsValues);
		}

		/**
		 * Returns list of column indices from list of column names list
		 */
		public List<int> GetColumnIndicesFromName(String tableName, List<String> colNames)
		{
			CheckIfDatabaseSelected();
			TableManager tableManager = new TableManager(DbManager.db.Name, tableName);
			List<int> columnIndices = new List<int>();

			if (colNames.Count == 0)
			{
				int size = tableManager.table.Columns.Count;
				for (int i = 0; i < size; i++)
					columnIndices.Add(i);
			}
			else
			{
				foreach (var colName in colNames)
				{
					Column col = tableManager.table.GetColumnByName(colName);
					
					int index = tableManager.table.GetColumnIndex(col);
					if(index == -1)
						throw new Exception("Some Column(s) does not exist in the table specified");
					
					columnIndices.Add(index);
				}
			}
			
			return columnIndices;
		}

		#endregion
	}
}