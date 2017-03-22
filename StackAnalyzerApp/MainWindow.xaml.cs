using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

using Microsoft.Win32;

namespace StackAnalyzerApp
{
    internal sealed partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonLoad_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { DefaultExt = "txt" };

            if (!dialog.ShowDialog().GetValueOrDefault())
                return;

            if (string.IsNullOrWhiteSpace(dialog.FileName))
            {
                MessageBox.Show("Empty file name");
                return;
            }

            Dump dump;

            try
            {
                dump = new Dump(dialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }

            dump.SetSearchString(txtSearch.Text);
            dump.SetHighlightString(txtHighlight.Text);

            DataContext = dump;

            Title = "Stack Analyzer - " + dialog.FileName;
        }

        private void ListBoxThreads_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = lbThreads.SelectedItem as Thread;

            rtbStackTrace.Document = selectedItem != null ? selectedItem.StackTrace : new FlowDocument();
        }

        private void TextBoxSearch_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            SetConditionString((dump, s) => dump.SetSearchString(s), txtSearch.Text);
        }

        private void TextBoxHighlight_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            SetConditionString((dump, s) => dump.SetHighlightString(s), txtHighlight.Text);
        }

        private void SetConditionString(Action<Dump, string> setter, string conditionString)
        {
            var dump = DataContext as Dump;
            if (dump == null)
                return;

            setter(dump, conditionString);

            var selectedItem = lbThreads.SelectedItem as Thread;

            var verticalOffset = rtbStackTrace.VerticalOffset;
            var horizontalOffset = rtbStackTrace.HorizontalOffset;

            rtbStackTrace.Document = selectedItem != null ? selectedItem.StackTrace : new FlowDocument();

            rtbStackTrace.ScrollToVerticalOffset(verticalOffset);
            rtbStackTrace.ScrollToHorizontalOffset(horizontalOffset);
        }
    }
}