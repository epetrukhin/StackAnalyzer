using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace StackAnalyzerApp
{
    internal sealed class Thread
    {
        #region Fields
        private readonly int number;

        private FlowDocument stackTrace;

        private readonly List<string> strings;

        private string searchString;
        private string highlightString;
        #endregion

        #region Ctor
        public Thread(List<string> strings)
        {
            if (strings == null)
                throw new ArgumentNullException("strings");
            if (strings.Count < 4)
                throw new ArgumentException("Too small stackTrace", "strings");


            var firstLine = strings[0];
            var parts = firstLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2 || parts[0] != "Thread" || !int.TryParse(parts[1], out number))
                throw new ArgumentException(string.Format("Invalid first line (header): {0}", firstLine), "strings");

            this.strings = strings;
        }
        #endregion

        #region Props
        public int Number
        {
            get { return number; }
        }

        public FlowDocument StackTrace
        {
            get { return stackTrace ?? (stackTrace = CreateDocument()); }
        }

        public bool Highlighted
        {
            get
            {
                return
                    !string.IsNullOrWhiteSpace(highlightString) &&
                    strings.Skip(1).Any(s => s.Contains(highlightString));
            }
        }

        public bool MatchSearchString
        {
            get
            {
                return
                    string.IsNullOrWhiteSpace(searchString) ||
                    strings.Skip(1).Any(s => s.Contains(searchString));
            }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return string.Format("Thread {0}", number);
        }

        internal void SetSearchString(string newSearchString)
        {
            if (searchString == newSearchString)
                return;

            searchString = newSearchString;
            stackTrace = null;
        }

        internal void SetHighlightString(string newHighlightString)
        {
            if (highlightString == newHighlightString)
                return;

            highlightString = newHighlightString;
            stackTrace = null;
        }

        private FlowDocument CreateDocument()
        {
            var para = new Paragraph();
            para.Inlines.AddRange(strings.SelectMany((line, index) => ConvertToInlines(line, index + 1)));

            return new FlowDocument(para) { PageWidth = 5000 };
        }

        private IEnumerable<Inline> ConvertToInlines(string line, int lineNumber)
        {
            yield return new Run(lineNumber.ToString().PadLeft(strings.Count.ToString().Length) + "    ") { Foreground = LineNumberBrush };
            if (lineNumber == 1)
            {
                var run = new Run(line) { Foreground = Brushes.Red, FontWeight = FontWeights.Bold };
                yield return run;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(searchString) || !line.Contains(searchString))
                {
                    var run = new Run(line);

                    if (!string.IsNullOrWhiteSpace(highlightString) && line.Contains(highlightString))
                        run.Foreground = Brushes.Blue;

                    yield return run;
                }
                else
                {
                    foreach (var str in Split(line, searchString))
                    {
                        var run = new Run(str);

                        if (str == searchString)
                            run.Background = Brushes.Yellow;

                        if (!string.IsNullOrWhiteSpace(highlightString) && line.Contains(highlightString))
                            run.Foreground = Brushes.Blue;

                        yield return run;
                    }
                }
            }

            yield return new LineBreak();
        }

        private static IEnumerable<string> Split(string line, string splitter)
        {
            if (line == string.Empty)
                yield break;

            var i = line.IndexOf(splitter);
            if (i < 0)
            {
                yield return line;
                yield break;
            }

            yield return line.Substring(0, i);
            yield return splitter;

            foreach (var str in Split(line.Substring(i + splitter.Length), splitter))
            {
                yield return str;
            }
        }
        #endregion

        #region Static Members
        private static readonly SolidColorBrush LineNumberBrush;

        static Thread()
        {
            LineNumberBrush = new SolidColorBrush(Color.FromRgb(43, 145, 175));
        }
        #endregion
    }
}