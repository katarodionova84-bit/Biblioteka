using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Biblioteka
{
    public partial class Book : Window
    {
        private readonly LibraryEntities db;
        private readonly Книги editingBook;

        public Book(LibraryEntities context)
        {
            InitializeComponent();
            db = context;
            editingBook = null;
            AuthorBox.ItemsSource = db.Авторы.ToList();
        }

        public Book(LibraryEntities context, Книги book)
        {
            InitializeComponent();
            db = context;
            editingBook = book;
            AuthorBox.ItemsSource = db.Авторы.ToList();

            TitleBox.Text = book.Название;
            AuthorBox.SelectedValue = book.КодАвтора;
            YearBox.Text = book.ГодИздания.ToString();
            GenreBox.Text = book.Жанр;
            PagesBox.Text = book.КоличествоСтраниц.ToString();

            Title = "Редактирование книги";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleBox.Text) ||
                AuthorBox.SelectedValue == null ||
                string.IsNullOrWhiteSpace(YearBox.Text) ||
                string.IsNullOrWhiteSpace(GenreBox.Text) ||
                string.IsNullOrWhiteSpace(PagesBox.Text))
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            if (!int.TryParse(YearBox.Text, out int year) || year < 1 || year > 9999)
            {
                MessageBox.Show("Введите корректный год!");
                return;
            }

            if (!int.TryParse(PagesBox.Text, out int pages) || pages <= 0)
            {
                MessageBox.Show("Количество страниц должно быть больше нуля!");
                return;
            }

            try
            {
                Книги book = editingBook ?? new Книги();
                book.Название = TitleBox.Text.Trim();
                book.КодАвтора = (int)AuthorBox.SelectedValue;
                book.ГодИздания = year;
                book.Жанр = GenreBox.Text.Trim();
                book.КоличествоСтраниц = pages;

                if (editingBook == null)
                    db.Книги.Add(book);

                db.SaveChanges();

                MessageBox.Show(editingBook == null
                    ? "Книга успешно добавлена!"
                    : "Изменения сохранены!");

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void AuthorBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }
}