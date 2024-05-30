using eindwerk.model;
using eindwerkModels.Model;
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

namespace wpf
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>

    public partial class AddCustomerWindow : Window
    {
        public Customer NewCustomer { get; set; }

        public AddCustomerWindow()
        {
            InitializeComponent();
            NewCustomer = new Customer();
            DataContext = NewCustomer;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Perform any necessary validation here
            if
               (

        string.IsNullOrWhiteSpace(NewCustomer.Name) ||
        string.IsNullOrWhiteSpace(NewCustomer.Address) ||
        NewCustomer.Id <= 999)
       
            {
                MessageBox.Show("Please fill out all fields and ensure that the Id is valid and that at least one offer is selected.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
            Close();
        }


    }
}

    