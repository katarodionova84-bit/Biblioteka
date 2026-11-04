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
using System.Windows.Shapes;

namespace Biblioteka
{
    public partial class IssueBookWindow : Window
    {
        private readonly LibraryEntities db;

        public IssueBookWindow(LibraryEntities context)
        {
            InitializeComponent();

            db = context;
            var availableBooks = db.Книги.ToList().Where(book => !db.ВыданныеКниги.Any(borrow => borrow.КодКниги == book.КодКниги && borrow.ФактическаяДатаВозврата == null)).ToList();
            BookBox.ItemsSource = availableBooks;
            ReaderBox.ItemsSource = db.Читатели.ToList();
            BorrowDatePicker.SelectedDate = DateTime.Today;
            ReturnDatePicker.SelectedDate = DateTime.Today.AddDays(14);
        }
        private void Issue_Click(object sender, RoutedEventArgs e)
        {
            if (BookBox.SelectedValue == null ||
                ReaderBox.SelectedValue == null ||
                BorrowDatePicker.SelectedDate == null ||
                ReturnDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }
            DateTime borrowDate = BorrowDatePicker.SelectedDate.Value.Date;
            DateTime returnDate = ReturnDatePicker.SelectedDate.Value.Date;
            if (returnDate <= borrowDate)
            {
                MessageBox.Show("Дата возврата должна быть позже даты выдачи!");
                return;
            }
            if (borrowDate > DateTime.Today)
            {
                MessageBox.Show("Дата выдачи не может быть в будущем!");
                return;
            }

            try
            {
                int bookId = (int)BookBox.SelectedValue;
                bool alreadyBorrowed = db.ВыданныеКниги.Any( b => b.КодКниги == bookId && b.ФактическаяДатаВозврата == null);
                if (alreadyBorrowed)
                {
                    MessageBox.Show("Эта книга уже выдана!");
                    return;
                }

                var borrowedBook = new ВыданныеКниги
                {
                    КодКниги = bookId,
                    КодЧитателя = (int)ReaderBox.SelectedValue,
                    ДатаВыдачи = borrowDate,
                    ДатаВозврата = returnDate,
                    ФактическаяДатаВозврата = null
                };

                db.ВыданныеКниги.Add(borrowedBook);
                db.SaveChanges();

                MessageBox.Show("Книга успешно выдана!");

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка выдачи книги: " + ex.Message);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}