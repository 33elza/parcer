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
using CsQuery;
using RabbitMQ;
using RabbitMQ.Client;

namespace Parcer
{
     class Program
    {
       // public List<String> orgs;
       public static void Main(string[] args)
        {
           Repository rep = new Repository();
           DataParcer dparc = new DataParcer();
           List<OrgResponce> orgList = rep.ReadCsvFile();
            
           foreach (OrgResponce or in orgList)
           {
               Debug.WriteLine(or.Name);
               //Debug.WriteLine(or.FullName);
               //Debug.WriteLine(or.Region);
               //Debug.WriteLine(or.RegionNumber);
               //Debug.WriteLine(or.Inn);
               //Debug.WriteLine(or.Kpp);
               //Debug.WriteLine("_____________________");
               //Debug.WriteLine(rep.CreateTenderUrl(or.Name));
               //Debug.WriteLine("_____________________");
               string url = rep.CreateTenderUrl(or.Name);
               string tenderResp = rep.GetTenderResponce("http://mmk.ru/for_suppliers/auction/source_all.php?LOCATION_CODE=" + url);
               List<String> lotJsons = rep.GetPageTendersOrg("http://mmk.ru/for_suppliers/auction/source_all.php?LOCATION_CODE=" + url);
               List<String> notModels = dparc.ParceTenders(tenderResp, lotJsons, or);
               foreach (string mod in notModels)
               {
                   rep.SendToRedis(mod);
               }               
           }

          // Debug.WriteLine(rep.GET("http://mmk.ru/for_suppliers/auction/source_all.php?LOCATION_CODE=%27%D0%9E%D0%9E%D0%9E%D0%9C%D0%A1%D0%A6%27&_=" + ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString()));
           
           Console.ReadLine();
        }
    }
   
}
