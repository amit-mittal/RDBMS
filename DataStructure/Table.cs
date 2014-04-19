using System;
using System.Collections.Generic;
using System.Linq;

namespace RDBMS.DataStructure
{
	[Serializable]
	internal class Table
	{
		public String DbName;
		public String Name;
		public List<Column> Columns;
		public List<Column> IndexColumns;
		public Column PrimaryKey;

		#region Constructors

		public Table(String dbName, String name, List<Column> columns, List<Column> indexColumns)
		{
			DbName = dbName;
			Name = name;
			Columns = columns;
			IndexColumns = indexColumns;
			PrimaryKey = null;
		}

		public Table(String dbName, String name, List<Column> columns, List<Column> indexColumns, Column primaryKey)
		{
			DbName = dbName;
			Name = name;
			Columns = columns;
			IndexColumns = indexColumns;
			PrimaryKey = primaryKey;
		}

		#endregion

		/**
		 * @returns true if column of the table 
		 * has been indexed
		 */
		public bool CheckIfColumnIndexed(Column column)
		{
			foreach (Column col in IndexColumns)
			{
				if (col.Equals(column))
					return true;
			}
			return false;
		}

		/**
		 * @returns true if Condition follows all the conditions
		 * related to table
		 */
		public bool CheckIfConditionValid(Condition condition)
		{
			if (condition == null)
				return true;

			if (GetColumnIndex(condition.Attribute) == -1)
				return false;

			return condition.CheckIfCondtionValueValid();
		}

		/**
		 * Returns the index of the column in the list
		 */
		public int GetColumnIndex(Column col)
		{
			for (int i = 0; i < Columns.Count; i++) //iterating throw each column
			{
				if (Columns[i].Equals(col))
					return i;
			}
			return -1; //if no column found
		}

		/**
		 * Returns the column corresponding to index
		 * in the list
		 */
		public Column GetColumn(int index)
		{
			return Columns[index];
		}

		/**
		 * Returns the column corresponding to name
		 * in the list
		 */
		public Column GetColumnByName(String colName)
		{
			foreach (var column in Columns)
			{
				if (column.Name == colName)
					return column;
			}
			throw new Exception("Column does not exist");
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
				if (record.Fields[i] != null)
					record.Fields[i].ToCharArray().CopyTo(arr, prev);
				prev += Columns[i].Length;
			}
			return arr;
		}

		/**
		 * Gets the position of the column in the 
		 * serialized record
		 */
		public int GetPositionInRecord(Column column)
		{
			int prev = 0;
			foreach (Column col in Columns)
			{
				if (column.Equals(col))
					break;
				prev += col.Length;
			}
			return prev;
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
					int v;
					if (Int32.TryParse(s, out v) == false)
					{
						fields.Add(null);
					}
					else
					{
						fields.Add(v.ToString());
					}
				}
				else if (col.Type == Column.DataType.Double)
				{
					double v;
					if (double.TryParse(s, out v) == false)
					{
						fields.Add(null);
					}
					else
					{
						fields.Add(v.ToString());
					}
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