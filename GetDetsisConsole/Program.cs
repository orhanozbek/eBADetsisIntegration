using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace GetDetsisConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string root = Path.GetDirectoryName(Path.GetDirectoryName(eBAAssemblyResolver.Resolver.CallingAssemblyDirectory));
            string pathCommon = Path.Combine(root, "Common");
            eBAAssemblyResolver.Resolver.AddPath(pathCommon);
            eBAAssemblyResolver.Resolver.AddPath(@"C:\Bimser2\eBA\Common");
            eBAAssemblyResolver.Resolver.AttachResolveEvent();
            eBAConfigurationHelper.ApplicationConfig.InstanceName= ConfigurationSettings.AppSettings["InstanceName"].ToString();

            AddToEventLog(EventLogEntryType.Information, "Detsis listesi alınıyor..");
            string serviceurl = ConfigurationSettings.AppSettings["ServiceUrl"].ToString();
            //DetsisSrv.Integration srv = new DetsisSrv.Integration();
            eBADetsisIntegration.Integration srv = new eBADetsisIntegration.Integration();
            //Console.WriteLine(srv.Url);
            //srv.Url = serviceurl;
            //srv.Timeout = 6000000;
            srv.GetDetsis();
        }
        public static void AddToEventLog(EventLogEntryType entryType, string msg)
        {
            if (!EventLog.SourceExists("DetsisIntegration"))
                EventLog.CreateEventSource("DetsisIntegration", "DetsisIntegrationLog");

            using (EventLog eventLog = new EventLog("DetsisIntegrationLog"))
            {
                eventLog.Source = "DetsisIntegrationLog";
                eventLog.WriteEntry(msg, entryType);
                eventLog.Close();
            }
        }
        /*
        public void GetDetsis()
        {
            try
            {
                DateTime DetsisTarih = DateTime.Now;
                //AddToEventLog(EventLogEntryType.Information, "GetDetsis metodu çağırıldı.");
                DateTime dt = new DateTime();
                if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["Tarih"].ToString()))
                    if (DateTime.TryParse(ConfigurationSettings.AppSettings["Tarih"].ToString(), out dt))
                    {
                        DetsisTarih = dt;
                    }
                kaysis.DETSISServis client = kaysis.DETSISServis();



                DataTable dtBirim = null;
                DataTable dtKrm = null;
                DataTable dtAll = null;
                DateTime date = DateTime.Now.AddDays(-1);
                if (DetsisTarih != null)
                {
                    if (DetsisTarih > new DateTime())
                    {
                        date = DetsisTarih;
                        //WriteLogeBA("Tarih", date.ToString());
                    }
                }
                //AddToEventLog(EventLogEntryType.Information, "Tarih" + date.ToString());

                DateTime oldDate = DateTime.Now.Date.AddDays(-1);
                string currentPath = HttpContext.Current.Server.MapPath(".") + "\\" + DateTime.Now.Date.Year + DateTime.Now.Month + DateTime.Now.Day + "-JsonData.json";
                string oldPath = HttpContext.Current.Server.MapPath(".") + "\\" + oldDate.Year + oldDate.Month + oldDate.Day + "-JsonData.json";

                //AddToEventLog(EventLogEntryType.Information, "currentPath" + currentPath);
                //AddToEventLog(EventLogEntryType.Information, "oldPath" + oldPath);
                if (File.Exists(oldPath))
                {
                    File.Delete(oldPath);
                    //AddToEventLog(EventLogEntryType.Information, "oldpath delete");
                }
                if (!File.Exists(currentPath))
                {
                    kaysis.SonuclarOOfKurumBirimWS disYazismaYapanlar = client.YeniDisYazismaYapanlariGetirWs(date.Day, date.Month, date.Year);
                    if (!string.IsNullOrEmpty(disYazismaYapanlar.HataMsj))
                    {
                        WriteLogeBA("Kurumlar Alınırken Hata Oluşutu", disYazismaYapanlar.HataMsj);
                        //AddToEventLog(EventLogEntryType.Information, "Kurumlar Alınırken Hata Oluşutu 1 " + disYazismaYapanlar.HataMsj);
                    }
                    SonuclarOOfKurumBirimWS icBirimler = client.KendiTumBirimleriGetirWs(serviceAuth, null);
                    if (!string.IsNullOrEmpty(icBirimler.HataMsj))
                    {
                        WriteLogeBA("İç Birimler Alınırken Hata Oluşutu", icBirimler.HataMsj);
                        //AddToEventLog(EventLogEntryType.Information, "İç Birimler Alınırken Hata Oluşutu" + icBirimler.HataMsj);
                    }
                    if (disYazismaYapanlar.Sonuclar != null && string.IsNullOrEmpty(disYazismaYapanlar.HataMsj))
                    {
                        KurumBirimWS[] kurum = disYazismaYapanlar.Sonuclar;
                        dtKrm = CreateDataTable(kurum);
                        dtAll = dtKrm;
                        //WriteToServer(TableName, dtKrm);
                    }
                    else
                    {
                        WriteLogeBA("Yeni Kurum Yok", "Aktarılacak Yeni Kurum Bulunamadı");
                        //AddToEventLog(EventLogEntryType.Information, "Yeni Kurum Yok" + "Aktarılacak Yeni Kurum Bulunamadı");
                    }

                    if (icBirimler.Sonuclar != null && string.IsNullOrEmpty(icBirimler.HataMsj))
                    {
                        KurumBirimWS[] birim = icBirimler.Sonuclar;
                        dtBirim = CreateDataTable(birim);
                        dtAll.Merge(dtBirim);
                        //WriteToServer(TableName, dtBirim);
                    }
                    else
                    {
                        WriteLogeBA("Yeni birim Yok", "Aktarılacak Yeni birim Bulunamadı");
                        //AddToEventLog(EventLogEntryType.Information, "Yeni birim Yok" + "Aktarılacak Yeni birim Bulunamadı");
                    }

                    string json = JsonConvert.SerializeObject(dtAll);
                    System.IO.File.WriteAllText(currentPath, json);
                }
                else
                {
                    using (StreamReader file = File.OpenText(currentPath))
                    {

                        dtAll = JsonConvert.DeserializeObject<DataTable>(File.ReadAllText(currentPath));
                    }
                }
                WriteToServer(TableName, dtAll);

            }
            catch (Exception ex)
            {
                WriteLogeBA("Kurumlar Alınırken Hata Oluşutu", ex.Message);
                //AddToEventLog(EventLogEntryType.Information, "Kurumlar Alınırken Hata Oluşutu 2 " + ex.Message);
                throw new Exception(ex.Message);
            }
        }
        */
    }
}
