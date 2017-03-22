using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace StackAnalyzerApp
{
    internal sealed class Dump : INotifyPropertyChanged
    {
        #region Consts
        private const char Separator = '-';
        #endregion

        #region Fields
        private readonly List<Thread> threads;

        private string searchString;
        private string highlightString;
        #endregion

        #region Ctor
        internal Dump(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            var lines = File.ReadAllLines(fileName);

            var stackLines = new List<List<string>>();
            List<string> currentStackLines = null;

            foreach (var line in SkipHeader(lines).Where(line => !string.IsNullOrWhiteSpace(line)))
            {
                if (line.All(c => c == Separator))
                {
                    if (currentStackLines != null && currentStackLines.Any())
                        stackLines.Add(currentStackLines);

                    currentStackLines = new List<string>();
                }
                else
                {
                    currentStackLines.Add(line);
                }
            }

            if (currentStackLines != null && currentStackLines.Any())
                stackLines.Add(currentStackLines);

            threads = stackLines
                .Select(stack => new Thread(stack))
                .OrderBy(thread => thread.Number)
                .ToList();
        }
        #endregion

        #region Props
        public IEnumerable<Thread> AllThreads
        {
            get { return threads.Where(t => t.MatchSearchString); }
        }

        public IEnumerable<Thread> HighlightedThreads
        {
            get { return AllThreads.Where(t => t.Highlighted); }
        }

        public IEnumerable<Thread> NonHighlightedThreads
        {
            get { return AllThreads.Where(t => !t.Highlighted); }
        }
        #endregion

        #region Property Changed
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            var temp = PropertyChanged;
            if (temp == null)
                return;

            temp(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Methods
        public void SetSearchString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                value = null;

            if (searchString == value)
                return;

            searchString = value;

            threads.ForEach(t => t.SetSearchString(searchString));

            RaisePropertyChangedEvents();
        }

        public void SetHighlightString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                value = null;

            if (highlightString == value)
                return;

            highlightString = value;

            threads.ForEach(t => t.SetHighlightString(highlightString));

            RaisePropertyChangedEvents();
        }

        private void RaisePropertyChangedEvents()
        {
            OnPropertyChanged("AllThreads");
            OnPropertyChanged("HighlightedThreads");
            OnPropertyChanged("NonHighlightedThreads");
        }

        private static IEnumerable<string> SkipHeader(IEnumerable<string> lines)
        {
            var skipped = false;

            foreach (var line in lines)
            {
                if (skipped)
                {
                    yield return line;
                }
                else
                {
                    if (line.All(c => c == Separator))
                    {
                        skipped = true;
                        yield return line;
                    }
                }
            }
        }
        #endregion
    }
}