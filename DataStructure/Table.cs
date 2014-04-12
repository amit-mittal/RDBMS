using System;
using System.Collections.Generic;
using System.Linq;

namespace RDBMS.DataStructure
{
	[Serializable]
	class Table
	{
		public String DbName;
		public String Name;
		public List<Column> Columns;
		//TODO: Also add keys and indices later

		public Table(String dbName, String name, List<Column> columns)
		{
			DbName = dbName;
			Name = name;
			Columns = columns;
		}

		/**
		 * Returns size of char array needed to stream the
		 * record
		 */
		public int GetSizeOfRecordArray()
		{
			return Columns.Sum(col => col.Length);
		}

		/**
		 * Returns a fixed length char array from a record
		 * using the list of column
		 */
		public char[] RecordToCharArray(Record record)
		{
			int size = GetSizeOfRecordArray();
			char[] arr = new char[size];
			int prev = 0;
			for (int i = 0; i < Columns.Count; i++)
			{
				if(record.Fields[i] != null)
					record.Fields[i].ToCharArray().CopyTo(arr, prev);
				prev += Columns[i].Length;
			}
			return arr;
		}

		/**
		 * Returns an instance of Record from a fixed
		 * length string using the list of columns
		 * 
		 * To get string for this format - use 
		 * Converter.BytesToChar function
		 */
		public Record StringToRecord(String str)
		{
			int prev = 0;
			List<String> fields = new List<string>();
			foreach (Column col in Columns)
			{
				String s = str.Substring(prev, col.Length).Replace("\0", string.Empty);
				if (col.Type == Column.DataType.Int)
				{
					int v = Int32.Parse(s);
					fields.Add(v.ToString());
				}
				else if (col.Type == Column.DataType.Double)
				{
					Double v = Double.Parse(s);
					fields.Add(v.ToString());
				}
				else if (col.Type == Column.DataType.Char)
				{
					fields.Add(s);
				}
				prev += col.Length;
			}
			return new Record(fields);
		}
	}
}
