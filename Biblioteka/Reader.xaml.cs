using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Biblioteka
{
    public partial class Reader : Window
    {
        private readonly LibraryEntities db;
        private readonly Читатели editingReader;

        public Reader(LibraryEntities context)
        {
            InitializeComponent();
            db = context;
            editingReader = null;
        }

        public Reader(LibraryEntities context, Читатели reader)
        {
            InitializeComponent();
            db = context;
            editingReader = reader;

            FullNameBox.Text = reader.ФИО;
            BirthDateBox.Text = reader.ДатаРождения.ToString("dd.MM.yyyy");
            PhoneBox.Text = reader.НомерТелефона;
            EmailBox.Text = reader.ЭлектроннаяПочта;
            PasswordInput.Password = reader.Пароль;

            foreach (ComboBoxItem item in RoleBox.Items)
            {
                if (item.Tag.ToString() == reader.Роль.ToString())
                {
                    RoleBox.SelectedItem = item;
                    break;
                }
            }

            Title = "Редактирование читателя";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FullNameBox.Text) ||
                string.IsNullOrWhiteSpace(BirthDateBox.Text) ||
                string.IsNullOrWhiteSpace(PhoneBox.Text) ||
                string.IsNullOrWhiteSpace(EmailBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordInput.Password) ||
                RoleBox.SelectedItem == null)
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            DateTime birthDate;
            if (!DateTime.TryParseExact(BirthDateBox.Text.Trim(), "dd.MM.yyyy",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out birthDate))
            {
                MessageBox.Show("Введите дату рождения в формате дд.мм.гггг!");
                return;
            }

            if (birthDate.Date > DateTime.Today)
            {
                MessageBox.Show("Дата рождения не может быть в будущем!");
                return;
            }

            if (!EmailBox.Text.Contains("@") || !EmailBox.Text.Contains("."))
            {
                MessageBox.Show("Введите корректную электронную почту!");
                return;
            }

            try
            {
                var selectedRole = (ComboBoxItem)RoleBox.SelectedItem;
                int role = int.Parse(selectedRole.Tag.ToString());

                Читатели reader = editingReader ?? new Читатели();
                reader.ФИО = FullNameBox.Text.Trim();
                reader.ДатаРождения = birthDate.Date;
                reader.НомерТелефона = PhoneBox.Text.Trim();
                reader.ЭлектроннаяПочта = EmailBox.Text.Trim();
                reader.Пароль = PasswordInput.Password;
                reader.Роль = role;

                if (editingReader == null)
                    db.Читатели.Add(reader);

                db.SaveChanges();

                MessageBox.Show(editingReader == null
                    ? "Читатель успешно добавлен!"
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

        private void RoleBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }
}