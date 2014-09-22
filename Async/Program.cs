using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Concurrent;
using System.Collections;

namespace Async
{
    public class Program
    {
        public static int GetID()
        {
            return Thread.CurrentThread.ManagedThreadId;
        }

        #region 旧格式
        static void DownloadSync(string url)
        {
            WebRequest req = HttpWebRequest.Create(url);
            WebResponse rep = req.GetResponse();
            Stream resp = rep.GetResponseStream();
            GetID();
            using (var sr = new StreamReader(resp))
            {
                Console.WriteLine(sr.ReadToEnd());
            }
        }

        static void DownloadAsync(string url)
        {
            GetID();
            WebRequest req = HttpWebRequest.Create(url);
            req.BeginGetResponse((ar) =>
            {
                WebResponse rep = req.EndGetResponse(ar);
                GetID();
                Stream resp = rep.GetResponseStream();
                using (var sr = new StreamReader(resp))
                {
                    GetID();
                }
            }, null);
        }
        #endregion

        public static IEnumerable<IAsync> ASyncDown(string url)
        {
            WebRequest req = HttpWebRequest.Create(url);
            Async<WebResponse> rep = new AsyncPrimitive<WebResponse>(req.BeginGetResponse, req.EndGetResponse);
            yield return rep;
            //MoveNext 执行下面的代码
            Stream resp = rep.Result.GetResponseStream();
            using (var sr = new StreamReader(resp))
            {
                Console.WriteLine("end");
                GetID();
            }
            WebRequest reqs = HttpWebRequest.Create("http://www.google.com");
            Async<WebResponse> reps = new AsyncPrimitive<WebResponse>(reqs.BeginGetResponse, reqs.EndGetResponse);
            yield return reps;
            //下个MoveNext
            Stream resps = reps.Result.GetResponseStream();
            using (var sr = new StreamReader(resps))
            {
                Console.WriteLine("last");
                GetID();
            }
        }
        public async static void SyncDown(string url)
        {
            WebRequest req = HttpWebRequest.Create(url);
            WebRequest reqs = HttpWebRequest.Create(url);

            var rep = await req.GetResponseAsync();
            Stream resp = rep.GetResponseStream();
            using (var sr = new StreamReader(resp))
            {
                Console.WriteLine("syncend  " + GetID());
            }
            var reps = await reqs.GetResponseAsync();
            Stream resps = reps.GetResponseStream();
            using (var sr = new StreamReader(resps))
            {
                Console.WriteLine("synclast  " + GetID());
                GetID();
            }
        }
        static void Main(string[] args)
        {
            //string url = "http://www.google.com";
            //Console.WriteLine("Begin  " + GetID());
            //ASyncDown(url).Execte();
            ////SyncDown(url);
            //Console.WriteLine("");

            //System.Collections.Concurrent.ConcurrentDictionary<int, int> dic = new ConcurrentDictionary<int, int>();

            //var s = Guid.Empty.ToString();
            //Guid ss = Guid.NewGuid();

            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(List<AlarmParameter>));
            AlarmParameter ap = new AlarmParameter();
            ap.Name = "AL";
            ap.Code = "AL";
            ap.Value = null;
            ap.Precision = 0;

            List<AlarmParameter> itemlist = new List<AlarmParameter>();
            itemlist.Add(ap);
            File.Delete("a.xml");

            var f = File.Create("a.xml");
            x.Serialize(f, itemlist);
            f.Close();


           
            Console.ReadKey();
        }

        public class AlarmParameter
        {
            public string Name { get; set; }

            public string Code { get; set; }

            public Double? Value { get; set; }

            public int Precision { get; set; }
        }

    }
}
