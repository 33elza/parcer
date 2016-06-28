using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Web;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parcer.jsonModels;
using Newtonsoft.Json.Serialization;
using System.Data;

namespace Parcer
{
    public class Program
    {
       // public List<String> orgs;
       public static void Main(string[] args)
        {
           List<String> orgs = new List<String>();
           List<String> tenderUrls = new List<String>();
           LoadOrganizations(orgs);
           foreach (string orgName in orgs)
           {              
               tenderUrls.Add("http://mmk.ru/for_suppliers/auction/source_all.php?LOCATION_CODE=" + TenderUrl(orgName));
           }

          // GET("http://mmk.ru/for_suppliers/auction/source_all.php?LOCATION_CODE=%27%D0%9E%D0%9E%D0%9E%D0%9C%D0%A1%D0%A6%27&_=" + ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString());
           Debug.WriteLine("______________");
           List<String> lotUrls = GetPageTendersOrg("http://mmk.ru/for_suppliers/auction/source_all.php?LOCATION_CODE=%27%D0%9E%D0%9E%D0%9E%D0%9C%D0%A1%D0%A6%27&_=");
           ParceTenders(lotUrls);
           Console.ReadLine();
        }

        public static void LoadOrganizations(List<String> organizations)
        {
            int c = 0;
            string line;
            StreamReader file = new StreamReader(Directory.GetCurrentDirectory() + @"\orgs.txt", Encoding.Default);
            while ((line = file.ReadLine())!=null)
            {
                organizations.Add(line);
               // Debug.WriteLine(line);
                c++;
            }
            file.Close();
        }

        public static string TenderUrl(string orgName)
        {
           int unixTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
           string url =  WebUtility.UrlEncode(String.Format("\'{0}\'", orgName)) + "&_=" + unixTime.ToString();
           

           Debug.WriteLine(url);
           return url;
        }
        public static List<String> GetPageTendersOrg(string url)
        {
            string resp = GET(url + ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString());
            Debug.WriteLine("resp= "+ resp);           
            JObject jsonObj = JObject.Parse(resp);

           // JObject jo = JObject.Parse(jsonObj["aaData"].ToString());
          //  string data = "\"aaData\" :" + jsonObj["aaData"].ToString();
           // Debug.WriteLine(data);
            TendersResponce dataSet = JsonConvert.DeserializeObject<TendersResponce>(resp);

            List<String> lotUrls = new List<String>();

            for (int i=0; i<dataSet.aaData.Count;i++)
            {
                string href = dataSet.aaData[i][1];
                string lotUrl = href.Substring(href.IndexOf("?")+1, href.IndexOf(">") - href.IndexOf("?") -1);
                lotUrls.Add(lotUrl);
                Debug.WriteLine(lotUrl);          
            }
            return lotUrls;
        }

        public static void ParceTenders(List<String> lotUrls)
        {
            foreach (string url in lotUrls)
            {
                Debug.WriteLine(url);
                string lotJson = GET("http://mmk.ru/for_suppliers/auction/source_dt_l.php?" + url);
                TendersResponce tr = JsonConvert.DeserializeObject<TendersResponce>(lotJson);
                

            }

        }

        public static string GET(string url)
        {
            Debug.WriteLine("++++++++++++++");
            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://mmk.ru/for_suppliers/auction/index.php");
            //HttpWebResponse responce = (HttpWebResponse)request.GetResponse();

            //CookieContainer cc = new CookieContainer();
            //foreach (Cookie c in responce.Cookies)
            //{
            //    cc.Add(c);
            //}
            Debug.WriteLine("++++++++++++++11111");
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
           // req.CookieContainer = cc;
            req.ContentType = "application/json";
            req.Method = "GET";
            req.Accept = "application/json";
            
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Debug.WriteLine("++++++++++++++222222222");
            StreamReader reader = new StreamReader(resp.GetResponseStream());
            StringBuilder output = new StringBuilder();
            output.Append(reader.ReadToEnd());
            Debug.WriteLine(resp.ContentType);
            resp.Close();
           
            string decodedOutput =  System.Text.RegularExpressions.Regex.Unescape(output.ToString());
            Debug.WriteLine(decodedOutput);
            reader.Close();
            return decodedOutput.ToString();
        }

    }
   
}
