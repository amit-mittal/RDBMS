using System;
using System.Collections.Generic;
using Irony.Parsing;
using RDBMS.DataStructure;
using RDBMS.FileManager;
using RDBMS.Util;

namespace RDBMS.QueryManager
{
	//TODO CHANGE GRAMMAR ACCORDING TO OURS
	//TODO SEGREGATE THE METHODS PROPERLY
	internal class QueryHandler
	{
		private DisplayMessage _messenger = new DisplayMessage();
		public SubQueryHandler subQueryHandler;
		public String query;
		public ParseTreeNode root;

		public QueryHandler()
		{
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

					//TODO testing pending
					//todo add index left
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
					else if (root.ChildNodes[0].Term.ToString() == "createIndexStmt")
					{
						CreateIndexOnColumn();
					}
					else
					{
						_messenger.Message("Some error in alloting query");
					}
				}
				else //invalid query
				{
					_messenger.Message("Invalid Query");
				}
			}
			catch (Exception e)
			{
				_messenger.Message(e.Message);
			}
		}

		#region Q U E R I E S

		private void CreateDatabase()
		{
			String dbName = root
				.ChildNodes[0].ChildNodes[2].ChildNodes[0]
				.Token.Value.ToString();
			
			subQueryHandler.CreateDatabase(dbName);
			_messenger.Message("Database Created");
		}

		private void DropDatabase()
		{
			String dbName = root
				.ChildNodes[0].ChildNodes[2].ChildNodes[0]
				.Token.Value.ToString();

			subQueryHandler.DropDatabase(dbName);
			_messenger.Message("Database Dropped");
		}

		private void ChangeDatabase()
		{
			String dbName = root
				.ChildNodes[0].ChildNodes[1].ChildNodes[0]
				.Token.Value.ToString();

			subQueryHandler.UseDatabase(dbName);
			_messenger.Message("Database Changed");
		}

		private void ShowTables()
		{
			List<String> tableNames = subQueryHandler.ShowTables();
			_messenger.Message("===TABLES===");
			foreach (string tableName in tableNames)
				_messenger.Message(tableName);
		}

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

		private void DropTable()
		{
			String tableName = root
				.ChildNodes[0].ChildNodes[2].ChildNodes[0]
				.Token.Value.ToString();

			subQueryHandler.DropTable(tableName);
			_messenger.Message("Table Dropped");
		}

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

		private void SelectRecordsFromTable()
		{
			//todo implement joins
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
				Console.WriteLine(colList.ChildNodes.Count);
				foreach (var colNameNode in colList.ChildNodes)
				{
					String colName = colNameNode.ChildNodes[0].ChildNodes[0].ChildNodes[0].Token.ValueString;
					colNames.Add(colName);
				}
				selectedColumns = subQueryHandler.GetColumnIndicesFromName(tableName, colNames);
			}

			//todo Selecting the tables - IF DOING JOINS

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

			//Dispaly the records in proper format
			foreach (Record record in possibleRecords.Values)
			{
				List<String> fields = record.Fields;
				String recAsString = "";
				foreach (int index in selectedColumns)
				{
					recAsString += (fields[index] + " | ");
				}
				_messenger.Message(recAsString);
			}
		}

		private void CreateIndexOnColumn()
		{
			ParseTreeNode topNode = root.ChildNodes[0];

			String tableName = topNode
					.ChildNodes[5].ChildNodes[0]
					.Token.ValueString;

			ParseTreeNode colList = topNode
				.ChildNodes[6];
			String colName = colList
				.ChildNodes[0].ChildNodes[0].ChildNodes[0]
				.Token.ValueString;

			subQueryHandler.CreateIndex(tableName, colName);
			_messenger.Message("Index Created");
		}

		#endregion

		#region Helper Functions

		/**
		 * Evaluates WHERE for binary expressions
		 * Binary Expression should be of form
		 *		colName
		 *		conditionType
		 *		Value
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
				String v2 = binExpr.ChildNodes[2].Token.ValueString;

				Condition condition = subQueryHandler.GetCondition(tableName, v1, opValue, v2);
				finalResult = subQueryHandler.SelectRecordsFromTable(tableName, condition);
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
		
		#endregion

	}
}