using System;

namespace RDBMS.DataStructure{
	class Column
	{
		public enum DataType
		{
			Int,
			Float,
			Char
		};

		public DataType Type;
		public String Name;
		public int Length;

		public Column(DataType type, String name, int length)
		{
			Type = type;
			Name = name;
			Length = length;
		}
	}
}
