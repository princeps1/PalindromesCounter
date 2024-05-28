using Palindrom.Services;
using Palindrom.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace Palindrom
{
    public class Program
    {
        static HttpListener listener = new HttpListener();

        static async Task Main(string[] args)
        {

            listener.Prefixes.Add($"http://localhost:5050/");
            listener.Start();

            //provera koliko imam fizickih jezgara - nepotrebno
            int brFizJezgara = Environment.ProcessorCount;
            Console.WriteLine($"Broj fizickih jezgara: {brFizJezgara}");

            //provera koliko dostupnih niti trenutno ima- nepotrebno
            int availableThreads;
            int completionPortThreads;
            ThreadPool.GetAvailableThreads(out availableThreads, out completionPortThreads);
            Console.WriteLine($"Dostupno niti: {availableThreads}");

            Console.WriteLine("Server startovan...");

            while (true)
            {
                // Ceka na novi zahtev
                HttpListenerContext context = await listener.GetContextAsync();

                //mnogo brze radi kreiranja Taska za svaki zahtev jer se ne kreira nova nit i nema nepotrebnog overhead-a
                _ = ProcessRequestAsync(context);//pozivanje asinhrone funkcije u istoj niti,nema await jer ne mora da se ceka rezultat metode 

                //za svaki zahtev se kreira nova nit i svaki zahtev se izvrsava u posebnoj niti,izvrsava se paralelno
                //_ = Task.Run(async () =>
                //{
                //    List<string> palindromes = new List<string>();//upisujem
                //    List<string> sentences = new List<string>();//samo citam


                //    Stopwatch stopwatch = new Stopwatch();
                //    object mutex = new object();
                //    int remainingTasks;//upisujem



                //    DateTime lastModified;//zadnje vreme pristupa kesu za neki fajl
                //    int count = 0;
                //    palindromes.Clear();
                //    sentences.Clear();


                //    var request = context.Request;
                //    string? filePath = FileUtil.GetFile(request);
                //    if (filePath == null)
                //    {
                //        await ResultManager.PrintAsync(context, "NE POSTOJI FAJL", null!, 0);
                //        return;
                //    }


                //    //Ako se nalazi u kesu
                //    lastModified = File.GetLastWriteTime(filePath);//dobijanje vremena zadnje modifikacije fajla
                //    Tuple<int, DateTime> tuple = new Tuple<int, DateTime>(count, lastModified);
                //    Tuple<int, DateTime>? cachedTuple = ResultManager.GetCache().Get(filePath) as Tuple<int, DateTime>;
                //    if (ResultManager.GetCache().Contains(filePath) &&
                //        tuple.Item2 == cachedTuple!.Item2)//ako kes sadrzi putanju fajla i ako je lastModified manje od vremena poslednje izmene
                //    {

                //        await ResultManager.PrintAsync(context, $"Broj palindroma iz kesa: {cachedTuple.Item1}", null!, 0);
                //    }
                //    //

                //    //ako se ne nalazi u kesu
                //    else
                //    {
                //        //izbrisi iz kesa put fajla jer nije zadnje modifikovan
                //        if (ResultManager.GetCache().Contains(filePath))
                //            ResultManager.GetCache().Remove(filePath);

                //        //Procitaj ceo tekst i u Listi sentences stavi sve recenice
                //        sentences = File.ReadAllText(filePath).Split('.', StringSplitOptions.RemoveEmptyEntries).ToList();

                //        //davanje vrednosti tuple objekta


                //        if (sentences.Count > 0)
                //        {
                //            stopwatch.Start();
                //            ManualResetEvent mainEvent = new ManualResetEvent(false);
                //            remainingTasks = sentences.Count;

                //            //za svaku recenicu pozivanje po jedne niti 
                //            foreach (var sentence in sentences)
                //            {
                //                string curSentence = sentence;
                //                ThreadPool.QueueUserWorkItem((state) =>
                //                {

                //                    string sentenceLower = sentence.ToLower();
                //                    List<string> words = new List<string>();
                //                    char[] separators = { ' ', ',', '.', '\n' };
                //                    words = sentenceLower.Split(separators, StringSplitOptions.RemoveEmptyEntries).ToList();
                //                    foreach (string word in words)
                //                    {
                //                        if (word.Length < 2)
                //                            continue;

                //                        int i = 0;
                //                        int j = word.Length - 1;
                //                        bool isPalindrome = true;

                //                        while (i < j)
                //                        {
                //                            if (word[i] != word[j])
                //                            {
                //                                isPalindrome = false;
                //                                break;
                //                            }
                //                            i++;
                //                            j--;
                //                        }

                //                        if (isPalindrome)
                //                        {

                //                            if (!palindromes.Contains(word))
                //                            {
                //                                lock (mutex)
                //                                {
                //                                    palindromes.Add(word);
                //                                    count++;
                //                                }
                //                            }
                //                        }
                //                    }
                //                    if (Interlocked.Decrement(ref remainingTasks) == 0)//Interlocked obezbedjuje atomicne operacije
                //                    {
                //                        mainEvent.Set();//signaliziraj kad zadnja nit zavrsi posao
                //                    }
                //                }, null);
                //            }
                //            mainEvent.WaitOne();   //glavna nit ceka da poslednja nit signalizira   
                //            stopwatch.Stop();




                //            //ISPISIVANJE
                //            if (count == 0)
                //                await ResultManager.PrintAsync(context, "Nema palindroma u datom fajlu", palindromes, count);
                //            else
                //            {
                //                await ResultManager.PrintAsync(context, $"Broj palindroma u fajlu je :{count}", palindromes, count);
                //            }
                //            tuple = new Tuple<int, DateTime>(count, lastModified);
                //            ResultManager.WriteInCache(filePath, context, tuple);

                //            Console.WriteLine($"Vreme za brojanje:{(int)Math.Round(stopwatch.Elapsed.TotalMilliseconds)} milisekundi\n=========================");
                //            stopwatch.Reset();
                //        }
                //        else
                //            await ResultManager.PrintAsync(context, "FAJL JE PRAZAN", palindromes, count);
                //    }
                //});
            }
        }

        static async Task ProcessRequestAsync(HttpListenerContext context)
        {
            List<string> palindromes = new List<string>();//upisujem
            List<string> sentences = new List<string>();//samo citam


            Stopwatch stopwatch = new Stopwatch();
            object mutex = new object();




            DateTime lastModified;//zadnje vreme pristupa kesu za neki fajl
            int count = 0;
            palindromes.Clear();
            sentences.Clear();


            var request = context.Request;
            string? filePath = FileUtil.GetFile(request);
            if (filePath == null)
            {
                await ResultManager.PrintAsync(context, "NE POSTOJI FAJL", null!, 0);
                return;
            }


            //Ako se nalazi u kesu
            lastModified = File.GetLastWriteTime(filePath);//dobijanje vremena zadnje modifikacije fajla
            Tuple<int, DateTime> tuple = new Tuple<int, DateTime>(count, lastModified);
            Tuple<int, DateTime>? cachedTuple = ResultManager.GetCache().Get(filePath) as Tuple<int, DateTime>;
            if (ResultManager.GetCache().Contains(filePath) &&
                tuple.Item2 == cachedTuple!.Item2)//ako kes sadrzi putanju fajla i ako je lastModified manje od vremena poslednje izmene
            {

                await ResultManager.PrintAsync(context, $"Broj palindroma iz kesa: {cachedTuple.Item1}", null!, 0);
            }
            //

            //ako se ne nalazi u kesu
            else
            {
                //izbrisi iz kesa put fajla jer nije zadnje modifikovan
                if (ResultManager.GetCache().Contains(filePath))
                    ResultManager.GetCache().Remove(filePath);

                //Procitaj ceo tekst i u Listi sentences stavi sve recenice
                sentences = File.ReadAllText(filePath).Split('.', StringSplitOptions.RemoveEmptyEntries).ToList();

                //davanje vrednosti tuple objekta


                if (sentences.Count > 0)
                {
                    stopwatch.Start();
                    ManualResetEvent mainEvent = new ManualResetEvent(false);
                    //int remainingTasks = sentences.Count;

                    //za svaku recenicu pozivanje po jedne niti 
                    foreach (var sentence in sentences)
                    {
                        string curSentence = sentence;
                        //ThreadPool.QueueUserWorkItem((state) =>
                        //{

                        string sentenceLower = sentence.ToLower();
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
                        //        if (Interlocked.Decrement(ref remainingTasks) == 0)//Interlocked obezbedjuje atomicne operacije
                        //        {
                        //            mainEvent.Set();//signaliziraj kad zadnja nit zavrsi posao
                        //        }
                        //    }, null);
                        //}
                        //mainEvent.WaitOne();   //glavna nit ceka da poslednja nit signalizira   
                        stopwatch.Stop();
                    }
                    //ISPISIVANJE
                    if (count == 0)
                        await ResultManager.PrintAsync(context, "Nema palindroma u datom fajlu", palindromes, count);
                    else
                    {
                        await ResultManager.PrintAsync(context, $"Broj palindroma u fajlu je :{count}", palindromes, count);
                    }
                    tuple = new Tuple<int, DateTime>(count, lastModified);
                    ResultManager.WriteInCache(filePath, context, tuple);

                    Console.WriteLine($"Vreme za brojanje:{(int)Math.Round(stopwatch.Elapsed.TotalMilliseconds)} milisekundi\n=========================");
                    stopwatch.Reset();
                }

                else
                    await ResultManager.PrintAsync(context, "FAJL JE PRAZAN", palindromes, count);
            }
        }
    }
}