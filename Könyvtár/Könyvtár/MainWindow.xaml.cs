using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace ModernLibrary
{
    public partial class MainWindow : Window
    {
        private string dataFilePath = "library_data.json";
        private List<Book> books = new List<Book>();
        private Book selectedBook = null;
        private bool isEditing = false;

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
            InitializeComboBoxes();
            UpdateStats();
            DisplayBooks();
        }

        private void LoadData()
        {
            if (File.Exists(dataFilePath))
            {
                try
                {
                    string json = File.ReadAllText(dataFilePath);
                    books = JsonConvert.DeserializeObject<List<Book>>(json) ?? new List<Book>();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading data: {ex.Message}");
                    books = new List<Book>();
                }
            }
        }

        private void SaveData()
        {
            try
            {
                string json = JsonConvert.SerializeObject(books, Formatting.Indented);
                File.WriteAllText(dataFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving data: {ex.Message}");
            }
        }

        private void InitializeComboBoxes()
        {
            // Genre combobox
            GenreComboBox.Items.Clear();
            string[] genres = { "Fiction", "Science Fiction", "Mystery", "Romance", "Thriller",
                              "Biography", "History", "Science", "Technology", "Fantasy",
                              "Horror", "Children", "Young Adult", "Poetry", "Drama" };
            foreach (string genre in genres)
            {
                GenreComboBox.Items.Add(genre);
            }

            // Status combobox
            StatusComboBox.Items.Clear();
            StatusComboBox.Items.Add("Available");
            StatusComboBox.Items.Add("Borrowed");
        }

        private void UpdateStats()
        {
            int total = books.Count;
            int available = books.Count(b => b.Status == "Available");
            int borrowed = books.Count(b => b.Status == "Borrowed");

            TotalBooksText.Text = total.ToString();
            AvailableBooksText.Text = available.ToString();
            BorrowedBooksText.Text = borrowed.ToString();
        }

        private void DisplayBooks()
        {
            BooksListBox.ItemsSource = null;
            BooksListBox.ItemsSource = books;
        }

        private void ClearForm()
        {
            TitleTextBox.Clear();
            AuthorTextBox.Clear();
            IsbnTextBox.Clear();
            YearTextBox.Clear();
            PublisherTextBox.Clear();
            GenreComboBox.SelectedIndex = -1;
            StatusComboBox.SelectedIndex = 0;

            FormTitle.Text = "Add New Book";
            AddButton.Visibility = Visibility.Visible;
            UpdateButton.Visibility = Visibility.Collapsed;

            selectedBook = null;
            isEditing = false;
        }

        // ===== EVENT HANDLERS =====

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
                Id = books.Count > 0 ? books.Max(b => b.Id) + 1 : 1,
                Title = TitleTextBox.Text.Trim(),
                Author = AuthorTextBox.Text.Trim(),
                ISBN = IsbnTextBox.Text.Trim(),
                Year = YearTextBox.Text.Trim(),
                Genre = GenreComboBox.SelectedItem?.ToString() ?? "Fiction",
                Publisher = PublisherTextBox.Text.Trim(),
                Status = StatusComboBox.SelectedItem?.ToString() ?? "Available"
            };

            books.Add(book);
            SaveData();
            UpdateStats();
            DisplayBooks();
            ClearForm();

            MessageBox.Show("Book added successfully!");
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedBook == null) return;

            if (string.IsNullOrWhiteSpace(TitleTextBox.Text) ||
                string.IsNullOrWhiteSpace(AuthorTextBox.Text))
            {
                MessageBox.Show("Please enter title and author!");
                return;
            }

            selectedBook.Title = TitleTextBox.Text.Trim();
            selectedBook.Author = AuthorTextBox.Text.Trim();
            selectedBook.ISBN = IsbnTextBox.Text.Trim();
            selectedBook.Year = YearTextBox.Text.Trim();
            selectedBook.Genre = GenreComboBox.SelectedItem?.ToString() ?? "Fiction";
            selectedBook.Publisher = PublisherTextBox.Text.Trim();
            selectedBook.Status = StatusComboBox.SelectedItem?.ToString() ?? "Available";

            SaveData();
            UpdateStats();
            DisplayBooks();
            ClearForm();

            MessageBox.Show("Book updated successfully!");
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int bookId = (int)button.Tag;
                var bookToDelete = books.FirstOrDefault(b => b.Id == bookId);

                if (bookToDelete != null)
                {
                    var result = MessageBox.Show($"Are you sure you want to delete '{bookToDelete.Title}'?",
                                               "Confirm Delete", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        books.Remove(bookToDelete);
                        SaveData();
                        UpdateStats();
                        DisplayBooks();

                        if (selectedBook != null && selectedBook.Id == bookId)
                        {
                            ClearForm();
                        }
                    }
                }
            }
        }

        private void BorrowReturnButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int bookId = (int)button.Tag;
                var book = books.FirstOrDefault(b => b.Id == bookId);

                if (book != null)
                {
                    if (book.Status == "Available")
                    {
                        book.Status = "Borrowed";
                        book.Borrower = "Borrower";
                        book.DueDate = DateTime.Now.AddDays(14).ToString("yyyy-MM-dd");
                        MessageBox.Show($"Book '{book.Title}' borrowed!");
                    }
                    else
                    {
                        book.Status = "Available";
                        book.Borrower = "";
                        book.DueDate = "";
                        MessageBox.Show($"Book '{book.Title}' returned!");
                    }

                    SaveData();
                    UpdateStats();
                    DisplayBooks();
                }
            }
        }

        private void BooksListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedBook = BooksListBox.SelectedItem as Book;
            if (selectedBook != null)
            {
                // Fill form with book data
                TitleTextBox.Text = selectedBook.Title;
                AuthorTextBox.Text = selectedBook.Author;
                IsbnTextBox.Text = selectedBook.ISBN;
                YearTextBox.Text = selectedBook.Year;
                GenreComboBox.SelectedItem = selectedBook.Genre;
                PublisherTextBox.Text = selectedBook.Publisher;
                StatusComboBox.SelectedItem = selectedBook.Status;

                // Update UI for editing
                FormTitle.Text = "Edit Book";
                AddButton.Visibility = Visibility.Collapsed;
                UpdateButton.Visibility = Visibility.Visible;

                isEditing = true;
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int bookId = (int)button.Tag;
                selectedBook = books.FirstOrDefault(b => b.Id == bookId);
                if (selectedBook != null)
                {
                    BooksListBox.SelectedItem = selectedBook;
                }
            }
        }
    }

    public class Book : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public string Year { get; set; }
        public string Genre { get; set; }
        public string Publisher { get; set; }

        private string _status;
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(StatusColor));
            }
        }

        public string Borrower { get; set; }
        public string DueDate { get; set; }

        public string StatusColor
        {
            get
            {
                return Status == "Available" ? "#10b981" : "#f59e0b";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}