﻿using System;
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
using CsQuery;

namespace Parcer
{
     class Program
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
            TendersResponce tr = JsonConvert.DeserializeObject<TendersResponce>(resp);

            List<String> lotUrls = new List<String>();

            for (int i = 0; i < tr.aaData.Count; i++)
            {
                string href = tr.aaData[i][1];
                string lotUrl = href.Substring(href.IndexOf("?") + 1, href.IndexOf(">") - href.IndexOf("?") - 1);
                lotUrls.Add(lotUrl);
                Debug.WriteLine(lotUrl);
            }
            CreateModel(tr);
            return lotUrls;
        }

        public static void CreateModel(TendersResponce tr)
        {
            for (int i = 0; i < tr.aaData.Count; i++)
            {
                Model model = new Model();
                model.Id = tr.aaData[i][0];
                model.Note = tr.aaData[i][1].Substring(tr.aaData[i][1].IndexOf(">") + 1, tr.aaData[i][1].LastIndexOf("<") - tr.aaData[i][1].IndexOf(">") - 1);
                model.SubmissionStartDateTime = DateTime.Parse(tr.aaData[i][2].Substring(0, tr.aaData[i][2].IndexOf("(")));
                model.SubmissionCloseDateTime = DateTime.Parse(tr.aaData[i][3].Substring(0, tr.aaData[i][3].IndexOf("(")));

                model.ContactPerson = new Contacts();
                string c = tr.aaData[i][4];
                model.ContactPerson.FIO = c.Substring(0, c.IndexOf("<"));
                model.ContactPerson.Position = c.Substring(c.IndexOf(">") + 1, c.Skip(c.IndexOf(">")).ToString().IndexOf(">"));
                model.ContactPerson.Email = c.Substring(c.IndexOf("mailto:")+7, c.Substring(c.IndexOf("mailto")).IndexOf(">")-7);

                model.Lots = new List<Lot>();
                
                    string href = tr.aaData[i][1];
                    string lotUrl = href.Substring(href.IndexOf("?") + 1, href.IndexOf(">") - href.IndexOf("?") - 1);
                    string lotJson = GET("http://mmk.ru/for_suppliers/auction/source_dt_l.php?" + lotUrl);
                    TendersResponce lotResp = JsonConvert.DeserializeObject<TendersResponce>(lotJson);
                    for (int j = 0; j < lotResp.aaData.Count;j++ )
                    {
                        Lot lot = new Lot();
                        lot.Name = lotResp.aaData[j][1];
                        lot.Measure = lotResp.aaData[j][3];
                        lot.Quantity = Convert.ToDouble(lotResp.aaData[j][4]);
                        model.Lots.Add(lot);
                    }
            }

        }

        public static void ParceTenders(List<String> lotUrls)
        {
            foreach (string url in lotUrls)
            {
                //Debug.WriteLine(url);
                string lotJson = GET("http://mmk.ru/for_suppliers/auction/source_dt_l.php?" + url);
                TendersResponce tr = JsonConvert.DeserializeObject<TendersResponce>(lotJson);
                

            }

        }

        public static string GET(string url)
        {
           // Debug.WriteLine("++++++++++++++");
            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://mmk.ru/for_suppliers/auction/index.php");
            //HttpWebResponse responce = (HttpWebResponse)request.GetResponse();

            //CookieContainer cc = new CookieContainer();
            //foreach (Cookie c in responce.Cookies)
            //{
            //    cc.Add(c);
            //}
           // Debug.WriteLine("++++++++++++++11111");
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
           // req.CookieContainer = cc;
            req.ContentType = "application/json";
            req.Method = "GET";
            req.Accept = "application/json";
            
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
           // Debug.WriteLine("++++++++++++++222222222");
            StreamReader reader = new StreamReader(resp.GetResponseStream());
            StringBuilder output = new StringBuilder();
            output.Append(reader.ReadToEnd());
           // Debug.WriteLine(resp.ContentType);
            resp.Close();
           
            string decodedOutput =  System.Text.RegularExpressions.Regex.Unescape(output.ToString());
            Debug.WriteLine(decodedOutput);
            reader.Close();
            return decodedOutput.ToString();
        }

    }
   
}
