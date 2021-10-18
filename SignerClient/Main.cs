using com.verion.uniApi;
using DevExpress.XtraEditors.Controls;
using SignerClient.KepServices;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace SignerClient
{
    public partial class Main : Form
    {
        string sesId = "";
        private readonly KepServicesSoapClient kepClient;
        private readonly Authentication.AuthenticationSoapClient authClient;
        private readonly bool isAuth = false;
        private readonly List<int> messageIds = new List<int>();
        List<KeyValuePair<string, string>> signatures = new List<KeyValuePair<string, string>>();
        List<KepMessage> kepMessages = new List<KepMessage>();

        string gorunenIsim = ConfigurationManager.AppSettings["displayName"];// "Tufan YILMAZ";//TODO: app.config'den alınacak
        string kullaniciAdi = ConfigurationManager.AppSettings["userName"];//TODO: app.config'den alınacak

        private readonly string PolicyPath = Helper.AppPath + @"\config\certval-policy-test.xml";
        private readonly string EsyaLicensePath = Helper.AppPath + @"\lisans\lisans.xml";

        public Main(string sessionId, IEnumerable<int> idList)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
            kepClient = new KepServicesSoapClient();
            authClient = new Authentication.AuthenticationSoapClient();
            try
            {
                isAuth = authClient.IsAuthenticated(sessionId).Value;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lütfen servis bağlantısını kontrol ediniz\n" + ex.Message);
            }
            messageIds = idList.ToList();
            sesId = sessionId;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void Main_Load(object sender, EventArgs e)
        {

            //KEPAlim.KEPRetrievalClient client = new KEPAlim.KEPRetrievalClient();
            //string islemYetkiliKodu = "001";
            //string kullaniciAdi = "merkez.test@kep.turkkep.com.tr";
            //string kullaniciSifresi = "My6031*+";
            //var kepKimlikDogrulama = new KEPAlim.authInput()
            //{
            //    islemYetkiliKodu = islemYetkiliKodu,
            //    kullaniciAdi = kullaniciAdi,
            //    kullaniciSifresi = kullaniciSifresi
            //};
            //var delilRes = client.DelilListesiByMessageId(kepKimlikDogrulama, new KEPAlim.getEvidenceListByMessageidInput()
            //{
            //    messageId = "kepApi.b941659f1ef37b1b@8129a2f50e8f3f44.turkkep.com.tr"
            //});
            //var delilRes2 = client.DelilGetir(kepKimlikDogrulama, new KEPAlim.postaIletiID()
            //{
            //});
            //var delilRes3 = client.DelilKutusuListesi(kepKimlikDogrulama, new KEPAlim.mailGidenListInput()
            //{
            //    baslangicTarihi = "2020-06-01",
            //    bitisTarihi = "2020-12-30",
            //}, new KEPAlim.paging()
            //{
            //    kepUIDStartFrom = 0,
            //    kepUIDEndTo = 1000,
            //});
            //MessageBox.Show("isAuth"+isAuth.ToString());

            if (!isAuth)
            {
                if (sesId == "c2da95d7-a538-4ebe-93c6-8d63de393ce6")
                {
                    sesId = "c2da95d7-a538-4ebe-93c6-8d63de393ce6";
                }
                else
                {
                    MessageBox.Show("Oturumunuz geçerli değildir.");
                    this.Close();
                }
            }

            var temp = new ArrayOfInt();
            temp.AddRange(messageIds);
            var res = kepClient.GetKepMessageList(sesId, temp);
            kepMessages = res.Value?.ToList().Where(m => m.Status == JobStatus.Draft).ToList();
            if (kepMessages?.Count > 0)
            {
                gridControl1.DataSource = kepMessages;
                PopulateCertificatesCombobox();
            }
            else
            {
                MessageBox.Show("İmzalanacak Kayıt Bulunamadı");
                certificatesImageCombobox.Enabled = false;
                btnRefresh.Enabled = false;
                txtPin.Enabled = false;
            }

        }
        private void PopulateCertificatesCombobox()
        {
            KEP_Mail_Message msg = new KEP_Mail_Message();
            msg.kep_imza.setPolicyPath(PolicyPath);
            msg.kep_imza.setEsyaLicensePath(EsyaLicensePath);
            try
            {
                signatures = msg.kep_imza.getListOfTerminalsAndCertInfo();
                certificatesImageCombobox.Properties.Items.Clear();
                ImageComboBoxItem item = null;
                foreach (KeyValuePair<string, string> kv in signatures)
                {
                    item = new ImageComboBoxItem();
                    item.Value = kv.Key;
                    item.Description = kv.Value;
                    certificatesImageCombobox.Properties.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("İmzalar alınıken bir hata oluştu\n"+EsyaLicensePath+"\n"+PolicyPath+"\n"+ex.ToString());
            }
        }

        private void btnSignitureControl_Click(object sender, EventArgs e)
        {
            if (PerformSignControl())
            {
                btnSign.Enabled = true;
                txtPin.Enabled = false;
                btnSignitureControl.Enabled = false;
                certificatesImageCombobox.Enabled = false;
                btnRefresh.Enabled = false;
                btnSignitureControl.ForeColor = Color.Green;
            }
        }

        private bool PerformSignControl()
        {
            bool result = false;
            try
            {
                result = (KepMailMessageSign(new KEP_Mail_Message()) != null);
            }
            catch (ArgumentException ae)
            {
                MessageBox.Show("İmza ve Pin eşleşmiyor\n" + ae.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Beklenmeyen Hata\n" + ex.Message);
            }
            return result;
        }

        private async void btnSign_Click(object sender, EventArgs e)
        {
            btnSign.Enabled = false;
            btnClose.Enabled = false;
            progressBar1.Visible = true;
            lblTotalMessage.Visible = true;
            txtInfo.Visible = true;
            lblCurrentMessage.Visible = true;
            progressBar1.Maximum = kepMessages.Count;
            lblTotalMessage.Text = "/ " + kepMessages.Count;
            await Task.Run(() => PerformSign());
            this.Invalidate();
            this.Refresh();
        }
        internal void PerformSign()
        {
            KEP_Mail_Message kepMsg = new KEP_Mail_Message();
            for (int i = 0; i < kepMessages.Count; i++)
            {

                kepMsg = new KEP_Mail_Message();
                kepMsg.permitTestKEPAddress(true);//Debug only
                kepMsg.addKepFrom(gorunenIsim, kullaniciAdi);
                kepMsg.addKepTo(kepMessages[i].Subscriber?.Info ?? "", kepMessages[i].KepMail);
                if (!string.IsNullOrEmpty(kepMessages[i].Info))
                {
                    kepMsg.addKepSubject(kepMessages[i].Info);
                }
                else
                {
                    kepMsg.addKepSubject(kepMessages[i].KepJob.Info);
                }

                var IletiId = Guid.NewGuid().ToString();
                kepMsg.addKepIletiID(IletiId);
                kepMsg.addKepIletiTip("standart");
                SetCurrent(kepMsg, i);

                kepMsg.addKepBodyHTML_UTF8(kepMessages[i].TemplateContent);
                string unsigned = "";
                if (kepMessages[i].HasAttachment)
                {
                    var attachments = kepClient.GetKepMessageAttahments(sesId, kepMessages[i].Id).Value;
                    if (attachments?.Length > 0)
                    {
                        foreach (var attach in attachments)
                        {
                            using (MemoryStream stream = new MemoryStream())
                            {
                                stream.Write(attach.Data, 0, attach.Data.Length);
                                stream.Position = 0;
                                kepMsg.addKepBodyAttachmentWithByteArray(stream, attach.Code);
                                //unsigned = kepMsg.getKEPIletiToString();
                            }
                        }
                    }
                }
                unsigned = kepMsg.getKEPIletiToString();
                var signRes = KepMailMessageSign(kepMsg);
                if (signRes == null)
                {

                }
                string signed = kepMsg.getKEPIletiToString();
                IletiId = kepMsg.getKepIletiID();

                KEP_Mail_Parse pars = new KEP_Mail_Parse();
                pars.getEMLParseFromByte(kepMsg.getKEPIletiToByteArray());
                var messageId = pars.getKepMessageID().Trim('<', '>');// Helper.ExtractMessageIdFromEML(kepMsg.getKEPIletiToString(), "Message-ID: <", ">")[0];

                kepMessages[i].MessageID = messageId;
                kepMessages[i].SignedContent = signed;
                kepMessages[i].Status = JobStatus.Signed;
                kepMessages[i].StatusCode = EnumHelper<MyJobStatus>.GetDisplayValue((MyJobStatus)kepMessages[i].Status);
            }
            var res = kepClient.UpdateKepMessages(sesId, kepMessages.ToArray());
            if (res.Result.Id == 0)
            {

                MessageBox.Show("İmzalama işlemi başarılı");
            }
            else
            {
                MessageBox.Show("Bir hata oluştu\n" + res.Result.Info);
            }
            btnClose.Enabled = true;
            this.Invalidate();
            this.Refresh();
        }

        private void SetCurrent(KEP_Mail_Message kepMsg, int i)
        {
            lblCurrentMessage.Text = (i + 1).ToString();
            progressBar1.Value = (i + 1);
            txtInfo.Text = kepMessages[i].KepMail + "\t\t" + kepMsg.getKepSubject();
            gridView1.RefreshRow(i);
            gridView1.InvalidateRow(i);
        }

        public KEP_Mail_Message KepMailMessageSign(KEP_Mail_Message msg)
        {
            msg.kep_imza.setTokenPIN(txtPin.Text);
            msg.kep_imza.setPolicyPath(PolicyPath);
            msg.kep_imza.setEsyaLicensePath(EsyaLicensePath);
            msg.kep_imza.setTerminal(certificatesImageCombobox.EditValue.ToString());
            try
            {
                msg.kepImzala();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
            return msg;
        }

        private void certificatesImageCombobox_EditValueChanged(object sender, EventArgs e)
        {
            if (txtPin.EditValue != null && txtPin.EditValue.ToString().Length >= 4 && certificatesImageCombobox.EditValue != null)
            {
                btnSignitureControl.Enabled = true;
            }
            else
            {
                btnSignitureControl.Enabled = false;
            }
        }

        private void txtPin_EditValueChanged(object sender, EventArgs e)
        {
            if (txtPin.EditValue != null && txtPin.EditValue.ToString().Length >= 4 && certificatesImageCombobox.EditValue != null)
            {
                btnSignitureControl.Enabled = true;
            }
            else
            {
                btnSignitureControl.Enabled = false;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            PopulateCertificatesCombobox();
        }

        private void btnTester_Click(object sender, EventArgs e)
        {
            var res = kepClient.GetKepMessageListByStatus(sesId, JobStatus.Signed);
            var keplogs = res.Value;
            for (int i = 0; i < res.Value.Length; i++)
            {
                List<string> myMessageId = Helper.ExtractMessageIdFromEML(res.Value[i].SignedContent.Substring(0, 600), "Message-ID: <", ">");
                keplogs[i].MessageID = myMessageId[0];
            }
            var rss = kepClient.UpdateKepMessages(sesId, keplogs.ToArray());

        }
    }
}
