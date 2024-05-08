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
        public static string? GetFile(HttpListenerRequest request)
        {
            try
            {
                string rootFolder = Path.GetFullPath(".");
                string filename = request!.Url!.Segments.Last();
                string[] files = Directory.GetFiles(rootFolder, filename, SearchOption.AllDirectories);

                if (files.Length > 0)
                    return files[0];
                else
                { 
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška prilikom pretrage fajla: {ex.Message}");
                Environment.Exit(0);
                return null;//nikad nece da se izvrsi
            }
        }
    }


}

