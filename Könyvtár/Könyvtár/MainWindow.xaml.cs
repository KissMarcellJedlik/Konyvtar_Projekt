using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
            BorrowerTextBox.Clear();
            DueDatePicker.SelectedDate = null;
            BorrowerSection.Visibility = Visibility.Collapsed;

            FormTitle.Text = "Add New Book";
            AddButton.Visibility = Visibility.Visible;
            UpdateButton.Visibility = Visibility.Collapsed;
            BorrowReturnButton.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Collapsed;

            selectedBook = null;
            isEditing = false;

            // Deselect from ListBox
            BooksListBox.SelectedItem = null;
        }

        private void ShowBorrowerSection()
        {
            BorrowerSection.Visibility = Visibility.Visible;
        }

        private void HideBorrowerSection()
        {
            BorrowerSection.Visibility = Visibility.Collapsed;
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
                Status = "Available",
                Borrower = "",
                DueDate = ""
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

            // Only update status if it's actually changed
            var newStatus = StatusComboBox.SelectedItem?.ToString() ?? "Available";
            if (selectedBook.Status != newStatus)
            {
                selectedBook.Status = newStatus;

                // If changing to Available, clear borrower info
                if (newStatus == "Available")
                {
                    selectedBook.Borrower = "";
                    selectedBook.DueDate = "";
                }
                // If changing to Borrowed, ensure borrower info is set
                else if (newStatus == "Borrowed" && string.IsNullOrWhiteSpace(selectedBook.Borrower))
                {
                    // Don't allow changing to Borrowed without borrower info
                    MessageBox.Show("Cannot change status to Borrowed without borrower information!");
                    return;
                }
            }

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

                        // If the deleted book was currently selected, clear the form
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
            // Get book from button tag
            var button = sender as Button;
            Book bookToUpdate = null;

            if (button?.Tag != null)
            {
                int bookId = (int)button.Tag;
                bookToUpdate = books.FirstOrDefault(b => b.Id == bookId);
            }

            // If not found via button tag, try the currently selected book
            if (bookToUpdate == null)
            {
                bookToUpdate = selectedBook;
            }

            if (bookToUpdate == null)
            {
                MessageBox.Show("No book selected!");
                return;
            }

            if (bookToUpdate.Status == "Available")
            {
                // Borrow book
                string borrowerName = "";
                DateTime? dueDate = null;

                // Check if we're in edit mode (form is showing the book)
                if (isEditing && selectedBook != null && selectedBook.Id == bookToUpdate.Id)
                {
                    // Use form data
                    if (string.IsNullOrWhiteSpace(BorrowerTextBox.Text))
                    {
                        MessageBox.Show("Please enter borrower name!");
                        BorrowerTextBox.Focus();
                        return;
                    }

                    if (DueDatePicker.SelectedDate == null)
                    {
                        MessageBox.Show("Please select due date!");
                        DueDatePicker.Focus();
                        return;
                    }

                    borrowerName = BorrowerTextBox.Text.Trim();
                    dueDate = DueDatePicker.SelectedDate;
                }
                else
                {
                    // Direct click from book card - use default values
                    borrowerName = "Borrower";
                    dueDate = DateTime.Now.AddDays(14);
                }

                bookToUpdate.Status = "Borrowed";
                bookToUpdate.Borrower = borrowerName;
                bookToUpdate.DueDate = dueDate.Value.ToString("yyyy-MM-dd");

                SaveData();
                UpdateStats();
                DisplayBooks();

                // Update the selectedBook reference if it's the same book
                if (selectedBook != null && selectedBook.Id == bookToUpdate.Id)
                {
                    selectedBook = bookToUpdate;
                }

                if (isEditing && selectedBook != null && selectedBook.Id == bookToUpdate.Id)
                {
                    ClearForm();
                }

                MessageBox.Show($"Book '{bookToUpdate.Title}' borrowed by {bookToUpdate.Borrower}");
            }
            else
            {
                // Return book
                string bookTitle = bookToUpdate.Title;

                bookToUpdate.Status = "Available";
                bookToUpdate.Borrower = "";
                bookToUpdate.DueDate = "";

                SaveData();
                UpdateStats();
                DisplayBooks();

                // Update the selectedBook reference if it's the same book
                if (selectedBook != null && selectedBook.Id == bookToUpdate.Id)
                {
                    selectedBook = bookToUpdate;
                }

                if (isEditing && selectedBook != null && selectedBook.Id == bookToUpdate.Id)
                {
                    ClearForm();
                }

                MessageBox.Show($"Book '{bookTitle}' returned successfully!");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void BooksListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If clicking on empty space to deselect, handle it
            if (BooksListBox.SelectedItem == null && !isEditing)
            {
                return;
            }

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
                BorrowerTextBox.Text = selectedBook.Borrower;

                if (!string.IsNullOrEmpty(selectedBook.DueDate) &&
                    DateTime.TryParse(selectedBook.DueDate, out DateTime dueDate))
                {
                    DueDatePicker.SelectedDate = dueDate;
                }
                else
                {
                    DueDatePicker.SelectedDate = DateTime.Now.AddDays(14);
                }

                // Update UI for editing
                FormTitle.Text = "Edit Book";
                AddButton.Visibility = Visibility.Collapsed;
                UpdateButton.Visibility = Visibility.Visible;
                CancelButton.Visibility = Visibility.Visible;

                // Show/hide borrower section based on status
                if (selectedBook.Status == "Borrowed")
                {
                    ShowBorrowerSection();
                    BorrowReturnButton.Content = "📖 Return";
                    BorrowReturnButton.Visibility = Visibility.Visible;
                }
                else
                {
                    ShowBorrowerSection();
                    BorrowReturnButton.Content = "📖 Borrow";
                    BorrowReturnButton.Visibility = Visibility.Visible;

                    // Clear borrower fields for new borrowing
                    if (string.IsNullOrWhiteSpace(BorrowerTextBox.Text))
                    {
                        BorrowerTextBox.Clear();
                        DueDatePicker.SelectedDate = DateTime.Now.AddDays(14);
                    }
                }

                isEditing = true;
            }
        }

        // Click on empty space to deselect
        private void BooksListBox_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Check if click is on empty space (not on an item)
            var listBox = sender as ListBox;
            if (listBox != null)
            {
                var hitTestResult = VisualTreeHelper.HitTest(listBox, e.GetPosition(listBox));
                if (hitTestResult != null)
                {
                    var listBoxItem = FindParent<ListBoxItem>(hitTestResult.VisualHit);
                    if (listBoxItem == null)
                    {
                        // Clicked on empty space - deselect
                        listBox.SelectedItem = null;
                        ClearForm();
                    }
                }
            }
        }

        // Helper method to find parent of specific type
        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            if (parent == null) return null;
            if (parent is T parentOfType) return parentOfType;
            return FindParent<T>(parent);
        }

        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isEditing && StatusComboBox.SelectedItem != null)
            {
                var selectedStatus = StatusComboBox.SelectedItem.ToString();

                if (selectedStatus == "Borrowed")
                {
                    ShowBorrowerSection();
                    BorrowReturnButton.Content = "📖 Borrow";
                    BorrowReturnButton.Visibility = Visibility.Visible;
                }
                else if (selectedStatus == "Available")
                {
                    HideBorrowerSection();
                    BorrowReturnButton.Content = "📖 Borrow";
                    BorrowReturnButton.Visibility = Visibility.Collapsed;

                    // Clear borrower fields when switching to Available
                    BorrowerTextBox.Clear();
                    DueDatePicker.SelectedDate = null;
                }
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