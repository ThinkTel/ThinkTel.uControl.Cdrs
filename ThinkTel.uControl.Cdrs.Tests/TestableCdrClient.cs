using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTel.uControl.Cdrs.Tests
{
	public class TestableCdrClient : CdrClient
	{
		public TestableCdrClient(string uri) : base(uri) { }

		public string Username { get { return Credentials.UserName; } }
		public string Password { get { return Credentials.Password; } }

		protected override async Task<string> FtpGetAsync(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				// list files
				return @"-rwxrwxrwx   1 owner    group             108 Nov  6  2010 20101104.csv
-rwxrwxrwx   1 owner    group             220 Nov  6  2010 20101104_7805551212.csv
-rwxrwxrwx   1 owner    group             220 Nov  6  2010 20101104_account.csv
-rwxrwxrwx   1 owner    group            7921 Dec 31  2014 20141231_11197_MetaSwitch.csv
-rwxrwxrwx   1 owner    group          409208 Jan 19 16:27 20150119_11313_MetaSwitch.csv
drwxrwxrwx   1 owner    group               0 Nov  1  2014 Stats".Trim();
			}
			else if (path.EndsWith("20150119_11313_MetaSwitch.csv"))
			{
				return @"Billing Number,Source Number,Destination Number,Call Date,Rounded Call Duration (Seconds),Usage Type,Billed Amount (Dollars),Source Location,Destination Location,Rate (Dollars Per Minute),Label,Raw Duration
7805551212,17005551111,15145551212,2014-05-18 20:01:10-06,11,_411,0.165,""Edmonton, AB"",""Montréal, QC"",0.015,""Test Trunk"",4.4
5875553434,17005551111,17004442222,2014-05-18 20:02:20-06,21,canada,0.63,""Edmonton, AB"",""Calgary, AB"",0.03,""Test Trunk 2"",5.8
7805551212,17004442222,17005551111,2014-05-18 20:03:30-06,31,incoming,1.395,""Calgary, AB"",""Edmonton, AB"",0.045,""Test Trunk"",5.2
5875553434,17005551111,01144324576894,2014-05-18 20:04:40-06,41,international,2.46,""Edmonton, AB"",""London, UK"",0.060,""Test Trunk 2"",2.5
7805551212,17005551111,17003337777,2014-05-18 20:05:50-06,51,itc_canada,3.825,""Edmonton, AB"",""Québec, QC"",0.075,""Test Trunk"",0
5875553434,17005551111,17007773333,2014-05-18 21:06:00-06,61,itc_usa,5.49,""Edmonton, AB"",""Whitefish, MT"",0.090,""Test Trunk 2"",5.8
7805551212,17005551111,17005552222,2014-05-18 21:07:10-06,71,local,0,""Edmonton, AB"",""Edmonton, AB"",0,""Test Trunk"",5.2
5875553434,17005551111,17005551100,2014-05-18 21:08:20-06,81,onnet,0.00,""Edmonton, AB"",""Edmonton, AB"",0.0,""Test Trunk 2"",798.3
7805551212,17004442222,18552003333,2014-05-18 21:09:30-06,91,tfin,0,""Calgary, AB"",""Toll Free"",0.00,""Test Trunk"",6.5
5875553434,17005551111,17008889999,2014-05-18 21:10:40-06,101,usa,0,""Edmonton, AB"",""New York, NY"",0,""Test Trunk 2"",5.7
7805551212,17005551111,17008889999,2014-05-18 21:11:50-06,,usa,,""Edmonton, AB"",""New York, NY"",,""Test Trunk"",5.7";
            }
            else
			{
				throw new NotImplementedException("Unknown FTP path " + path);
			}
		}
	}
}
