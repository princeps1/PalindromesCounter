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

        static void Main(string[] args)
        {
            FileUtil.StartListening($"http://localhost:5050/");
            //Counter.GetNumberOfPalindromes();
        }
    }
}
