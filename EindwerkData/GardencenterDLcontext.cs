using eindwerk;
using eindwerk.model;
using eindwerkModels.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;

namespace eindwerkDL
{
    public class GardenCenterDbContext
    {
        private readonly string _connectionString;

        // Constructor voor GardenCenterDbContext
        // Accepteert een verbindingsreeks en wijst deze toe aan _connectionString
        public GardenCenterDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }



        // Asynchrone methode om producten op te halen

        public async Task<Dictionary<int, decimal>> GetProductsPricesAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT Id, Price FROM Product", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var productprices = new Dictionary<int, decimal>();
                        while (await reader.ReadAsync())
                        {
                            productprices[reader.GetInt32(reader.GetOrdinal("Id"))] = reader.GetDecimal(reader.GetOrdinal("Price"));
                        }
                        return productprices;
                    }
                }
            }
        }




        // Synchronische methode om klanten op te halen
        public List<Customer> GetCustomers()
        {
            // Maak een nieuwe SQL-verbinding met behulp van de verbindingsreeks
            using (var connection = new SqlConnection(_connectionString))
            {
                // Open de verbinding met de database
                connection.Open();

                // Maak een SQL-commando om alle klanten te selecteren
                var command = new SqlCommand("SELECT * FROM Customer", connection);

                // Voer het SQL-commando uit om de resultaten te lezen
                var reader = command.ExecuteReader();

                // Initialiseer een lijst om klanten op te slaan
                var customers = new List<Customer>();

                // Lees elke rij in het resultaat en maak een Customer object
                while (reader.Read())
                {
                    var id = reader.GetInt32(reader.GetOrdinal("id"));
                    var name = reader.IsDBNull(reader.GetOrdinal("Name")) ? null : reader.GetString(reader.GetOrdinal("Name"));
                    var address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address"));
                    var customer = new Customer(id, name, address);
                    customers.Add(customer);
                }

                // Retourneer de lijst van klanten
                return customers;
            }
        }

        // Synchronische methode om producten op te halen
        public List<Product> GetProducts()
        {
            // Maak een nieuwe SQL-verbinding met behulp van de verbindingsreeks
            using (var connection = new SqlConnection(_connectionString))
            {
                // Open de verbinding met de database
                connection.Open();

                // Maak een SQL-commando om alle producten te selecteren
                var command = new SqlCommand("SELECT * FROM Product", connection);

                // Voer het SQL-commando uit om de resultaten te lezen
                var reader = command.ExecuteReader();

                // Initialiseer een lijst om producten op te slaan
                var products = new List<Product>();

                // Lees elke rij in het resultaat en maak een Product object
                while (reader.Read())
                {
                    var Id = reader.GetInt32(reader.GetOrdinal("Id"));
                    var DutchName = reader.IsDBNull(reader.GetOrdinal("DutchName")) ? null : reader.GetString(reader.GetOrdinal("DutchName"));
                    var ScientificName = reader.IsDBNull(reader.GetOrdinal("ScientificName")) ? null : reader.GetString(reader.GetOrdinal("ScientificName"));
                    var Price = reader.GetDecimal(reader.GetOrdinal("Price"));
                    var Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description"));

                    var product = new Product(Id, DutchName, ScientificName, Price, Description);
                    products.Add(product);
                }

                // Sluit de verbinding met de database
                connection.Close();

                // Retourneer de lijst van producten
                return products;
            }
        }
        // Methode om een nieuw product toe te voegen aan de database
        public int InsertProduct(Product product)
        {
            // Maak een nieuwe SQL-verbinding met behulp van de verbindingsreeks
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                // Definieer de SQL-query om een nieuw product in te voegen
                string query = @"
            INSERT INTO Product (Id, DutchName, ScientificName, Price, Description) 
            VALUES (@Id, @DutchName, @ScientificName, @Price, @Description);
            SELECT @Id;
        ";

                // Maak een SQL-commando met de query en de verbinding
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Voeg parameters toe aan het SQL-commando voor elk productattribuut
                    command.Parameters.AddWithValue("@Id", product.Id);
                    command.Parameters.AddWithValue("@DutchName", product.DutchName);
                    command.Parameters.AddWithValue("@ScientificName", product.ScientificName);
                    command.Parameters.AddWithValue("@Price", product.Price);
                    command.Parameters.AddWithValue("@Description", product.Description);

                    // Open de verbinding met de database
                    connection.Open();

                    // Voer het SQL-commando uit om het product in te voegen en haal de ingevoegde ID op
                    int insertedId = (int)command.ExecuteScalar();

                    // Retourneer de ingevoegde ID van het product
                    return insertedId;
                }
            }
        }

        // Methode om producten vanuit een bestand toe te voegen aan de database
        public void UploadProductsFromFile(string filePath)
        {
            // Initialiseer een lijst om producten op te slaan
            var products = new List<Product>();

            // Open het opgegeven bestand voor lezen met behulp van een StreamReader
            using (var reader = new StreamReader(filePath))
            {
                string line;
                // Lees elke regel in het bestand
                while ((line = reader.ReadLine()) != null)
                {
                    // Splits de regel op het '|' teken om de velden te verkrijgen
                    var fields = line.Split('|');

                    // Controleer of het aantal velden in de regel correct is
                    if (fields.Length != 5)
                    {
                        throw new FormatException("Each line must contain exactly 5 fields: Id, DutchName, ScientificName, Price, Description");
                    }

                    // Haal de velden op en maak een nieuw Product object
                    int id = int.Parse(fields[0]);
                    string dutchName = fields[1];
                    string scientificName = fields[2];
                    decimal price = decimal.Parse(fields[3]);
                    string description = fields[4];

                    var product = new Product(id, dutchName, scientificName, price, description);

                    // Voeg het product toe aan de lijst van producten
                    products.Add(product);
                }
            }

            // Loop door elke product in de lijst en voeg deze toe aan de database
            foreach (var product in products)
            {
                InsertProduct(product);
            }
        }




        private async Task BulkinsertCustomers(List<Customer> customers, string connectionString)
        {
            // Maak een nieuwe DataTable voor de aanbiedingen
            var dataTable = new DataTable();
            // Definieer de kolommen van de DataTable
            dataTable.Columns.Add("Id", typeof(int));
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("adress", typeof(string));

            // Vul de DataTable met gegevens van de aanbiedingen
            foreach (var customer in customers)
            {
                dataTable.Rows.Add(customer.Id, customer.Name, customer.Address);
            }

            // Maak een nieuwe SQL-verbinding met behulp van de verbindingsreeks
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                // Open de SQL-verbinding
                await sqlConnection.OpenAsync();
                // Maak een nieuwe SQLBulkCopy om de gegevens naar de database te schrijven
                using (var sqlBulkCopy = new SqlBulkCopy(sqlConnection))
                {
                    // Stel de bestemmingstabel in voor de SQLBulkCopy
                    sqlBulkCopy.DestinationTableName = "Customer";
                    // Schrijf de gegevens van de DataTable naar de database
                    await sqlBulkCopy.WriteToServerAsync(dataTable);
                }
            }
        }

        // Methode om klanten vanuit een bestand toe te voegen aan de database
        public async void UploadCustomersFromFile(string filePath)
        {
            // Initialiseer een lijst om klanten op te slaan
            var customers = new List<Customer>();

            // Open het opgegeven bestand voor lezen met behulp van een StreamReader
            using (var reader = new StreamReader(filePath))
            {
                string line;
                // Lees elke regel in het bestand
                while ((line = reader.ReadLine()) != null)
                {
                    // Splits de regel op het '|' teken om de velden te verkrijgen
                    var fields = line.Split('|');

                    // Controleer of het aantal velden in de regel correct is
                    if (fields.Length != 3)
                    {
                        throw new FormatException("Each line must contain exactly 3 fields: Id, Name, Address");
                    }

                    // Haal de velden op en maak een nieuw Customer object
                    int id = int.Parse(fields[0]);
                    string name = fields[1];
                    string address = fields[2];

                    var customer = new Customer(id, name, address);

                    // Voeg de klant toe aan de lijst van klanten
                    customers.Add(customer);
                }
            }

            // Loop door elke klant in de lijst en voeg deze toe aan de database
            foreach (var customer in customers)
            {
                await BulkinsertCustomers(customers, _connectionString);
            }
        }



        // Asynchrone methode om aanbiedingen vanuit een bestand toe te voegen aan de database
        public async Task UploadOffersFromFile(string filePath)
        {
            // Initialiseer een lijst om aanbiedingen op te slaan
            var offers = new List<Offer>();

            // Open het opgegeven bestand voor lezen met behulp van een FileStream
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 0);
            using var bufferedStream = new BufferedStream(fileStream);
            using var streamReader = new StreamReader(bufferedStream);

            // Lees elke regel in het bestand
            while (await streamReader.ReadLineAsync() is { } fileLine)
            {
                // Splits de regel op het '|' teken om de velden te verkrijgen
                var fields = fileLine.Split('|');

                // Controleer of het aantal velden in de regel correct is
                if (fields.Length != 6)
                {
                    throw new FormatException("Each line must contain exactly 6 fields: Id, Date, CustomerId, Pickup, GardenLayout, TotalPrice");
                }

                // Haal de velden op en maak een nieuw Offer object
                int id = int.Parse(fields[0]);
                DateTime date = DateTime.Parse(fields[1]);
                int customerId = int.Parse(fields[2]);
                bool pickup = bool.Parse(fields[3]);
                bool gardenLayout = bool.Parse(fields[4]);
                int aantalprperoffer = int.Parse(fields[5]);
                var offer = new Offer(id, date, customerId, pickup, gardenLayout, aantalprperoffer);

                // Voeg het aanbod toe aan de lijst van aanbiedingen
                offers.Add(offer);
            }

            // Haal productprijzen op uit de database
            var productPrices = await GetProductsPricesAsync();

            // Haal aanbiedingsproducten op uit de database
            var offerProducts = GetOfferProducts();

            // Bereken de totale prijs voor elke aanbieding parallel
            Parallel.ForEach(offers, offer => offer.TotalPrice = GetTotalPrice(offer, productPrices, offerProducts));

            // Voeg alle aanbiedingen toe aan de database in bulk
            await BulkInsertOffersAsync(offers, _connectionString);
        }


        // Methode om aanbiedingen in bulk in de database in te voegen
        private async Task BulkInsertOffersAsync(List<Offer> offers, string connectionString)
        {
            // Maak een nieuwe DataTable voor de aanbiedingen
            var dataTable = new DataTable();
            // Definieer de kolommen van de DataTable
            dataTable.Columns.Add("Id", typeof(int));
            dataTable.Columns.Add("Date", typeof(DateTime));
            dataTable.Columns.Add("CustomerId", typeof(int));
            dataTable.Columns.Add("Pickup", typeof(bool));
            dataTable.Columns.Add("GardenLayout", typeof(bool));
            dataTable.Columns.Add("AantalPRperOffer", typeof(int));
            dataTable.Columns.Add("TotalPrice", typeof(decimal));

            // Vul de DataTable met gegevens van de aanbiedingen
            foreach (var offer in offers)
            {
                dataTable.Rows.Add(offer.Id, offer.Date, offer.CustomerId, offer.Pickup, offer.GardenLayout, offer.AantalPRperOffer, offer.TotalPrice);
            }

            // Maak een nieuwe SQL-verbinding met behulp van de verbindingsreeks
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                // Open de SQL-verbinding
                await sqlConnection.OpenAsync();
                // Maak een nieuwe SQLBulkCopy om de gegevens naar de database te schrijven
                using (var sqlBulkCopy = new SqlBulkCopy(sqlConnection))
                {
                    // Stel de bestemmingstabel in voor de SQLBulkCopy
                    sqlBulkCopy.DestinationTableName = "Offer";
                    // Schrijf de gegevens van de DataTable naar de database
                    await sqlBulkCopy.WriteToServerAsync(dataTable);
                }
            }
        }

        // Methode om de totale prijs van een aanbieding te berekenen
        private decimal GetTotalPrice(Offer offer, Dictionary<int, decimal> productPrices, Dictionary<int, List<OfferProduct>> offerProductsDictionary)
        {
            decimal totalPrice = 0;

            // Controleer of de aanbieding producten heeft
            if (!offerProductsDictionary.ContainsKey(offer.Id))
                return totalPrice;

            var offerProducts = offerProductsDictionary[offer.Id];

            // Bereken de totale prijs op basis van de productprijzen en de hoeveelheden in de aanbieding
            foreach (var offerProduct in offerProducts)
            {
                totalPrice += productPrices[offerProduct.ProductId] * offerProduct.Quantity;
            }

            // Pas eventuele kortingen toe op basis van de tuinlayout en ophaalopties
            // Voeg 15% extra toe als de tuinlayout is geselecteerd en de totale prijs onder of gelijk aan 2000 is
            if (offer.GardenLayout && totalPrice <= 2000)
                totalPrice *= 1.15m;

            // Voeg 10% extra toe als de tuinlayout is geselecteerd en de totale prijs tussen 2000 en 5000 ligt
            if (offer.GardenLayout && totalPrice > 2000 && totalPrice < 5000)
                totalPrice *= 1.10m;

            // Voeg 5% extra toe als de tuinlayout is geselecteerd en de totale prijs 5000 of meer is
            if (offer.GardenLayout && totalPrice >= 5000)
                totalPrice *= 1.05m;

            // Trek 5% korting af als de totale prijs tussen 2000 en 5000 ligt
            if (totalPrice > 2000 && totalPrice <= 5000)
                totalPrice *= 0.95m;

            // Trek 10% korting af als de totale prijs 5000 of meer is
            if (totalPrice > 5000)
                totalPrice *= 0.90m;

            // Voeg toeslag toe op basis van ophaaloptie
            if (!offer.Pickup)
            {
                // Voeg 100 toe als de totale prijs onder 500 ligt
                if (totalPrice < 500)
                    totalPrice += 100;

                // Voeg 50 toe als de totale prijs tussen 500 en 1000 ligt
                if (totalPrice >= 500 && totalPrice < 1000)
                    totalPrice += 50;
            }



            return totalPrice;
        }


        // Methode om een product op te halen op basis van ID
        public Product GetProductById(int id)
        {
            // Maak een nieuwe SQL-verbinding met behulp van de verbindingsreeks
            using (var connection = new SqlConnection(_connectionString))
            {
                // Open de verbinding met de database
                connection.Open();

                // Maak een SQL-commando om een product op te halen op basis van het opgegeven ID
                var command = new SqlCommand("SELECT * FROM Product WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);

                // Voer het SQL-commando uit en lees de resultaten
                var reader = command.ExecuteReader();
                var product = new Product();
                while (reader.Read())
                {
                    // Haal de attributen van het product op uit de database
                    var Id = reader.GetInt32(reader.GetOrdinal("Id"));
                    var DutchName = reader.IsDBNull(reader.GetOrdinal("DutchName")) ? null : reader.GetString(reader.GetOrdinal("DutchName"));
                    var ScientificName = reader.IsDBNull(reader.GetOrdinal("ScientificName")) ? null : reader.GetString(reader.GetOrdinal("ScientificName"));
                    var Price = reader.GetDecimal(reader.GetOrdinal("Price"));
                    var Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description"));

                    // Maak een nieuw Product object met de opgehaalde attributen
                    product = new Product(Id, DutchName, ScientificName, Price, Description);
                }

                // Sluit de verbinding met de database
                connection.Close();

                // Retourneer het opgehaalde product
                return product;
            }
        }

        // Methode om alle aanbiedingen op te halen uit de database
        public List<Offer> GetOffers()
        {
            // Maak een nieuwe SQL-verbinding met behulp van de verbindingsreeks
            using (var connection = new SqlConnection(_connectionString))
            {
                // Open de verbinding met de database
                connection.Open();

                // Maak een SQL-commando om alle aanbiedingen op te halen
                var command = new SqlCommand("SELECT * FROM Offer", connection);

                // Voer het SQL-commando uit en lees de resultaten
                var reader = command.ExecuteReader();
                var offers = new List<Offer>();
                while (reader.Read())
                {
                    // Haal de attributen van de aanbieding op uit de database
                    var Id = reader.GetInt32(reader.GetOrdinal("Id"));
                    var Date = reader.GetDateTime(reader.GetOrdinal("Date"));
                    var CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId"));
                    var Pickup = reader.GetBoolean(reader.GetOrdinal("Pickup"));
                    var GardenLayout = reader.GetBoolean(reader.GetOrdinal("GardenLayout"));
                    var aantalPrPerOffer = reader.GetInt32(reader.GetOrdinal("AantalPRperOffer"));
                    var TotalPrice = reader.GetDecimal(reader.GetOrdinal("TotalPrice"));

                    // Maak een nieuw Offer object met de opgehaalde attributen
                    var offer = new Offer(Id, Date, CustomerId, Pickup, GardenLayout, aantalPrPerOffer);
                    offer.TotalPrice = TotalPrice;
                    offers.Add(offer);
                }

                // Sluit de verbinding met de database
                connection.Close();

                // Retourneer alle opgehaalde aanbiedingen
                return offers;
            }
        }

        // Methode om een aanbieding op te halen op basis van ID
        public Offer GetOffer(int id)
        {
            // Maak een nieuwe SQL-verbinding met behulp van de verbindingsreeks
            using (var connection = new SqlConnection(_connectionString))
            {
                // Open de verbinding met de database
                connection.Open();

                // Maak een SQL-commando om een aanbieding op te halen op basis van het opgegeven ID
                var command = new SqlCommand("SELECT * FROM Offer where Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);

                // Voer het SQL-commando uit en lees de resultaten
                var reader = command.ExecuteReader();
                var offer = new Offer();
                while (reader.Read())
                {
                    // Haal de attributen van de aanbieding op uit de database
                    var Id = reader.GetInt32(reader.GetOrdinal("Id"));
                    var Date = reader.GetDateTime(reader.GetOrdinal("Date"));
                    var CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId"));
                    var Pickup = reader.GetBoolean(reader.GetOrdinal("Pickup"));
                    var GardenLayout = reader.GetBoolean(reader.GetOrdinal("GardenLayout"));
                    var aantalPrPerOffer = reader.GetInt32(reader.GetOrdinal("AantalPRperOffer"));
                    var TotalPrice = reader.GetDecimal(reader.GetOrdinal("TotalPrice"));


                    // Maak een nieuw Offer object met de opgehaalde attributen
                    offer = new Offer(Id, Date, CustomerId, Pickup, GardenLayout, aantalPrPerOffer);
                    offer.TotalPrice = TotalPrice;
                }

                // Sluit de verbinding met de database
                connection.Close();

                // Retourneer de opgehaalde aanbieding
                return offer;
            }
        }


        // Methode om aanbiedingsproducten vanuit een bestand te uploaden naar de database
        public async Task UploadOfferProductsFromFile(string filePath)
        {
            var offerProducts = new List<OfferProduct>();

            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 0);
            using var bufferedStream = new BufferedStream(fileStream);
            using var streamReader = new StreamReader(bufferedStream);
            int batch = 100;

            while (await streamReader.ReadLineAsync() is { } line)
            {
                try
                {
                    var fields = line.Split('|');

                    if (fields.Length != 3)
                    {
                        // Log the error and skip this line
                        Console.WriteLine($"Skipping line due to incorrect format: {line}");
                        continue;
                    }

                    int offerId = int.Parse(fields[0]);
                    int productId = int.Parse(fields[1]);
                    int quantity = int.Parse(fields[2]);

                    var offerProduct = new OfferProduct(offerId, productId, quantity);
                    offerProducts.Add(offerProduct);

                    if (offerProducts.Count >= batch)
                    {
                        await BulkInsertOfferProductsAsync(offerProducts, _connectionString);
                        offerProducts.Clear();
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception and continue with the next line
                    Console.WriteLine($"Error processing line: {line}. Exception: {ex.Message}");
                }
            }

            // Insert the final batch if any
            if (offerProducts.Any())
            {
                await BulkInsertOfferProductsAsync(offerProducts, _connectionString);
            }

        }
        private async Task BulkInsertOfferProductsAsync(List<OfferProduct> offerProducts, string connectionString)
        {
            // Maak een nieuwe DataTable voor de aanbiedingen
            var dataTable = new DataTable();
            // Definieer de kolommen van de DataTable
            dataTable.Columns.Add("OfferId", typeof(int));
            dataTable.Columns.Add("ProductId", typeof(int));
            dataTable.Columns.Add("Quantity", typeof(int));

            // Vul de DataTable met gegevens van de aanbiedingen
            foreach (var offerProduct in offerProducts)
            {
                dataTable.Rows.Add(offerProduct.OfferId, offerProduct.ProductId, offerProduct.Quantity);
            }

            // Maak een nieuwe SQL-verbinding met behulp van de verbindingsreeks
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                // Open de SQL-verbinding
                await sqlConnection.OpenAsync();
                // Maak een nieuwe SQLBulkCopy om de gegevens naar de database te schrijven
                using (var sqlBulkCopy = new SqlBulkCopy(sqlConnection))
                {
                    // Stel de bestemmingstabel in voor de SQLBulkCopy
                    sqlBulkCopy.DestinationTableName = "OfferProduct";
                    // Schrijf de gegevens van de DataTable naar de database
                    await sqlBulkCopy.WriteToServerAsync(dataTable);
                }
            }
        }


        // Methode om aanbiedingsproducten op te halen op basis van aanbiedings-ID
        public List<OfferProduct> GetOfferProductsByOfferId(int offerId)
        {
            // Maak een nieuwe lijst voor aanbiedingsproducten
            var offerProducts = new List<OfferProduct>();

            // Maak een SQL-verbinding met behulp van de verbindingsreeks
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                // Definieer de SQL-query om aanbiedingsproducten op te halen op basis van aanbiedings-ID
                string query = "SELECT * FROM OfferProduct WHERE OfferId = @OfferId";

                // Maak een nieuw SQL-commando met de query en de verbinding
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Voeg de parameter toe aan het SQL-commando
                    command.Parameters.AddWithValue("@OfferId", offerId);

                    // Open de verbinding met de database
                    connection.Open();

                    // Voer het SQL-commando uit en lees de resultaten met behulp van een SQLDataReader
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Lees elk rijresultaat en voeg het toe aan de lijst van aanbiedingsproducten
                        while (reader.Read())
                        {
                            var productId = reader.GetInt32(reader.GetOrdinal("ProductId"));
                            var quantity = reader.GetInt32(reader.GetOrdinal("Quantity"));

                            offerProducts.Add(new OfferProduct(offerId, productId, quantity));
                        }
                    }
                }
            }
            // Retourneer de lijst van aanbiedingsproducten
            return offerProducts;
        }

        // Methode om alle aanbiedingsproducten op te halen
        public Dictionary<int, List<OfferProduct>> GetOfferProducts()
        {
            // Maak een nieuwe dictionary voor aanbiedingsproducten
            var offerProducts = new Dictionary<int, List<OfferProduct>>();

            // Maak een SQL-verbinding met behulp van de verbindingsreeks
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                // Definieer de SQL-query om alle aanbiedingsproducten op te halen
                string query = "SELECT * FROM OfferProduct";

                // Maak een nieuw SQL-commando met de query en de verbinding
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Open de verbinding met de database
                    connection.Open();

                    // Voer het SQL-commando uit en lees de resultaten met behulp van een SQLDataReader
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Lees elk rijresultaat en voeg het toe aan de dictionary van aanbiedingsproducten
                        while (reader.Read())
                        {
                            var offerId = reader.GetInt32(reader.GetOrdinal("OfferId"));
                            var productId = reader.GetInt32(reader.GetOrdinal("ProductId"));
                            var quantity = reader.GetInt32(reader.GetOrdinal("Quantity"));

                            if (!offerProducts.ContainsKey(offerId))
                            {
                                offerProducts[offerId] = new List<OfferProduct>();
                            }
                            offerProducts[offerId].Add(new OfferProduct(offerId, productId, quantity));
                        }
                    }
                }
            }
            // Retourneer de dictionary van aanbiedingsproducten
            return offerProducts;
        }






        // Methode om een aanbieding in de database bij te werken
        public void UpdateOfferInDatabase(Offer offer)
        {
            // Maak een nieuwe SQL-verbinding met behulp van de verbindingsreeks
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                // Definieer de SQL-query om een aanbieding bij te werken
                string query = @"
            UPDATE Offer 
            SET Date = @Date, CustomerId = @CustomerId, Pickup = @Pickup, GardenLayout = @GardenLayout, AantalPRperOffer = @AantalPRperOffer, TotalPrice = @TotalPrice
            WHERE Id = @Id;
        ";

                // Maak een nieuw SQL-commando met de query en de verbinding
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Voeg parameters toe aan het SQL-commando voor de aanbieding
                    command.Parameters.AddWithValue("@Date", offer.Date);
                    command.Parameters.AddWithValue("@CustomerId", offer.CustomerId);
                    command.Parameters.AddWithValue("@Pickup", offer.Pickup);
                    command.Parameters.AddWithValue("@GardenLayout", offer.GardenLayout);
                    command.Parameters.AddWithValue("@TotalPrice", offer.TotalPrice);
                    command.Parameters.AddWithValue("@Id", offer.Id);
                    command.Parameters.AddWithValue("@AantalPRperOffer", offer.AantalPRperOffer);

                    // Open de verbinding met de database
                    connection.Open();
                    // Voer het SQL-commando uit om de aanbieding bij te werken
                    command.ExecuteNonQuery();
                }
            }
        }

        // Methode om een klant toe te voegen aan de database
        public void AddCustomer(Customer customer)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                // SQL-query om een klant toe te voegen
                string query = @"
            INSERT INTO Customer (Id, Name, Address)
            VALUES  (@Id, @Name, @Address);
        ";

                // Maak een nieuw SQL-commando met de query en de verbinding
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Voeg parameters toe aan het SQL-commando voor de klantgegevens
                    command.Parameters.AddWithValue("@Id", customer.Id);
                    command.Parameters.AddWithValue("@Name", customer.Name);
                    command.Parameters.AddWithValue("@Address", customer.Address);

                    // Open de verbinding met de database
                    connection.Open();

                    // Voer het SQL-commando uit om de klant toe te voegen
                    command.ExecuteNonQuery();
                }
            }
        }

        public void AddOffer(Offer offer)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Insert Offer
                        string insertOfferQuery = @"
            INSERT INTO Offer (Id, Date, CustomerId, Pickup, GardenLayout, TotalPrice)
            VALUES (@Id, @Date, @CustomerId, @Pickup, @GardenLayout, @TotalPrice);
            ";
                        using (SqlCommand command = new SqlCommand(insertOfferQuery, connection, transaction))
                        {
                            offer.Date = DateTime.Today; // Set the date to today
                            command.Parameters.AddWithValue("@Id", offer.Id);
                            command.Parameters.AddWithValue("@Date", offer.Date);
                            command.Parameters.AddWithValue("@CustomerId", offer.CustomerId);
                            command.Parameters.AddWithValue("@Pickup", offer.Pickup);
                            command.Parameters.AddWithValue("@GardenLayout", offer.GardenLayout);
                            command.Parameters.AddWithValue("@TotalPrice", offer.TotalPrice);

                            command.ExecuteNonQuery();
                        }

                        // Insert OfferProducts


                        // Commit transaction
                        transaction.Commit();
                    }
                    catch
                    {
                        // Rollback transaction if something went wrong
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        public void AddofferProduct(OfferProduct offerProducts)
        {



            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {

                        string insertOfferProductQuery = @"
                    INSERT INTO OfferProduct (OfferId, ProductId, Quantity)
                    VALUES (@OfferId, @ProductId, @Quantity);
                    ";


                        using (SqlCommand command = new SqlCommand(insertOfferProductQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@OfferId", offerProducts.OfferId);
                            command.Parameters.AddWithValue("@ProductId", offerProducts.ProductId);
                            command.Parameters.AddWithValue("@Quantity", offerProducts.Quantity);

                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }



                    catch
                    {
                        // Rollback transaction if something went wrong
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }




        // Methode om klanten op te halen aan de hand van een klant-ID
        public List<Customer> GetCustomersById(int customerId)
        {

            var customers = new List<Customer>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                // SQL-query om klanten op te halen aan de hand van een klant-ID
                string query = @"
            SELECT c.Id, c.Name, c.Address, 
                   o.Id AS OfferId, o.Date, o.CustomerId, o.Pickup, o.GardenLayout, o.TotalPrice
            FROM Customer c
            LEFT JOIN Offer o ON c.Id = o.CustomerId
            WHERE c.Id = @CustomerId";

                // Maak een nieuw SQL-commando met de query en de verbinding
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Voeg parameters toe aan het SQL-commando voor het klant-ID
                    command.Parameters.AddWithValue("@CustomerId", customerId);

                    // Open de verbinding met de database
                    connection.Open();

                    // Voer het SQL-commando uit en lees de resultaten
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        Customer customer = null;

                        while (reader.Read())
                        {
                            if (customer == null)
                            {
                                // Maak een nieuwe klant aan als deze nog niet bestaat
                                customer = new Customer
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Address = reader.GetString(reader.GetOrdinal("Address")),
                                    Offers = new List<Offer>()
                                };
                            }

                            if (!reader.IsDBNull(reader.GetOrdinal("OfferId")))
                            {
                                // Voeg aanbiedingen toe aan de klant
                                var offer = new Offer
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("OfferId")),
                                    Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                                    CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                    Pickup = reader.GetBoolean(reader.GetOrdinal("Pickup")),
                                    GardenLayout = reader.GetBoolean(reader.GetOrdinal("GardenLayout")),
                                    TotalPrice = reader.GetDecimal(reader.GetOrdinal("TotalPrice"))
                                };

                                customer.Offers.Add(offer);
                            }
                        }

                        if (customer != null)
                        {
                            customers.Add(customer);
                        }
                    }
                }
            }

            return customers;
        }

        // Methode om klanten op te halen aan de hand van een naam (deels overeenkomend)
        public List<Customer> GetCustomersByName(string name)
        {
            var customers = new List<Customer>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                // SQL-query om klanten op te halen aan de hand van een naam
                string query = @"
            SELECT c.Id, c.Name, c.Address, 
                   o.Id AS OfferId, o.Date, o.CustomerId, o.Pickup, o.GardenLayout, o.TotalPrice
            FROM Customer c
            LEFT JOIN Offer o ON c.Id = o.CustomerId
            WHERE c.Name LIKE @Name";

                // Maak een nieuw SQL-commando met de query en de verbinding
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Voeg parameters toe aan het SQL-commando voor de naam (met wildcards)
                    command.Parameters.AddWithValue("@Name", "%" + name + "%");

                    // Open de verbinding met de database
                    connection.Open();

                    // Voer het SQL-commando uit en lees de resultaten
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        Dictionary<int, Customer> customerDict = new Dictionary<int, Customer>();

                        while (reader.Read())
                        {
                            int customerId = reader.GetInt32(reader.GetOrdinal("Id"));

                            if (!customerDict.TryGetValue(customerId, out var customer))
                            {
                                // Maak een nieuwe klant aan als deze nog niet bestaat
                                customer = new Customer
                                {
                                    Id = customerId,
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Address = reader.GetString(reader.GetOrdinal("Address")),
                                    Offers = new List<Offer>()
                                };
                                customerDict[customerId] = customer;
                            }

                            if (!reader.IsDBNull(reader.GetOrdinal("OfferId")))
                            {
                                // Voeg aanbiedingen toe aan de klant
                                var offer = new Offer
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("OfferId")),
                                    Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                                    CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                    Pickup = reader.GetBoolean(reader.GetOrdinal("Pickup")),
                                    GardenLayout = reader.GetBoolean(reader.GetOrdinal("GardenLayout")),
                                    TotalPrice = reader.GetDecimal(reader.GetOrdinal("TotalPrice"))
                                };

                                customer.Offers.Add(offer);
                            }
                        }

                        // Voeg alle klanten uit de dictionary toe aan de lijst van klanten
                        customers = customerDict.Values.ToList();
                    }
                }
            }

            return customers;
        }
        public decimal GetProductPriceFromDatabase(int productId)
        {
            // Assuming you have a method in your data access layer (_dbContext) to fetch the product price by its ID
            // Here's a hypothetical example assuming you have a Product table with a Price column in your database:

            decimal productPrice = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT Price FROM Product WHERE Id = @ProductId";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductId", productId);

                    try
                    {
                        connection.Open();
                        object result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            productPrice = Convert.ToDecimal(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle the exception appropriately (e.g., logging, displaying error to user)
                        Console.WriteLine("Error fetching product price: " + ex.Message);
                    }
                }
            }

            return productPrice;
        }

        public void AddOfferProducts(ObservableCollection<OfferProduct> addedProducts, int offerId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (var product in addedProducts)
                        {
                            string insertOfferProductQuery = @"
                        INSERT INTO OfferProduct (OfferId, ProductId, Quantity)
                        VALUES (@OfferId, @ProductId, @Quantity);
                    ";

                            using (SqlCommand command = new SqlCommand(insertOfferProductQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@OfferId", offerId);
                                command.Parameters.AddWithValue("@ProductId", product.ProductId);
                                command.Parameters.AddWithValue("@Quantity", product.Quantity);

                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}

      





