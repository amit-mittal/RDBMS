using System;
using System.Collections.Generic;
using System.IO;
using RDBMS.DataStructure;
using RDBMS.Util;

namespace RDBMS.FileManager
{
	/**
	 * Handles all the smaller queries
	 * coming from the top layer and returns
	 * data accordingly or throws exception
	 */
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
		 * Adds primary key to table
		 */
		public void CreatePrimaryKey(String tableName, String colName)
		{
			CheckIfDatabaseSelected();
			TableManager tableManager = new TableManager(DbManager.db.Name, tableName);
			Column column = tableManager.table.GetColumnByName(colName);

			Dictionary<int, Record> allRecords = SelectRecordsFromTable(tableName, null);
			if(allRecords.Count > 0)
				throw new Exception("Primary Key can be added only when table is empty");

			//checking if primary key already exists
			if (tableManager.table.PrimaryKey != null)
				throw new Exception("Table already contains a primary key");
			if (tableManager.table.CheckIfColumnIndexed(column))
				throw new Exception("Column already indexed - first remove it");

			tableManager.AddPrimaryKey(column);
		}

		/**
		 * Drop index on column specified
		 */
		public void DropIndex(String tableName, String colName)
		{
			CheckIfDatabaseSelected();
			TableManager tableManager = new TableManager(DbManager.db.Name, tableName);
			Column column = tableManager.table.GetColumnByName(colName);

			if (tableManager.table.PrimaryKey!=null && column.Name == tableManager.table.PrimaryKey.Name)
				throw new Exception("Column is primary key so cant drop index");

			//checking if index on it does not exist
			if (!tableManager.table.CheckIfColumnIndexed(column))
				throw new Exception("Column is not indexed");

			tableManager.DropIndex(column);
		}

		/**
		 * Drops primary key of the table
		 */
		public void DropPrimaryKey(String tableName)
		{
			CheckIfDatabaseSelected();
			TableManager tableManager = new TableManager(DbManager.db.Name, tableName);

			//checking if index on it does not exist
			if (tableManager.table.PrimaryKey == null)
				throw new Exception("Tables does not contain primary key");

			tableManager.DropPrimaryKey();
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
			if (tableManager.table.PrimaryKey != null)
			{
				Column pKey = tableManager.table.PrimaryKey;
				int pKeyIndex = tableManager.table.GetColumnIndex(pKey);
				String pKeyVal = record.Fields[pKeyIndex];
				
				//checking if record null
				if(pKeyVal == null)
					throw new Exception("Primary Key Value can't be null");
				
				//check for duplicacy
				Dictionary<int, Record> records = SelectRecordsFromTable(tableName, 
					new Condition(pKey, Condition.ConditionType.Equal, pKeyVal));
				if(records.Count > 0)
					throw new Exception("Primary Key Value already exists");
			}
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
			if (tableManager.table.PrimaryKey != null)
			{
				Column pKey = tableManager.table.PrimaryKey;
				int pKeyIndex = tableManager.table.GetColumnIndex(pKey);
				String pKeyVal = updatedColumns.Fields[pKeyIndex];

				//checking if more than 1 record will get changed
				if(oldRecords.Count > 1 && pKeyVal != null)
					throw new Exception("More than 1 record satisfies condition");
			}
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

			if (tableManager.table.PrimaryKey != null)
			{
				Column pKey = tableManager.table.PrimaryKey;
				int pKeyIndex = tableManager.table.GetColumnIndex(pKey);
				String pKeyVal = updatedColumns.Fields[pKeyIndex];

				//checking if more than 1 record will get changed
				if (oldRecords.Count > 1 && pKeyVal != null)
					throw new Exception("More than 1 record satisfies condition");
			}
			
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

		#region Other Methods

		public List<String> ShowDatabases()
		{
			String path = GetFilePath.Root;
			DirectoryInfo dirInfo = new DirectoryInfo(path);
			DirectoryInfo[] subdirInfo = dirInfo.GetDirectories();

			List<String> subdirNames = new List<string>();
			foreach (DirectoryInfo subdir in subdirInfo)
				subdirNames.Add(subdir.Name);
			return subdirNames;
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

		/**
		 * Returns the column list in the order of the table columns
		 */
		public List<String> SortColumnsInOrder(String tableName, List<int> columnIndices)
		{
			columnIndices.Sort();

			TableManager tableManager = new TableManager(DbManager.db.Name, tableName);

			List<String> newList = new List<string>();
			foreach (var i in columnIndices)
				newList.Add(tableManager.table.GetColumn(i).Name);
			return newList;
		}

		public bool IsRecordSatisfyingCondition(String tableName, Record record, 
			String col1Name, String col2Name, String op)
		{
			CheckIfDatabaseSelected();
			TableManager tableManager = new TableManager(DbManager.db.Name, tableName);

			Column col1 = tableManager.table.GetColumnByName(col1Name);
			Column col2 = tableManager.table.GetColumnByName(col2Name);
			int col1Index = tableManager.table.GetColumnIndex(col1);
			int col2Index = tableManager.table.GetColumnIndex(col2);
			if (col1Index == -1 || col2Index == -1)
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

			if(col1.Type != col2.Type)
				throw new Exception("Both columns do not have same data type");

			if (col1.Type == Column.DataType.Int)
			{
				int valueInt;
				if (int.TryParse(record.Fields[col2Index], out valueInt))
				{
					Condition cond = new Condition(col1, conditionType, record.Fields[col2Index]);
					return cond.CompareIntegers(int.Parse(record.Fields[col1Index]));
				}
			}
			else if (col1.Type == Column.DataType.Double)
			{
				double valueDouble;
				if (double.TryParse(record.Fields[col2Index], out valueDouble))
				{
					Condition cond = new Condition(col1, conditionType, record.Fields[col2Index]);
					return cond.CompareDoubles(double.Parse(record.Fields[col1Index]));
				}
			}
			else if (col1.Type == Column.DataType.Char)
			{
				if (conditionType != Condition.ConditionType.Equal)
					throw new Exception("These type of columns only support equality conditions");

				if (record.Fields[col2Index] != null)
				{
					Condition cond = new Condition(col1, conditionType, record.Fields[col2Index]);
					return cond.CompareStrings(record.Fields[col1Index]);
				}
			}
			else
			{
				throw new Exception("Condition value is not valid");
			}
			return false;
		}

		public bool IsRecordSatisfyingCondition(String tableName1, String tableName2, 
			Record record1, Record record2,
			String col1Name, String col2Name,
			String op)
		{
			CheckIfDatabaseSelected();
			TableManager tableManager1 = new TableManager(DbManager.db.Name, tableName1);
			TableManager tableManager2 = new TableManager(DbManager.db.Name, tableName2);

			Column col1 = tableManager1.table.GetColumnByName(col1Name);
			Column col2 = tableManager2.table.GetColumnByName(col2Name);
			int col1Index = tableManager1.table.GetColumnIndex(col1);
			int col2Index = tableManager2.table.GetColumnIndex(col2);
			if (col1Index == -1 || col2Index == -1)
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

			if (col1.Type != col2.Type)
				throw new Exception("Both columns do not have same data type");

			if (col1.Type == Column.DataType.Int)
			{
				int valueInt;
				if (int.TryParse(record2.Fields[col2Index], out valueInt))
				{
					Condition cond = new Condition(col1, conditionType, record2.Fields[col2Index]);
					return cond.CompareIntegers(int.Parse(record1.Fields[col1Index]));
				}
			}
			else if (col1.Type == Column.DataType.Double)
			{
				double valueDouble;
				if (double.TryParse(record2.Fields[col2Index], out valueDouble))
				{
					Condition cond = new Condition(col1, conditionType, record2.Fields[col2Index]);
					return cond.CompareDoubles(double.Parse(record1.Fields[col1Index]));
				}
			}
			else if (col1.Type == Column.DataType.Char)
			{
				if (conditionType != Condition.ConditionType.Equal)
					throw new Exception("These type of columns only support equality conditions");

				if (record2.Fields[col2Index] != null)
				{
					Condition cond = new Condition(col1, conditionType, record2.Fields[col2Index]);
					return cond.CompareStrings(record1.Fields[col1Index]);
				}
			}
			else
			{
				throw new Exception("Condition value is not valid");
			}
			return false;
		}

		#endregion
	}
}