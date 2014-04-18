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

			_messenger.Message(tableName);

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

			List<String> values = new List<String>();

			if (allFieldsNode.ChildNodes.Count != allValuesNode.ChildNodes.Count)
				throw new Exception("Number of Columns and Values are not matching");

			String tableName = topNode
				.ChildNodes[2].ChildNodes[0]
				.Token.ValueString;

			Table table = subQueryHandler.DescribeTable(tableName);

			foreach (var column in table.Columns)
			{
				bool found = false;
				for (int i = 0; i < allFieldsNode.ChildNodes.Count; i++)
				{
					String colName = allFieldsNode.ChildNodes[i].ChildNodes[0].Token.ValueString;
					if (colName == column.Name)
					{
						found = true;
						String value = allValuesNode.ChildNodes[i].Token.ValueString;
						values.Add(value);
					}
				}
				if (!found)
					values.Add(null);
			}
			
			subQueryHandler.InsertRecordToTable(tableName, new Record(values));
			_messenger.Message("Record Succesfully inserted");
		}

		private void UpdateRecordOfTable()
		{
			ParseTreeNode topNode = root.ChildNodes[0];
			
			String tableName = topNode
					.ChildNodes[1].ChildNodes[0]
					.Token.ValueString;

			//todo get the record from assignments

			ParseTreeNode allAssignments = topNode.ChildNodes[3];
			if (topNode.ChildNodes[4].ChildNodes.Count > 1)
			{
				ParseTreeNode binExpr = topNode.ChildNodes[4].ChildNodes[1];

				SolveWhereClause(tableName, binExpr);
			}
			else
			{
				subQueryHandler.UpdateRecordToTable(tableName, null, (Condition) null);
			}
			

			_messenger.Message("Record(s) successfully updated");
		}

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

				//optimized query
				if (result1.Count > result2.Count)
				{
					foreach (var pair in result2)
					{
						if(result1.ContainsKey(pair.Key) && result2.ContainsKey(pair.Key))
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
			}
			else if (opValue == "or")
			{
				Dictionary<int, Record> result1 = SolveWhereClause(tableName, binExpr.ChildNodes[0]);
				Dictionary<int, Record> result2 = SolveWhereClause(tableName, binExpr.ChildNodes[2]);

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

		#endregion

		#region Helper Functions

		
		#endregion

	}
}