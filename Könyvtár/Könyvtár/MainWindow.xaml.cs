using System.Collections.Generic;
using System.Windows;

namespace ModernLibrary
{
    public partial class MainWindow : Window
    {
        private List<Book> books = new List<Book>();

        public MainWindow()
        {
            InitializeComponent();
            InitializeComboBoxes();
        }

        private void InitializeComboBoxes()
        {
            // Genre combobox
            GenreComboBox.Items.Clear();
            string[] genres = { "Fiction", "Science Fiction", "Mystery", "Romance", "Thriller",
                              "Biography", "History", "Science", "Technology", "Fantasy" };
            foreach (string genre in genres)
            {
                GenreComboBox.Items.Add(genre);
            }

            // Status combobox
            StatusComboBox.Items.Clear();
            StatusComboBox.Items.Add("Available");
            StatusComboBox.Items.Add("Borrowed");
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text) ||
                string.IsNullOrWhiteSpace(AuthorTextBox.Text))
            {
                MessageBox.Show("Please enter title and author!");
                return;
            }

            var book = new Book
            {
                Id = books.Count > 0 ? books.Count + 1 : 1,
                Title = TitleTextBox.Text.Trim(),
                Author = AuthorTextBox.Text.Trim(),
                ISBN = IsbnTextBox.Text.Trim(),
                Year = YearTextBox.Text.Trim(),
                Genre = GenreComboBox.SelectedItem?.ToString() ?? "Fiction",
                Publisher = PublisherTextBox.Text.Trim(),
                Status = StatusComboBox.SelectedItem?.ToString() ?? "Available",
                Borrower = "",
                DueDate = ""
            };

            books.Add(book);
            MessageBox.Show("Book added successfully!");
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            TitleTextBox.Clear();
            AuthorTextBox.Clear();
            IsbnTextBox.Clear();
            YearTextBox.Clear();
            PublisherTextBox.Clear();
            GenreComboBox.SelectedIndex = -1;
            StatusComboBox.SelectedIndex = 0;
        }
    }
}