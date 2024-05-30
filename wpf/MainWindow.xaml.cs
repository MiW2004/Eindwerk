using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using eindwerk.model;
using eindwerkDL; // Change this to the correct namespace for GardenCenterDbContext
using eindwerkModels.Model;
using Microsoft.Win32;
using wpf;

namespace WpfApp
{

    public partial class MainWindow : Window
    {
        private GardenCenterDbContext _dbContext;
        private OfferCalculator _offercalculator;
        private List<Customer> _customers;
        private List<Product> _products;
        private List<Offer> _offers;
        private List<OfferProduct> _offerproducts;

        public MainWindow()
        {
            InitializeComponent();
            _dbContext = new GardenCenterDbContext("Data Source=MW\\sqlexpress;Initial Catalog=Product;Integrated Security=True");
            _offercalculator = new OfferCalculator();
            _customers = new List<Customer>();
            _products = new List<Product>();
            _offers = new List<Offer>();
            _offerproducts = new List<OfferProduct>();
        }

        private void GetCustomers_Click(object sender, RoutedEventArgs e)
        {
            var customers = _dbContext.GetCustomers();
            ResultsListBox.Items.Clear();
            foreach (var customer in customers)
            {
                ResultsListBox.Items.Add($"ID: {customer.Id}, Name: {customer.Name}, Address: {customer.Address}");
            }
        }

        private void GetProducts_Click(object sender, RoutedEventArgs e)
        {
            var products = _dbContext.GetProducts();
            ResultsListBox.Items.Clear();
            foreach (var product in products)
            {
                ResultsListBox.Items.Add($"ID: {product.Id}, Dutch Name: {product.DutchName}, Scientific Name: {product.ScientificName}, Price: {product.Price}, Description: {product.Description}");
            }
        }

        private void GetOffers_Click(object sender, RoutedEventArgs e)
        {
            var offers = _dbContext.GetOffers();
            ResultsListBox.Items.Clear();

            foreach (var offer in offers)
            {
                ResultsListBox.Items.Add($"ID: {offer.Id}, Date: {offer.Date}, Customer ID: {offer.CustomerId}, Pickup: {offer.Pickup}, Garden Layout: {offer.GardenLayout}, AantalPRperOffer: {offer.AantalPRperOffer} , Total Price: {offer.TotalPrice}");
            }
        }

        private void UploadProducts_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt";
            if (openFileDialog.ShowDialog() == true)
            {
                _dbContext.UploadProductsFromFile(openFileDialog.FileName);
                MessageBox.Show("Products uploaded successfully.");
            }
        }
        private void UploadCustomers_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt";
            if (openFileDialog.ShowDialog() == true)
            {
                _dbContext.UploadCustomersFromFile(openFileDialog.FileName);
                MessageBox.Show("Customers uploaded successfully.");
            }
        }
        private async void UploadOffers_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt";
            if (openFileDialog.ShowDialog() == true)
            {
                await _dbContext.UploadOffersFromFile(openFileDialog.FileName);
                MessageBox.Show("Offers uploaded successfully.");
            }
        }
        private async void UploadOfferProducts_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt";
            if (openFileDialog.ShowDialog() == true)
            {
                await _dbContext.UploadOfferProductsFromFile(openFileDialog.FileName);
                MessageBox.Show("Offer products uploaded successfully.");
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            ShowInputDialog();
        }

        private void ShowInputDialog()
        {
            if (ResultsListBox.SelectedItem is null)
            {
                return;
            }

            var selectedId = ((string)ResultsListBox.SelectedItem).Split(',')[0].Split(':')[1].Trim();
            var offer = _dbContext.GetOffer(int.Parse(selectedId));

            InputDialog inputDialog = new InputDialog(offer);
            if (inputDialog.ShowDialog() == true)
            {
                Offer result = inputDialog.EditedOffer;
                _dbContext.UpdateOfferInDatabase(result);

                // Optional: Update the offer in the UI
                // You can refresh the ResultsListBox or update the specific item if needed.
            }

        }
        private void SearchCustomer_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = SearchTextBox.Text.Trim();
            if (string.IsNullOrEmpty(searchTerm))
            {
                MessageBox.Show("Please enter a name or customer number to search.");
                return;
            }

            List<Customer> customers;
            if (int.TryParse(searchTerm, out int customerId))
            {
                // Search by customer number
                customers = _dbContext.GetCustomersById(customerId);
            }
            else
            {
                // Search by name
                customers = _dbContext.GetCustomersByName(searchTerm);
            }

            DisplayCustomers(customers);
        }

        private void DisplayCustomers(List<Customer> customers)
        {
            ResultsListBox.Items.Clear();

            foreach (var customer in customers)
            {
                var customerInfo = $"Customer Number: {customer.Id}, Name: {customer.Name}, Address: {customer.Address}, Number of Offers: {customer.Offers.Count}";
                ResultsListBox.Items.Add(customerInfo);

                foreach (var offer in customer.Offers)
                {
                    var offerInfo = $"  Offer Number: {offer.Id}, Price: {offer.TotalPrice:C}";
                    ResultsListBox.Items.Add(offerInfo);
                }
            }
        }

        private void AddCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            AddCustomerWindow addCustomerWindow = new AddCustomerWindow();
            if (addCustomerWindow.ShowDialog() == true)
            {

                _customers.Add(addCustomerWindow.NewCustomer);
                _dbContext.AddCustomer(addCustomerWindow.NewCustomer);



                // Optional: Update the offer in the UI
                // You can refresh the ResultsListBox or update the specific item if needed.
            }

        }
        private void AddOfferButton_Click(object sender, RoutedEventArgs e)
        {
            var addOfferWindow = new AddOfferWindow();
            if (addOfferWindow.ShowDialog() == true)
            {
                var newOffer = addOfferWindow.newOffer;
               

                if (newOffer != null)
                {

                    
                    _dbContext.AddOffer(newOffer);
                    


                }
                else
                {
                    MessageBox.Show("Failed to add offer. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}








