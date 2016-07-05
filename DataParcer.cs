using Newtonsoft.Json;
using Parcer.jsonModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parcer
{
    class DataParcer
    {
        public List<string> ParceLotUrl(string responce)
        {
            responce.Replace("\"", "");
            Debug.WriteLine(responce);
            TendersResponce tr = JsonConvert.DeserializeObject<TendersResponce>(responce);

            List<String> lotUrls = new List<String>();

            for (int i = 0; i < tr.aaData.Count; i++)
            {
                string href = tr.aaData[i][1];
                Debug.WriteLine(href);
                string lotUrl = href.Substring(href.IndexOf("?") + 1, href.IndexOf(">") - href.IndexOf("?") - 1);
                Debug.WriteLine(lotUrl);
                lotUrls.Add(lotUrl);
                Debug.WriteLine(lotUrl);
            }

            return lotUrls; 
        }

        public List<string> ParceTenders(string tenderResponce, List<string> lotJsons, OrgResponce orgResp)
        {
            List<string> notmodelJsons = new List<string>();
            TendersResponce tr = JsonConvert.DeserializeObject<TendersResponce>(tenderResponce);
           for (int i = 0; i < tr.aaData.Count; i++)
           {
               NotificationModel notModel = new NotificationModel();
               List<OrganisationId> cust = new List<OrganisationId>();
               OrganisationId orgId = new OrganisationId();
               orgId.Inn = orgResp.Inn;
               orgId.Kpp = orgResp.Kpp;
               cust.Add(orgId);
               notModel.Customers = cust;
               notModel.PlacingWayId = 5000;
               notModel.RegionCode = Convert.ToInt32(orgResp.RegionNumber);
               notModel.Organisations = orgResp.FullName + orgResp.Name;
               if (tr.iTotalDisplayRecords > 1)
                   notModel.Multilot = true;
               else notModel.Multilot = false;

               Model model = new Model();
               model.Id = tr.aaData[i][0];
               model.Note = tr.aaData[i][1].Substring(tr.aaData[i][1].IndexOf(">") + 1, tr.aaData[i][1].LastIndexOf("<") - tr.aaData[i][1].IndexOf(">") - 1);
               model.SubmissionStartDateTime = DateTime.Parse(tr.aaData[i][2].Substring(0, tr.aaData[i][2].IndexOf("(")));
               model.SubmissionCloseDateTime = DateTime.Parse(tr.aaData[i][3].Substring(0, tr.aaData[i][3].IndexOf("(")));

               model.ContactPerson = new Contacts();
               string c = tr.aaData[i][4];
               model.ContactPerson.FIO = c.Substring(0, c.IndexOf("<"));
               model.ContactPerson.Position = c.Substring(c.IndexOf(">") + 1, c.Skip(c.IndexOf(">")).ToString().IndexOf(">"));
               model.ContactPerson.Email = c.Substring(c.IndexOf("mailto:") + 7, c.Substring(c.IndexOf("mailto")).IndexOf(">") - 7);

               model.Lots = new List<Lot>();
              
               foreach (string lotJson in lotJsons)
               {
                   TendersResponce lotResp = JsonConvert.DeserializeObject<TendersResponce>(lotJson);
                   for (int j = 0; j < lotResp.aaData.Count; j++)
                   {
                       Lot lot = new Lot();
                       lot.Name = lotResp.aaData[j][1];
                       lot.Measure = lotResp.aaData[j][3];
                       lot.Quantity = Convert.ToDouble(lotResp.aaData[j][4]);
                       model.Lots.Add(lot);
                   }
               }

               notModel.Json = JsonConvert.SerializeObject(model);
               notmodelJsons.Add(JsonConvert.SerializeObject(notModel));
           }
           return notmodelJsons;

       }
        
    }
}
