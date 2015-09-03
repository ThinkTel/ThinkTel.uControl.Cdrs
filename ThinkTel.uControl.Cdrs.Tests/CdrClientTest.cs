using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ThinkTel.uControl.Cdrs.Tests
{
	public class CdrClientTest
	{
		private const string USERNAME = "user";
		private const string PASSWORD = "pass";
		private static readonly string CTOR_URI = string.Format("ftp://{0}:{1}@localhost/", USERNAME, PASSWORD);
		private const string TEST_FILE = "20150119_11313_MetaSwitch.csv";

		private TestableCdrClient cdr;
		public CdrClientTest()
		{
			cdr = new TestableCdrClient(CTOR_URI);
		}

		[Fact]
		public async Task UsernameAndPasswordFromUri()
		{
			Assert.Equal(USERNAME, cdr.Username);
			Assert.Equal(PASSWORD, cdr.Password);
		}

		[Fact]
		public async Task ListCdrFilesAsync()
		{
			var expected = new CdrFile[] 
			{
				new CdrFile { Filename = "20141231_11197_MetaSwitch.csv", Timestamp = new DateTime(2014,12,31,0,0,0), Size = 7921 },
				new CdrFile { Filename = "20150119_11313_MetaSwitch.csv", Timestamp = new DateTime(2015,1,19,16,27,0), Size = 409208 },
			};

			var actual = await cdr.ListCdrFilesAsync();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public async Task GetCdrFileAsync()
		{
			var expected = new Cdr[] {
				MakeCdr(17005551111, 15145551212, CdrUsageType._411),
				MakeCdr(17005551111, 17004442222, CdrUsageType.canada),
				MakeCdr(17004442222, 17005551111, CdrUsageType.incoming),
				MakeCdr(17005551111, 1144324576894, CdrUsageType.international),
				MakeCdr(17005551111L, 17003337777, CdrUsageType.itc_canada),
				MakeCdr(17005551111L, 17007773333, CdrUsageType.itc_usa),
				MakeCdr(17005551111L, 17005552222, CdrUsageType.local),
				MakeCdr(17005551111L, 17005551100, CdrUsageType.onnet),
				MakeCdr(17004442222, 18552003333, CdrUsageType.tfin),
				MakeCdr(17005551111L, 17008889999, CdrUsageType.usa),
                MakeCdr(17005551111L, 17008889999, CdrUsageType.usa, 5)
            };

			var actual = await cdr.GetCdrFileAsync(TEST_FILE);

			Assert.Equal(expected, actual);

			await ThrowsAsync<ArgumentNullException>(async () => await cdr.GetCdrFileAsync(null));
			await ThrowsAsync<ArgumentException>(async () => await cdr.GetCdrFileAsync("invalid file"));
		}

		private static int lineCnt = 2;
		private static Dictionary<string, string> LOCALS = new Dictionary<string,string> {
			{"1700555", "Edmonton, AB"}, 
			{"1514555", "Montréal, QC"},
			{"1700444", "Calgary, AB"},
			{"1700333", "Québec, QC"},
			{"1700777", "Whitefish, MT"},
			{"1700888", "New York, NY"},
			{"1855200", "Toll Free"}
		};
		private static string INTERNATIONAL = "London, UK";

		private static Cdr MakeCdr(long srcNum, long dstNum, CdrUsageType type, int? blankDuration = null)
		{
			var date = new DateTime(2014,5,18,20+(lineCnt >= 7 ? 1 : 0), lineCnt-1, ((lineCnt-1) * 10) % 60, DateTimeKind.Utc);
			date = date.AddHours(6).ToLocalTime();

            var dur = (lineCnt - 1) * 10 + 1;
            if (blankDuration.HasValue)
                dur = blankDuration.Value;

            var rate = (lineCnt - 1) * 0.015m;
			if (rate > 0.1m || blankDuration.HasValue)
                rate = 0;

			return new Cdr
			{
				CdrFile = TEST_FILE,
				LineNumber = lineCnt++,
				BillingNumber = lineCnt % 2 == 1 ? 7805551212L : 5875553434L,
				SourceNumber = srcNum,
				DestinationNumber = dstNum,
				Dated = date,
				RoundedDuration = dur,
				UsageType = type,
				BilledAmount = rate*dur,
				SourceLocation = LOCALS[srcNum.ToString().Substring(0,7)],
				DestinationLocation = (dstNum > 19999999999L ? INTERNATIONAL : LOCALS[dstNum.ToString().Substring(0,7)]),
				Rate = rate
			};
		}

		private static async Task ThrowsAsync<T>(Func<Task> codeUnderTest) where T : Exception
		{
			try
			{
				await codeUnderTest();
				Assert.Throws<T>(() => { });
			}
			catch (T) { }
		}
	}
}
