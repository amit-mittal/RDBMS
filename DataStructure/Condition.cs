using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		public Dummy Attribute;
		public ConditionType Sign;
		public int Value;//TODO think if add value or attribute

		public Condition(Dummy attribute, ConditionType sign, int value)
		{
			Attribute = attribute;
			Sign = sign;
			Value = value;
		}
	}
}
