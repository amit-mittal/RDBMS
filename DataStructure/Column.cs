using System;

namespace RDBMS.DataStructure{
	[Serializable]
	class Column
	{
		public enum DataType
		{
			Int,
			Double,
			Char
		};

		public DataType Type;
		public String Name;
		public int Length;

		public Column()
		{
		}

		public Column(DataType type, String name, int length)
		{
			Type = type;
			Name = name;
			if (type == DataType.Int)
				Length = sizeof (Int32);
			else if (type == DataType.Double)
				Length = sizeof (Double);
			else if (type == DataType.Char)
				Length = length;
		}

		public override bool Equals(object obj)
		{
			Column col = (Column) obj;
			if(Type != col.Type)
				return false;
			if(!Name.Equals(col.Name))
				return false;
			if(Length != col.Length)
				return false;
			return true;
		}
	}
}
