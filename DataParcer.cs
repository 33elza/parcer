using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parcer.jsonModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parcer
{
    class DataParcer
    {
        public List<string> ParceLotUrl(string responce)
        {

            try
            {
                TendersResponce tr = JsonConvert.DeserializeObject<TendersResponce>(responce);

                List<String> lotUrls = new List<String>();

                for (int i = 0; i < tr.aaData.Count; i++)
                {
                    string href = tr.aaData[i][1];
                    Debug.WriteLine(href);
                    string lotUrl = href.Substring(href.IndexOf("?") + 1, href.IndexOf(">") - href.IndexOf("?") - 1);
                    Debug.WriteLine(lotUrl);
                    lotUrls.Add(lotUrl);
                   
                }

                return lotUrls; 
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                Debug.WriteLine(e.Message);
                List<String> lotUrls = new List<String>();

                List<String> tends = new List<string>();
                Regex reg = new Regex(@"<a href=/for_suppliers/.*</a>");
              //  string pattern = @"\[.*\]";
                List<string> t = new List<string>();
                t = responce.Split(',').ToList();
                foreach (string ts in t)
                {
                    foreach (Match match in reg.Matches(ts))
                    {
                        tends.Add(match.Value);
                       // Debug.WriteLine(match.Value);
                    }
                }
                foreach (string tend in tends)
                {
                    string lotUrl = tend.Substring(tend.IndexOf("?") + 1, tend.IndexOf(">") - tend.IndexOf("?") - 1);
                   // Debug.WriteLine(lotUrl);
                    lotUrls.Add(lotUrl);
                }
                return lotUrls;
            }
                     
        }

        public string RepMatch(Match match)
        {
            string str = match.ToString();
            str = str.Substring(0, str.LastIndexOf('"'));
            str = str.Insert(str.Length - 1, "\\");
            str = "\\" + str;
            Debug.WriteLine("====" + str);

            return str;

        }
        public List<string> ParceTenders(string tenderResponce, List<string> lotJsons, OrgResponce orgResp)
        {
            List<string> notmodelJsons = new List<string>();
            try
            {
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
                        try
                        {
                            TendersResponce lotResp = JsonConvert.DeserializeObject<TendersResponce>(lotJson);
                            for (int j = 0; j < lotResp.aaData.Count; j++)
                            {
                                Lot lot = new Lot();
                                lot.Name = lotResp.aaData[j][1];
                                lot.Measure = lotResp.aaData[j][3];
                                lot.Quantity = Convert.ToDouble(lotResp.aaData[j][4], CultureInfo.GetCultureInfo("en-US"));
                                model.Lots.Add(lot);
                            }
                        }
                        catch (Newtonsoft.Json.JsonReaderException e)
                        {
                            Debug.WriteLine(e.Data);
                            Debug.WriteLine(e.LineNumber);
                            Debug.WriteLine(e.LinePosition);
                            Debug.WriteLine(e.Message);
                            Debug.WriteLine(e.Path);
                          
                            Regex reg = new Regex("(\")\\w+\\W(\")");
                            string pattern = "(\")\\w+\\W(\")";
                            string slot = lotJson;
                            foreach (Match match in reg.Matches(lotJson))
                            {
                                Debug.WriteLine("!!!!" + match);
                                MatchEvaluator evaluator = new MatchEvaluator(RepMatch);
                                slot = Regex.Replace(lotJson, pattern, evaluator);
                                Debug.WriteLine("++++" + slot);
                            }

                            TendersResponce lotResp = JsonConvert.DeserializeObject<TendersResponce>(slot);
                            for (int j = 0; j < lotResp.aaData.Count; j++)
                            {
                                Lot lot = new Lot();
                                lot.Name = lotResp.aaData[j][1];
                                lot.Measure = lotResp.aaData[j][3];
                                lot.Quantity = Convert.ToDouble(lotResp.aaData[j][4], CultureInfo.GetCultureInfo("en-US"));
                                model.Lots.Add(lot);
                            }                  
                        }
                    }

                    notModel.Json = JsonConvert.SerializeObject(model);
                    notmodelJsons.Add(JsonConvert.SerializeObject(notModel));
                }
                return notmodelJsons;
            }
            catch (JsonReaderException e)
            {
               Debug.WriteLine(e.Message);

              // List<string> tres = new List<string>();
              // Regex regex = new Regex("([.*])"); 
              //// tres = tenderResponce.Split(',').ToList();
              // tres = regex.Split(tenderResponce).ToList();
              //  foreach (string tr in tres)
              //  {
              //      Debug.WriteLine(tr);
              //  }
                    return notmodelJsons;
            }
           

       }
        
    }
}
