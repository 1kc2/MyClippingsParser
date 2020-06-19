using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MyClippingsParser
{
    /* Format:
     Title (Author)
     - Your Highlight on page X | Location X-X | Added on LongDate
     Highlight string
     ==========
     */
    class Program
    {
        static Regex _pattern = new Regex(@"^(?<title>.+) \((?<author>.+)\)\r\n- ((Your Highlight on page (?<page>\d+) \| location (?<locationStart>\d+)-(?<locationEnd>\d+))|(Your Highlight at location (?<locationStart>\d+)-(?<locationEnd>\d+))) \| Added on (?<date>.+)\r\n\r\n(?<highlight>.+)\r\n==========", RegexOptions.Multiline);

        static void Main(string[] args)
        {
            string fname = args.Length > 0 ? args[0] : @"My Clippings.txt";

            if (args.Length == 0 && !File.Exists(fname))
            {
                Console.Write("No such file");
                return;
            }
            string file = File.ReadAllText(fname);
            var parsed = Parse(file);
            Output(parsed);
        }

        private static bool ContainsOrIsContained(string str1, string str2)
        {
            return str1.Contains(str2) || str2.Contains(str1);
        }

        private static Dictionary<string, List<Match>> Parse(string str)
        {
            var highlightsTmp = new Dictionary<string, List<Match>>();
            var highlights = new Dictionary<string, List<Match>>();

            

            var matches = _pattern.Matches(str);
            //split them by book first
            for (int i = 0; i < matches.Count; i++)
            {
                AddMatch(highlightsTmp, matches[i]);
            }


            foreach(var kvp in highlightsTmp)
            {
                var key = kvp.Key;
                var bookMatches = kvp.Value;
                highlights.Add(key, new List<Match>());
                string prevTxt = "";

                for (int i = 0; i < bookMatches.Count; i++)
                {
                    var match = bookMatches[i];
                    string highlight = match.Groups["highlight"].Value;
                    string title = match.Groups["title"].Value;
                    if (prevTxt != "" && !ContainsOrIsContained(prevTxt, highlight))
                    {
                        highlights[key].Add(bookMatches[i - 1]);
                    }

                    if (i == bookMatches.Count - 1)
                    {
                        highlights[key].Add(match);
                    }
                    prevTxt = highlight;

                }
            }


            return highlights;
        }

        private static void AddMatch(Dictionary<string, List<Match>> highlights, Match match)
        {
            string title = match.Groups["title"].Value;
            string author = match.Groups["author"].Value;
            string comb = $"{author} - {title}";
            if (!highlights.ContainsKey(comb))
            {
                highlights.Add(comb, new List<Match>());
            }

            highlights[comb].Add(match);
        }

        private static void Output(Dictionary<string, List<Match>> highlights)
        {
            Directory.CreateDirectory("notes");

            foreach (var kvp in highlights)
            {
                string textWithPages = ToStringWithPages(kvp.Value);
                var file = File.CreateText(Path.Combine("notes", ToSafeFileName(kvp.Key + ".txt")));
                file.Write(textWithPages);
                file.Close();

                string textWithoutPages = ToStringWithoutPages(kvp.Value);
                file = File.CreateText(Path.Combine("notes", ToSafeFileName(kvp.Key + "_nopages.txt")));
                file.Write(textWithoutPages);
                file.Close();
            }
        }

        private static string ToStringWithPages(IEnumerable<Match> matches)
        {
            var highlights = matches.Select(x => 
                (string.IsNullOrEmpty(x.Groups["page"].Value)
                    ? "Loc: " + x.Groups["locationStart"].Value
                    : "Page: " + x.Groups["page"].Value) 
            	+ "\r\n\r\n" + x.Groups["highlight"].Value);
			
            return string.Join("\r\n\r\n----\r\n\r\n", highlights);
        }

        private static string ToStringWithoutPages(IEnumerable<Match> matches)
        {
            var highlights = matches.Select(x => "\r\n\r\n\"" + x.Groups["highlight"].Value + "\"");
            return string.Join("", highlights);
        }

        private static string ToSafeFileName(string s)
        {
            return s
                .Replace("\\", "")
                .Replace("/", "")
                .Replace("\"", "")
                .Replace("*", "")
                .Replace(":", "")
                .Replace("?", "")
                .Replace("<", "")
                .Replace(">", "")
                .Replace("|", "");
        }
    }
}

