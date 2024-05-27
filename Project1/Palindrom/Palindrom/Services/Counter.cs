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


        static MemoryCache cache = new MemoryCache("PrincepsCash");

        public static void GetNumberOfPalindromes(HttpListenerContext context)
        {
            List<string> palindromes = new List<string>();//upisujem
            List<string> sentences = new List<string>();//samo citam
           
 
            Stopwatch stopwatch = new Stopwatch();
            object mutex = new object();
            int remainingTasks;//upisujem



            DateTime lastModified;//zadnje vreme pristupa kesu za neki fajl
            int count = 0;
            palindromes.Clear();
            sentences.Clear();


            var request = context.Request;
            string? filePath = FileUtil.GetFile(request);
            if (filePath == null)
            {
                Print(context, "NE POSTOJI FAJL",null!,0);
                return;
            }

            
            //Ako se nalazi u kesu
            lastModified = File.GetLastWriteTime(filePath);//dobijanje vremena zadnje modifikacije fajla
            Tuple<int, DateTime> tuple = new Tuple<int, DateTime>(count, lastModified);
            Tuple<int, DateTime>? cachedTuple = cache.Get(filePath) as Tuple<int,DateTime>;
            if (cache.Contains(filePath) && 
                tuple.Item2 == cachedTuple!.Item2)//ako kes sadrzi putanju fajla i ako je lastModified manje od vremena poslednje izmene
            {
                
                Print(context, $"Broj palindroma iz kesa: {cachedTuple.Item1}",null!,0);
            }
            //

            //ako se ne nalazi u kesu
            else
            {
                //izbrisi iz kesa put fajla jer nije zadnje modifikovan
                if (cache.Contains(filePath))
                    cache.Remove(filePath);

                //Procitaj ceo tekst i u Listi sentences stavi sve recenice
                sentences = File.ReadAllText(filePath).Split('.', StringSplitOptions.RemoveEmptyEntries).ToList();

                //davanje vrednosti tuple objekta
                

                if (sentences.Count > 0)
                {
                    stopwatch.Start();
                    ManualResetEvent mainEvent = new ManualResetEvent(false);
                    remainingTasks = sentences.Count;
                    //za svaku recenicu pozivanje po jedne niti 
                    foreach (var sentence in sentences)
                    {
                        string curSentence = sentence;
                        ThreadPool.QueueUserWorkItem((state) =>
                        {

                            string sentenceLower = curSentence.ToLower();
                            List<string> words = new List<string>();
                            char[] separators = { ' ', ',', '.', '\n' };
                            words = sentenceLower.Split(separators, StringSplitOptions.RemoveEmptyEntries).ToList();
                            foreach (string word in words)
                            {
                                if (word.Length < 2)
                                    continue;

                                int i = 0;
                                int j = word.Length - 1;
                                bool isPalindrome = true;

                                while (i < j)
                                {
                                    if (word[i] != word[j])
                                    {
                                        isPalindrome = false;
                                        break;
                                    }
                                    i++;
                                    j--;
                                }

                                if (isPalindrome)
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
                            }
                            if (Interlocked.Decrement(ref remainingTasks) == 0)//Interlocked obezbedjuje atomicne operacije
                            {
                                mainEvent.Set();//signaliziraj kad zadnja nit zavrsi posao
                            }
                        }, null);
                    }
                    mainEvent.WaitOne();   //glavna nit ceka da poslednja nit signalizira   
                    stopwatch.Stop();




                    //ISPISIVANJE
                    if (count == 0)
                        Print(context, "Nema palindroma u datom fajlu", palindromes,count);
                    else
                    {
                        Print(context, $"Broj palindroma u fajlu je :{count}", palindromes,count);
                    }
                    tuple = new Tuple<int,DateTime>(count, lastModified);
                    WriteInCache(filePath, context,tuple);

                    Console.WriteLine($"Vreme za brojanje:{(int)Math.Round(stopwatch.Elapsed.TotalMilliseconds)} milisekundi\n=========================");
                    stopwatch.Reset();
                }
                else
                    Print(context, "FAJL JE PRAZAN",palindromes,count);
            }

        }


        static public void WriteInCache(string filePath, HttpListenerContext context,Tuple<int,DateTime> tuple)
        {
            try
            {
                cache.Set(filePath, tuple, DateTimeOffset.UtcNow.AddHours(1));//dodavanje u kes fajl koji je trenutno prebrojan
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška prilikom postavljanja rezultata u keš: {ex.Message}");
            }
        }

        static public void Print(HttpListenerContext context, string tekst,List<string> palindromes,int count)
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
                int i = 1;
                html.Append("<ul>");
                foreach (var palindrom in palindromes)
                {
                    html.AppendFormat("<li>" + i + ". " + palindrom + "</li>");
                    i++;
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
