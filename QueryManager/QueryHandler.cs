using System;
using System.Collections;
using System.Collections.Generic;
using Irony.Parsing;
using RDBMS.DataStructure;
using RDBMS.FileManager;
using RDBMS.Util;

namespace RDBMS.QueryManager
{
	//TODO if second thing is a column name - TEST this
	internal class QueryHandler
	{
		private DisplayMessage _messenger;
		public SubQueryHandler subQueryHandler;
		public String query;
		public ParseTreeNode root;

		public QueryHandler()
		{
			_messenger = new DisplayMessage();
			subQueryHandler = new SubQueryHandler();
		}

		public QueryHandler(SqlForm form)
		{
			_messenger = new DisplayMessage(form);
			subQueryHandler = new SubQueryHandler();
		}

		public void SetQuery(String str)
		{
			query = str.ToLower();
			Init();
		}

		private void Init()
		{
			QueryParser parser = new QueryParser(query);
			root = parser.GetRoot();

			try
			{
				if (root != null)
				{
					_messenger.Message("Valid Query");
					parser.PrintNode(root, 0);
					_messenger.Message("====================");

					if (root.ChildNodes[0].Term.ToString() == "createDatabaseStmt")
					{
						CreateDatabase();
					}
					else if (root.ChildNodes[0].Term.ToString() == "dropDatabaseStmt")
					{
						DropDatabase();
					}
					else if (root.ChildNodes[0].Term.ToString() == "useDatabaseStmt")
					{
						ChangeDatabase();
					}
					else if (root.ChildNodes[0].Term.ToString() == "showTablesStmt")
					{
						ShowTables();
					}
					else if (root.ChildNodes[0].Term.ToString() == "showDatabasesStmt")
					{
						ShowDatabases();
					}
					else if (root.ChildNodes[0].Term.ToString() == "describeTableStmt")
					{
						DescribeTable();
					}
					else if (root.ChildNodes[0].Term.ToString() == "createTableStmt")
					{
						CreateTable();
					}
					else if (root.ChildNodes[0].Term.ToString() == "dropTableStmt")
					{
						DropTable();
					}
					else if (root.ChildNodes[0].Term.ToString() == "insertStmt")
					{
						InsertRecordIntoTable();
					}
					else if (root.ChildNodes[0].Term.ToString() == "updateStmt")
					{
						UpdateRecordOfTable();
					}
					else if (root.ChildNodes[0].Term.ToString() == "deleteStmt")
					{
						DeleteRecordsFromTable();
					}
					else if (root.ChildNodes[0].Term.ToString() == "selectStmt")
					{
						SelectRecordsFromTable();
					}
					else if (root.ChildNodes[0].Term.ToString() == "selectJoinStmt")
					{
						SelectRecordsFromMultipleTable();
					}
					else if (root.ChildNodes[0].Term.ToString() == "createIndexStmt")
					{
						CreateIndexOnColumn();
					}
					else if (root.ChildNodes[0].Term.ToString() == "dropIndexStmt")
					{
						DropIndexOnColumn();
					}
					else if (root.ChildNodes[0].Term.ToString() == "addPrimaryKeyStmt")
					{
						CreatePrimaryKey();
					}
					else if (root.ChildNodes[0].Term.ToString() == "dropPrimaryKeyStmt")
					{
						DropPrimaryKey();
					}
					else
					{
						_messenger.Error("Some error in alloting query");
					}
				}
				else //invalid query
				{
					_messenger.Error("Invalid Query");
				}
			}
			catch (Exception e)
			{
				_messenger.Error(e.Message);
			}
		}

		#region Q U E R I E S

		/**
		 * Query:
		 * CREATE DATABASE database_name
		 */
		private void CreateDatabase()
		{
			String dbName = root
				.ChildNodes[0].ChildNodes[2].ChildNodes[0]
				.Token.Value.ToString();
			
			subQueryHandler.CreateDatabase(dbName);
			_messenger.Message("Database Created");
		}

		/**
		 * Query:
		 * DROP DATABASE database_name
		 */
		private void DropDatabase()
		{
			String dbName = root
				.ChildNodes[0].ChildNodes[2].ChildNodes[0]
				.Token.Value.ToString();

			subQueryHandler.DropDatabase(dbName);
			_messenger.Message("Database Dropped");
		}

		/**
		 * Query:
		 * USE database_name
		 */
		private void ChangeDatabase()
		{
			String dbName = root
				.ChildNodes[0].ChildNodes[1].ChildNodes[0]
				.Token.Value.ToString();

			subQueryHandler.UseDatabase(dbName);
			_messenger.Message("Database Changed");
		}

		/**
		 * Query:
		 * SHOW TABLES
		 */
		private void ShowTables()
		{
			List<String> tableNames = subQueryHandler.ShowTables();
			_messenger.Message("===TABLES===");
			foreach (string tableName in tableNames)
				_messenger.Message(tableName);
		}

		/**
		 * Query:
		 * SHOW TABLES
		 */
		private void ShowDatabases()
		{
			List<String> dbNames = subQueryHandler.ShowDatabases();
			_messenger.Message("===DATABASES===");
			foreach (string dbName in dbNames)
				_messenger.Message(dbName);
		}

		/**
		 * Query:
		 * DESCRIBE table_name
		 */
		private void DescribeTable()
		{
			String tableName = root
				.ChildNodes[0].ChildNodes[1].ChildNodes[0]
				.Token.Value.ToString();

			Table table = subQueryHandler.DescribeTable(tableName);
			
			_messenger.Message(table.Name);
			
			_messenger.Message("==COLUMNS==");
			foreach (var column in table.Columns)
				_messenger.Message(column.Name + "\t" + column.Type + "\t" + column.Length);
			
			_messenger.Message("==INDICES==");
			foreach (var index in table.IndexColumns)
				_messenger.Message(index.Name + "\t" + index.Type + "\t" + index.Length);
		}

		/**
		 * Query:
		 * CREATE TABLE table_name(
		 *		col_name_1 col_1_type [(col_1_len)],
		 *		col_name_2 col_2_type [(col_2_len)]
		 * )
		 */
		private void CreateTable()
		{
			ParseTreeNode topNode = root.ChildNodes[0];
			ParseTreeNode allFieldsNode = topNode.ChildNodes[3];
			List<Column> columns = new List<Column>();

			String tableName = topNode
				.ChildNodes[2].ChildNodes[0]
				.Token.ValueString;

			foreach (var fieldNode in allFieldsNode.ChildNodes)
			{
				Column col;

				String colName = fieldNode
					.ChildNodes[0].ChildNodes[0]
					.Token.ValueString;
				String colType = fieldNode
					.ChildNodes[1].ChildNodes[0]
					.Token.ValueString;
				if (fieldNode.ChildNodes[2].ChildNodes.Count > 0)
				{
					String colLength = fieldNode
						.ChildNodes[2].ChildNodes[0]
						.Token.ValueString;
					
					col = new Column(colType, colName, colLength);
				}
				else
				{
					col = new Column(colType, colName, "");
				}
				
				columns.Add(col);
			}

			subQueryHandler.CreateTable(tableName, columns);
			_messenger.Message("Table Successfully Created");
		}

		/**
		 * Query:
		 * DROP TABLE table_name
		 */
		private void DropTable()
		{
			String tableName = root
				.ChildNodes[0].ChildNodes[2].ChildNodes[0]
				.Token.Value.ToString();

			subQueryHandler.DropTable(tableName);
			_messenger.Message("Table Dropped");
		}

		/**
		 * Query:
		 * INSERT INTO table_name 
		 * (col_1, col_2) 
		 * VALUES 
		 * (col_1_val, col_2_val)
		 */
		private void InsertRecordIntoTable()
		{
			ParseTreeNode topNode = root.ChildNodes[0];
			ParseTreeNode allFieldsNode = topNode.ChildNodes[3].ChildNodes[0];
			ParseTreeNode allValuesNode = topNode.ChildNodes[4].ChildNodes[1];

			List<String> colNames = new List<String>();
			List<String> values = new List<String>();

			if (allFieldsNode.ChildNodes.Count != allValuesNode.ChildNodes.Count)
				throw new Exception("Number of Columns and Values are not matching");

			String tableName = topNode
				.ChildNodes[2].ChildNodes[0]
				.Token.ValueString;

			for (int i = 0; i < allFieldsNode.ChildNodes.Count; i++)
			{
				String colName = allFieldsNode.ChildNodes[i].ChildNodes[0].Token.ValueString;
				String value = allValuesNode.ChildNodes[i].Token.ValueString;
				
				colNames.Add(colName);
				values.Add(value);
			}

			Record record = subQueryHandler.GetRecordFromValues(tableName, colNames, values);

			subQueryHandler.InsertRecordToTable(tableName, record);
			_messenger.Message("Record Succesfully inserted");
		}

		/**
		 * Query:
		 * UPDATE table_name 
		 * SET col_1 = val_1, col_2 = val_2
		 * [WHERE] ((col_3 = val_3) AND (col_4 = col_5))
		 */
		private void UpdateRecordOfTable()
		{
			ParseTreeNode topNode = root.ChildNodes[0];
			
			String tableName = topNode
					.ChildNodes[1].ChildNodes[0]
					.Token.ValueString;

			List<String> colNames = new List<String>();
			List<String> values = new List<String>();

			ParseTreeNode allAssignments = topNode.ChildNodes[3];

			foreach (var assignment in allAssignments.ChildNodes)
			{
				String colName = assignment.ChildNodes[0].ChildNodes[0].Token.ValueString;
				String value = assignment.ChildNodes[2].Token.ValueString;

				colNames.Add(colName);
				values.Add(value);
			}

			Record record = subQueryHandler.GetRecordFromValues(tableName, colNames, values);
			
			if (topNode.ChildNodes[4].ChildNodes.Count > 1)
			{
				ParseTreeNode binExpr = topNode.ChildNodes[4].ChildNodes[1];

				Dictionary<int, Record> possibleRecords = SolveWhereClause(tableName, binExpr);
				subQueryHandler.UpdateRecordToTable(tableName, record, possibleRecords);
			}
			else
			{
				subQueryHandler.UpdateRecordToTable(tableName, record, (Condition) null);
			}
			
			_messenger.Message("Record(s) successfully updated");
		}

		/**
		 * Query:
		 * DELETE FROM table_name 
		 * [WHERE] ((col_3 = val_3) AND (col_4 = col_5))
		 */
		private void DeleteRecordsFromTable()
		{
			ParseTreeNode topNode = root.ChildNodes[0];

			String tableName = topNode
					.ChildNodes[2].ChildNodes[0]
					.Token.ValueString;

			if (topNode.ChildNodes[3].ChildNodes.Count > 1)//if where clause is there
			{
				ParseTreeNode binExpr = topNode.ChildNodes[3].ChildNodes[1];

				Dictionary<int, Record> possibleRecords = SolveWhereClause(tableName, binExpr);
				subQueryHandler.DeleteRecordsFromTable(tableName, possibleRecords);
			}
			else
			{
				subQueryHandler.DeleteRecordsFromTable(tableName, (Condition) null);
			}

			_messenger.Message("Record(s) successfully deleted");
		}

		/**
		 * Query:
		 * SELECT [*] [col_1, col_2]
		 * FROM table_1 
		 * [WHERE] ((col_3 = val_3) AND (col_4 = col_5))
		 * [ORDER BY] col_name [ASC|DESC]
		 */
		private void SelectRecordsFromTable()
		{
			ParseTreeNode topNode = root.ChildNodes[0];

			//Selecting the table
			ParseTreeNode tableListNode = topNode
				.ChildNodes[4].ChildNodes[1];
			String tableName = tableListNode.ChildNodes[0].ChildNodes[0].Token.ValueString;


			//Selecting the columns
			ParseTreeNode colList = topNode.ChildNodes[2].ChildNodes[0];
			List<int> selectedColumns;
			if (colList.ChildNodes.Count == 0)//'*' clause
			{
				selectedColumns = subQueryHandler.GetColumnIndicesFromName(tableName, new List<string>());
			}
			else
			{
				List<String> colNames = new List<string>();
				foreach (var colNameNode in colList.ChildNodes)
				{
					String colName = colNameNode.ChildNodes[0].ChildNodes[0].ChildNodes[0].Token.ValueString;
					colNames.Add(colName);
				}
				selectedColumns = subQueryHandler.GetColumnIndicesFromName(tableName, colNames);
			}


			//Selecting the records
			Dictionary<int, Record> possibleRecords;
			if (topNode.ChildNodes[5].ChildNodes.Count > 1)
			{
				ParseTreeNode binExpr = topNode.ChildNodes[5].ChildNodes[1];
				possibleRecords = SolveWhereClause(tableName, binExpr);
			}
			else
			{
				possibleRecords = subQueryHandler.SelectRecordsFromTable(tableName, null);
			}


			//Getting the order by clause
			List<Record> orderedList = new List<Record>(possibleRecords.Values);
			if (topNode.ChildNodes[8].ChildNodes.Count > 1)
			{
				ParseTreeNode orderByNode = topNode.ChildNodes[8].ChildNodes[2];
				String orderByColName = orderByNode.ChildNodes[0]
					.ChildNodes[0].ChildNodes[0].Token.ValueString;
				
				List<String> orderByList = new List<string>(1);
				orderByList.Add(orderByColName);
				int toSortIndex = subQueryHandler.GetColumnIndicesFromName(tableName, orderByList)[0];
				Column.DataType toSortType = subQueryHandler.DescribeTable(tableName).Columns[toSortIndex].Type;
				
				String orderByType = "asc";
				if (orderByNode.ChildNodes[0].ChildNodes[1].ChildNodes.Count > 0)
				{
					orderByType = orderByNode.ChildNodes[0].ChildNodes[1]
						.ChildNodes[0].Token.ValueString;
				}

				orderedList.Sort(new RecordComparer(toSortIndex, toSortType));
				if (orderByType == "desc")
					orderedList.Reverse();
			}


			//Displaying the heading of selected records
			List<String> sortedSelectedCols = subQueryHandler.SortColumnsInOrder(tableName, selectedColumns);
			foreach (String colName in sortedSelectedCols)
			{
				_messenger.Message(colName + "|");
			}


			//Dispaly the records in proper format
			foreach (Record record in orderedList)
			{
				List<String> fields = record.Fields;
				String recAsString = "";
				foreach (int index in selectedColumns)
					recAsString += (fields[index] + " | ");
				_messenger.Message(recAsString);
			}
		}

		/**
		 * Query:
		 * SELECT col_1 OF t1, col_2 OF t2 
		 * FROM t1 OF table_1, t2 OF table_2 
		 * [WHERE] (col_3 OF t1 = col_4 of t2) 
		 */
		private void SelectRecordsFromMultipleTable()
		{
			ParseTreeNode topNode = root.ChildNodes[0];

			//Selecting the columns
			ParseTreeNode colList = topNode.ChildNodes[2];
			Dictionary<String, String> idColumnMap = new Dictionary<string, string>();
			foreach (var colNode in colList.ChildNodes)
			{
				String colName = colNode.ChildNodes[0].ChildNodes[0].Token.ValueString;
				String colId = colNode.ChildNodes[2].ChildNodes[0].Token.ValueString;

				idColumnMap.Add(colId, colName);
			}

			//Selecting the tables
			ParseTreeNode tableListNode = topNode.ChildNodes[3].ChildNodes[1];
			Dictionary<String, String> idTableMap = new Dictionary<string, string>();
			foreach (var tableNode in tableListNode.ChildNodes)
			{
				String tableId = tableNode.ChildNodes[0].ChildNodes[0].Token.ValueString;
				String tableName = tableNode.ChildNodes[2].ChildNodes[0].Token.ValueString;
				
				idTableMap.Add(tableId, tableName);
			}


			//Parsing the where clause
			Dictionary<int, Dictionary<string, Record>> finalRecords;
			if (topNode.ChildNodes[4].ChildNodes.Count > 1)
			{
				ParseTreeNode binExpr = topNode.ChildNodes[4].ChildNodes[1].ChildNodes[0];
				finalRecords = SolveWhereClause(idTableMap, binExpr);
			}
			else
			{
				finalRecords = new Dictionary<int, Dictionary<string, Record>>();
				foreach (var tableName in idTableMap.Values)
				{
					Dictionary<int, Record> temp = subQueryHandler.SelectRecordsFromTable(tableName, null);
					finalRecords = CrossJoin(finalRecords, temp, tableName);
				}
			}

			//TODO select only columns specified not done
			
			//Printing the records
			foreach (var finalRecord in finalRecords)//iterating each row
			{
				String rowAsString = "";
				foreach (var idToRecord in finalRecord.Value)//iterating on each table
				{
					rowAsString += (idToRecord.Key + " | ");
					foreach (string fieldStr in idToRecord.Value.Fields)
						rowAsString += (fieldStr + " | ");
				}
				_messenger.Message(rowAsString);
			}
		}

		/**
		 * Query:
		 * CREATE INDEX column_name ON table_name
		 */
		private void CreateIndexOnColumn()
		{
			ParseTreeNode topNode = root.ChildNodes[0];

			String tableName = topNode
					.ChildNodes[5].ChildNodes[0]
					.Token.ValueString;

			String colName = topNode
					.ChildNodes[3].ChildNodes[0]
					.Token.ValueString;

			subQueryHandler.CreateIndex(tableName, colName);
			_messenger.Message("Index Created");
		}

		/**
		 * Query:
		 * CREATE PRIMARY KEY column_name ON table_name
		 */
		private void CreatePrimaryKey()
		{
			ParseTreeNode topNode = root.ChildNodes[0];

			String tableName = topNode
					.ChildNodes[5].ChildNodes[0]
					.Token.ValueString;

			String colName = topNode
					.ChildNodes[3].ChildNodes[0]
					.Token.ValueString;

			subQueryHandler.CreatePrimaryKey(tableName, colName);
			_messenger.Message("Primary Key Created");
		}

		/**
		 * Query:
		 * DROP PRIMARY KEY ON table_name
		 */
		private void DropPrimaryKey()
		{
			ParseTreeNode topNode = root.ChildNodes[0];

			String tableName = topNode
					.ChildNodes[4].ChildNodes[0]
					.Token.ValueString;

			subQueryHandler.DropPrimaryKey(tableName);
			_messenger.Message("Primary Key Dropped");
		}

		/**
		 * Query:
		 * DROP INDEX column_name ON table_name
		 */
		private void DropIndexOnColumn()
		{
			ParseTreeNode topNode = root.ChildNodes[0];

			String tableName = topNode
					.ChildNodes[4].ChildNodes[0]
					.Token.ValueString;

			String colName = topNode
					.ChildNodes[2].ChildNodes[0]
					.Token.ValueString;

			subQueryHandler.DropIndex(tableName, colName);
			_messenger.Message("Index Dropped");
		}

		#endregion

		#region Helper Functions

		/**
		 * Evaluates WHERE for binary expressions
		 * Binary Expression should be of form
		 *		colName
		 *		conditionType
		 *		Value/colName
		 */
		private Dictionary<int, Record> SolveWhereClause(String tableName, ParseTreeNode binExpr)
		{
			Dictionary<int, Record> finalResult = new Dictionary<int, Record>();
			String opValue = binExpr.ChildNodes[1].ChildNodes[0].Token.ValueString;

			if (opValue == "and")
			{
				Dictionary<int, Record> result1 = SolveWhereClause(tableName, binExpr.ChildNodes[0]);
				Dictionary<int, Record> result2 = SolveWhereClause(tableName, binExpr.ChildNodes[2]);

				finalResult = TakeIntersection(result1, result2);
			}
			else if (opValue == "or")
			{
				Dictionary<int, Record> result1 = SolveWhereClause(tableName, binExpr.ChildNodes[0]);
				Dictionary<int, Record> result2 = SolveWhereClause(tableName, binExpr.ChildNodes[2]);

				finalResult = TakeUnion(result1, result2);
			}
			else
			{
				//Solving the Base Expression
				String v1 = binExpr.ChildNodes[0].ChildNodes[0].Token.ValueString;

				if (binExpr.ChildNodes[2].ChildNodes.Count == 0)
				{
					String v2 = binExpr.ChildNodes[2].Token.ValueString;
					Condition condition = subQueryHandler.GetCondition(tableName, v1, opValue, v2);
					finalResult = subQueryHandler.SelectRecordsFromTable(tableName, condition);
				}
				else
				{
					String v2 = binExpr.ChildNodes[0].ChildNodes[0].Token.ValueString;
					Dictionary<int, Record> allRecs = subQueryHandler.SelectRecordsFromTable(tableName, null);
					foreach (var pair in allRecs)
					{
						if (subQueryHandler.IsRecordSatisfyingCondition(tableName, pair.Value, v1, v2, opValue))
							finalResult.Add(pair.Key, pair.Value);
					}
				}
			}

			return finalResult;
		}

		/**
		 * Take union of both the dictionaries to result 
		 * a final dictionary
		 */
		private Dictionary<int, Record> TakeUnion(Dictionary<int, Record> result1, Dictionary<int, Record> result2)
		{
			Dictionary<int, Record> finalResult = new Dictionary<int, Record>();
			//optimized query
			if (result1.Count > result2.Count)
			{
				foreach (var pair in result1)
				{
					finalResult.Add(pair.Key, pair.Value);
				}
				foreach (var pair in result2)
				{
					if (!finalResult.ContainsKey(pair.Key))
						finalResult.Add(pair.Key, pair.Value);
				}
			}
			else
			{
				foreach (var pair in result2)
				{
					finalResult.Add(pair.Key, pair.Value);
				}
				foreach (var pair in result1)
				{
					if (!finalResult.ContainsKey(pair.Key))
						finalResult.Add(pair.Key, pair.Value);
				}
			}

			return finalResult;
		}

		/**
		 * Take intersction of both the dictionaries to result 
		 * a final dictionary
		 */
		private Dictionary<int, Record> TakeIntersection(Dictionary<int, Record> result1, Dictionary<int, Record> result2)
		{
			Dictionary<int, Record> finalResult = new Dictionary<int, Record>();
			//optimized query
			if (result1.Count > result2.Count)
			{
				foreach (var pair in result2)
				{
					if (result1.ContainsKey(pair.Key) && result2.ContainsKey(pair.Key))
						finalResult.Add(pair.Key, pair.Value);
				}
			}
			else
			{
				foreach (var pair in result1)
				{
					if (result1.ContainsKey(pair.Key) && result2.ContainsKey(pair.Key))
						finalResult.Add(pair.Key, pair.Value);
				}
			}

			return finalResult;
		}

		private Dictionary<int, Dictionary<string, Record>> SolveWhereClause(Dictionary<string, string> idTableMap, 
			ParseTreeNode binExpr)
		{
			Dictionary<int, Dictionary<string, Record>> records = new Dictionary<int, Dictionary<string, Record>>();
			String opValue = binExpr.ChildNodes[1].ChildNodes[0].Token.ValueString;

			if (opValue == "and")
			{
				ParseTreeNode leftBinExpr = binExpr.ChildNodes[0].ChildNodes[0];
				ParseTreeNode rightBinExpr = binExpr.ChildNodes[2].ChildNodes[0];
				Dictionary<int, Dictionary<string, Record>> left = SolveWhereClause(idTableMap, leftBinExpr);
				Dictionary<int, Dictionary<string, Record>> right = SolveWhereClause(idTableMap, rightBinExpr);

				records = ANDCrossJoin(left, right);
			}
			else if (opValue == "or")
			{
				ParseTreeNode leftBinExpr = binExpr.ChildNodes[0].ChildNodes[0];
				ParseTreeNode rightBinExpr = binExpr.ChildNodes[2].ChildNodes[0];
				Dictionary<int, Dictionary<string, Record>> left = SolveWhereClause(idTableMap, leftBinExpr);
				Dictionary<int, Dictionary<string, Record>> right = SolveWhereClause(idTableMap, rightBinExpr);

				records = ORCrossJoin(left, right);
			}
			else
			{
				//Solving the Base Expression
				ParseTreeNode left = binExpr.ChildNodes[0].ChildNodes[0].ChildNodes[0];
				String leftTableInstance = left.ChildNodes[2].ChildNodes[0].Token.ValueString;
				String leftColName = left.ChildNodes[0].ChildNodes[0].Token.ValueString;
				if(!idTableMap.ContainsKey(leftTableInstance))
					throw new Exception("Table instance does not exist");
				String leftTableName = idTableMap[leftTableInstance];

				if (binExpr.ChildNodes[2].ChildNodes[0].ChildNodes[0].ChildNodes.Count > 1)//both sides column
				{
					ParseTreeNode right = binExpr.ChildNodes[2].ChildNodes[0].ChildNodes[0];
					String rightTableInstance = right.ChildNodes[2].ChildNodes[0].Token.ValueString;
					String rightColName = right.ChildNodes[0].ChildNodes[0].Token.ValueString;
					if (!idTableMap.ContainsKey(rightTableInstance))
						throw new Exception("Table instance does not exist");
					String rightTableName = idTableMap[rightTableInstance];

					if (rightTableInstance.Equals(leftTableInstance))//same table
					{
						int c = 0;
						Dictionary<int, Record> allRecs = subQueryHandler.SelectRecordsFromTable(leftTableName, null);
						foreach (var pair in allRecs)
						{
							if (subQueryHandler
								.IsRecordSatisfyingCondition(leftTableName, pair.Value, leftColName, rightColName, opValue))
							{
								records.Add(c, GetDictFromRecord(pair.Value, leftTableName));
								++c;
							}
						}
					}
					else//different tables
					{
						int c = 0;
						Dictionary<int, Record> allRecs1 = subQueryHandler.SelectRecordsFromTable(leftTableName, null);
						Dictionary<int, Record> allRecs2 = subQueryHandler.SelectRecordsFromTable(rightTableName, null);
						foreach (var pair1 in allRecs1)//taking cross join
						{
							foreach (var pair2 in allRecs2)
							{
								if (subQueryHandler
								.IsRecordSatisfyingCondition(leftTableName, rightTableName, pair1.Value, pair2.Value, leftColName, rightColName, opValue))
								{
									Dictionary<string, Record> dict = new Dictionary<string, Record>(2);
									dict.Add(leftTableName, pair1.Value);
									dict.Add(rightTableName, pair2.Value);
									records.Add(c, dict);
									++c;
								}
							}
						}
					}
				}
				else//right side is value
				{
					String value = binExpr.ChildNodes[2].ChildNodes[0].ChildNodes[0].Token.ValueString;
					Condition condition = subQueryHandler.GetCondition(leftTableName, leftColName, opValue, value);
					Dictionary<int, Record> addressRecordMap = subQueryHandler.SelectRecordsFromTable(leftTableName, condition);

					int c = 0;
					foreach (var record in addressRecordMap.Values)
					{
						records.Add(c, GetDictFromRecord(record, leftTableName));
						++c;
					}
					//bug: if other table is there take complete cross join
				}
			}
			return records;
		}

		private bool CheckIfPresent(Dictionary<string, Record> smallSet, Dictionary<string, Record> LargeSet)
		{
			if(smallSet.Keys.Count > LargeSet.Keys.Count)
				return false;

			foreach (var table in smallSet.Keys)
			{
				if(!LargeSet.ContainsKey(table))
					return false;
			}
			return true;
		}

		private Dictionary<string, Record> GetDictFromRecord(Record r, String tableName)
		{
			Dictionary<string, Record> dict = new Dictionary<string, Record>();
			dict.Add(tableName, r);
			return dict;
		}

		/**
		 * Simply takes the cross join without any condition
		 * Tables in the 2 parameters must also be mutually exclusive
		 */
		private Dictionary<int, Dictionary<string, Record>> CrossJoin(Dictionary<int, 
			Dictionary<string, Record>> t1, Dictionary<int, Record> t2, String tableName)
		{
			Dictionary<int, Dictionary<string, Record>> finalDict = new Dictionary<int, Dictionary<string, Record>>();
			
			int c = 0;
			if (t1.Count == 0)
			{
				foreach (var record in t2)
				{
					Dictionary<string, Record> newDict = new Dictionary<string, Record>();
					newDict.Add(tableName, record.Value);
					finalDict.Add(c, newDict);
					++c;
				}
				return finalDict;
			}

			foreach (var pair in t1)
			{
				foreach (var record in t2)
				{
					Dictionary<string, Record> newDict = new Dictionary<string, Record>(pair.Value);
					newDict.Add(tableName, record.Value);
					finalDict.Add(c, newDict);
					++c;
				}
			}

			return finalDict;
		}

		private Dictionary<int, Dictionary<string, Record>> ANDCrossJoin(Dictionary<int, Dictionary<string, Record>> t1,
			Dictionary<int, Dictionary<string, Record>> t2)
		{
			Dictionary<int, Dictionary<string, Record>> dict = new Dictionary<int, Dictionary<string, Record>>();

			int c = 0;
			foreach (var pair1 in t1.Values)
			{
				foreach (var pair2 in t2.Values)
				{
					Dictionary<string, Record> temp1 = new Dictionary<string, Record>(pair2);
					foreach (string tableName1 in pair1.Keys)
					{
						if (!temp1.ContainsKey(tableName1))
							temp1.Add(tableName1, pair1[tableName1]);
						//BUG: else both should satisify each other's condition
					}

					Dictionary<string, Record> temp2 = new Dictionary<string, Record>(pair1);
					foreach (string tableName2 in pair2.Keys)
					{
						if (!temp2.ContainsKey(tableName2))
							temp2.Add(tableName2, pair2[tableName2]);
					}

					if (CheckIfDictIdentical(temp1, temp2))
					{
						dict.Add(c, temp1);
						++c;
					}
					else
					{
						dict.Add(c, temp1);
						++c;
						dict.Add(c, temp2);
						++c;
					}
				}
			}

			return dict;
		}

		private Dictionary<int, Dictionary<string, Record>> ORCrossJoin(Dictionary<int, Dictionary<string, Record>> t1,
			Dictionary<int, Dictionary<string, Record>> t2)
		{
			Dictionary<int, Dictionary<string, Record>> dict = new Dictionary<int, Dictionary<string, Record>>();
			if (t2.Count > 0)
			{
				foreach (var tableName in t2[0].Keys)
				{
					if (t1.Count > 0 && t1[0].ContainsKey(tableName))
						continue;
					Dictionary<int, Record> temp = subQueryHandler.SelectRecordsFromTable(tableName, null);
					t1 = CrossJoin(t1, temp, tableName);
				}
				for (int i = 0; i < t1.Count; i++)
					dict.Add(i, t1[i]);
			}

			if (t1.Count > 0)
			{
				int c = dict.Count;
				foreach (var tableName in t1[0].Keys)
				{
					if (t2.Count > 0 && t2[0].ContainsKey(tableName))
						continue;
					Dictionary<int, Record> temp = subQueryHandler.SelectRecordsFromTable(tableName, null);
					t2 = CrossJoin(t2, temp, tableName);
				}
				for (int i = 0; i < t2.Count; i++)
				{
					bool found = false;
					foreach (var pair in dict.Values)
					{
						if (CheckIfDictIdentical(pair, t2[i]))
						{
							found = true;
							break;
						}
					}
					if (found)
						continue;
					dict.Add(c, t2[i]);
					++c;
				}
			}

			return dict;
		}

		private bool CheckIfDictIdentical(Dictionary<string, Record> t1, Dictionary<string, Record> t2)
		{
			if(t1.Count != t2.Count)
				return false;

			foreach (KeyValuePair<string, Record> pair in t1)
			{
				if (t2.ContainsKey(pair.Key))
				{
					if (pair.Value.CompareTo(t2[pair.Key]) != 0)
						return false;
				}
				else
					return false;
			}
			return true;
		}

		#endregion
	}
}