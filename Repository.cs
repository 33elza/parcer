using Parcer.jsonModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;

namespace Parcer
{
    class Repository
    {
        public void LoadOrganizations(List<String> organizations)
        {
            int c = 0;
            string line;
            StreamReader file = new StreamReader(Directory.GetCurrentDirectory() + @"\orgs.txt", Encoding.Default);
            while ((line = file.ReadLine()) != null)
            {
                organizations.Add(line);
                // Debug.WriteLine(line);
                c++;
            }
            file.Close();
        }
        public List<OrgResponce> ReadCsvFile()
        {
            string[] orgs = File.ReadAllLines(Directory.GetCurrentDirectory() + @"\organisations.csv", Encoding.Default);
            string[] orgValues = null;
            List<OrgResponce> orgList = new List<OrgResponce>();
            for (int i=0; i<orgs.Length; i++)
            {
                if (!String.IsNullOrEmpty(orgs[i]))
                {
                    orgValues = orgs[i].Split(';');
                    OrgResponce orgResp = new OrgResponce();
                    orgResp.Name = orgValues[0];
                    orgResp.FullName = orgValues[1];
                    orgResp.Region = orgValues[2];
                    orgResp.RegionNumber = orgValues[3];
                    orgResp.Inn = orgValues[4];
                    orgResp.Kpp = orgValues[5];

                    orgList.Add(orgResp);
                }
            }
            return orgList;

        }
        public string ReadProxyAddress()
        {
           StreamReader file = new StreamReader(Directory.GetCurrentDirectory() + @"\proxy.txt", Encoding.Default);
           string address = file.ReadLine();
           Debug.WriteLine(address);
           return address;

        }

        public string GET(string url)
        {
            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://mmk.ru/for_suppliers/auction/index.php");
            //HttpWebResponse responce = (HttpWebResponse)request.GetResponse();

            //CookieContainer cc = new CookieContainer();
            //foreach (Cookie c in responce.Cookies)
            //{
            //    cc.Add(c);
            //}
            if (ConfigurationManager.AppSettings["use_proxy"] == "True")
            {
                WebRequest webreq = WebRequest.Create(url);
                WebProxy wp = new WebProxy(ReadProxyAddress());
               // wp.Credentials = new NetworkCredential();
                webreq.Proxy = wp;
                WebResponse webresp = webreq.GetResponse();
                StreamReader sreader = new StreamReader(webresp.GetResponseStream());
                StringBuilder sb = new StringBuilder();
                sb.Append(sreader.ReadToEnd());
                webresp.Close();
                string decSb = System.Text.RegularExpressions.Regex.Unescape(sb.ToString());
                sreader.Close();
                return decSb.ToString();
            }
            else
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                // req.CookieContainer = cc;
                req.ContentType = "application/json";
                req.Method = "GET";
                req.Accept = "application/json";

                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                StreamReader reader = new StreamReader(resp.GetResponseStream());
                StringBuilder output = new StringBuilder();
                output.Append(reader.ReadToEnd());
                resp.Close();

                string decodedOutput = System.Text.RegularExpressions.Regex.Unescape(output.ToString());
                Debug.WriteLine(decodedOutput);
                reader.Close();
                return decodedOutput.ToString();
            }
            
        }

        public string CreateTenderUrl(string orgName)
        {
            string name = orgName.Replace("\"", String.Empty).Trim().Replace(" ", String.Empty);
            Debug.WriteLine(name);
            int unixTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            string url = WebUtility.UrlEncode(String.Format("\'{0}\'", name)) + "&_=";//+ unixTime.ToString();


            Debug.WriteLine(url);
            return url;
        }

        public string GetTenderResponce(string url)
        {
            string resp = GET(url + ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString());
            return resp;
        }
        public List<String> GetPageTendersOrg(string url)
        {
            string resp = GET(url + ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString());
            Debug.WriteLine("resp= " + resp);

            DataParcer dp = new DataParcer();
            List<string> lotJsons = new List<string>();
            List<string> lotUrls = dp.ParceLotUrl(resp);
            foreach (string lotUrl in lotUrls)
            {
                 string lotJson = GET("http://mmk.ru/for_suppliers/auction/source_dt_l.php?" + lotUrl);
                 lotJsons.Add(lotJson);
            }

           
           // CreateModel(tr);
            return lotJsons;
        }
        public void SendToRedis(string message)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: ConfigurationManager.AppSettings["qeueuname"],
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

               
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                                     routingKey: ConfigurationManager.AppSettings["qeueuname"],
                                     basicProperties: null,
                                     body: body);
                Console.WriteLine(" [x] Sent {0}", message);
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }


    }
}
