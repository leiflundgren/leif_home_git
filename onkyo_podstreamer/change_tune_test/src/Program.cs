using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
        static int send_Counte = 0;
        static int pushed_buffers = 0;

        private static IPAddress FetchIP()
        {
            //Get all IP registered
            // List<string> IPList = new List<string>();

            foreach (IPAddress ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }

            return null;
        }

        static void ConsoleReader()
        {
            System.Threading.Thread.CurrentThread.IsBackground = true;
            while (true)
            {
                if (Console.In.Read() == 13)
                {
                    Console.Out.WriteLine("Change file requested");
                    ++file_pos;
                    mp3stream = null;
                }
            }
        }

        static void Main(string[] args)
        {
            if (args.Length > 0)
                dir = args[0];
            if (!Directory.Exists(dir))
                dir = @"C:\Users\leif\Music";
            if (!Directory.Exists(dir))
                dir = ".";

            files = new List<string>(Directory.EnumerateFiles(dir, "*.mp3"));
            file_pos = 0;

            if (files.Count == 0)
            {
                Console.Out.WriteLine("Failed to find any mp3 in \"" + dir + "\"");
                return;
            }

            Console.Out.WriteLine("Files is " + dir + ":\r\n" + String.Join(", ", files.ToArray()));

            IPAddress addr = FetchIP();
            TcpListener server = new TcpListener(addr, 8091);
            server.Start();
            Console.Out.WriteLine("Opened server on "  + addr.ToString() + ":" + 8091);
            var ctx = server.AcceptTcpClient();
            
            Console.Out.WriteLine("Got request from " + ctx.Client.RemoteEndPoint.ToString() );

            outstream = ctx.GetStream();

            var reader = new Thread(new ThreadStart(ConsoleReader));
            reader.IsBackground = true;
            reader.Start();

            byte[] request_bytes = new byte[1024];
            int len = ctx.GetStream().Read(request_bytes, 0, request_bytes.Length);
            String request = System.Text.Encoding.UTF8.GetString(request_bytes, 0, len);
            Console.Out.WriteLine("Got request\r\n" + request);

            string headers =
@"HTTP/1.0 200 OK
Content-Type: audio/mpeg
icy-br: 128
ice-audio-info: bitrate=128
icy-description: (null)
icy-genre: Classical
icy-name: Onkyo Testing
icy-pub: 1
icy-url: http://www.concertzender.nl/
Server: Icecast 2.3.3
Cache-Control: no-cache" + "\r\n\r\n";
            byte[] header_bytes = System.Text.Encoding.UTF8.GetBytes(headers);

            outstream.Write(header_bytes, 0, header_bytes.Length);

            while (file_pos < files.Count)
            {
                while ( pushed_buffers < 5 )
                {
                    PushData();
                }

                System.Threading.Thread.Sleep(50);
            }
            Console.Out.WriteLine("Done!");
            System.Threading.Thread.Sleep(5000);
        }

        private static void PushData()
        {
            if (mp3stream == null)
            {
                if (file_pos >= files.Count)
                    return;
                mp3stream = new FileStream(Path.Combine(dir, files[file_pos]), FileMode.Open, FileAccess.Read);
                Console.Out.WriteLine("Playing " + files[file_pos] + " " + file_pos + " of  " + files.Count + ". Enter to skip ahead");
            }

            var buf = new byte[1024];
            int len = mp3stream.Read(buf, 0, buf.Length);

            if (len <= 0)
            {
                Console.WriteLine("Reached end of file. " + files[file_pos]);
                file_pos++;
                mp3stream.Close();
                mp3stream = null;
                PushData();
                return;
            }

            Console.WriteLine("Sending " + len + " bytes. Chunk " + (++send_Counte));
            try
            {
                outstream.BeginWrite(buf, 0, len, data_sent, null);
                System.Threading.Interlocked.Increment(ref pushed_buffers);
            }
            catch ( Exception ex )
            {
                Console.Out.WriteLine("Caught " + ex + "\r\nGuessing remote disconnect. Terminating.");
                file_pos = files.Count;
                mp3stream.Close();
                mp3stream = null;
                return;
            }
        }

        private static void data_sent(IAsyncResult ar)
        {
            System.Threading.Interlocked.Decrement(ref pushed_buffers);
            
            // PushData();
        }

    }
}
