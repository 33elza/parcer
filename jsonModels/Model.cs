using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parcer.jsonModels
{
    class Model
    {
        public string Id { get; set; }
        public string Note { get; set; }
        public DateTime? SubmissionStartDateTime { get; set; }
        public DateTime? SubmissionCloseDateTime { get; set; }
        public string CurrencyName { get; set; }
        public string TermsOfPayment { get; set; }
        public string DeliveryConditions { get; set; }
        public string EnterpriseInitiator { get; set; }
        public double? AmountLot { get; set; }
        public List<Lot> Lots { get; set; }
        public List<Attachment> Attachments { get; set; }
        public Contacts ContactPerson { get; set; }
    }

}
