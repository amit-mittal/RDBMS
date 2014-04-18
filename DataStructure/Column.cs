using System;
using RDBMS.Util;

namespace RDBMS.DataStructure
{
	[Serializable]
	internal class Column
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

		#region CONSTRUCTORS

		public Column(DataType type, String name, int length)
		{
			Type = type;
			Name = name;
			if (type == DataType.Int)
				Length = Constants.IntStringLen;
			else if (type == DataType.Double)
				Length = Constants.DoubleStringLen;
			else if (type == DataType.Char)
				Length = length;
		}

		public Column(String type, String name, String length)
		{
			Name = name;
			if (int.TryParse(length, out Length) == false)
				Length = Constants.DefaultLen;

			if(type == "char")
				Type = DataType.Char;
			else if(type == "int")
				Type = DataType.Int;
			else if (type == "double")
				Type = DataType.Double;
			else
			throw new Exception("Data Type not supported");
		}

		#endregion

		public override bool Equals(object obj)
		{
			Column col = (Column) obj;
			if (Type != col.Type)
				return false;
			if (!Name.Equals(col.Name))
				return false;
			if (Length != col.Length)
				return false;
			return true;
		}
	}
}