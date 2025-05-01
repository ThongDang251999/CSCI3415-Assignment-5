// TinyMart Product Catalog System in C# (Enhanced Version with Extra Credit Features)
// This version includes file I/O, search, operator overloading, and a new sentPoint property.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using prod_id_t = System.Int32;

namespace TinyMart
{
    public struct NameType
    {
        public string FirstName;
        public string LastName;

        public NameType(string first, string last)
        {
            FirstName = first;
            LastName = last;
        }

        public override string ToString() => $"{FirstName} {LastName}";
    }

    public abstract class Product
    {
        private static prod_id_t nextID = 1;
        protected prod_id_t productID;
        protected string productName;
        protected double price;
        protected float reviewRate;
        protected double sentPoint;

        public Product(string aProdName = "!No Name Product!", double aPrice = 1.0)
        {
            productID = nextID++;
            productName = string.IsNullOrEmpty(aProdName) ? "!No Name Product!" : aProdName;
            price = (aPrice > 0.0 && aPrice < 1000.0) ? aPrice : 1.0;
            reviewRate = 0.0f;
            sentPoint = 0.0;
        }

        public prod_id_t ProdID => productID;
        public string ProdName => productName;
        public double Price => price;
        public float ReviewRate => reviewRate;
        public double SentPoint => sentPoint;

        public void SetProdName(string name) => productName = name;
        public void SetPrice(double val) => price = val;
        public void SetReviewRate(float rate) => reviewRate = rate;
        public void SetSentPoint(double point) => sentPoint = point;

        public abstract string GetProdTypeStr();
        public abstract void DisplayContentsInfo();

        public virtual void DisplayProdInfo()
        {
            Console.WriteLine($"[{GetProdTypeStr()}]");
            Console.WriteLine($"Product ID: {productID}   Product Name: {productName}");
            Console.WriteLine($"Price: ${price:F2}    Product Review Rate: {reviewRate}");
            DisplayContentsInfo();
        }

        public override string ToString()
        {
            return $"[{GetProdTypeStr()}] ID: {ProdID} - {ProdName}, Price: ${Price:F2}, Review: {ReviewRate}, SentPoint: {SentPoint}";
        }

        public abstract string ToDataString();
    }

    public class CartOverflowException : Exception
    {
        public CartOverflowException(string msg) : base(msg) { }
    }

    public class CartUnderflowException : Exception
    {
        public CartUnderflowException(string msg) : base(msg) { }
    }

    public enum GenreType { Blues, Classical, Country, Folk, Jazz, Metal, Pop, RnB, Rock }
    public static class GenreHelper { public static string ToStr(this GenreType genre) => genre.ToString(); }

    public enum FilmRateType { NotRated, G, PG, PG_13, R, NC_17 }
    public static class FilmRateHelper { public static string ToStr(this FilmRateType rate) => rate.ToString(); }

    public class AudioProduct : Product
    {
        private NameType singer;
        private GenreType genre;

        public AudioProduct(string name, double price, NameType singer, GenreType genre) : base(name, price)
        {
            this.singer = singer;
            this.genre = genre;
        }

        public override string GetProdTypeStr() => "Music";

        public override void DisplayContentsInfo()
        {
            Console.WriteLine($"Singer Name: {singer}");
            Console.WriteLine($"Genre: {genre.ToStr()}");
        }

        public override string ToDataString()
        {
            return $"Audio,{ProdName},{Price},{singer},{genre.ToStr()},{ReviewRate}";
        }
    }

    public class VideoProduct : Product
    {
        private NameType director;
        private FilmRateType filmRate;
        private int releaseYear;
        private int runTime;

        public VideoProduct(string name, double price, NameType director, int year, int time, FilmRateType rate) : base(name, price)
        {
            this.director = director;
            releaseYear = year;
            runTime = time;
            filmRate = rate;
        }

        public override string GetProdTypeStr() => "Movie";

        public override void DisplayContentsInfo()
        {
            Console.WriteLine($"Director Name: {director}");
            Console.WriteLine($"Film Rating: {filmRate.ToStr()}");
            Console.WriteLine($"Release Year: {releaseYear}");
            Console.WriteLine($"Runtime: {runTime} minutes");
        }

        public override string ToDataString()
        {
            return $"Video,{ProdName},{Price},{director},{releaseYear},{runTime},{filmRate.ToStr()},{ReviewRate}";
        }
    }

    public abstract class BookProduct : Product
    {
        protected NameType author;
        protected int pages;

        protected BookProduct(string name, double price, NameType author, int pages) : base(name, price)
        {
            this.author = author;
            this.pages = pages;
        }

        public override void DisplayContentsInfo()
        {
            Console.WriteLine($"Author: {author}");
            Console.WriteLine($"Pages: {pages}");
        }
    }

    public class EBook : BookProduct
    {
        public EBook(string name, double price, NameType author, int pages) : base(name, price, author, pages) { }

        public override string GetProdTypeStr() => "E book";

        public override string ToDataString()
        {
            return $"Ebook,{ProdName},{Price},{author},{pages},{ReviewRate}";
        }
    }

    public class PaperBook : BookProduct
    {
        public PaperBook(string name, double price, NameType author, int pages) : base(name, price, author, pages) { }

        public override string GetProdTypeStr() => "Paper book";

        public override string ToDataString()
        {
            return $"Paperbook,{ProdName},{Price},{author},{pages},{ReviewRate}";
        }
    }

    public class Cart
    {
        private const int MAX_ITEMS = 7;
        private NameType owner;
        private List<Product> purchasedItems = new();

        public Cart(NameType owner) => this.owner = owner;

        public bool AddItem(Product item)
        {
            try
            {
                if (purchasedItems.Count >= MAX_ITEMS)
                    throw new CartOverflowException($"Cart overflow: {item.ProdName}");
                purchasedItems.Add(item);
                return true;
            }
            catch (CartOverflowException e)
            {
                Console.Error.WriteLine(e.Message);
                return false;
            }
        }

        public bool RemoveItem(prod_id_t id)
        {
            try
            {
                if (!purchasedItems.Any())
                    throw new CartUnderflowException("Cart underflow: the cart is empty");

                var item = purchasedItems.FirstOrDefault(p => p.ProdID == id);
                if (item != null)
                {
                    purchasedItems.Remove(item);
                    return true;
                }
                return false;
            }
            catch (CartUnderflowException e)
            {
                Console.Error.WriteLine(e.Message);
                return false;
            }
        }

        public Product SearchProduct(string prodName) => purchasedItems.FirstOrDefault(p => p.ProdName == prodName);

        public bool SaveCart(Cart theCart, string fileName)
        {
            try
            {
                File.WriteAllLines(fileName, theCart.purchasedItems.Select(p => p.ToDataString()));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ReadFromFile(string fileName)
        {
            try
            {
                var lines = File.ReadAllLines(fileName);
                foreach (var line in lines)
                {
                    var tokens = line.Split(',');
                    string type = tokens[0];
                    string name = tokens[1];
                    double price = double.Parse(tokens[2]);
                    float reviewRate = float.Parse(tokens[^1]);

                    Product product = null;
                    switch (type.ToLower())
                    {
                        case "audio":
                            var singerParts = tokens[3].Split(' ');
                            var genre = Enum.Parse<GenreType>(tokens[4]);
                            product = new AudioProduct(name, price, new NameType(singerParts[0], singerParts[1]), genre);
                            break;
                        case "video":
                            var directorParts = tokens[3].Split(' ');
                            int year = int.Parse(tokens[4]);
                            int time = int.Parse(tokens[5]);
                            var rate = Enum.Parse<FilmRateType>(tokens[6]);
                            product = new VideoProduct(name, price, new NameType(directorParts[0], directorParts[1]), year, time, rate);
                            break;
                        case "ebook":
                            var authorParts = tokens[3].Split(' ');
                            int ePages = int.Parse(tokens[4]);
                            product = new EBook(name, price, new NameType(authorParts[0], authorParts[1]), ePages);
                            break;
                        case "paperbook":
                            var paperAuthorParts = tokens[3].Split(' ');
                            int pPages = int.Parse(tokens[4]);
                            product = new PaperBook(name, price, new NameType(paperAuthorParts[0], paperAuthorParts[1]), pPages);
                            break;
                    }

                    if (product != null)
                    {
                        product.SetReviewRate(reviewRate);
                        AddItem(product);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static Cart operator +(Cart cart, Product item)
        {
            cart.AddItem(item);
            return cart;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("My Cart\n======");
            sb.AppendLine($"Cart Owner: {owner}");
            double total = 0;
            foreach (var item in purchasedItems)
            {
                sb.AppendLine(item.ToString());
                total += item.Price;
            }
            sb.AppendLine("===== Summary of Purchase ======");
            sb.AppendLine($"Total number of purchases: {purchasedItems.Count}");
            sb.AppendLine($"Total purchasing amount: ${total:F2}");
            sb.AppendLine($"Average cost: ${(purchasedItems.Count == 0 ? 0.0 : total / purchasedItems.Count):F2}");
            return sb.ToString();
        }
    }

    class Program
    {
        static void Main()
        {
            var owner = new NameType("John", "Doe");
            var cart = new Cart(owner);

            if (cart.ReadFromFile("cart_input.txt"))
            {
                var toRemove1 = cart.SearchProduct("Rock Anthem");
                var toRemove2 = cart.SearchProduct("SciFi Saga");

                if (toRemove1 != null) cart.RemoveItem(toRemove1.ProdID);
                if (toRemove2 != null) cart.RemoveItem(toRemove2.ProdID);

                Console.WriteLine(cart.ToString());
                cart.SaveCart(cart, "cart_output.txt");
            }
            else
            {
                Console.WriteLine("Failed to load products from file.");
            }
        }
    }
}