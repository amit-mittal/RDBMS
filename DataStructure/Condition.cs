
namespace RDBMS.DataStructure
{
	class Condition
	{
		public enum ConditionType
		{
			Equal,
			Less,
			Greater,
			LessEqual,
			GreaterEqual
		}

		public Column Attribute;
		public ConditionType Sign;
		public int Value;//TODO think if add value or attribute

		public Condition(Column attribute, ConditionType sign, int value)
		{
			Attribute = attribute;
			Sign = sign;
			Value = value;
		}
	}
}
