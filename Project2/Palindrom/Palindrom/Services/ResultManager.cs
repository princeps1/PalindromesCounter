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
    class ResultManager
    {


        static MemoryCache cache = new MemoryCache("PrincepsCash");

        static public MemoryCache GetCache() { return cache; }


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

        static public async Task PrintAsync(HttpListenerContext context, string tekst, List<string> palindromes, int count)
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

            // Asinhrono pisanje
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }

    }
}
