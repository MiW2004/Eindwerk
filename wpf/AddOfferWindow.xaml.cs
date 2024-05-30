using eindwerkDL;
using eindwerkModels.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace wpf
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
  
        public partial class AddOfferWindow : Window, INotifyPropertyChanged
        {
            private ObservableCollection<OfferProduct> _addedProducts;
            public ObservableCollection<OfferProduct> AddedProducts
            {
                get => _addedProducts;
                set
                {
                    _addedProducts = value;
                    OnPropertyChanged();
                }
            }

            public Offer newOffer { get; set; }
            private GardenCenterDbContext _dbContext;


        public AddOfferWindow()
            {
                InitializeComponent();
                AddedProducts = new ObservableCollection<OfferProduct>();
                newOffer = new Offer { Date = DateTime.Today }; // Set the Date property to today
                DataContext = this; // Set DataContext to the whole window
                _dbContext = new GardenCenterDbContext("Data Source=MW\\sqlexpress;Initial Catalog=Product;Integrated Security=True"); // Initialize _dbContext with appropriate parameters
            }

            private void AddProductButton_Click(object sender, RoutedEventArgs e)
{
    // Validate input fields
    if (!int.TryParse(ProductIdTextBox.Text, out int productId) ||
        !int.TryParse(QuantityTextBox.Text, out int quantity))
    {
        MessageBox.Show("Please enter valid numbers for Product ID and Quantity.",
                        "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
    }

    // Add the product to the list of added products
    AddedProducts.Add(new OfferProduct
    {
        ProductId = productId,
        Quantity = quantity
    });

    // Optionally clear the input fields after adding
    ProductIdTextBox.Clear();
    QuantityTextBox.Clear();
}

        private void AddOfferButton_Click(object sender, RoutedEventArgs e)
        {
            // Perform any necessary validation here
            if (string.IsNullOrWhiteSpace(OfferNumberTextBox.Text) ||
                string.IsNullOrWhiteSpace(CustomerIdTextBox.Text) ||
                AddedProducts.Count == 0)
            {
                MessageBox.Show("Please fill out all fields and add at least one product.",
                                "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Create a new offer with the entered details
            var newOffer = new Offer
            {
                Id = int.Parse(OfferNumberTextBox.Text),
                CustomerId = int.Parse(CustomerIdTextBox.Text),
                Pickup = PickupCheckBox.IsChecked ?? false,
                GardenLayout = GardenLayoutCheckBox.IsChecked ?? false,
                TotalPrice = CalculateTotalPrice(AddedProducts)
            };

            // Optionally, save the offer and its associated products to the database
            _dbContext.AddOffer(newOffer);
            ObservableCollection<OfferProduct> addedProducts = AddedProducts;
            

            // Close the window
            DialogResult = true;
            Close();
        }

        // Function to calculate the total price based on the added products
        private decimal CalculateTotalPrice(ObservableCollection<OfferProduct> products)
        {
            decimal totalPrice = 0;
            foreach (var product in products)
            {
                // Replace this with logic to calculate the price based on the product ID and quantity
                totalPrice += product.Quantity * _dbContext.GetProductPriceFromDatabase(product.ProductId);
            }
            return totalPrice;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}


