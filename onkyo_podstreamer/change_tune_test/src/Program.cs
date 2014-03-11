using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace change_tune_test
{
    class Program
    {
        static Stream mp3stream;
        static Stream outstream;
        static List<string> files;
        static int file_pos;
        static string dir = @"C:\Users\leif\Music";

						
        private static string FetchIP()
      {
	//Get all IP registered
	// List<string> IPList = new List<string>();

	foreach (IPAddress ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
	  {
	    if (ip.AddressFamily == AddressFamily.InterNetwork)
	      {
		return ip.ToString();
		//IPList.Add(ip.ToString());
	      }
	  }

	return null;
      }

        static void Main(string[] args)
        {
	  if ( args.Length > 0 )
	    dir = args[0];
	  if ( !Directory.Exists(dir) )
	    dir = @"C:\Users\leif\Music";
	  if ( !Directory.Exists(dir) )
	    dir = ".";

            files = new List<string>(Directory.EnumerateFiles(dir, "*.mp3"));
            file_pos = 0;


            string port = @"http://" + FetchIP() + ":8091/";
            HttpListener server = new HttpListener();
            server.Prefixes.Add(port);
            server.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            server.Start();
	    Console.Out.WriteLine("Opened server on " + port);

            var ctx = server.GetContext();

            ctx.Response.ContentType = "application/mp3";
            outstream = ctx.Response.OutputStream;

            

            PushData();

            while ( file_pos < files.Count)
            {
                Console.Out.WriteLine("Playing " + files[file_pos] + " " + file_pos + " of  " + files.Count + ". Enter to skip ahead");
                Console.In.Read();
                ++file_pos;
                mp3stream = null;
            }
            System.Threading.Thread.Sleep(5000);
        }

        private static void PushData()
        {
            if ( mp3stream == null )
            {
                if (file_pos >= files.Count)
                    return;
                mp3stream = new FileStream(Path.Combine(dir, files[file_pos]), FileMode.Open, FileAccess.Read);
            }

            var buf = new byte[1024];
            int len = mp3stream.Read(buf, 0, buf.Length);

            Console.WriteLine("Sending " + len + " bytes");
            outstream.BeginWrite(buf, 0, len, data_sent, null);
        }

        private static void data_sent(IAsyncResult ar)
        {
            PushData();
        }

    }
}
