using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.IO;
using Newtonsoft.Json;

namespace BitCruncher
{

    public sealed class Plumbing
    {
        public static string Time1 = "TIME1";
        public static string Time2 = "TIME2";
        
        public SortedList<string, bool> Headers { get; } = new SortedList<string, bool>();
        public SortedList<string, Node>[] AlmostCsvs { get; }

        public Plumbing(int chunkSize)
        {
            AlmostCsvs = Enumerable.Range(0, chunkSize).Select(i => new SortedList<string, Node>(new Dictionary<string, Node>
            {
                [Time1] = new Node(),
                [Time2] = new Node(),
            })).ToArray();
        }
    }

    public sealed class Node
    {
        public bool UseMe { get; set; }
        public object Payload { get; set; }
    }

    public sealed class EventData
    {
        public DateTime EnqueuedTimeUtc { get; set; }
        public MemoryStream Payload { get; set; }

        public EventData(byte[] raw)
        {
            Payload = new MemoryStream(raw);
        }
    }

    public static class DataManipulation
    {
        private static readonly RecyclableMemoryStreamManager StreamManager = new RecyclableMemoryStreamManager();

        public static MemoryStream ChunkToCsv(IEnumerable<EventData> es)
        {
            es = es.ToList();
            var plumbing = new Plumbing(es.Count());
            var i = 0;
            foreach (var e in es)
                ParseRawFlatEvent(i++, plumbing, e, e.Payload);
            var headerKeys = plumbing.Headers.Keys.ToArray();
            var csvStream = ToCsvStream(headerKeys, new ArraySegment<SortedList<string, Node>>(plumbing.AlmostCsvs, 0, i));
            plumbing.Headers.Clear();
            return csvStream;
        }

        public static SortedList<string, Node> ParseRawFlatEvent(int i, Plumbing plumbing, EventData data, Stream rawStream)
        {
            var res = plumbing.AlmostCsvs[i];
            var headers = plumbing.Headers;
            FillSystemDates(data, headers, res);
            using (var streamReader = new StreamReader(rawStream))
            using (var reader = new JsonTextReader(streamReader) {DateParseHandling = DateParseHandling.None})
            {
                while (reader.Read())
                {
                    if (reader.TokenType != JsonToken.PropertyName) continue;
                    var prop = reader.Path;
                    headers[prop] = true;
                    reader.Read();
                    var value = reader.TokenType == JsonToken.StartArray ? ReadArrayAsString(reader) : reader.Value;
                    Node node;
                    if (res.TryGetValue(prop, out node))
                    {
                        node.UseMe = true;
                        node.Payload = value;
                    }
                    else
                    {
                        res.Add(prop, new Node {UseMe = true, Payload = value});
                    }
                }
            }
            return res;
        }

        private static void FillSystemDates(EventData data, SortedList<string, bool> headers, SortedList<string, Node> res)
        {
            headers[Plumbing.Time1] = true;
            headers[Plumbing.Time2] = true;
            var enqP = res[Plumbing.Time1];
            enqP.UseMe = true;
            enqP.Payload = data.EnqueuedTimeUtc.ToString("o");
            var procP = res[Plumbing.Time2];
            procP.UseMe = true;
            procP.Payload = DateTime.UtcNow.ToString("o");
        }

        private static object ReadArrayAsString(JsonTextReader reader)
        {
            var res = new StringBuilder("[");
            var val = reader.ReadAsString();
            while (reader.TokenType != JsonToken.EndArray)
            {
                if (val != null) res = res.Append(val);
                res = res.Append(',');
                val = reader.ReadAsString();
            }
            res = res.Length > 1 ? res.Remove(res.Length - 1, 1) : res;
            res = res.Append(']');
            return res.ToString();
        }

        public static MemoryStream ToCsvStream(IList<string> headers, IEnumerable<SortedList<string, Node>> range)
        {
            var stream = new MemoryStream(); //StreamManager.GetStream("to-csv");
            using (var writer = new StreamWriter(stream, Encoding.UTF8, 4096, true))
            using (var csv = new CsvWriter(writer))
            {
                foreach (var header in headers)
                    csv.WriteField(header);
                csv.NextRecord();
                foreach (var ev in range)
                {
                    using (var eI = ev.GetEnumerator())
                    using (var hI = headers.GetEnumerator())
                    {
                        var enumerating = eI.MoveNext();
                        var hasHeaders = hI.MoveNext();
                        while (enumerating)
                        {
                            var fieldName = eI.Current.Key;
                            var fieldValue = eI.Current.Value;
                            if (!fieldValue.UseMe) enumerating = eI.MoveNext();
                            else if (hI.Current == fieldName)
                            {
                                csv.WriteField(fieldValue.Payload);
                                fieldValue.UseMe = false;
                                enumerating = eI.MoveNext();
                                hasHeaders = hI.MoveNext();
                            }
                            else
                            {
                                csv.WriteField(null);
                                hasHeaders = hI.MoveNext();
                            }
                        }
                        while (hasHeaders)
                        {
                            hasHeaders = hI.MoveNext();
                            csv.WriteField(null);
                        }
                    }

                    csv.NextRecord();
                }
                writer.Flush();
            }
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
    }
}