using System;

namespace RDBMS.QueryManager
{
	//class not getting used right now
	internal class Error
	{
		public int ErrorCode;
		public String ErrorMsg;

		public Error(int errCode, String errorMsg)
		{
			ErrorCode = errCode;
			ErrorMsg = errorMsg;
		}
	}
}