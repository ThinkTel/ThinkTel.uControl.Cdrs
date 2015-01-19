using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ThinkTel.uControl.Cdrs
{
	public static class CsvExtensions
	{
		private static Regex PS_DATETIME_FORMAT = new Regex(@"^\d{1,2}/\d{1,2}/\d{4} \d{1,2}:\d{1,2}:\d{1,2} [AP]M$");
		public static List<T> FromCsv<T>(this Stream stream, params string[] fields) where T : class,new()
		{
			var records = new List<T>();
			var tType = typeof(T);
			var props = fields.Select(x => (x == null ? null : tType.GetProperty(x))).ToArray();
			//if (props.Any(x => x == null))
			//    throw new ArgumentException("Unknown field", "fields");

			using (var rdr = new StreamReader(stream))
			{
				// assume there is a header row and skip it
				var headers = rdr.ReadLine();
				// then read each data row
				while (!rdr.EndOfStream)
				{
					var row = rdr.ReadLine().Trim().SplitCsvLine();
					var t = new T();
					records.Add(t);
					for (var i = 0; i < props.Length; i++)
					{
						var p = props[i];
						if (p == null) continue; // skip column without field mapping

						object val = null;
						if (p.PropertyType == typeof(string))
						{
							val = row[i];
						}
						else if (p.PropertyType == typeof(bool))
						{
							if (string.IsNullOrEmpty(row[i]))
								val = false;
							else if (string.Equals("true", row[i], StringComparison.CurrentCultureIgnoreCase))
								val = true;
							else if (string.Equals("t", row[i], StringComparison.CurrentCultureIgnoreCase))
								val = true;
							else if (string.Equals("1", row[i], StringComparison.CurrentCultureIgnoreCase))
								val = true;
							else if (string.Equals("false", row[i], StringComparison.CurrentCultureIgnoreCase))
								val = false;
							else if (string.Equals("f", row[i], StringComparison.CurrentCultureIgnoreCase))
								val = false;
							else if (string.Equals("0", row[i], StringComparison.CurrentCultureIgnoreCase))
								val = false;
							else
								throw new FormatException("[" + row[i] + "] was not recognized as a valid Boolean");
						}
						else if (p.PropertyType == typeof(DateTime))
						{
							if (PS_DATETIME_FORMAT.IsMatch(row[i]))
								try
								{
									val = DateTime.ParseExact(row[i], "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
								}
								catch (Exception ex)
								{
									throw new Exception("Failed to parse " + row[i], ex);
								}
							else
								val = DateTime.Parse(row[i]);
						}
						else if (p.PropertyType == typeof(Guid))
						{
							val = Guid.Parse(row[i]);
						}
						else if (p.PropertyType.IsEnum)
						{
							val = Enum.Parse(p.PropertyType, row[i]);
						}
						else
						{
							throw new NotImplementedException("Unsupported CSV record property type: " + p.PropertyType);
						}
						if (val != null)
							p.SetValue(t, val, null);
					}
				}
			}
			return records;
		}

		private static Regex SPLIT_CSV_LINE = new Regex("(?:\"(?<m>[^\"]*)\")|(?<m>[^,]+)");
		public static string[] SplitCsvLine(this string line)
		{
			line = line.Replace(",,", @","""",");
			var values = SPLIT_CSV_LINE.Matches(line);
			return values.Cast<Match>().Select(x => x.Groups["m"].Value).ToArray();
		}

		public static string ToCsv<T>(this IEnumerable<T> records, params string[] fields)
		{
			var sb = new StringBuilder();
			sb.AppendLine(fields.ToCsvLine());
			foreach (var row in records.Select(x => x.ExtractFields(fields)))
				sb.AppendLine(row.ToCsvLine());
			return sb.ToString();
		}

		private static object[] ExtractFields<T>(this T record, params string[] fields)
		{
			var t = typeof(T);
			return fields.Select(x => t.GetProperty(x).GetValue(record, null)).ToArray();
		}

		public static string ToCsvLine(this object[] line)
		{
			return string.Join(",", line.Select(x => x.ToEscapedCsvString()).ToArray());
		}

		private static string ToEscapedCsvString(this object value)
		{
			if (value == null) return "";
			//if (value is INullable && ((INullable)value).IsNull) return "";
			if (value is DateTime)
			{
				if (((DateTime)value).TimeOfDay.TotalSeconds == 0)
					return ((DateTime)value).ToString("yyyy-MM-dd");
				return ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
			}
			string output = value.ToString();
			if (output.Contains(",") || output.Contains("\""))
				output = '"' + output.Replace("\"", "\"\"") + '"';
			return output;
		}
	}
}
