using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace ModernLibrary
{
    public partial class MainWindow : Window
    {
        private string dataFilePath = "library_data.json";
        private List<Book> books = new List<Book>();

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
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

        private void DisplayBooks()
        {
            BooksListBox.ItemsSource = null;
            BooksListBox.ItemsSource = books;
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
                Status = "Available"
            };

            books.Add(book);
            SaveData();
            DisplayBooks();

            TitleTextBox.Clear();
            AuthorTextBox.Clear();
            IsbnTextBox.Clear();
            YearTextBox.Clear();
            GenreTextBox.Clear();

            MessageBox.Show("Book added successfully!");
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
        public string Status { get; set; } = "Available";

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}