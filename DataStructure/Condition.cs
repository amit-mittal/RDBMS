using System;
using RDBMS.Util;

namespace RDBMS.DataStructure
{
	internal class Condition
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
		public String Value;

		public Condition(Column attribute, ConditionType sign, String value)
		{
			Attribute = attribute;
			Sign = sign;
			Value = value;
		}

		/**
		 * Checks if condition object id valid or not
		 */

		public bool CheckIfCondtionValueValid()
		{
			if (Attribute.Type == Column.DataType.Char)
			{
				if (Value == null)
					return false;
				return true;
			}
			if (Attribute.Type == Column.DataType.Int)
			{
				if (Value == null)
					return false;
				int dummy;
				return int.TryParse(Value, out dummy);
			}
			if (Attribute.Type == Column.DataType.Double)
			{
				if (Value == null)
					return false;
				double dummy;
				return double.TryParse(Value, out dummy);
			}
			return false;
		}

		/**
		 * Compare 2 integers
		 * @returns bool
		 * true - if condition is satisfied
		 * else false
		 */
		public bool CompareIntegers(int v1)
		{
			int v2 = int.Parse(Value);
			switch (Sign)
			{
				case ConditionType.Equal:
				{
					if (v1 == v2)
						return true;
					return false;
				}
				case ConditionType.Less:
				{
					if (v1 < v2)
						return true;
					return false;
				}
				case ConditionType.Greater:
				{
					if (v1 > v2)
						return true;
					return false;
				}
				case ConditionType.LessEqual:
				{
					if (v1 <= v2)
						return true;
					return false;
				}
				case ConditionType.GreaterEqual:
				{
					if (v1 >= v2)
						return true;
					return false;
				}
				default:
				{
					return false;
				}
			}
		}

		/**
		 * Compare 2 decimal no.s
		 * @returns bool
		 * true - if condition is satisfied
		 * else false
		 */
		public bool CompareDoubles(double v1)
		{
			double v2 = double.Parse(Value);
			switch (Sign)
			{
				case ConditionType.Equal:
				{
					if (Math.Abs(v1 - v2) <= Constants.Epsilon)
						return true;
					return false;
				}
				case ConditionType.Less:
				{
					if (v1 < v2)
						return true;
					return false;
				}
				case ConditionType.Greater:
				{
					if (v1 > v2)
						return true;
					return false;
				}
				case ConditionType.LessEqual:
				{
					if (v1 < v2 || Math.Abs(v1 - v2) <= Constants.Epsilon)
						return true;
					return false;
				}
				case ConditionType.GreaterEqual:
				{
					if (v1 > v2 || Math.Abs(v1 - v2) <= Constants.Epsilon)
						return true;
					return false;
				}
				default:
				{
					return false;
				}
			}
		}

		/**
		 * Compare 2 strings
		 * @returns bool
		 * true - if condition is satisfied
		 * else false
		 */
		public bool CompareStrings(String s1)
		{
			String s2 = Value;
			switch (Sign)
			{
				case ConditionType.Equal:
				{
					if (String.Compare(s1, s2) == 0)
						return true;
					return false;
				}
				case ConditionType.Less:
				{
					if (String.Compare(s1, s2) < 0)
						return true;
					return false;
				}
				case ConditionType.Greater:
				{
					if (String.Compare(s1, s2) > 0)
						return true;
					return false;
				}
				case ConditionType.LessEqual:
				{
					if (String.Compare(s1, s2) <= 0)
						return true;
					return false;
				}
				case ConditionType.GreaterEqual:
				{
					if (String.Compare(s1, s2) >= 0)
						return true;
					return false;
				}
				default:
				{
					return false;
				}
			}
		}
	}
}