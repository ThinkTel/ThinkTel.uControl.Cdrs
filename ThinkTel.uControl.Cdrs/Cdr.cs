using System;

namespace ThinkTel.uControl.Cdrs
{
	public class Cdr
	{
		public string CdrFile { get; set; }
		public int LineNumber { get; set; }

		public long BillingNumber { get; set; }		// CSV column 0
		public long SourceNumber { get; set; }		// CSV column 1
		public long DestinationNumber { get; set; } // CSV column 2
		public DateTime Dated { get; set; }			// CSV column 3
		public int RoundedDuration { get; set; }	// CSV column 4
		public CdrUsageType UsageType { get; set; }	// CSV column 5

		// CSV column 6 billed amount (rounded duration / 60 * rate)
		public decimal BilledAmount { get; set; }

		public string SourceLocation { get; set; }		// CSV column 7
		public string DestinationLocation { get; set; } // CSV column 8
		public decimal Rate { get; set; }				// CSV column 9

		// CSV column 10 label
		// CSV column 11 raw duration (fractional seconds)

		// public decimal Amount { get { return Math.Round((RoundedDuration / 60.0m) * Rate, 2); } }
		public bool IsRated { get { return Rate > 0.0m; } }
		public TimeSpan Duration { get { return new TimeSpan(TimeSpan.TicksPerSecond * RoundedDuration); } }

		public override string ToString()
		{
			return string.Format("{0:yyyy-MM-dd HH:mm} {1,-15} to {2,-15} for {3,5}s {4:c}", Dated, SourceNumber, DestinationNumber, RoundedDuration, BilledAmount);
		}

		public override bool Equals(object obj)
		{
			if (obj is Cdr)
			{
				var other = (Cdr)obj;
				return string.Equals(CdrFile, other.CdrFile) && LineNumber == other.LineNumber && BillingNumber == other.BillingNumber &&
					SourceNumber == other.SourceNumber && DestinationNumber == other.DestinationNumber && Dated == other.Dated &&
					RoundedDuration == other.RoundedDuration && UsageType == other.UsageType && BilledAmount == other.BilledAmount &&
					SourceLocation == other.SourceLocation && DestinationLocation == other.DestinationLocation && Rate == other.Rate;
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return string.Join("-", new object[] { 
				CdrFile, LineNumber, BillingNumber, 
				SourceNumber, DestinationNumber, Dated, 
				RoundedDuration, UsageType, BilledAmount, 
				SourceLocation, DestinationLocation, Rate
			}).GetHashCode();
		}
	}
}