using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections;
using System.Net.Http;
using System.Net;
using System.Reflection;
using System.IO;
using System.Security.Cryptography;
//using NPOI.HSSF.UserModel;
using System.Windows.Forms;
using System.Configuration;
using SKGeo.SafeMonitor.CreateExcelReport;
using System.Diagnostics;

namespace App
{
    // 需求：编写一个计算器，继承上面的CalculatorBase类，并实现Start和Dispose方法。
    // Start用于启动该计算器，Dispose则用于销毁，而启动和销毁的具体实现已经由基类
    // 的StartCore与DisposeCore提供，直接调用即可，确保成功，不会抛出一场。不过问
    // 题在于，Start和Dispose方法可能会被并发地执行，顺序不定，次数不定，但它们的
    // 实现必须满足：

    // 1. StartCore和DisposeCore都是相对较为耗时的操作，且最多只能被调用一次。
    // 2. StartCore和DisposeCore一旦开始执行，则无法终止，只能执行成功。
    // 3. StartCore和DisposeCore必须顺序地执行，不可有任何并行。
    // 5. 假如调用Dispose时StartCore已经执行，则必须调用DisposeCore，否则不用。
    // 6. 调用Dispose意味着要销毁对象，对象销毁以后的任何访问，都不会执行任何操作。
    // 7. Start及Dispose方法可立即返回，不必等待StartCore或DisposeCore完成。不过，
    // 8. 计算器本身不发起任何多线程或异步操作，一切都在Start和Dispose方法里完成。

    // 参考实现：一个最简单的实现方法便是用锁来保护Start和Dispose方法：

    public abstract class CalculatorBase : IDisposable
    {

        protected void StartCore()
        {
            // ...
        }

        protected void DisposeCore()
        {
            // ...
        }

        public abstract void Start();

        public abstract void Dispose();
    }

    public class BigLockCalculator : CalculatorBase
    {

        private readonly object _gate = new object();

        private enum Status
        {
            NotStarted, //0
            Started, //1
            Disposed, //2
            Starting, //3
            Disposing //4
        }

        private int _status = 0;

        public override void Start()
        {
            int cStatus = Interlocked.CompareExchange(ref this._status, 3, 0);
            if (_status == 0)
            {
                StartCore();
                Interlocked.Exchange(ref this._status, 1);
            }

            cStatus = Interlocked.CompareExchange(ref this._status, 4, 1);
            if (cStatus == 1)
            {
                DisposeCore();
                Interlocked.Exchange(ref this._status, 2);
            }
        }

        public override void Dispose()
        {
            int cStatus = Interlocked.CompareExchange(ref this._status, 4, 1);
            if (_status == 1)
            {
                DisposeCore();
                Interlocked.Exchange(ref this._status, 2);
            }
        }
    }

    class CTest
    {

        private string meg { get; set; }

        public void Work(CTest test)
        {
            this.meg = test.meg;
            
        }
    }

    class GT<T, TResult>
    {
        public TResult Get(T arg)
        {
            return (TResult)(object)DateTime.Now;
        }
    }

    static class GTExt
    {
        public static DateTime Get(this GT<long, DateTime> gt, long lg)
        {
            return DateTime.Now;
        }
    }

    class Program
    {
        public static IEnumerable<int> GetMyArray(int[] array)
        {
            if (array == null) yield break;
            foreach (var item in array)
            {
                if (item != 4) //判断条件
                    yield return item;
            }
        }

        public static IEnumerator<int> Power(int number, int exponent)
        {
            int counter = 0;
            int result = 1;
            while (counter++ < exponent)
            {
                result = result * number;

                yield return result;
            }
        }

        static Task<int> DoWork()
        {
            Task<int> task = new Task<int>(() =>
            {
                return 10;
            });
            task.Start();
            return task;
        }

        static async void Delay2000Async()
        {
            Console.WriteLine("ss" + Thread.CurrentThread.ManagedThreadId);
            await Task.Delay(2000);
            Console.WriteLine("dd" + Thread.CurrentThread.ManagedThreadId);
        }

        class TreeNode<T>
        {
            public TreeNode(T t)
            {
                Value = t;
            }
            public TreeNode<T> Left { get; set; }
            public TreeNode<T> Right { get; set; }
            public T Value { get; set; }
        }

        static IEnumerable<T> Traverse<T>(TreeNode<T> node)
        {
            if (node == null) yield break;
            IEnumerator<T> child = Traverse(node.Left).GetEnumerator();
            while (child.MoveNext())
            {
                yield return child.Current;
            }
            yield return node.Value;
            foreach (var s in Traverse(node.Right))
                yield return s;
        }

        class ObTest
        {
            public void work()
            {
                Console.WriteLine(this.GetHashCode());
                FileStream stream = File.Create("a.txt");
                Console.WriteLine("正式1" + Thread.CurrentThread.ManagedThreadId);
                string txt = "dfdfdf";
                Encoding e = Encoding.UTF8;
                var ay = e.GetBytes(txt);
                var ar = stream.BeginWrite(ay, 0, ay.Length, AsyncCallback, null);
                ar.AsyncWaitHandle.WaitOne();
                stream.Flush();
                stream.Close();
                ar.AsyncWaitHandle.Close(); //释放资源  
                Console.WriteLine("正式2" + Thread.CurrentThread.ManagedThreadId);
                Thread.SpinWait(500000);
            }
            void AsyncCallback(IAsyncResult ar)
            {
                Console.WriteLine("回调" + Thread.CurrentThread.ManagedThreadId);
                Console.WriteLine(this.GetHashCode());
            }
        }

        public static string GetFileName()
        {
            string fileName = string.Empty;
            fileName = System.IO.Path.GetFileName(Application.ExecutablePath);
            return fileName;
        }

        private static void testc(string str)
        {
            str = "2323";
        }

        static void Main(string[] args)
        {
            //Dictionary<int,int> d= new Dictionary<int,int>(
            
            //Nullable<int> d = null;
            //object obj = null;
            //Nullable<int> t = (Nullable<int>)obj;
            //var type = obj.GetType();

            //string ddf = "000";
            //testc(ddf);

            var sr = AesCodeManger.AESEncrypt("Database=tianandb;Data Source=112.124.110.196; Port=3306;User Id=softuser;Password=geo2003", "sky", "abcdefghijklmnop");
            //sr = AesCodeManger.AESDecrypt("dY4CaUAxtXdZwIz77ZVuYyiPz7PkoHbwr0bPphatJU6ScYzFzZo0GBKst3QMVvkr9Z65uT4hoK2VuqCOAz2Tbt1+xmeaq3h68tZjUr5jb5pZ1OPXIwWdJ2NyZ3Iw+tak&#xD;&#xA;", "sky", "abcdefghijklmnop");
            //ExcelData();
            //ExeConfigurationFileMap map = new ExeConfigurationFileMap { ExeConfigFilename = GetFileName()+".config" };
            //Configuration config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

            //Double a = 2.12345671234567;
            //var sdf= Math.Round(a, 2);
            //Type t = typeof(TemplateDefine);
            //var ss = t.GetProperties();

            //foreach (var i in ss)
            //{
            //    var rr = t.Namespace + "." + t.Name + "." + i.Name;
            //    Console.WriteLine(rr);
            //}
            // Exec.Work();


            Console.ReadKey();
        }

        static void ExcelData()
        {
            //HSSFWorkbook wb = new HSSFWorkbook(new FileStream(@"C:\Users\Administrator\Desktop\曲线测试.xls", FileMode.Open));

            //var sheet1 = wb.GetSheet("#管线曲线");
            //DateTime dt = DateTime.Now.AddYears(-1);
            //for (int i = 0; i < 50; i++)
            //{
            //    var row = sheet1.CreateRow(i + 1);
            //    row.CreateCell(0).SetCellValue(i);
            //    dt = dt.AddDays(1);
            //    row.CreateCell(1).SetCellValue(dt);
            //    row.CreateCell(2).SetCellValue(i);
            //}
            ////Write the stream data of workbook to the root directory
            //FileStream file = new FileStream(@"test.xls", FileMode.Create);
            //wb.Write(file);
            //file.Close();

            Guid id = new Guid();
        }
        static void cdeal(object obj)
        {
            IEnumerable ien = obj as IEnumerable;
            if (ien == null)
                throw new NotSupportedException();
            foreach (object item in ien)
            {
                PropertyInfo[] infoarray = item.GetType().GetProperties();
                foreach (PropertyInfo info in infoarray)
                {
                    Console.WriteLine(info.GetValue(item, null));
                }
            }
        }
    }

    public class AesCodeManger
    {
        /// <summary>
        /// 有密码的AES加密 
        /// </summary>
        /// <param name="text">加密字符</param>
        /// <param name="password">加密的密码</param>
        /// <param name="iv">密钥</param>
        /// <returns></returns>
        public static string AESEncrypt(string text, string password, string iv)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();

            rijndaelCipher.Mode = CipherMode.CBC;

            rijndaelCipher.Padding = PaddingMode.PKCS7;

            rijndaelCipher.KeySize = 128;

            rijndaelCipher.BlockSize = 128;

            byte[] pwdBytes = System.Text.Encoding.UTF8.GetBytes(password);

            byte[] keyBytes = new byte[16];

            int len = pwdBytes.Length;

            if (len > keyBytes.Length) len = keyBytes.Length;

            System.Array.Copy(pwdBytes, keyBytes, len);

            rijndaelCipher.Key = keyBytes;


            byte[] ivBytes = System.Text.Encoding.UTF8.GetBytes(iv);
            rijndaelCipher.IV = ivBytes;

            ICryptoTransform transform = rijndaelCipher.CreateEncryptor();

            byte[] plainText = Encoding.UTF8.GetBytes(text);

            byte[] cipherBytes = transform.TransformFinalBlock(plainText, 0, plainText.Length);

            return Convert.ToBase64String(cipherBytes);

        }

        /// <summary>
        /// 随机生成密钥
        /// </summary>
        /// <returns></returns>
        public static string GetIv(int n)
        {
            char[] arrChar = new char[]{
           'a','b','d','c','e','f','g','h','i','j','k','l','m','n','p','r','q','s','t','u','v','w','z','y','x',
           '0','1','2','3','4','5','6','7','8','9',
           'A','B','C','D','E','F','G','H','I','J','K','L','M','N','Q','P','R','T','S','V','U','W','X','Y','Z'
          };

            StringBuilder num = new StringBuilder();

            Random rnd = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < n; i++)
            {
                num.Append(arrChar[rnd.Next(0, arrChar.Length)].ToString());
            }

            return num.ToString();
        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="text"></param>
        /// <param name="password"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string AESDecrypt(string text, string password, string iv)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();

            rijndaelCipher.Mode = CipherMode.CBC;

            rijndaelCipher.Padding = PaddingMode.PKCS7;

            rijndaelCipher.KeySize = 128;

            rijndaelCipher.BlockSize = 128;

            byte[] encryptedData = Convert.FromBase64String(text);

            byte[] pwdBytes = System.Text.Encoding.UTF8.GetBytes(password);

            byte[] keyBytes = new byte[16];

            int len = pwdBytes.Length;

            if (len > keyBytes.Length) len = keyBytes.Length;

            System.Array.Copy(pwdBytes, keyBytes, len);

            rijndaelCipher.Key = keyBytes;

            byte[] ivBytes = System.Text.Encoding.UTF8.GetBytes(iv);
            rijndaelCipher.IV = ivBytes;

            ICryptoTransform transform = rijndaelCipher.CreateDecryptor();

            byte[] plainText = transform.TransformFinalBlock(encryptedData, 0, encryptedData.Length);

            return Encoding.UTF8.GetString(plainText);

        }
    }
}
namespace SKGeo.SafeMonitor.CreateExcelReport
{
    public class TemplateDefine
    {
        public static string Name { get; set; }

        public static string Value { get; set; }
    }
}
