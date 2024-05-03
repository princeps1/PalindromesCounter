using Palindrom.Services;
using Palindrom.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace Palindrom
{
    public class Program
    {
        static HttpListener listener = new HttpListener();

        static void Main(string[] args)
        {

            listener.Prefixes.Add($"http://localhost:5050/");
            listener.Start();

            Counter.GetNumberOfPalindromes(listener);
        }

    }
}
