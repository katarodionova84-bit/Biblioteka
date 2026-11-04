using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Biblioteka
{
    public partial class MainWindow : Window
    {
        private LibraryEntities db = new LibraryEntities();
        public MainWindow()
        {
            InitializeComponent();
            LoadData();
        }
        private void LoadData()
        {
            try
            {
                BooksGrid.ItemsSource = db.Книги
                    .ToList()
                    .Select(b => new
                    {
                        Код = b.КодКниги,
                        Название = b.Название,
                        КодАвтора = b.КодАвтора,
                        ГодИздания = b.ГодИздания,
                        Жанр = b.Жанр,
                        Страниц = b.КоличествоСтраниц
                    })
                    .ToList();
                AuthorsGrid.ItemsSource = db.Авторы
                    .ToList()
                    .Select(a => new
                    {
                        Код = a.КодАвтора,
                        Имя = a.Имя,
                        ДатаРождения = a.ДатаРождения.ToString("dd.MM.yyyy")
                    })
                    .ToList();

                ReadersGrid.ItemsSource = db.Читатели
                    .ToList()
                    .Select(r => new
                    {
                        Код = r.КодЧитателя,
                        ФИО = r.ФИО,
                        ДатаРождения = r.ДатаРождения.ToString("dd.MM.yyyy"),
                        Телефон = r.НомерТелефона,
                        Email = r.ЭлектроннаяПочта,
                        Роль = r.Роль
                    })
                    .ToList();

                BorrowedGrid.ItemsSource = db.ВыданныеКниги
                    .ToList()
                    .Select(v => new
                    {
                        Код = v.КодВыдачи,
                        КодКниги = v.КодКниги,
                        КодЧитателя = v.КодЧитателя,
                        ДатаВыдачи = v.ДатаВыдачи.ToString("dd.MM.yyyy"),
                        ДатаВозврата = v.ДатаВозврата.ToString("dd.MM.yyyy"),
                        ФактическийВозврат = v.ФактическаяДатаВозврата.HasValue
                            ? v.ФактическаяДатаВозврата.Value.ToString("dd.MM.yyyy")
                            : "Не возвращена"
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось загрузить данные:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        private void BookSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string search = BookSearch.Text.Trim().ToLower();

                var books = db.Книги.ToList();
                var authors = db.Авторы.ToList();

                var result = books
                    .Where(b =>
                    {
                        var author = authors.FirstOrDefault(
                            a => a.КодАвтора == b.КодАвтора);

                        return
                            (b.Название != null &&
                             b.Название.ToLower().Contains(search))
                            ||
                            (b.Жанр != null &&
                             b.Жанр.ToLower().Contains(search))
                            ||
                            (author != null &&
                             author.Имя != null &&
                             author.Имя.ToLower().Contains(search));
                    })
                    .Select(b =>
                    {
                        var author = authors.FirstOrDefault(
                            a => a.КодАвтора == b.КодАвтора);

                        return new
                        {
                            Код = b.КодКниги,
                            Название = b.Название,
                            Автор = author != null ? author.Имя : "Неизвестен",
                            ГодИздания = b.ГодИздания,
                            Жанр = b.Жанр,
                            Страниц = b.КоличествоСтраниц
                        };
                    })
                    .ToList();

                BooksGrid.ItemsSource = result;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка поиска: " + ex.Message);
            }
        }

        private void AddBook_Click(object sender, RoutedEventArgs e)
        {
            AddBookWindow window = new AddBookWindow(db);
            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                LoadData();
            }
        }
        private void EditBook_Click(object sender, RoutedEventArgs e)
        {
            if (BooksGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите книгу для редактирования!");
                return;
            }
            try
            {
                var selected = BooksGrid.SelectedItem;
                int bookId = (int)selected.GetType()
                    .GetProperty("Код").GetValue(selected);

                var book = db.Книги.Find(bookId);

                if (book == null)
                {
                    MessageBox.Show("Книга не найдена!");
                    return;
                }

                AddBookWindow window = new AddBookWindow(db, book);
                window.Owner = this;

                if (window.ShowDialog() == true)
                {
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка редактирования: " + ex.Message);
            }
        }
        private void DeleteBook_Click(object sender, RoutedEventArgs e)
        {
            if (BooksGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите книгу для удаления!");
                return;
            }

            try
            {
                var selected = BooksGrid.SelectedItem;
                int bookId = (int)selected.GetType()
                    .GetProperty("Код").GetValue(selected);

                var book = db.Книги.Find(bookId);

                if (book == null)
                {
                    MessageBox.Show("Книга не найдена!");
                    return;
                }

                bool hasHistory = db.ВыданныеКниги
                    .Any(b => b.КодКниги == bookId);

                if (hasHistory)
                {
                    MessageBox.Show(
                        "Нельзя удалить книгу, у которой есть история выдач!");
                    return;
                }

                var result = MessageBox.Show(
                    "Удалить книгу «" + book.Название + "»?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    db.Книги.Remove(book);
                    db.SaveChanges();

                    LoadData();

                    MessageBox.Show("Книга удалена!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message);
            }
        }

        private void AddAuthor_Click(object sender, RoutedEventArgs e)
        {
            AddAuthorWindow window = new AddAuthorWindow(db);
            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                LoadData();
            }
        }
        private void EditAuthor_Click(object sender, RoutedEventArgs e)
        {
            if (AuthorsGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите автора!");
                return;
            }

            try
            {
                var selected = AuthorsGrid.SelectedItem;

                int authorId = (int)selected.GetType()
                    .GetProperty("Код").GetValue(selected);

                var author = db.Авторы.Find(authorId);

                if (author == null)
                {
                    MessageBox.Show("Автор не найден!");
                    return;
                }

                AddAuthorWindow window = new AddAuthorWindow(db, author);
                window.Owner = this;

                if (window.ShowDialog() == true)
                {
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка редактирования: " + ex.Message);
            }
        }
        private void DeleteAuthor_Click(object sender, RoutedEventArgs e)
        {
            if (AuthorsGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите автора!");
                return;
            }

            try
            {
                var selected = AuthorsGrid.SelectedItem;

                int authorId = (int)selected.GetType()
                    .GetProperty("Код").GetValue(selected);

                var author = db.Авторы.Find(authorId);

                if (author == null)
                {
                    MessageBox.Show("Автор не найден!");
                    return;
                }

                bool hasBooks = db.Книги.Any(
                    b => b.КодАвтора == authorId);

                if (hasBooks)
                {
                    MessageBox.Show(
                        "Нельзя удалить автора, у которого есть книги!");
                    return;
                }

                var result = MessageBox.Show(
                    "Удалить автора «" + author.Имя + "»?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    db.Авторы.Remove(author);
                    db.SaveChanges();

                    LoadData();

                    MessageBox.Show("Автор удалён!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message);
            }
        }

        private void AddReader_Click(object sender, RoutedEventArgs e)
        {
            AddReaderWindow window = new AddReaderWindow(db);
            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                LoadData();
            }
        }
        private void EditReader_Click(object sender, RoutedEventArgs e)
        {
            if (ReadersGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите читателя!");
                return;
            }

            try
            {
                var selected = ReadersGrid.SelectedItem;

                int readerId = (int)selected.GetType()
                    .GetProperty("Код").GetValue(selected);

                var reader = db.Читатели.Find(readerId);

                if (reader == null)
                {
                    MessageBox.Show("Читатель не найден!");
                    return;
                }

                AddReaderWindow window = new AddReaderWindow(db, reader);
                window.Owner = this;

                if (window.ShowDialog() == true)
                {
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка редактирования: " + ex.Message);
            }
        }
        private void DeleteReader_Click(object sender, RoutedEventArgs e)
        {
            if (ReadersGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите читателя!");
                return;
            }

            try
            {
                var selected = ReadersGrid.SelectedItem;

                int readerId = (int)selected.GetType()
                    .GetProperty("Код").GetValue(selected);

                var reader = db.Читатели.Find(readerId);

                if (reader == null)
                {
                    MessageBox.Show("Читатель не найден!");
                    return;
                }

                bool hasHistory = db.ВыданныеКниги.Any(
                    b => b.КодЧитателя == readerId);

                if (hasHistory)
                {
                    MessageBox.Show(
                        "Нельзя удалить читателя, у которого есть история выдач!");
                    return;
                }

                var result = MessageBox.Show(
                    "Удалить читателя «" + reader.ФИО + "»?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    db.Читатели.Remove(reader);
                    db.SaveChanges();

                    LoadData();

                    MessageBox.Show("Читатель удалён!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message);
            }
        }

        private void IssueBook_Click(object sender, RoutedEventArgs e)
        {
            IssueBookWindow window = new IssueBookWindow(db);
            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                LoadData();
            }
        }
        private void ReturnBook_Click(object sender, RoutedEventArgs e)
        {
            if (BorrowedGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите запись о выдаче книги!");
                return;
            }

            try
            {
                var selected = BorrowedGrid.SelectedItem;

                int borrowId = (int)selected.GetType()
                    .GetProperty("Код").GetValue(selected);

                var borrowedBook = db.ВыданныеКниги.Find(borrowId);

                if (borrowedBook == null)
                {
                    MessageBox.Show("Запись не найдена!");
                    return;
                }

                if (borrowedBook.ФактическаяДатаВозврата != null)
                {
                    MessageBox.Show("Эта книга уже возвращена!");
                    return;
                }

                DateTime actualReturnDate = DateTime.Today;

                if (actualReturnDate < borrowedBook.ДатаВыдачи)
                {
                    MessageBox.Show(
                        "Дата возврата не может быть раньше даты выдачи!");
                    return;
                }

                var result = MessageBox.Show(
                    "Оформить возврат книги?",
                    "Подтверждение возврата",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    borrowedBook.ФактическаяДатаВозврата =
                        actualReturnDate;

                    db.SaveChanges();

                    LoadData();

                    MessageBox.Show("Возврат книги успешно оформлен!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка оформления возврата: " + ex.Message);
            }
        }

        private void RefreshBorrowed_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
    }
}