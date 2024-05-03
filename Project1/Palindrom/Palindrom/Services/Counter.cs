using Palindrom.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Caching;


namespace Palindrom.Services
{
    class Counter

    {
        static List<string> palindromes = new List<string>();
        static List<string> words = new List<string>();
        static Tuple<List<string>, DateTime> tuple = new Tuple<List<string>, DateTime>(words, DateTime.Now);

        static MemoryCache cache = new MemoryCache("PrincepsCash");

        static int count;
        static Stopwatch stopwatch = new Stopwatch();

        public static void GetNumberOfPalindromes(HttpListener listener)
        {
            Console.WriteLine("Server startovan");
            DateTime lastModified;//zadnje vreme pristupa kesu za neki fajl

            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                

                var request = context.Request;
                string filePath = FileUtil.GetFile(request);

                
                lastModified = File.GetLastWriteTime(filePath);//dobijanje vremena zadnje modifikacije fajla

                if (cache.Contains(filePath) && tuple.Item2 > lastModified)//ako kes sadrzi putanju fajla i ako je lastModified(zadnje vreme pristupa kesu)
                                                                           //manje od vremena poslednje izmene
                {
                    tuple = new Tuple<List<string>, DateTime>(words, DateTime.Now);
                    int brojReci = (int)cache.Get(filePath);//pribavljanje broja reci iz kesa
                    Console.WriteLine($"Broj reci iz kesa: {brojReci}\n=========================");
                }

                else
                {
                    //izbrisi iz kesa put fajla jer nije zadnje modifikovan
                    if (cache.Contains(filePath))
                        cache.Remove(filePath);

                    //Procitaj ceo tekst i u Listi words stavi sve reci
                    char[] separators = { ' ', ',', '.', '\n'};
                    words = File.ReadAllText(filePath).Split(separators, StringSplitOptions.RemoveEmptyEntries).ToList();

                    //davanje vrednosti tuple objekta
                    tuple = new Tuple<List<string>, DateTime>(words, DateTime.Now);
                    count = 0;


                    if (words.Count > 0)
                    {
                        stopwatch.Start();
                        MultiThreading();
                        //Sequential();
                        stopwatch.Stop();

                        WriteInCacheAndPrint(filePath);

                        Console.WriteLine($"Vreme za brojanje:{(int)Math.Round(stopwatch.Elapsed.TotalMilliseconds)} milisekundi\n=========================");
                        stopwatch.Reset();
                    }
                    else
                        Console.WriteLine("Fajl je prazan \n=========================");
                }
            }
        }

        static public bool isPalindrom(string word)
        {
            word = word.ToLower();
            if (word.Length < 2)
                return false;

            int i = 0;
            int j = word.Length - 1;

            // provera petljom da li je rec palindrom ili ne
            while (i <= j)
            {
                if (word[i] != word[j])
                    return false;
                i++;
                j--;
            }

            //vrati true ako je rec izasla iz while
            return true;
        }

        static public void WriteInCacheAndPrint(string filePath)
        {
            try
            {
                cache.Set(filePath, count, DateTimeOffset.UtcNow.AddHours(1));//dodavanje u kes fajl koji je trenutno prebrojan
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Greška prilikom postavljanja rezultata u keš: {ex.Message}");
            }
            finally
            {
                if (palindromes.Count == 0)
                    Console.WriteLine("Nema palindroma u tekstu");

                else
                {
                    Console.WriteLine($"Broj palindroma u datom tekstu je: {count}\n Palindromi iz teksta su:");
                    foreach (var palindrom in palindromes)
                    {
                        Console.Write($"{palindrom}||");
                    }
                    Console.WriteLine();
                    palindromes.Clear();
                    words.Clear();
                }
            }
        }

        //sekvencijalno izvrsenje
        public static void Sequential()
        {
            foreach (var word in tuple.Item1)
            {
                if (isPalindrom(word))
                {
                    if (!palindromes.Contains(word))
                    {
                        palindromes.Add(word);
                        count++;
                    }
                }
            }
        }

        public static void MultiThreading()
        {
            //provera koliko imam fizickih jezgara - nepotrebno
            int brFizJezgara = Environment.ProcessorCount;
            Console.WriteLine($"Broj fizickih jezgara: {brFizJezgara}");

            //provera koliko dostupnih niti trenutno ima
            int availableThreads;
            int completionPortThreads;
            ThreadPool.GetAvailableThreads(out availableThreads, out completionPortThreads);
            Console.WriteLine($"Dostupno niti: {availableThreads}");

            ManualResetEvent resetEvent = new ManualResetEvent(false);
            int remainingTasks = words.Count;

            
            //za svaku rec pozivanje po jedne niti da proveri da li je palindrom
            foreach (var word in tuple.Item1)
            {
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    if (isPalindrom(word))
                    {

                        if (!palindromes.Contains(word))
                        {
                            palindromes.Add(word);
                            count++;
                        }
                    }
                    if (Interlocked.Decrement(ref remainingTasks) == 0)//Interlocked obezbedjuje atomicne operacije
                    {
                        resetEvent.Set();//signaliziraj kad zadnja nit zavrsi posao
                    }
                }, null);
            }
            resetEvent.WaitOne();   //glavna nit ceka da poslednja nit signalizira        
        }
    }
}
