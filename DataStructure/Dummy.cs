using System;

namespace RDBMS.DataStructure
{
	[Serializable]
	internal class Dummy
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

		public Dummy()
		{
		}

		public Dummy(DataType type, char[] name, int length)
		{
			Type = type;
			name.CopyTo(Name, 0);
			Length = length;
		}

		public Dummy(DataType type, String name, int length)
		{
			Type = type;
			name.ToCharArray().CopyTo(Name, 0);
			Length = length;
		}

		public override bool Equals(object obj)
		{
			Dummy col = (Dummy) obj;
			if (Type != col.Type)
				return false;
			if (!Name.ToString().Equals(col.Name.ToString()))
				return false;
			if (Length != col.Length)
				return false;
			return true;
		}
	}
}