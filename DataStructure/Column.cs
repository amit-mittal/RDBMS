﻿using System;

namespace RDBMS.DataStructure{
	[Serializable]
	class Column
	{
		public enum DataType
		{
			Int,
			Float,
			Char
		};

		public DataType Type;
		public char[] Name = new char[50];
		public int Length;

		public Column()
		{
		}

		public Column(DataType type, char[] name, int length)
		{
			Type = type;
			name.CopyTo(Name, 0);
			Length = length;
		}

		public Column(DataType type, String name, int length)
		{
			Type = type;
			name.ToCharArray().CopyTo(Name, 0);
			Length = length;
		}

		public override bool Equals(object obj)
		{
			Column col = (Column) obj;
			if(Type != col.Type)
				return false;
			if(!Name.ToString().Equals(col.Name.ToString()))
				return false;
			if(Length != col.Length)
				return false;
			return true;
		}
	}
}
