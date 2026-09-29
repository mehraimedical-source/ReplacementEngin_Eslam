using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Smart_Report.SpatialOcrBlockBuilder
{
    // Purpose-built parser for the OCR JSON contract. No external JSON package is required.
    public static class SpatialOcrJsonParser
    {
        public static List<SpatialOcrItem> Parse(string json, RowDetectionOptions options)
        {
            if (json == null) throw new ArgumentNullException("json");
            if (options == null) options = new RowDetectionOptions();

            JsonReader r = new JsonReader(json);
            object root = r.ReadValue();
            List<object> array = root as List<object>;
            if (array == null) throw new FormatException("OCR JSON root must be an array.");

            List<SpatialOcrItem> result = new List<SpatialOcrItem>();
            for (int i = 0; i < array.Count; i++)
            {
                Dictionary<string, object> obj = array[i] as Dictionary<string, object>;
                if (obj == null) continue;

                string text = GetString(obj, "Text");
                if (string.IsNullOrEmpty(text) || text.Trim().Length == 0) continue;

                List<object> points = GetArray(obj, "BoxPoints");
                if (points == null || points.Count < 2) continue;

                double left = double.MaxValue, top = double.MaxValue;
                double right = double.MinValue, bottom = double.MinValue;
                for (int p = 0; p < points.Count; p++)
                {
                    Dictionary<string, object> point = points[p] as Dictionary<string, object>;
                    if (point == null) continue;
                    double x = GetDouble(point, "X");
                    double y = GetDouble(point, "Y");
                    if (x < left) left = x;
                    if (x > right) right = x;
                    if (y < top) top = y;
                    if (y > bottom) bottom = y;
                }

                if (right <= left || bottom <= top) continue;
                double score = GetDouble(obj, "Score");

                SpatialOcrItem item = new SpatialOcrItem();
                item.SourceIndex = i;
                item.Text = text.Trim();
                item.Score = score;
                item.IsLowConfidence = score < options.LowConfidenceThreshold;
                item.Bounds = new SpatialBounds();
                item.Bounds.Left = left; item.Bounds.Top = top;
                item.Bounds.Right = right; item.Bounds.Bottom = bottom;
                result.Add(item);
            }
            return result;
        }

        private static string GetString(Dictionary<string, object> o, string key)
        {
            object v; return o.TryGetValue(key, out v) && v != null ? Convert.ToString(v, CultureInfo.InvariantCulture) : "";
        }
        private static double GetDouble(Dictionary<string, object> o, string key)
        {
            object v; return o.TryGetValue(key, out v) && v != null ? Convert.ToDouble(v, CultureInfo.InvariantCulture) : 0.0;
        }
        private static List<object> GetArray(Dictionary<string, object> o, string key)
        {
            object v; return o.TryGetValue(key, out v) ? v as List<object> : null;
        }

        private sealed class JsonReader
        {
            private readonly string s; private int p;
            public JsonReader(string text) { s = text; }

            public object ReadValue()
            {
                Skip();
                if (p >= s.Length) throw Error("Unexpected end");
                char c = s[p];
                if (c == '{') return ReadObject();
                if (c == '[') return ReadArray();
                if (c == '"') return ReadString();
                if (c == 't') { Expect("true"); return true; }
                if (c == 'f') { Expect("false"); return false; }
                if (c == 'n') { Expect("null"); return null; }
                return ReadNumber();
            }

            private Dictionary<string, object> ReadObject()
            {
                Dictionary<string, object> d = new Dictionary<string, object>();
                p++; Skip(); if (Take('}')) return d;
                while (true)
                {
                    Skip(); string key = ReadString(); Skip();
                    if (!Take(':')) throw Error("Expected ':'");
                    d[key] = ReadValue(); Skip();
                    if (Take('}')) return d;
                    if (!Take(',')) throw Error("Expected ','");
                }
            }

            private List<object> ReadArray()
            {
                List<object> a = new List<object>();
                p++; Skip(); if (Take(']')) return a;
                while (true)
                {
                    a.Add(ReadValue()); Skip();
                    if (Take(']')) return a;
                    if (!Take(',')) throw Error("Expected ','");
                }
            }

            private string ReadString()
            {
                if (!Take('"')) throw Error("Expected string");
                StringBuilder b = new StringBuilder();
                while (p < s.Length)
                {
                    char c = s[p++];
                    if (c == '"') return b.ToString();
                    if (c != '\\') { b.Append(c); continue; }
                    if (p >= s.Length) throw Error("Invalid escape");
                    c = s[p++];
                    switch (c)
                    {
                        case '"': b.Append('"'); break; case '\\': b.Append('\\'); break;
                        case '/': b.Append('/'); break; case 'b': b.Append('\b'); break;
                        case 'f': b.Append('\f'); break; case 'n': b.Append('\n'); break;
                        case 'r': b.Append('\r'); break; case 't': b.Append('\t'); break;
                        case 'u':
                            if (p + 4 > s.Length) throw Error("Invalid unicode escape");
                            b.Append((char)int.Parse(s.Substring(p, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture)); p += 4; break;
                        default: throw Error("Invalid escape");
                    }
                }
                throw Error("Unterminated string");
            }

            private double ReadNumber()
            {
                int start = p;
                while (p < s.Length && "-+0123456789.eE".IndexOf(s[p]) >= 0) p++;
                double n;
                if (!double.TryParse(s.Substring(start, p - start), NumberStyles.Float, CultureInfo.InvariantCulture, out n))
                    throw Error("Invalid number");
                return n;
            }

            private void Skip() { while (p < s.Length && char.IsWhiteSpace(s[p])) p++; }
            private bool Take(char c) { Skip(); if (p < s.Length && s[p] == c) { p++; return true; } return false; }
            private void Expect(string x) { if (p + x.Length > s.Length || s.Substring(p, x.Length) != x) throw Error("Expected " + x); p += x.Length; }
            private FormatException Error(string m) { return new FormatException(m + " at position " + p.ToString(CultureInfo.InvariantCulture)); }
        }
    }
}