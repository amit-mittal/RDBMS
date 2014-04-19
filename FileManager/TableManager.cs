using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Kw.Data;
using RDBMS.DataStructure;
using RDBMS.SpaceManager;
using RDBMS.Util;

namespace RDBMS.FileManager
{
	/**
	 * Handles queries related to the table
	 * 
	 * Tests in TableManagerTest.cs
	 */

	internal class TableManager
	{
		public Table table;
		public StorageManager storageManager;

		#region Constructors

		public TableManager()
		{
			storageManager = new StorageManager();
		}

		/**
		 * This constructor would be used to init the conf
		 * of table
		 * Will be needed when using select, insert, etc.
		 */

		public TableManager(String dbName, String tableName)
		{
			String path = GetFilePath.Table(dbName, tableName);
			String conf = GetFilePath.TableConf(dbName, tableName);
			if (!Directory.Exists(path))
			{
				throw new Exception("Table does not exist");
			}
			else
			{
				storageManager = new StorageManager();
				table = (Table) Converter.FileToObject(conf);
			}
		}

		#endregion

		#region Table Basic Methods

		/**
		 * Creates all the folders/files related to a new table
		 * IMPORTANT - Presently assumed indexColumns would be empty
		 * Index can be added on later using AddIndex
		 */

		public void CreateTable(string dbName, string tableName, List<Column> columns)
		{
			String path = GetFilePath.Table(dbName, tableName);
			String conf = GetFilePath.TableConf(dbName, tableName);
			String records = GetFilePath.TableRecords(dbName, tableName);
			if (Directory.Exists(path))
			{
				throw new Exception("Table already exists");
			}
			else
			{
				table = new Table(dbName, tableName, columns, new List<Column>());
				storageManager.CreateFolder(path); //table folder
				Converter.ObjectToFile(table, conf); //schema file of table
				byte[] record = Converter.CharToBytes(new char[table.GetSizeOfRecordArray()]);
				storageManager.CreateFile(records, record.Length, true); //records file of table
			}
		}

		public void DropTable(String dbName, String tableName)
		{
			String path = GetFilePath.Table(dbName, tableName);
			if (!Directory.Exists(path))
			{
				throw new Exception("Table does not exist");
			}
			else
			{
				storageManager.DropFolder(path);
			}
		}

		public Table DescribeTable()
		{
			return table;
		}

		/**
		 * Writes the table data structure in memory
		 * to the conf file
		 */

		public void UpdateTableToFile()
		{
			String conf = GetFilePath.TableConf(table.DbName, table.Name);
			Converter.ObjectToFile(table, conf);
		}

		#endregion

		#region Index Methods

		/**
		 * Adding index to particular column
		 * if some records were already there
		 */
		public void AddIndex(Column index)
		{
			//making the index file
			String indexFile = GetFilePath.TableColumnIndex(table.DbName, table.Name, index.Name);
			Dictionary<int, Record> allRecords = GetAddressRecordDict(null);
			int position = table.GetColumnIndex(index);

			BtreeDictionary<Index<int>, int> intbptree;
			BtreeDictionary<Index<String>, int> stringbptree;
			BtreeDictionary<Index<double>, int> doublebptree;

			if (index.Type == Column.DataType.Int)
			{
				intbptree = new BtreeDictionary<Index<int>, int>();
				//making a b+tree
				foreach (KeyValuePair<int, Record> keyValuePair in allRecords)
				{
					String s = keyValuePair.Value.Fields[position];
					if (s != null)
					{
						Index<int> idx = new Index<int>(int.Parse(s), keyValuePair.Key);
						intbptree.Add(idx, keyValuePair.Key);
					}
					//BUG not inserting empty or null values as of now
					/*else
					{
						Index<int> idx = new Index<int>(Constants.NullAsInt, keyValuePair.Key);
						intbptree.Add(idx, keyValuePair.Key);
					}*/
				}

				//writing b+tree to the file
				Converter.ObjectToFile(intbptree, indexFile);
			}
			else if (index.Type == Column.DataType.Double)
			{
				doublebptree = new BtreeDictionary<Index<double>, int>();
				//making a b+tree
				foreach (KeyValuePair<int, Record> keyValuePair in allRecords)
				{
					String s = keyValuePair.Value.Fields[position];
					if (s != null)
					{
						Index<double> idx = new Index<double>(double.Parse(s), keyValuePair.Key);
						doublebptree.Add(idx, keyValuePair.Key);
					}
				}

				//writing b+tree to the file
				Converter.ObjectToFile(doublebptree, indexFile);
			}
			else if (index.Type == Column.DataType.Char)
			{
				stringbptree = new BtreeDictionary<Index<String>, int>();
				//making a b+tree
				foreach (KeyValuePair<int, Record> keyValuePair in allRecords)
				{
					String s = keyValuePair.Value.Fields[position];
					if (s != null)
					{
						Index<String> idx = new Index<String>(s, keyValuePair.Key);
						stringbptree.Add(idx, keyValuePair.Key);
					}
				}

				//writing b+tree to the file
				Converter.ObjectToFile(stringbptree, indexFile);
			}

			//adding entry to list in table and update back to file also
			table.IndexColumns.Add(index);
			UpdateTableToFile();
		}

		/**
		 * Adding primary key to particular column
		 * if some records were already there
		 * todo test pending
		 */
		public void AddPrimaryKey(Column pKey)
		{
			AddIndex(pKey);
			table.PrimaryKey = pKey;
			UpdateTableToFile();
		}

		/**
		 * Drop index from a particular column
		 */
		public void DropIndex(Column index)
		{
			//making the index file
			String indexFile = GetFilePath.TableColumnIndex(table.DbName, table.Name, index.Name);
			storageManager.DropFile(indexFile);

			//removing entry from list in table and update back to file also
			table.IndexColumns.Remove(index);
			UpdateTableToFile();
		}

		/**
		 * Drop primary key of the table
		 */
		public void DropPrimaryKey()
		{
			DropIndex(table.PrimaryKey);
			table.PrimaryKey = null;
			UpdateTableToFile();
		}

		#endregion

		#region Insert Methods

		/**
		 * Inserts a record into the indices of the table
		 * record -> to be inserted
		 * address -> entry in the records file
		 */

		public void InsertRecordToIndices(Record record, int address)
		{
			foreach (Column indexColumn in table.IndexColumns)
			{
				int position = table.GetColumnIndex(indexColumn);
				String indexFile = GetFilePath.TableColumnIndex(table.DbName, table.Name, table.Columns[position].Name);
				String value = record.Fields[position];
				if (table.Columns[position].Type == Column.DataType.Int)
				{
					if (value != null)
					{
						BtreeDictionary<Index<int>, int> intbptree = (BtreeDictionary<Index<int>, int>) Converter.FileToObject(indexFile);
						intbptree.Add(new Index<int>(int.Parse(value), address), address);
						Converter.ObjectToFile(intbptree, indexFile);
					}
				}
				else if (table.Columns[position].Type == Column.DataType.Double)
				{
					if (value != null)
					{
						BtreeDictionary<Index<double>, int> doublebptree =
							(BtreeDictionary<Index<double>, int>) Converter.FileToObject(indexFile);
						doublebptree.Add(new Index<double>(double.Parse(value), address), address);
						Converter.ObjectToFile(doublebptree, indexFile);
					}
				}
				else if (table.Columns[position].Type == Column.DataType.Char)
				{
					if (value != null)
					{
						BtreeDictionary<Index<String>, int> stringbptree =
							(BtreeDictionary<Index<String>, int>) Converter.FileToObject(indexFile);
						stringbptree.Add(new Index<String>(value, address), address);
						Converter.ObjectToFile(stringbptree, indexFile);
					}
				}
			}
		}

		/**
		 * Inserts a record into the table
		 * Those fields should be null which have not been set
		 * @returns Address where it is inserted
		 */

		public int InsertRecord(Record record)
		{
			//Inserting entry into record file
			String recordsPath = GetFilePath.TableRecords(table.DbName, table.Name);
			Stream fs = new FileStream(recordsPath, FileMode.OpenOrCreate);
			int address = storageManager.Allocate(recordsPath, fs);
			char[] recordStream = table.RecordToCharArray(record);
			storageManager.Write(fs, address, Converter.CharToBytes(recordStream));
			fs.Close();

			return address;
		}

		#endregion

		#region AddressDictionary Methods

		/**
		 * @returns Dictionary<key, Record>
		 * key => address
		 * Record => record corresponding to the address
		 * which satisfies the given condition
		 * 
		 * if condition is null means select all records
		 * Does Linear Search of all the records
		 */

		public Dictionary<int, Record> GetAddressRecordDict(Condition condition)
		{
			int index = -1; //means unitialized
			if (condition != null)
			{
				index = table.GetColumnIndex(condition.Attribute);
				if (index == -1) //no column matched
					return new Dictionary<int, Record>();
			}

			//initializing the variables
			Dictionary<int, Record> finalDict = new Dictionary<int, Record>();
			Stream fs = new FileStream(GetFilePath.TableRecords(table.DbName, table.Name), FileMode.OpenOrCreate);
			int recordSize = storageManager.GetRecordSize(fs);
			int endOfFile = storageManager.GetEndOfFile(fs);

			//getting the whole bitmap here so that we know there is no record there
			Stream bitmapFs = new FileStream(GetFilePath.TableRecordsBitmap(table.DbName, table.Name), FileMode.OpenOrCreate);
			List<int> bitmapList = Converter.BytesToIntList(storageManager.GetCompleteFile(bitmapFs));
			HashSet<int> bitmapSet = new HashSet<int>(bitmapList);
			bitmapFs.Close();

			//traversing the whole file
			int NumRecords = Constants.MaxSelectRecords; //BUG use this if read more than a chunk
			for (int offset = storageManager.HeaderSize; offset < endOfFile; offset += recordSize)
			{
				if (!bitmapSet.Contains(offset))
				{
					char[] recordStr = new char[recordSize];
					Converter.BytesToChar(storageManager.Read(fs, offset)).CopyTo(recordStr, 0);

					Record record = table.StringToRecord(new string(recordStr));
					if (condition == null)
					{
						finalDict.Add(offset, record);
					}
					else if (IsRecordValid(record, condition, index))
					{
						finalDict.Add(offset, record);
					}
				}
			}
			fs.Close();
			return finalDict;
		}

		/**
		 * @returns Dictionary<key, Record>
		 * key => address
		 * Record => record corresponding to the address
		 * which satisfies the given condition
		 * 
		 * Does searching in the btree
		 * Call this function only if that column is an index
		 * @param name="condition" NotNullable
		 */

		public Dictionary<int, Record> GetAddressRecordDictOnIndex(Condition condition)
		{
			//initializing the variables
			Dictionary<int, Record> finalDict = new Dictionary<int, Record>();
			List<int> addresses = new List<int>();

			int index = -1; //means unitialized
			if (condition != null)
				index = table.GetColumnIndex(condition.Attribute);
			if (index == -1 || condition == null) //not feasible columns
				return finalDict;

			String indexFile = GetFilePath.TableColumnIndex(table.DbName, table.Name, condition.Attribute.Name);
			String recordsFile = GetFilePath.TableRecords(table.DbName, table.Name);

			//getting the b+ tree into the main memory
			BtreeDictionary<Index<int>, int> intbptree;
			BtreeDictionary<Index<double>, int> doublebptree;
			BtreeDictionary<Index<String>, int> stringbptree;

			//traversing the whole btree
			if (condition.Attribute.Type == Column.DataType.Int)
			{
				//getting tree into main memory
				intbptree = (BtreeDictionary<Index<int>, int>) Converter.FileToObject(indexFile);

				//initialized possible feasible range of the records
				IEnumerable range;
				if (condition.Sign == Condition.ConditionType.Equal)
					range = intbptree.BetweenKeys(new Index<int>(int.Parse(condition.Value), int.MinValue),
						new Index<int>(int.Parse(condition.Value), int.MaxValue));
				else if (condition.Sign == Condition.ConditionType.Less || condition.Sign == Condition.ConditionType.LessEqual)
					range = intbptree.BetweenKeys(new Index<int>(int.MinValue, int.MinValue),
						new Index<int>(int.Parse(condition.Value), int.MaxValue));
				else
					range = intbptree.BetweenKeys(new Index<int>(int.Parse(condition.Value), int.MinValue),
						new Index<int>(int.MaxValue, int.MaxValue));

				//initialized the selected addresses list
				foreach (KeyValuePair<Index<int>, int> pair in range)
				{
					if (IsColumnValueValid(pair.Key.Key.ToString(), condition, index))
						addresses.Add(pair.Value);
				}
			}
			else if (condition.Attribute.Type == Column.DataType.Double)
			{
				//getting tree into main memory
				doublebptree = (BtreeDictionary<Index<double>, int>) Converter.FileToObject(indexFile);

				//initialized possible feasible range of the records
				IEnumerable range;
				if (condition.Sign == Condition.ConditionType.Equal)
					range = doublebptree.BetweenKeys(new Index<double>(double.Parse(condition.Value), int.MinValue),
						new Index<double>(double.Parse(condition.Value), int.MaxValue));
				else if (condition.Sign == Condition.ConditionType.Less || condition.Sign == Condition.ConditionType.LessEqual)
					range = doublebptree.BetweenKeys(new Index<double>(double.MinValue, int.MinValue),
						new Index<double>(double.Parse(condition.Value), int.MaxValue));
				else
					range = doublebptree.BetweenKeys(new Index<double>(double.Parse(condition.Value), int.MinValue),
						new Index<double>(double.MaxValue, int.MaxValue));

				//initialized the selected addresses list
				foreach (KeyValuePair<Index<double>, int> pair in range)
				{
					if (IsColumnValueValid(pair.Key.Key.ToString(), condition, index))
						addresses.Add(pair.Value);
				}
			}
			else if (condition.Attribute.Type == Column.DataType.Char)
			{
				//getting tree into main memory
				stringbptree = (BtreeDictionary<Index<String>, int>) Converter.FileToObject(indexFile);

				//initialized possible feasible range of the records
				IEnumerable range;
				if (condition.Sign == Condition.ConditionType.Equal)
					range = stringbptree.BetweenKeys(new Index<String>(condition.Value, int.MinValue),
						new Index<String>(condition.Value, int.MaxValue));
				else
					throw new Exception("This type of condition for strings is not supported");
				//BUG not implementing less than greater than for strings

				//initialized the selected addresses list
				foreach (KeyValuePair<Index<String>, int> pair in range)
				{
					if (IsColumnValueValid(pair.Key.Key, condition, index))
						addresses.Add(pair.Value);
				}
			}

			//getting all records corresponding to the addresses list
			Stream fs = new FileStream(recordsFile, FileMode.OpenOrCreate);
			foreach (int address in addresses)
			{
				byte[] recordsBytes = storageManager.Read(fs, address);
				Record record = table.StringToRecord(new string(Converter.BytesToChar(recordsBytes)));
				finalDict.Add(address, record);
			}
			fs.Close();

			return finalDict;
		}

		#endregion

		#region Update Methods

		/**
		 * Gets the dictionary containing the address => updated record
		 */
		public void UpdateRecordToIndices(Dictionary<int, Record> oldRecords, Dictionary<int, Record> updatedRecords)
		{
			DeleteRecordsFromIndices(oldRecords);
			foreach (KeyValuePair<int, Record> pair in updatedRecords)
			{
				InsertRecordToIndices(pair.Value, pair.Key);
			}
		}

		/**
		 * Gets the dictionary containing the address => updated record
		 */

		public void UpdateRecord(Dictionary<int, Record> updatedRecords)
		{
			String recordsPath = GetFilePath.TableRecords(table.DbName, table.Name);
			Stream fs = new FileStream(recordsPath, FileMode.OpenOrCreate);
			foreach (int address in updatedRecords.Keys)
			{
				char[] recordStream = table.RecordToCharArray(updatedRecords[address]);
				storageManager.Write(fs, address, Converter.CharToBytes(recordStream));
			}
			fs.Close();

			//remove old key
			//add new key
		}

		#endregion

		#region Delete Methods

		/**
		 * Takes dictionary containing the address => to be deleted record
		 * And deletes that particular column value from the index path
		 */
		public void DeleteRecordsFromIndices(Dictionary<int, Record> uselessRecords)
		{
			foreach (Column indexColumn in table.IndexColumns)
			{
				int position = table.GetColumnIndex(indexColumn);
				String indexFile = GetFilePath.TableColumnIndex(table.DbName, table.Name, table.Columns[position].Name);

				if (table.Columns[position].Type == Column.DataType.Int)
				{
					BtreeDictionary<Index<int>, int> intbptree = (BtreeDictionary<Index<int>, int>) Converter.FileToObject(indexFile);
					foreach (KeyValuePair<int, Record> pair in uselessRecords)
					{
						int address = pair.Key;
						String value = pair.Value.Fields[position];
						if (value != null)
							intbptree.Remove(new Index<int>(int.Parse(value), address));
					}
					Converter.ObjectToFile(intbptree, indexFile);
				}
				else if (table.Columns[position].Type == Column.DataType.Double)
				{
					BtreeDictionary<Index<double>, int> doublebptree =
						(BtreeDictionary<Index<double>, int>) Converter.FileToObject(indexFile);
					foreach (KeyValuePair<int, Record> pair in uselessRecords)
					{
						int address = pair.Key;
						String value = pair.Value.Fields[position];
						if (value != null)
							doublebptree.Remove(new Index<double>(double.Parse(value), address));
					}
					Converter.ObjectToFile(doublebptree, indexFile);
				}
				else if (table.Columns[position].Type == Column.DataType.Char)
				{
					BtreeDictionary<Index<String>, int> stringbptree =
						(BtreeDictionary<Index<String>, int>) Converter.FileToObject(indexFile);
					foreach (KeyValuePair<int, Record> pair in uselessRecords)
					{
						int address = pair.Key;
						String value = pair.Value.Fields[position];
						if (value != null)
							stringbptree.Remove(new Index<String>(value, address));
					}
					Converter.ObjectToFile(stringbptree, indexFile);
				}
			}
		}

		/**
		 * Takes dictionary containing the address => to be deleted record
		 */

		public void DeleteRecords(Dictionary<int, Record> uselessRecords)
		{
			String recordsPath = GetFilePath.TableRecords(table.DbName, table.Name);
			Stream fs = new FileStream(recordsPath, FileMode.OpenOrCreate);
			List<int> addresses = new List<int>(uselessRecords.Keys);
			storageManager.Deallocate(recordsPath, addresses);
			fs.Close();
		}

		#endregion

		#region Select Methods

		/**
		 * @returns List<Record> 
		 * which satisfies the given condition
		 * 
		 * if condition is null means select all records
		 */

		public List<Record> SelectRecords(Condition condition)
		{
			Dictionary<int, Record> selectedRecords = GetAddressRecordDict(condition);
			return new List<Record>(selectedRecords.Values);
		}

		/**
		 * @returns List<Record> 
		 * which satisfies the given condition on the indexed column
		 */

		public List<Record> SelectRecordsOnIndex(Condition condition)
		{
			Dictionary<int, Record> selectedRecords = GetAddressRecordDictOnIndex(condition);
			return new List<Record>(selectedRecords.Values);
		}

		#endregion

		#region Helper Functions

		/**
		 * Tells if the record satisfies the condition or not
		 * @param name="index" column index on which condition is applied
		 */

		private bool IsRecordValid(Record record, Condition condition, int index)
		{
			String value = record.Fields[index];
			return IsColumnValueValid(value, condition, index);
		}

		/**
		 * Tells if the column value satisfies the condition or not
		 * @param name="index" column index on which condition is applied
		 */

		private bool IsColumnValueValid(String value, Condition condition, int index)
		{
			if (value == null)
			{
				return false;
			}

			if (table.Columns[index].Type == Column.DataType.Int)
			{
				if (value.Length == 0)
					return false;
				return condition.CompareIntegers(int.Parse(value));
			}

			if (table.Columns[index].Type == Column.DataType.Double)
			{
				if (value.Length == 0)
					return false;
				return condition.CompareDoubles(double.Parse(value));
			}

			if (table.Columns[index].Type == Column.DataType.Char)
			{
				return condition.CompareStrings(value);
			}

			return false;
		}

		/**
		 * Checks if record contains some error or not
		 * Thorws exception if some error found
		 */

		public void CheckRecord(Record record)
		{
			//checking if some error is in the field values
			for (int i = 0; i < record.Fields.Count; i++)
			{
				String value = record.Fields[i];
				if (table.Columns[i].Type == Column.DataType.Int)
				{
					int dummy;
					if (value != null && int.TryParse(value, out dummy) == false)
					{
						throw new Exception((i + 1) + " parameter expects an integer");
					}
				}
				else if (table.Columns[i].Type == Column.DataType.Double)
				{
					double dummy;
					if (value != null && double.TryParse(value, out dummy) == false)
					{
						throw new Exception((i + 1) + " parameter expects a double");
					}
				}
				else if (table.Columns[i].Type == Column.DataType.Char)
				{
				}

				if (value != null && value.Length > table.Columns[i].Length)
				{
					throw new Exception((i + 1) + " parameter has length more than alloted to it");
				}
			}
		}

		#endregion
	}
}