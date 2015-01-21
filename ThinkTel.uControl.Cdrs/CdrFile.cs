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

		public override bool Equals(object obj)
		{
			if (obj is CdrFile)
			{
				var other = (CdrFile)obj;
				return string.Equals(Filename, other.Filename) && Timestamp == other.Timestamp && Size == other.Size;
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return string.Format("{0}-{1}-{2}", Filename, Timestamp, Size).GetHashCode();
		}
	}
}