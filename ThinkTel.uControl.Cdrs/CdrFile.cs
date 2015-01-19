using System;

namespace ThinkTel.uControl.Cdrs
{
	public class CdrFile
	{
		public string Filename { get; set; }
		public DateTime Timestamp { get; set; }
		public long Size { get; set; }

		public override string ToString()
		{
			return string.Format("{0,-50} {1} {2,10}", Filename, Timestamp.ToString("yyyy-MM-dd HH:mm"), Size);
		}
	}
}