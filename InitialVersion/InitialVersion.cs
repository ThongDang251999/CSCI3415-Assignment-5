// TinyMart Product Catalog System in C#
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// Type alias
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

        public Product(string aProdName = "!No Name Product!", double aPrice = 1.0)
        {
            productID = nextID++;
            productName = string.IsNullOrEmpty(aProdName) ? "!No Name Product!" : aProdName;
            price = (aPrice > 0.0 && aPrice < 1000.0) ? aPrice : 1.0;
            reviewRate = 0.0f;
        }

        public prod_id_t ProdID => productID;
        public string ProdName => productName;
        public double Price => price;
        public float ReviewRate => reviewRate;

        public void SetProdName(string name) => productName = name;
        public void SetPrice(double val) => price = val;
        public void SetReviewRate(float rate) => reviewRate = rate;

        public abstract string GetProdTypeStr();
        public abstract void DisplayContentsInfo();
        public virtual void DisplayProdInfo()
        {
            Console.WriteLine($"[{GetProdTypeStr()}]");
            Console.WriteLine($"Product ID: {productID}   Product Name: {productName}");
            Console.WriteLine($"Price: ${price:F2}    Product Review Rate: {reviewRate}");
            DisplayContentsInfo();
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
    public static class GenreHelper
    {
        public static string ToStr(this GenreType genre) => genre.ToString();
    }

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

    public enum FilmRateType { NotRated, G, PG, PG_13, R, NC_17 }
    public static class FilmRateHelper
    {
        public static string ToStr(this FilmRateType rate) => rate.ToString();
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

        public Cart(NameType owner)
        {
            this.owner = owner;
        }

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

        public Product SearchProduct(string name) => purchasedItems.FirstOrDefault(p => p.ProdName == name);

        public void DisplayCart()
        {
            Console.WriteLine("\nMy Cart\n======");
            Console.WriteLine($"Cart Owner: {owner}");
            double total = 0;
            foreach (var item in purchasedItems)
            {
                item.DisplayProdInfo();
                total += item.Price;
                Console.WriteLine();
            }

            Console.WriteLine("===== Summary of Purchase ======");
            Console.WriteLine($"Total number of purchases: {purchasedItems.Count}");
            Console.WriteLine($"Total purchasing amount: ${total:F2}");
            Console.WriteLine($"Average cost: ${(purchasedItems.Count == 0 ? 0.0 : total / purchasedItems.Count):F2}");
        }

        public bool SaveCart(string fileName)
        {
            try
            {
                File.WriteAllLines(fileName, purchasedItems.Select(p => p.ToDataString()));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    class Program
    {
        static void Main()
        {
            var owner = new NameType("John", "Doe");
            var myCart = new Cart(owner);

            var song1 = new AudioProduct("Jazz Vibes", 12.99, new NameType("Ella", "Fitzgerald"), GenreType.Jazz);
            var song2 = new AudioProduct("Rock Anthem", 14.50, new NameType("Jim", "Morrison"), GenreType.Rock);
            var song3 = new AudioProduct("Metal Storm", 11.00, new NameType("James", "Hetfield"), GenreType.Metal);
            var movie1 = new VideoProduct("SciFi Saga", 19.99, new NameType("George", "Lucas"), 1977, 120, FilmRateType.PG);
            var movie2 = new VideoProduct("Animated Joy", 15.75, new NameType("Hayao", "Miyazaki"), 2001, 125, FilmRateType.G);
            var ebook1 = new EBook("Digital Fortress", 9.99, new NameType("Dan", "Brown"), 350);
            var paper1 = new PaperBook("The Hobbit", 17.25, new NameType("J.R.R.", "Tolkien"), 310);
            var paper2 = new PaperBook("War and Peace", 25.99, new NameType("Leo", "Tolstoy"), 1225);

            myCart.AddItem(song1);
            myCart.AddItem(song2);
            myCart.AddItem(song3);
            myCart.AddItem(movie1);
            myCart.AddItem(movie2);
            myCart.AddItem(ebook1);
            myCart.AddItem(paper1);
            myCart.AddItem(paper2); // overflow

            myCart.RemoveItem(song2.ProdID);
            myCart.RemoveItem(movie1.ProdID);

            myCart.DisplayCart();

            if (myCart.SaveCart("cart_output.txt"))
                Console.WriteLine("\nCart saved to cart_output.txt");
            else
                Console.WriteLine("\nFailed to save cart.");
        }
    }
}
