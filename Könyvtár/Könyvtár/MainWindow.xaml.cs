using System.Windows;

namespace ModernLibrary
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // Később implementáljuk
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            TitleTextBox.Clear();
            AuthorTextBox.Clear();
            IsbnTextBox.Clear();
        }
    }
}