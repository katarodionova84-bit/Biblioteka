using System;
using System.Globalization;
using System.Windows;

namespace Biblioteka
{
    public partial class Author : Window
    {
        private readonly LibraryEntities db;
        private readonly Авторы editingAuthor;

        public Author(LibraryEntities context)
        {
            InitializeComponent();
            db = context;
            editingAuthor = null;
        }

        public Author(LibraryEntities context, Авторы author)
        {
            InitializeComponent();
            db = context;
            editingAuthor = author;

            NameBox.Text = author.Имя;
            BirthDateBox.Text = author.ДатаРождения.ToString("dd.MM.yyyy");
            Title = "Редактирование автора";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Введите имя автора!");
                return;
            }

            DateTime birthDate;
            if (!DateTime.TryParseExact(BirthDateBox.Text.Trim(), "dd.MM.yyyy",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out birthDate))
            {
                MessageBox.Show("Введите дату в формате дд.мм.гггг!");
                return;
            }

            if (birthDate.Date > DateTime.Today)
            {
                MessageBox.Show("Дата рождения не может быть в будущем!");
                return;
            }

            try
            {
                Авторы author = editingAuthor ?? new Авторы();
                author.Имя = NameBox.Text.Trim();
                author.ДатаРождения = birthDate.Date;

                if (editingAuthor == null)
                    db.Авторы.Add(author);

                db.SaveChanges();

                MessageBox.Show(editingAuthor == null
                    ? "Автор успешно добавлен!"
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
    }
}

