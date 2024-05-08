﻿using Palindrom.Utils;
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
        static object mutex = new object();

        public static void GetNumberOfPalindromes(HttpListener listener)
        {
            Console.WriteLine("Server startovan...");
            DateTime lastModified;//zadnje vreme pristupa kesu za neki fajl

            while (true)
            {
                HttpListenerContext context = listener.GetContext();


                var request = context.Request;
                string? filePath = FileUtil.GetFile(request);
                if (filePath == null)
                {

                    Print(context, "NE POSTOJI FAJL");
                    continue;
                }



                lastModified = File.GetLastWriteTime(filePath);//dobijanje vremena zadnje modifikacije fajla
                if (cache.Contains(filePath) && tuple.Item2 > lastModified)//ako kes sadrzi putanju fajla i ako je lastModified(zadnje vreme pristupa kesu)
                                                                           //manje od vremena poslednje izmene
                {
                    tuple = new Tuple<List<string>, DateTime>(words, DateTime.Now);
                    int brojPal = (int)cache.Get(filePath);//pribavljanje broja reci iz kesa
                                                           //Console.WriteLine($"Broj palindroma iz kesa: {brojPal}\n=========================");
                    Print(context, $"Broj palindroma iz kesa: {brojPal}");
                }
                else
                {
                    //izbrisi iz kesa put fajla jer nije zadnje modifikovan
                    if (cache.Contains(filePath))
                        cache.Remove(filePath);

                    //Procitaj ceo tekst i u Listi words stavi sve reci
                    char[] separators = { ' ', ',', '.', '\n' };
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

                        WriteInCacheAndPrint(filePath, context);

                        Console.WriteLine($"Vreme za brojanje:{(int)Math.Round(stopwatch.Elapsed.TotalNanoseconds)} nanosekundi\n=========================");
                        stopwatch.Reset();
                    }
                    else
                        Print(context, "FAJL JE PRAZAN");
                }
            }
        }

        public static void MultiThreading()
        {
            //provera koliko imam fizickih jezgara - nepotrebno
            int brFizJezgara = Environment.ProcessorCount;
            Console.WriteLine($"Broj fizickih jezgara: {brFizJezgara}");

            //provera koliko dostupnih niti trenutno ima- nepotrebno
            int availableThreads;
            int completionPortThreads;
            ThreadPool.GetAvailableThreads(out availableThreads, out completionPortThreads);
            Console.WriteLine($"Dostupno niti: {availableThreads}");

            ManualResetEvent mainEvent = new ManualResetEvent(false);
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
                            lock (mutex)
                            {

                                palindromes.Add(word);
                                count++;
                            }
                        }
                    }
                    if (Interlocked.Decrement(ref remainingTasks) == 0)//Interlocked obezbedjuje atomicne operacije
                    {
                        mainEvent.Set();//signaliziraj kad zadnja nit zavrsi posao
                    }
                }, null);
            }
            mainEvent.WaitOne();   //glavna nit ceka da poslednja nit signalizira   
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

        static public void WriteInCacheAndPrint(string filePath, HttpListenerContext context)
        {
            try
            {
                cache.Set(filePath, count, DateTimeOffset.UtcNow.AddHours(1));//dodavanje u kes fajl koji je trenutno prebrojan
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška prilikom postavljanja rezultata u keš: {ex.Message}");
            }
            finally
            {

                if (count == 0)
                    Print(context, "Nema palindroma u datom fajlu");
                else
                {
                    Print(context, $"Broj palindroma u fajlu je :{count}");
                    palindromes.Clear();
                    words.Clear();

                }
            }
        }

        static public void Print(HttpListenerContext context, string tekst)
        {
            var html = new StringBuilder("<html><head>");
            html.Append("<style>");
            html.Append("body { font-family: Arial, sans-serif; background-color: #f4f4f4; display: flex; justify-content: center; align-items: center; height: 100vh; }");
            html.Append(".container { background-color: #f8f8f8; padding: 20px; border-radius: 10px; box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1); text-align: center; }");
            html.Append("h1 { color: #333; }");
            html.Append("ul { list-style-type: none; padding: 0; }");
            html.Append("li { background-color: #fff; padding: 10px; margin: 5px 0; border-radius: 5px; box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1); }");
            html.Append("</style>");
            html.Append("</head><body>");

            html.Append("<div class='container'>");
            html.Append("<h1>" + tekst + "</h1>");

            if (count > 0)
            {
                html.Append("<ul>");
                foreach (var palindrom in palindromes)
                {
                    html.AppendFormat("<li>" + palindrom + "</li>");
                }
                html.Append("</ul>");
            }
            html.Append("</div>");

            html.Append("</body></html>");

            var buffer = Encoding.UTF8.GetBytes(html.ToString());
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);


        }
    }
}
