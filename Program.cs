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
using System.Threading;

namespace Parcer
{
     class Program
    {
       public static void Main(string[] args)
        {
           while (true)
           {
               Repository rep = new Repository();
               DataParcer dparc = new DataParcer();
               List<OrgResponce> orgList = rep.ReadCsvFile();

               if (orgList.Count > 0)
               {

                   foreach (OrgResponce or in orgList)
                   {
                       Debug.WriteLine(or.Name);
                       string url = rep.CreateTenderUrl(or.Name);
                       string tenderResp = rep.GetTenderResponce("http://mmk.ru/for_suppliers/auction/source_all.php?LOCATION_CODE=" + url);
                       List<String> lotJsons = rep.GetPageTendersOrg("http://mmk.ru/for_suppliers/auction/source_all.php?LOCATION_CODE=" + url);
                       List<String> notModels = dparc.ParceTenders(tenderResp, lotJsons, or);
                       foreach (string mod in notModels)
                       {
                           rep.SendToRedis(mod);
                       }
                   }
                   Thread.Sleep(3600000);
               }


               Console.ReadLine();
               
           }
           
        }
    }
   
}
