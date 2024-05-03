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
        public static string GetFile(HttpListenerRequest request)
        {
            try
            {
                string rootFolder = Path.GetFullPath("."); // Dobijanje apsolutne putanje do root foldera
                string filename = request.Url.Segments.Last(); // Odvajanje poslednjeg segmenta URL adrese
                string[] files = Directory.GetFiles(rootFolder, filename, SearchOption.AllDirectories);

                if (files.Length > 0)
                    return files[0];
                else
                    throw new FileNotFoundException();
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Fajl nije pronađen");
                Environment.Exit(1); // Izađi iz programa sa kodom greške 1
                return null;//nikad se nece izvrsiti
            }
        }


    }
}
