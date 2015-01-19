using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ThinkTel.uControl.Cdrs
{
	public class InvalidCdrException : Exception
	{
		public string Filename { get; private set; }
		public int LineNumber { get; private set; }
		public string Line { get; private set; }

		public InvalidCdrException(string filename, int lineNumber, string line, Exception cause) : 
			base(string.Format("{0} [{1}]: {2}", filename, lineNumber, line), cause) 
		{
			Filename = filename;
			LineNumber = lineNumber;
			Line = line;
		}
	}
}