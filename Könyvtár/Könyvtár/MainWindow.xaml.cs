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
            InitializeComboBoxes();
            LoadData();
            UpdateStats();
            DisplayBooks();
        }

        private void InitializeComboBoxes()
        {
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
            GenreTextBox.Clear();

            FormTitle.Text = "Add New Book";
            AddButton.Visibility = Visibility.Visible;
            UpdateButton.Visibility = Visibility.Collapsed;

            selectedBook = null;
            isEditing = false;
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
                Id = books.Count > 0 ? books.Max(b => b.Id) + 1 : 1,
                Title = TitleTextBox.Text.Trim(),
                Author = AuthorTextBox.Text.Trim(),
                ISBN = IsbnTextBox.Text.Trim(),
                Year = YearTextBox.Text.Trim(),
                Genre = GenreTextBox.Text.Trim(),
                Status = StatusComboBox.SelectedItem?.ToString() ?? "Available"
            };

            books.Add(book);
            SaveData();
            UpdateStats();
            DisplayBooks();
            ClearForm();

            MessageBox.Show("Book added successfully!");
        }

        private void BooksListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedBook = BooksListBox.SelectedItem as Book;
            if (selectedBook != null)
            {
                TitleTextBox.Text = selectedBook.Title;
                AuthorTextBox.Text = selectedBook.Author;
                IsbnTextBox.Text = selectedBook.ISBN;
                YearTextBox.Text = selectedBook.Year;
                GenreTextBox.Text = selectedBook.Genre;
                StatusComboBox.SelectedItem = selectedBook.Status;

                FormTitle.Text = "Edit Book";
                AddButton.Visibility = Visibility.Collapsed;
                UpdateButton.Visibility = Visibility.Visible;
                isEditing = true;
            }
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
            selectedBook.Genre = GenreTextBox.Text.Trim();
            selectedBook.Status = StatusComboBox.SelectedItem?.ToString() ?? "Available";

            SaveData();
            UpdateStats();
            DisplayBooks();
            ClearForm();

            MessageBox.Show("Book updated successfully!");
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedBook == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete '{selectedBook.Title}'?",
                                        "Confirm Delete", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                books.Remove(selectedBook);
                SaveData();
                UpdateStats();
                DisplayBooks();
                ClearForm();
            }
        }
    }

    public class Book : INotifyPropertyChanged
    {
        private string _status;

        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public string Year { get; set; }
        public string Genre { get; set; }
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

        public string StatusColor => Status == "Available" ? "#10b981" : "#f59e0b";

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}