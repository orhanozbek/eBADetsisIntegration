using eBADB;
using eBADetsisIntegration.DETSISService;
using eBADetsisIntegration.Model;
using eBALogAPIHelper.Helper;
using eBAPI.Connection;
using eBAPI.DocumentManagement;
using eBAPI.Workflow;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Xml.Serialization;

namespace eBADetsisIntegration
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Integration : System.Web.Services.WebService
    {
        #region Configuration
        public string KRMID { get; set; }
        public string KRMPASS { get; set; }
        static string KurumID = string.Empty;
        static string KurumSifre = string.Empty;
        static string TableName { get; set; }
        static DateTime DetsisTarih { get; set; }
        public string LogPath { get; set; }
        static string ConnectionString { get; set; }
        static string ServiceType { get; set; }
        static string Provider { get; set; }
        static string ServiceAddress { get; set; }
        public Integration(string KrmId, string KrmPass)
        {
            KurumID = KrmId;
            KurumSifre = KrmPass;
        }
        public string currentPath = string.Empty;
        public Integration()
        {
            //Debugger.Launch();
            KurumID = ConfigurationSettings.AppSettings["KurumID"].ToString();//string.Empty;
            KurumSifre = ConfigurationSettings.AppSettings["KurumSifre"].ToString();//string.Empty;
            TableName = ConfigurationSettings.AppSettings["TableName"].ToString();
            LogPath = ConfigurationSettings.AppSettings["LogPath"].ToString();
            ConnectionString = ConfigurationSettings.AppSettings["ConnectionString"].ToString();
            Provider = ConfigurationSettings.AppSettings["Provider"].ToString();
            ServiceType = ConfigurationSettings.AppSettings["ServiceType"].ToString();
            ServiceAddress = ConfigurationSettings.AppSettings["ServiceAddress"].ToString();
            DateTime dt = new DateTime();
            if (DateTime.TryParse(ConfigurationSettings.AppSettings["Tarih"].ToString(), out dt))
            {
                DetsisTarih = dt;
            }
        }

        public BBDETSISWSClient getWSClient()
        {
            try
            {
                BasicHttpBinding basicHttpBinding = new BasicHttpBinding();
                basicHttpBinding.Name = "BBDETSISWS";
                basicHttpBinding.MaxBufferSize = int.MaxValue;
                basicHttpBinding.ReceiveTimeout = new TimeSpan(0, 30, 0);
                basicHttpBinding.SendTimeout = new TimeSpan(0, 30, 0);
                basicHttpBinding.OpenTimeout = new TimeSpan(0, 30, 0);
                basicHttpBinding.CloseTimeout = new TimeSpan(0, 30, 0);
                basicHttpBinding.MaxReceivedMessageSize = (long)int.MaxValue;
                basicHttpBinding.MaxBufferPoolSize = 524288L;
                basicHttpBinding.MessageEncoding = WSMessageEncoding.Text;
                basicHttpBinding.TextEncoding = Encoding.UTF8;

                basicHttpBinding.Security.Mode = BasicHttpSecurityMode.Transport;
                //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 |
                                                       SecurityProtocolType.Tls | SecurityProtocolType.Tls11;

                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                if(ServiceType=="Kamu")
                {
                    ServiceAddress= "https://bbws.kaysis.gov.tr/DETSISServis.asmx";
                }

                EndpointAddress remoteAddress = new EndpointAddress(ServiceAddress);

                BBDETSISWSClient bbdetsiswsClient = new BBDETSISWSClient((Binding)basicHttpBinding, remoteAddress);
                return bbdetsiswsClient;
            }
            catch (Exception ex)
            {
                WriteLogeBA("Servis bağlantısı kurulamadı", ex.Message);
                return null;
            }

        }
        public BbServiceAuthentication serviceAuthentication()
        {
            //Configuration dllConfig = ConfigurationManager.OpenExeConfiguration(System.Reflection.Assembly.GetExecutingAssembly().Location);
            //AppSettingsSection conf = (AppSettingsSection)dllConfig.GetSection("appSettings");

            if (string.IsNullOrWhiteSpace(KurumID) && string.IsNullOrWhiteSpace(KurumID))
            {
                KurumID = ConfigurationSettings.AppSettings["KurumID"].ToString();//conf.Settings["KurumID"].Value;
                KurumSifre = ConfigurationSettings.AppSettings["KurumSifre"].ToString();//conf.Settings["KurumSifre"].Value;
            }
            BbServiceAuthentication BbServiceAuthentication = new BbServiceAuthentication();
            BbServiceAuthentication.KurumID = KurumID;
            BbServiceAuthentication.Password = KurumSifre;



            return BbServiceAuthentication;
        }


        public eBAConnection CreateServerConnection()
        {
            return ebanet.ApplicationServerTier.CreateConnection();
        }



        public DataTable ConvertArrayToDataTableXML(object[] inArray)
        {
            System.Data.DataTable dt = new System.Data.DataTable("Tablo1");

            if (inArray != null)
            {
                Type type = inArray.GetType();
                XmlSerializer serializer = new XmlSerializer(type);
                System.IO.StringWriter sw = new System.IO.StringWriter();
                serializer.Serialize(sw, inArray);
                System.Data.DataSet ds = new System.Data.DataSet();

                System.IO.StringReader reader = new System.IO.StringReader(sw.ToString());
                ds.ReadXml(reader);
                if (ds.Tables.Count > 0)
                {
                    dt = ds.Tables[0];
                }
                else
                {
                    dt = ToDataTable<DTVTTable>(new DTVTTable { });
                }
            }
            else
            {
                throw new Exception("Kayıt Bulunamadı");
            }
            return dt;
        }

        public DataTable ConvertArrayToDataTableXMLYerKodlari(object[] inArray)
        {
            System.Data.DataTable dt = new System.Data.DataTable("Tablo1");
            if (inArray != null)
            {
                Type type = inArray.GetType();
                XmlSerializer serializer = new XmlSerializer(type);
                System.IO.StringWriter sw = new System.IO.StringWriter();
                serializer.Serialize(sw, inArray);
                System.Data.DataSet ds = new System.Data.DataSet();

                System.IO.StringReader reader = new System.IO.StringReader(sw.ToString());
                ds.ReadXml(reader);
                if (ds.Tables.Count > 0)
                {
                    dt = ds.Tables[0];
                }
                else
                {
                    dt = ToDataTable<ResYerKodlari>(new ResYerKodlari { });
                }
            }
            else
            {
                dt = ToDataTable<ResYerKodlari>(new ResYerKodlari { });
            }
            return dt;
        }
        private DataTable ToDataTable<T>(T entity) where T : class
        {
            var properties = typeof(T).GetProperties();
            var table = new DataTable("Tablo1");

            foreach (var property in properties)
            {
                table.Columns.Add(property.Name, property.PropertyType);
            }
            return table;
        }

        public string KurumKepAdresi(int[] kurumid)
        {
            BBDETSISWSClient client = getWSClient();
            BbServiceAuthentication serviceAuth = serviceAuthentication();

            //DataTable dt = new DataTable();
            KepBilgileriWs[] keps = client.KEPAdresleriniGetir(serviceAuth, kurumid).Sonuclar;
            string kep = keps.FirstOrDefault().KepAdresi;
            return kep;
        }

        public static void WriteToServer(string qualifiedTableName, DataTable dataTable)
        {
            string connectionString = (!string.IsNullOrEmpty(ConnectionString) ? ConnectionString : eBAConfigurationHelper.ApplicationConfig.CreateDatabaseConnection().Connection.ConnectionString);
            ServerType serverType = eBAConfigurationHelper.ApplicationConfig.CreateDatabaseConnection().ServerType;
            if (dataTable != null)
            {
                try
                {
                    if (serverType == ServerType.Oracle)
                    {

                        WriteToOracle(qualifiedTableName, dataTable, connectionString);

                    }
                    else
                    {
                        SqlConnection sqlConnection = new SqlConnection(connectionString);

                        sqlConnection.Open();
                        using (SqlBulkCopy bulkCopy = new SqlBulkCopy(sqlConnection))
                        {
                            bulkCopy.DestinationTableName = qualifiedTableName;
                            bulkCopy.WriteToServer(dataTable);
                        }
                        sqlConnection.Close();
                        sqlConnection.Dispose();
                    }
                }
                catch (Exception ex)
                {

                    WriteLogeBA("Kurumlar Aktarılırken Hata Oluştu", ex.Message);
                    throw ex;
                }
            }
        }

        private static void WriteToOracle(string qualifiedTableName, DataTable dataTable, string connectionString)
        {
            if (Provider == "DataAccess")
            {
                Oracle.DataAccess.Client.OracleConnection oracleConnection = new Oracle.DataAccess.Client.OracleConnection(connectionString);
                oracleConnection.Open();
                using (Oracle.DataAccess.Client.OracleBulkCopy bulkCopy = new Oracle.DataAccess.Client.OracleBulkCopy(oracleConnection))
                {
                    bulkCopy.DestinationTableName = qualifiedTableName;
                    bulkCopy.WriteToServer(dataTable);
                }
                oracleConnection.Close();
                oracleConnection.Dispose();
            }
            else
            {
                Oracle.DataAccess.Client.OracleConnection oracleConnection = new Oracle.DataAccess.Client.OracleConnection(connectionString);
                oracleConnection.Open();


                using (Oracle.DataAccess.Client.OracleBulkCopy bulkCopy = new Oracle.DataAccess.Client.OracleBulkCopy(oracleConnection))
                {
                    bulkCopy.DestinationTableName = qualifiedTableName;
                    bulkCopy.WriteToServer(dataTable);
                }
                oracleConnection.Close();
                oracleConnection.Dispose();
            }
        }
        private static string Serialize(object dataToSerialize)
        {
            if (dataToSerialize == null) return null;

            using (StringWriter stringwriter = new System.IO.StringWriter())
            {
                var serializer = new XmlSerializer(dataToSerialize.GetType());
                serializer.Serialize(stringwriter, dataToSerialize);
                return stringwriter.ToString();
            }
        }

        public DataTable KEPAdressList()
        {
            BBDETSISWSClient client = getWSClient();
            BbServiceAuthentication serviceAuth = serviceAuthentication();
            //DataTable dt = new DataTable();
            KepBilgileriWs[] keps = client.AktifKEPAdresleriniGetir(serviceAuth).Sonuclar;
            //KepBilgileriWs[] keps =  client.KEPAdresleriniGetir(serviceAuth, detsisId).Sonuclar;
            DataTable dt = CreateDataTable(keps);


            return dt;
        }

        public string TebligatNumarasiGetir(string ID)
        {
            string UETSNo = string.Empty;
            BBDETSISWSClient client = getWSClient();
            BbServiceAuthentication serviceAuth = serviceAuthentication();
            //DataTable dt = new DataTable();
            TebligatNumarasiWs[] UETS = client.TebligatNumarasiGetir(serviceAuth, Convert.ToInt32(ID)).Sonuclar;

            if (UETS.Count() > 0)
            {
                UETSNo = UETS[0].TebligatNo;
            }

            return UETSNo;

        }

        public DataTable CreateDataTable<T>(IEnumerable<T> list)
        {
            DataTable dataTable = new DataTable("Tablo1");
            if (list != null)
            {
                Type type = typeof(T);
                var properties = type.GetProperties();


                foreach (PropertyInfo info in properties)
                {
                    dataTable.Columns.Add(new DataColumn(info.Name, Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType));
                }

                foreach (T entity in list)
                {
                    if (entity != null)
                    {
                        object[] values = new object[properties.Length];
                        for (int i = 0; i < properties.Length; i++)
                        {
                            values[i] = properties[i].GetValue(entity);
                        }

                        dataTable.Rows.Add(values);
                    }
                }
            }

            return dataTable;
        }

        static void WriteLogeBA(string caption, string description)
        {
            eBALogAPI logApi = new eBALogAPI("Detsis", eBAConfigurationHelper.ApplicationConfig.InstanceName);
            //logApi.AddLogAsync(caption, description, ex == null ? eBALogType.None : eBALogType.Error, "", ex);
            logApi.AddLogAsync(caption, description);
        }

        public void WriteLog(string logMessage)
        {
            if (!Directory.Exists(LogPath))
                Directory.CreateDirectory(LogPath);

            StreamWriter writer = new StreamWriter(Path.Combine(LogPath,
                string.Format("{0}{1}{2}.txt", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)), true);
            writer.WriteLine(logMessage + " - " + DateTime.Now);
            writer.Close();

        }
        public DataTable CreateDataTable(KurumBirimWS[] kurumBirimWs)
        {
            DataTable kurumDt = new DataTable("Kurum");
            DataView kepDT = KEPAdressList().DefaultView;
            List<DTVTTable> dtvt = new List<DTVTTable>();
            string eposta = string.Empty;
            string faks = string.Empty;
            string telefon = string.Empty;
            string kep = string.Empty;
            string il = string.Empty;
            string ilce = string.Empty;
            string ulke = string.Empty;
            string webAdres = string.Empty;
            foreach (KurumBirimWS item in kurumBirimWs)
            {
                if (kepDT.Table.Rows.Count > 0)
                {
                    kepDT.RowFilter = "KurumDetsisNo='" + item.DETSISNo + "'";
                    if (kepDT.ToTable().Rows.Count > 0)
                    {
                        kep = kepDT.ToTable().Rows[0]["KepAdresi"].ToString();
                    }
                }
                if (item != null)
                {
                    dtvt.Add(new DTVTTable
                    {
                        ID = item.DETSISNo.ToString(),
                        PARENTID = item.ParentIdareKimlikKodu.ToString(),
                        NAME = item.Ad,
                        TELEFON = telefon,
                        FAKS = faks,
                        EPOSTA = eposta,
                        ADRES = (!string.IsNullOrEmpty(item.Adres) ? item.Adres : ""),
                        WEBADRES = webAdres,
                        YERKODU = item.Fk_BulunduguYerID.Value.ToString(),
                        ILCE = ilce,
                        ULKE = ulke,
                        KEP = kep,
                        ANAKURUMIDARE = item.AnaKurumIdareKimlikKodu.ToString(),
                        HIYERARSIK = item.KurumHiyerarsik,
                        HIYERARSIKSONUC = string.Join(System.Environment.NewLine, item.KurumHiyerarsik.Split('>')),
                        USTKURUM = string.Empty,
                        PARENTIDARE = item.ParentIdareKimlikKodu.ToString(),
                        PARENTIDARE2 = item.ParentIdareKimlikKodu2.ToString(),
                        YERID = item.Fk_BulunduguYerID.Value.ToString(),
                        BIRIMTIP1 = item.Fk_KurumBirimTipID1.ToString(),
                        BIRIMTIP2 = item.Fk_KurumBirimTipID2.ToString(),
                        DISYAZISMAYAPIYOR = (item.DisYazismaMuhatabiOlupOlmadigi) ? "1" : "0"
                    });
                }
            }
            kurumDt = ConvertArrayToDataTableXML(dtvt.ToArray());
            return kurumDt;
        }

        #endregion
        #region WebMethod


        [WebMethod]
        public void GetDetsis()
        {
            try
            {
                //AddToEventLog(EventLogEntryType.Information, "GetDetsis metodu çağırıldı.");
                DateTime dt = new DateTime();
                if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["Tarih"].ToString()))
                    if (DateTime.TryParse(ConfigurationSettings.AppSettings["Tarih"].ToString(), out dt))
                    {
                        DetsisTarih = dt;
                    }
                BBDETSISWSClient client = getWSClient();


                BbServiceAuthentication serviceAuth = serviceAuthentication();

                DataTable dtBirim = null;
                DataTable dtKrm = null;
                DataTable dtAll = null;
                DateTime date = DateTime.Now.AddDays(-1);
                if (DetsisTarih != null)
                {
                    if (DetsisTarih > new DateTime())
                    {
                        date = DetsisTarih;
                        WriteLogeBA("Tarih", date.ToString());
                    }
                }
                //AddToEventLog(EventLogEntryType.Information, "Tarih" + date.ToString());

                string apppath = "";
                try
                {
                    apppath = HttpContext.Current.Server.MapPath(".");
                }
                catch
                {
                    apppath = Path.GetDirectoryName(eBAAssemblyResolver.Resolver.CallingAssemblyDirectory);
                }
                DateTime oldDate = DateTime.Now.Date.AddDays(-1);
                currentPath = apppath + "\\" + DateTime.Now.Date.Year + DateTime.Now.Month + DateTime.Now.Day + "-JsonData.json";
                string oldPath = apppath + "\\" + oldDate.Year + oldDate.Month + oldDate.Day + "-JsonData.json";

                //AddToEventLog(EventLogEntryType.Information, "currentPath" + currentPath);
                //AddToEventLog(EventLogEntryType.Information, "oldPath" + oldPath);
                if (File.Exists(oldPath))
                {
                    File.Delete(oldPath);
                    //AddToEventLog(EventLogEntryType.Information, "oldpath delete");
                }
                if (!File.Exists(currentPath))
                {
                    SonuclarOOfKurumBirimWS disYazismaYapanlar = client.YeniDisYazismaYapanlariGetirWs(serviceAuth, date.Day, date.Month, date.Year);
                    if (!string.IsNullOrEmpty(disYazismaYapanlar.HataMsj))
                    {
                        WriteLogeBA("Kurumlar Alınırken Hata Oluşutu", disYazismaYapanlar.HataMsj);
                        //AddToEventLog(EventLogEntryType.Information, "Kurumlar Alınırken Hata Oluşutu 1 " + disYazismaYapanlar.HataMsj);
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

                    if (ServiceType == "Kamu")
                    {
                        SonuclarOOfKurumBirimWS icBirimler = client.KendiTumBirimleriGetirWs(serviceAuth, null);
                        if (!string.IsNullOrEmpty(icBirimler.HataMsj))
                        {
                            WriteLogeBA("İç Birimler Alınırken Hata Oluşutu", icBirimler.HataMsj);
                            //AddToEventLog(EventLogEntryType.Information, "İç Birimler Alınırken Hata Oluşutu" + icBirimler.HataMsj);
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


        [WebMethod]
        public void YerBilgisi()
        {
            BBDETSISWSClient client = getWSClient();
            BbServiceAuthentication serviceAuth = serviceAuthentication();
            //return client.TumYerKodlariGetirWs(serviceAuth).Sonuclar.Where(a => a.ID== ilId).Select(a=>new ILBILGISI {ILADI=a.IlAdi,ILKODU=a.IlKodu.ToString()+"-"+a.ID+"-"+a.Fk_YerTipiID }).FirstOrDefault();
            ResYerKodlari[] yerKodlari = client.TumYerKodlariGetirWs(serviceAuth).Sonuclar;
            DataTable dt = ConvertArrayToDataTableXMLYerKodlari(yerKodlari);
            WriteToServer("DTVTYERKODLARI", dt);



        }
        [WebMethod]
        public DataTable eBADETSISNoKurumBirimWs(int kurumId)
        {
            BBDETSISWSClient client = getWSClient();
            BbServiceAuthentication serviceAuth = serviceAuthentication();
            DataView kepDT = null;
            List<DTVTTable> dtvt = new List<DTVTTable>();
            string eposta = string.Empty;
            string faks = string.Empty;
            string telefon = string.Empty;
            string kep = string.Empty;
            string il = string.Empty;
            string ilce = string.Empty;
            string ulke = string.Empty;
            string webAdres = string.Empty;
            try
            {
                kepDT = KEPAdressList().DefaultView;
            }
            catch (Exception ex)
            {
                WriteLogeBA("KEP Adresleri Alınamadı", ex.Message);
            }
            try
            {
                client.Open();
                SonucO sonuc = client.DETSISNoKurumBirimWs(serviceAuth, kurumId);
                WriteLogeBA("Aranan Kurum Detsis No: ", kurumId.ToString());
                if (!sonuc.SonucHatali)
                {
                    foreach (var item in sonuc.Sonuclar)
                        dtvt.Add(new DTVTTable
                        {
                            ID = item.DETSISNo.ToString(),
                            PARENTID = item.ParentIdareKimlikKodu.ToString(),
                            NAME = item.Ad,
                            TELEFON = telefon,
                            FAKS = faks,
                            EPOSTA = eposta,
                            ADRES = (!string.IsNullOrEmpty(item.Adres) ? item.Adres : ""),
                            WEBADRES = webAdres,
                            YERKODU = item.Fk_BulunduguYerID.Value.ToString(),
                            ILCE = ilce,
                            ULKE = ulke,
                            KEP = kep,
                            ANAKURUMIDARE = item.AnaKurumIdareKimlikKodu.ToString(),
                            HIYERARSIK = item.KurumHiyerarsik,
                            HIYERARSIKSONUC = string.Join(System.Environment.NewLine, item.KurumHiyerarsik.Split('>')),
                            USTKURUM = string.Empty,
                            PARENTIDARE = item.ParentIdareKimlikKodu.ToString(),
                            PARENTIDARE2 = item.ParentIdareKimlikKodu2.ToString(),
                            YERID = item.Fk_BulunduguYerID.Value.ToString(),
                            BIRIMTIP1 = item.Fk_KurumBirimTipID1.ToString(),
                            BIRIMTIP2 = item.Fk_KurumBirimTipID2.ToString()
                        });
                }
                else
                {
                    WriteLogeBA("Detsis", sonuc.HataMsj);
                }
                return ConvertArrayToDataTableXML(dtvt.ToArray());
            }
            finally
            {
                client.Close();
            }
        }

        [WebMethod]
        /// <param name="Ara">Kurum Adı veya Detsis Numarası.</param>
        /// <param name="UstBirimKodu">Üst Birim Kodu biliniyor ise yazılmalı.</param>
        /// merhabaa semih bey
        public DataTable AdaGoreKurumBirimSorgula(string Ara, string UstBirimKodu = "0")
        {
            BBDETSISWSClient client = getWSClient();

            BbServiceAuthentication serviceAuth = serviceAuthentication();
            DataTable kurumDt = new DataTable("Kurum");
            KurumBirimWS[] kurum = null;
            SonucO sonuc;
            DataView kepDT = null;
            List<DTVTTable> dtvt = new List<DTVTTable>();
            try
            {
                kepDT = KEPAdressList().DefaultView;
            }
            catch (Exception ex)
            {
                WriteLogeBA("KEP Adresleri Alınamadı", ex.Message);
            }

            try
            {
                client.Open();
                //if (client.State == CommunicationState.Opened)
                //{


                int kurumid = 0;
                int UKurumId = 0;

                try
                {
                    if (int.TryParse(Ara, out kurumid))
                    {

                        kurum = client.DETSISNoKurumBirimWs(serviceAuth, kurumid).Sonuclar;
                        WriteLogeBA("Aranan Kurum Detsis No: ", Ara);
                    }
                    else
                    {
                        kurum = client.AdaGoreKurumBirimSorgulaWs(serviceAuth, Ara, UKurumId).Sonuclar;
                        WriteLogeBA("Aranan Kurum: ", Ara);

                    }
                }
                catch (Exception ex)
                {
                    WriteLogeBA("Detsis Servisinden veri alanımadı.", ex.Message);
                }

                //DataTable yerBilgileri = getYerBilgileri();

                string eposta = string.Empty;
                string faks = string.Empty;
                string telefon = string.Empty;
                string kep = string.Empty;
                string il = string.Empty;
                string ilce = string.Empty;
                string ulke = string.Empty;
                string webAdres = string.Empty;
                //DataView dw = yerBilgileri.DefaultView;
                WriteLogeBA("Kurum", kurum.ToString());
                if (kurum != null)
                {
                    WriteLogeBA("Bulunan Kurum Sayısı: ", kurum.Count().ToString());

                    foreach (KurumBirimWS item in kurum)
                    {

                        if (item != null)
                        {
                            if (kepDT != null)
                            {
                                kepDT.RowFilter = "KurumDetsisNo='" + item.DETSISNo + "'";
                                if (kepDT.ToTable().Rows.Count > 0)
                                {
                                    kep = kepDT.ToTable().Rows[0]["KepAdresi"].ToString();
                                }

                            }

                            dtvt.Add(new DTVTTable
                            {
                                ID = item.DETSISNo.ToString(),
                                PARENTID = item.ParentIdareKimlikKodu.ToString(),
                                NAME = item.Ad,
                                TELEFON = telefon,
                                FAKS = faks,
                                EPOSTA = eposta,
                                ADRES = (!string.IsNullOrEmpty(item.Adres) ? item.Adres : ""),
                                WEBADRES = webAdres,
                                YERKODU = item.Fk_BulunduguYerID.Value.ToString(),
                                ILCE = ilce,
                                ULKE = ulke,
                                KEP = kep,
                                ANAKURUMIDARE = item.AnaKurumIdareKimlikKodu.ToString(),
                                HIYERARSIK = item.KurumHiyerarsik,
                                HIYERARSIKSONUC = string.Join(System.Environment.NewLine, item.KurumHiyerarsik.Split('>')),
                                USTKURUM = string.Empty,
                                PARENTIDARE = item.ParentIdareKimlikKodu.ToString(),
                                PARENTIDARE2 = item.ParentIdareKimlikKodu2.ToString(),
                                YERID = item.Fk_BulunduguYerID.Value.ToString(),
                                BIRIMTIP1 = item.Fk_KurumBirimTipID1.ToString(),
                                BIRIMTIP2 = item.Fk_KurumBirimTipID2.ToString()
                            });
                        }
                        else
                        {
                            WriteLogeBA("Hata Kurum Bilgileri Alınamadı", (int.TryParse(Ara, out kurumid)) ? "Kurum Detsis No : " + Ara : "Kurum Adı : " + Ara);
                        }
                    }

                }
                kurumDt = ConvertArrayToDataTableXML(dtvt.ToArray());


                // }


            }
            catch (Exception ex)
            {
                WriteLogeBA("Kurum Bilgileri Alınamadı. Hata : ", ex.Message);
            }
            finally
            {
                client.Close();
            }

            return kurumDt;

        }

        [WebMethod]
        public DataTable KurumIletisimBilgileri(string detsisNo)
        {
            BBDETSISWSClient client = getWSClient();
            BbServiceAuthentication serviceAuth = serviceAuthentication();
            DataTable dt = new DataTable("Iletisim");
            dt.Columns.Add("WEBADR");
            dt.Columns.Add("EPOSTA");
            dt.Columns.Add("TELEFON");
            dt.Columns.Add("FAKS");
            int dtNo = 0;
            try
            {
                if (int.TryParse(detsisNo, out dtNo))
                {
                    client.Open();
                    SonucIletisimBilgileri iletisim = client.KurumBirimIletisimBilgileriGetirWs(serviceAuth, dtNo);
                    if (!iletisim.SonucHatali)
                    {
                        DataRow dr = dt.NewRow();
                        foreach (IletisimBilgileriWS item in iletisim.Sonuclar)
                        {

                            switch (item.IletisimTip)
                            {
                                case 1:
                                    dr["WEBADR"] = item.Iletisim;
                                    break;
                                case 4:
                                    dr["TELEFON"] = item.Iletisim;
                                    break;
                                case 5:
                                    dr["FAKS"] = item.Iletisim;
                                    break;
                                case 2:
                                    dr["EPOSTA"] = item.Iletisim;
                                    break;
                            }

                        }
                        dt.Rows.Add(dr);
                    }
                    else
                    {
                        try
                        {
                            AddToEventLog(EventLogEntryType.Error, $"{detsisNo} numaralı kaydın iletişim bilgileri çekilirken hata oluştu. Hata detayı : " + iletisim.HataMsj);
                        }
                        catch { }
                    }
                }

            }
            finally
            {
                client.Close();
            }
            return dt;
        }


        //Kurum Hiyerarşisi için
        //Kadir İçin
        [WebMethod]
        public DataTable HiyerarsiGetirKurumBirim(string DetsisNo)
        {
            BBDETSISWSClient client = getWSClient();
            BbServiceAuthentication serviceAuth = serviceAuthentication();
            string hiyerarsi = string.Empty;
            DataTable dataTable = new DataTable("BHIYERARSI");
            dataTable.Columns.Add("HIYERARSI");
            dataTable.Columns.Add("HATA");
            DataRow dr = dataTable.NewRow();
            KurumBirimWS[] kurum;
            try
            {
                int detsisno = 0;
                if (int.TryParse(DetsisNo, out detsisno))
                {
                    kurum = client.HiyerarsiGetirKurumBirimWs(serviceAuth, detsisno).Sonuclar;
                    dr["HIYERARSI"] = kurum.FirstOrDefault().KurumHiyerarsik;
                    dataTable.Rows.Add(dr);
                }

            }
            catch (Exception ex)
            {
                dr["HATA"] = ex.Message;
            }
            return dataTable;
        }

        [WebMethod]
        public DataTable KurumTebligatNumarasi(string detsisNo)
        {
            BBDETSISWSClient client = getWSClient();
            BbServiceAuthentication serviceAuth = serviceAuthentication();
            DataTable dt = new DataTable("Tebligat");
            dt.Columns.Add("TEBLIGATNO");
            int dtNo = 0;
            try
            {
                if (int.TryParse(detsisNo, out dtNo))
                {
                    client.Open();
                    SonuclarOOfTebligatNumarasiWs tebno = client.TebligatNumarasiGetir(serviceAuth, dtNo);
                    if (!tebno.SonucHatali)
                    {
                        DataRow dr = dt.NewRow();
                        foreach (TebligatNumarasiWs item in tebno.Sonuclar)
                        {

                            dr["TEBLIGATNO"] = item.TebligatNo;
                        }
                        dt.Rows.Add(dr);
                    }
                    else
                    {
                        try
                        {
                            AddToEventLog(EventLogEntryType.Error, $"{detsisNo} numaralı kaydın tebligat numarası bilgileri çekilirken hata oluştu. Hata detayı : " + tebno.HataMsj);
                        }
                        catch { }
                    }
                }

            }
            finally
            {
                client.Close();
            }
            return dt;
        }


        #endregion

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
    }
}