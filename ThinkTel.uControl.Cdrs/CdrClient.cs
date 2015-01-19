using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ThinkTel.uControl.Cdrs
{
    public class CdrClient : ICdrClient
    {
		private Uri _uri;
		private NetworkCredential _credentials;

		public CdrClient(string uri) : this(new Uri(uri)) { }
		public CdrClient(Uri uri)
		{
			_uri = uri;
			if (!string.IsNullOrEmpty(_uri.UserInfo))
			{
				var array = _uri.UserInfo.Split(':');
				_credentials = new NetworkCredential(array[0], array[1]);
			}
		}

		// SEE: http://stackoverflow.com/questions/966578/parse-response-from-ftp-list-command-syntax-variations
		private static Regex FTP_LIST_PARSER = new Regex(@"^((?<DIR>([dD]))|)(?<ATTRIBS>(.*))\s(?<SIZE>([0-9]+))\s(?<DATE>((?<MONTHDAY>((?<MONTH>(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec))\s(?<DAY>([0-9\s]{2}))))\s(\s(?<YEAR>([0-9]{4}))|(?<TIME>([\s0-9]{2}\:[0-9]{2})))))\s(?<NAME>([A-Za-z0-9\-\._\s]+))$");
		private static List<string> FTP_MONTHS = new List<string> { "", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
		private const string CDR_FILE_SUFFIX = "_MetaSwitch.csv";
		public async Task<CdrFile[]> ListCdrFilesAsync()
		{
			var files = new List<CdrFile>();
			var list = await FtpGetAsync(null);

			// for some reason, maybe because of how large the string it, using a StringReader takes too long
			//		Split and Trim are much faster to get the individual lines
			var lineNum = 1;
			foreach(var line in list.Trim().Split('\n').Select(x => x.Trim()))
			{
				var md = FTP_LIST_PARSER.Match(line);
				if (!md.Success)
					throw new Exception("Parse error, line " + lineNum + ": " + line);

				if (md.Groups["DIR"].Success || !md.Groups["NAME"].Value.EndsWith(CDR_FILE_SUFFIX))
					continue; // skip directories and file not ending in MetaSwitch.csv

				var year = (md.Groups["YEAR"].Success ? int.Parse(md.Groups["YEAR"].Value) : DateTime.Now.Year);
				var mth = FTP_MONTHS.IndexOf(md.Groups["MONTH"].Value);
				var day = int.Parse(md.Groups["DAY"].Value.Trim());
				var time = (md.Groups["TIME"].Success ? md.Groups["TIME"].Value.Split(':') : new string[] { "0", "0" });
				var hour = int.Parse(time[0].Trim());
				// trim the leading 0 to prevent this from being interpreted as base 8
				var mins = int.Parse(time[1][0] == '0' && time[1].Length > 1 ? time[1].Substring(1) : time[1]);

				files.Add(new CdrFile
				{
					Filename = md.Groups["NAME"].Value,
					Size = int.Parse(md.Groups["SIZE"].Value),
					Timestamp = new DateTime(year, mth, day, hour, mins, 0)
				});
				lineNum++;
			}

			return files.ToArray(); // can't yield because by the time we're ready to do something with it, the response is closed
		}

		private static Regex SPLIT_CSV_LINE = new Regex("(?:\"(?<m>[^\"]*)\")|(?<m>[^,]+)");
		public async Task<Cdr[]> GetCdrFileAsync(string cdrFile)
		{
			if (string.IsNullOrEmpty(cdrFile))
				throw new ArgumentNullException("cdrFile");
			if (!cdrFile.EndsWith(CDR_FILE_SUFFIX))
				throw new ArgumentException("cdrFile");

			var cdrs = new List<Cdr>();

			// expected syntax is \d{8}_\d{4}_MetaSwitch.csv
			var data = await FtpGetAsync(cdrFile);

			var lineNum = 2; // we start at the 2nd line since we skip 1 in the foreach
			// the Replace is to work around a bug where some fields are blank without being quoted
			// the Skip is to jump over the header line
			foreach (var line in data.Trim().Split('\n').Select(x => x.Trim().Replace(",,", ",\"\",")).Skip(1))
			{
				// parse CSV
				var row = SPLIT_CSV_LINE.Matches(line).Cast<Match>().Select(x => x.Groups["m"].Value).ToArray();
				try
				{
					cdrs.Add(new Cdr
					{
						CdrFile = cdrFile,
						LineNumber = lineNum,
						BillingNumber = long.Parse(row[0]),
						// in the old format a blocked caller id is blank
						SourceNumber = (string.IsNullOrEmpty(row[1]) ? 0 : long.Parse(row[1])),
						DestinationNumber = long.Parse(row[2]),
						Dated = DateTime.Parse(row[3]),
						RoundedDuration = int.Parse(row[4]),
						UsageType = (CdrUsageType)Enum.Parse(typeof(CdrUsageType), row[5]),
						BilledAmount = decimal.Parse(row[6]),
						SourceLocation = row[7],
						DestinationLocation = row[8],
						Rate = decimal.Parse(row[9])
						// row[10] is label and row[11] is raw duration (float)
					});
				}
				catch (Exception e)
				{
					throw new InvalidCdrException(cdrFile, lineNum, line, e);
				}
				lineNum++;
			}

			return cdrs.ToArray();
		}

		private async Task<string> FtpGetAsync(string path)
		{
			var ftpUrl = "ftp://" + _uri.Host + "/" + (path ?? ""); // %2f/";
			var req = (FtpWebRequest)FtpWebRequest.Create(ftpUrl);
			if (ftpUrl.EndsWith("/"))
				req.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
			else
				req.Method = WebRequestMethods.Ftp.DownloadFile;
			req.KeepAlive = false;
			req.UsePassive = true;
			req.UseBinary = false; // maybe true for files?
			req.Credentials = _credentials;

			using (var resp = await req.GetResponseAsync())
			{
				// for some reason, if we try to parse one line at a time we get a 550 error 
				//		so instead we read the whole result and parse it after
				using (var rdr = new StreamReader(resp.GetResponseStream()))
					return await rdr.ReadToEndAsync();
			}
		}
	}
}