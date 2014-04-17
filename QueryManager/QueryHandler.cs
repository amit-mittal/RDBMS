using System;
using Irony.Parsing;
using RDBMS.FileManager;

namespace RDBMS.QueryManager
{
	internal class QueryHandler
	{
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
					Console.WriteLine("Valid Query");
					parser.PrintNode(root, 0);
					Console.WriteLine("====================");

					if (root.ChildNodes[0].Term.ToString() == "createDatabaseStmt")
					{
						CreateDatabase();
						Console.WriteLine("Database Created");
					}
					else if (root.ChildNodes[0].Term.ToString() == "createDatabaseStmt")
					{
						CreateDatabase();
						Console.WriteLine("Database Created");
					}
					else if (root.ChildNodes[0].Term.ToString() == "createDatabaseStmt")
					{
						CreateDatabase();
						Console.WriteLine("Database Created");
					}
					else
					{
						Console.WriteLine("Some error in alloting query");
					}
				}
				else //invalid query
				{
					Console.WriteLine("Invalid Query");
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}

		private void CreateDatabase()
		{
			String dbName = root
				.ChildNodes[0].ChildNodes[2].ChildNodes[0]
				.Token.Value.ToString();
			
			subQueryHandler.CreateDatabase(dbName);
		}
	}
}