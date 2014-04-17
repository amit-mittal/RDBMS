using System;
using Irony.Parsing;

namespace RDBMS.QueryManager
{
	//after evaluating the query, call optimizer to find order
	internal class QueryParser
	{
		private String query;

		public QueryParser(String query)
		{
			this.query = query;
		}

		public ParseTreeNode GetRoot()
		{
			SqlGrammar grammar = new SqlGrammar();
			LanguageData language = new LanguageData(grammar);
			Parser parser = new Parser(language);
			ParseTree parseTree = parser.Parse(query);
			ParseTreeNode root = parseTree.Root;

			return root;
		}

		public void PrintNode(ParseTreeNode node, int level)
		{
			foreach (ParseTreeNode child in node.ChildNodes)
			{
				for (int i = 0; i < level; i++)
					Console.Write("-");

				Console.WriteLine(child.Term);
				PrintNode(child, level + 1);
			}
		}
	}
}