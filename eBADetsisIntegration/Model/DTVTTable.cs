using eBADetsisIntegration.DETSISService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eBADetsisIntegration.Model
{
    public class ILBILGISI
    {
        public string ILKODU { get; set; }
        public string ILADI { get; set; }
    }
    public class DTVTTable
    {
        public string ID { get; set; }
        public string PARENTID { get; set; }
        public string NAME { get; set; }
        public string TELEFON { get; set; }
        public string FAKS { get; set; }
        public string EPOSTA { get; set; }
        public string ADRES { get; set; }
        public string WEBADRES { get; set; }
        public string YERKODU { get; set; }
        public string ILCE { get; set; }
        public string ULKE { get; set; }
        public string KEP { get; set; }
        public string ANAKURUMIDARE { get; set; }
        public string HIYERARSIK { get; set; }
        public string HIYERARSIKSONUC { get; set; }
        public string USTKURUM { get; set; }
        public string PARENTIDARE { get; set; }
        public string PARENTIDARE2 { get; set; }
        public string YERID { get; set; }
        public string BIRIMTIP1 { get; set; }
        public string BIRIMTIP2 { get; set; }
        public string DISYAZISMAYAPIYOR { get; set; }
    }
}