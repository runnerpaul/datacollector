using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataCollector
{
    class Program
    {
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime appStart = new DateTime(2017, 11, 20, 18, 0, 35);

        /// <summary>
        /// Convert epoch time to DateTime
        /// </summary>
        /// <param name="unixTime"></param>
        /// <returns></returns>
        private static DateTime FromUnixTime(long unixTime)
        {
            return epoch.AddSeconds(unixTime);
        }

        /// <summary>
        /// Greate an empty json file
        /// </summary>
        /// <param name="filePath"></param>
        private static void CreateFile(string filePath)
        {
            // Create the file, or overwrite if the file exists.
            using (FileStream fs = File.Create(filePath))
            {
                byte[] info = new UTF8Encoding(true).GetBytes("[]");
                fs.Write(info, 0, info.Length);
            }
        }

        /// <summary>
        /// Calculate the data rate
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="difference"></param>
        /// <returns></returns>
        private static long DataRate(long offset, TimeSpan difference)
        {
            long dataRate = offset / ((long)difference.TotalSeconds * 1440);
            return dataRate;
        }

        static void Main(string[] args)
        {

            // read JSON directly from a file
            using (StreamReader file = File.OpenText(@"./burrow_lag.json"))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JObject o = (JObject)JToken.ReadFrom(reader);

                long startOffset = (long) o.SelectToken("status.partitions[0].start.offset");
                long startTimestamp = (long) o.SelectToken("status.partitions[0].start.timestamp");
                long endOffset = (long)o.SelectToken("status.partitions[0].end.offset");
                long endTimestamp = (long) o.SelectToken("status.partitions[0].end.timestamp");

                DateTimeOffset startDateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(startTimestamp);
                DateTimeOffset endDateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(endTimestamp);
                TimeSpan starTimeDifference = startDateTimeOffset.Subtract(appStart);
                TimeSpan endTimeDifference = endDateTimeOffset.Subtract(appStart);
                long startRate = DataRate(startOffset, starTimeDifference);
                long endRate = DataRate(endOffset, endTimeDifference);

                var filePath = @"./performancedata.json";

                if (!File.Exists(filePath))
                {
                    try
                    {
                        // Create the file
                        CreateFile(filePath);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
                // Read existing json data
                var jsonData = System.IO.File.ReadAllText(filePath);
                // De-serialize to object or create new list
                var dataList = JsonConvert.DeserializeObject<List<PerformanceData>>(jsonData) 
                                    ?? new List<PerformanceData>();

                // Add any new employees
                dataList.Add(new PerformanceData()
                {
                    StartTime = appStart.ToString("MM/dd/yyyy H:mm:ss tt"),
                    EndTime = endDateTimeOffset.ToString("MM/dd/yyyy H:mm:ss tt"),
                    Difference = (long) endTimeDifference.TotalSeconds,
                    StartOffset = startOffset,
                    EndOffset = endOffset,
                    StartRate = startRate,
                    EndRate = endRate
                });

                // Update json data string
                jsonData = JsonConvert.SerializeObject(dataList, Formatting.Indented);
                System.IO.File.WriteAllText(filePath, jsonData);
            }
        }
    }
}
