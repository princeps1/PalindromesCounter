using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Palindrom.Utils
{
    public class FileUtil
    {
        static HttpListener listener = new HttpListener();

        public static void StartListening(string prefix)//u slucaju da funkcija nema parametara
                                                        //string? prefix=null
        {
            listener.Prefixes.Add(prefix);
            listener.Start();
        }
        public static string GetFile();



    }
}
