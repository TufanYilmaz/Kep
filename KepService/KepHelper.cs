using com.verion.uniApi;
using KepNotificationDev.Helpers;
using KepNotificationDev.Helpers.Service;
using KepNotificationDev.Models;
using System;
using System.Configuration;
using System.Linq;
using System.Text;

namespace KepService
{
    public static class KepHelper
    {
        public static string AppPath => ConfigurationManager.AppSettings["AppPath"];
        public static void KepGonder(MyMailMessage myMsg)
        {
            KEP_Mail_Message kepMsg = new KEP_Mail_Message();
            kepMsg.permitTestKEPAddress(true);
            kepMsg.addKepFrom("Tufan", "merkez.test@kep.turkkep.com.tr");
            foreach (var reciever in myMsg.Recievers)
            {
                //kepMsg.addKepTo("", reciever);
            }
            kepMsg.addKepSubject(myMsg.Subject);
            kepMsg.addKepIletiID("");
            kepMsg.addKepIletiTip("standart");
            kepMsg.addKepBodyHTML_UTF8(myMsg.Body);
            KepMailMessageSign(kepMsg);
            var EMLKepIletisi = kepMsg.getKEPIletiToString();

            var kepGonderimDogrulama = new KepGonderimi.authInput()
            {
                islemYetkiliKodu = "001",
                kullaniciAdi = "merkez.test@kep.turkkep.com.tr",
                kullaniciSifresi = ""
            };
            var base64EML = Convert.ToBase64String(Encoding.UTF8.GetBytes(EMLKepIletisi));

            var kepIcerigi = new KepGonderimi.postaIcerik()
            {
                iletiIcerigi = base64EML//EML
            };


            KepGonderimi.KEPGonderimClient kepClient = new KepGonderimi.KEPGonderimClient();
            var result = kepClient.KEPGonderimi(kepGonderimDogrulama, kepIcerigi);
        }
        public static KEP_Mail_Message KepMailMessageSign(KEP_Mail_Message msg)
        {
            msg.kep_imza.setTokenPIN("9687");
            msg.kep_imza.setPolicyPath(AppPath + @"config\certval-policy-test.xml");
            msg.kep_imza.setEsyaLicensePath(AppPath + @"lisans\lisans.xml");
            var res = msg.kep_imza.getListOfTerminalsAndCertInfo();
            msg.kep_imza.setTerminal(res[0].Key);
            msg.kepImzala();
            return msg;
        }
        public static void PerformSendTest()
        {
            KEP_Mail_Message testMsg = new KEP_Mail_Message();
            testMsg.cleanKEPMessageContents();
            testMsg.kep_imza.setTokenPIN("9687");
            testMsg.kep_imza.setPolicyPath(AppPath + @"\config\certval-policy-test.xml");
            testMsg.kep_imza.setEsyaLicensePath(AppPath + @"\lisans\lisans.xml");

            var res = testMsg.kep_imza.getListOfTerminalsAndCertInfo();
            testMsg.permitTestKEPAddress(true);
            testMsg.addKepFrom("Tufan", "merkez.test@kep.turkkep.com.tr");
            testMsg.addKepTo("", "tufan@merkez.com.tr");
            testMsg.addKepTo("", "test@kep.turkkep.com.tr");
            testMsg.addKepSubject("Konu deneme");
            testMsg.addKepIletiID("");
            testMsg.addKepIletiTip("standart");
            testMsg.addKepBodyText_UTF8("Deneme İçeriği");
            testMsg.addKepBodyHTML_UTF8("");
            //testMsg.addKepBodyAttachment
            testMsg.kep_imza.setTerminal(res[0].Key);
            testMsg.kepImzala();
            var EMLKepIletisi = testMsg.getKEPIletiToString();
            var kepGonderimDogrulama = new KepGonderimi.authInput()
            {
                islemYetkiliKodu = "001",
                kullaniciAdi = "merkez.test@kep.turkkep.com.tr",
                kullaniciSifresi = ""
            };
            var base64EML = Convert.ToBase64String(Encoding.UTF8.GetBytes(EMLKepIletisi));

            var kepIcerigi = new KepGonderimi.postaIcerik()
            {
                iletiIcerigi = base64EML//EML
            };


            KepGonderimi.KEPGonderimClient kepClient = new KepGonderimi.KEPGonderimClient();
            var result = kepClient.KEPGonderimi(kepGonderimDogrulama, kepIcerigi);
        }

        public static long ProcessInbox(KepAlimi.mailListOutput[] mailList)
        {
            var result =
                mailList
                .GroupBy(x => x.kepUAMessageIdentifier)
                .Select(g => g.OrderByDescending(y => y.kepUID)
                    .First()
                 );
            var max = result.Max(m => m.kepUID);
            var min = result.Min(m => m.kepUID);
            bool transactionFailure = false;
            foreach (var item in result)
            {
                try
                {
                    DelilTipi tip;
                    var kepEvidenceType = item.kepEvidenceType;
                    var evidenceCodePart = item.kepEventCode.Split('#');
                    var evidenceCode = evidenceCodePart[1];
                    if (kepEvidenceType == "SubmissionAcceptanceRejection")
                    {
                        if (evidenceCode == "Acceptance")
                        {
                            tip = DelilTipi.SubmissionAcceptance;
                        }
                        else
                        {
                            tip = DelilTipi.SubmissionRejection;
                        }
                    }
                    else if (kepEvidenceType == "REMMDAcceptanceRejection")
                    {
                        if (evidenceCode == "Acceptance")
                        {
                            tip = DelilTipi.REMMDAcceptance;
                        }
                        else
                        {
                            tip = DelilTipi.REMMDRejection;
                        }
                    }
                    else if (kepEvidenceType == "RelayToREMMDFailure")
                    {
                        tip = DelilTipi.RelayToREMMDFailure;
                    }
                    else if (kepEvidenceType == "DeliveryNonDeliveryToRecipient")
                    {
                        if (evidenceCode == "Delivery")
                        {
                            tip = DelilTipi.DeliveryToRecipient;
                        }
                        else
                        {
                            tip = DelilTipi.NonDeliveryToRecipient;
                        }
                    }
                    else if (kepEvidenceType == "RetrievalNonRetrievalByRecipient")
                    {
                        if (evidenceCode == "Retrieval")
                        {
                            tip = DelilTipi.RetrievalByRecipient;
                        }
                        else
                        {
                            tip = DelilTipi.NonRetrievalByRecipient;
                        }
                    }
                    else
                    {
                        tip = DelilTipi.Unknown;
                    }
                    int KepStatus = DBHelper.GetKepStatusByMessageId(item.kepUAMessageIdentifier);
                    if (KepStatus > 0)
                    {
                        if (KepStatus > (int)tip)
                        {
                            continue;
                        }
                    }
                    //item.kepSendDate
                    DBHelper.UpdateKepStatusByMessageId(item.kepUAMessageIdentifier, (int)tip, item.kepSendDate);
                }
                catch (Exception ex)
                {
                    transactionFailure = true;
                }
            }
            if (!transactionFailure)
            {
                DBHelper.SetKepUIDLast(max.ToString());
                return max;
            }
            return min;
        }
    }
}
