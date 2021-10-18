using DevExpress.Web;
using KepNotificationDev.Helpers.Service;
using KepNotificationDev.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using IntervalTypes = KepNotificationDev.Helpers.Service.IntervalTypes;

namespace KepNotificationDev.Helpers
{
    //public enum DatasetType
    //{
    //    All=-1,
    //    Dataset=0,
    //    Recievers=1
    //}
    //public enum IntervalType
    //{
    //    [Display(Name ="Saat")]
    //    Saat=0,
    //    [Display(Name ="Gün")]
    //    Gün = 1,
    //    [Display(Name ="Ay")]
    //    Ay = 2
    //}
    public static class DBHelper
    {
        static readonly string connString = ConfigurationManager.ConnectionStrings["AbysisDbConn"].ToString();
        public static IEnumerable<Job> GetJobs()
        {
            var result = new List<Job>();
            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("Select * from tb_MESSAGER_JOBS WHERE CANCELLED=0", conn))
                    {
                        using (SqlDataAdapter adap = new SqlDataAdapter(cmd))
                        {

                            DataTable dt = new DataTable();
                            adap.Fill(dt);
                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    Job temp = new Job()
                                    {

                                        Id = Convert.ToInt32(row["ID"]),
                                        Code = row["CODE"].ToString(),
                                        Info = row["INFO"].ToString(),
                                        IntervalType = (IntervalTypes)(Convert.ToInt32(row["INTERVAL_TYPE"])),
                                        Interval = Convert.ToInt32(row["INTERVAL"]),
                                        MessageChannels = (MessageChannels)Convert.ToInt32(row["SEND_TYPE"]),
                                        ReceiverType = (ReceiverType)Convert.ToInt32(row["RECEIVER_TYPE"]),
                                        ReceiverId = Convert.ToInt32(row["RECIEVER_ID"]),
                                        ReportId = Convert.ToInt32(row["REPORT_ID"]),
                                        //ReceiverEmails = row["RECEIVER_EMAILS"].ToString(),
                                        //ReceiverGSMs = row["RECEIVER_GSMS"].ToString(),
                                        AutoSend = Convert.ToBoolean(row["AUTO_SEND"]),
                                        StartTime = Convert.ToDateTime(row["START_TIME"]),
                                        Active = Convert.ToBoolean(row["ACTIVE"]),
                                        Created_By = row["CREATED_BY"].ToString(),
                                        Created_Date = Convert.ToDateTime(row["CREATED_DATE"]),
                                        RelevantAddress = row["RELEVANT_MAIL"].ToString(),
                                    };
                                    result.Add(temp);
                                    if (row["LAST_EXECUTED_TIME"] != DBNull.Value)
                                    {
                                        temp.LastExecutedTime = Convert.ToDateTime(row["LAST_EXECUTED_TIME"]);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
            return result;
        }

        public static List<Template> GetTemplatesByReferance(string refTable)
        {
            var result = new List<Template>();
            using (SqlConnection conn = new SqlConnection(connString))
            {

                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT * from tb_KEP_TEMPLATES WHERE CANCELLED=0 AND REF_TABLE=@refTable", conn))
                    {
                        using (SqlDataAdapter adap = new SqlDataAdapter(cmd))
                        {
                            cmd.Parameters.AddWithValue("@refTable", refTable);
                            DataTable dt = new DataTable();
                            adap.Fill(dt);
                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {

                                    result.Add(new Template()
                                    {
                                        Id = Convert.ToInt32(row["ID"]),
                                        Code = row["CODE"].ToString(),
                                        Info = row["INFO"].ToString(),
                                        Created_Date = Convert.ToDateTime(row["CREATED_DATE"])
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    conn.Close();
                }
            }
            return result;
        }

        public static List<Receiver> GetAccounts(bool all = false)
        {
            var result = new List<Receiver>();
            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string cmdSql = @"SELECT ID,CODE,INFO,EMail,Gsm,KEP_MAIL FROM tb_ACCOUNTS
                    WHERE ACTIVE_STATUS=0" + (all ? "" : " AND (ISNULL(EMail,'')<>'' OR ISNULL(Gsm, '') <> '' OR ISNULL(KEP_MAIL, '') <> '')");
                    using (SqlCommand cmd = new SqlCommand(cmdSql, conn))
                    {
                        using (SqlDataAdapter adap = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adap.Fill(dt);
                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    result.Add(new Receiver()
                                    {
                                        Id = Convert.ToInt32(row["ID"]),
                                        Code = row["CODE"].ToString(),
                                        Info = row["INFO"].ToString(),
                                        Email = row["Email"].ToString(),
                                        ManagementGsm = row["Gsm"].ToString(),
                                        KepMail = row["KEP_MAIL"].ToString(),
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    conn.Close();
                }
            }
            return result;
        }

        public static void MessegerLogAttachmentAdd(int insertedLogId, params int[] insertedAttaches)
        {
            string sqlQuery = @"INSERT INTO tb_MESSAGER_LOG_ATTACHMENTS
                                VALUES(
                                @MESSAGER_LOG_ID,
                                @MESSAGER_ATTACHMENT_ID)";
            using (SqlConnection conn = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    conn.Open();
                    foreach (var attachId in insertedAttaches)
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@MESSAGER_LOG_ID", insertedLogId);
                        cmd.Parameters.AddWithValue("@MESSAGER_ATTACHMENT_ID", attachId);
                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    conn.Close();
                }
            }
        }

        public static List<int> UploadedAttachmentsInsert(List<UploadedFile> files, string subject)
        {
            var result = new List<int>();

            foreach (var file in files)
            {
                result.Add(AttachmentInsert(file.FileName, file.FileBytes, subject));
            }
            return result;
        }
        public static int AttachmentInsert(string filename, byte[] file, string info = "")
        {
            int result = 0;
            string sqlQuery = @"INSERT INTO tb_MESSAGER_ATTACHMENT
                            VALUES(
                            @CODE,
                            @INFO,
                            @DATA,
                            @CREATED_DATE,
                            @CREATED_BY)";
            using (SqlConnection conn = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    conn.Open();

                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@CODE", filename);
                    cmd.Parameters.AddWithValue("@INFO", info + " konulu ileti eklentisi");
                    cmd.Parameters.AddWithValue("@DATA", file);
                    cmd.Parameters.AddWithValue("@CREATED_DATE", DateTime.Now);
                    cmd.Parameters.AddWithValue("@CREATED_BY", Session.Username ?? "Servis");
                    try
                    {
                        result = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    catch (Exception ex)
                    {
                    }

                    conn.Close();
                }
            }
            return result;
        }
        public static List<MyUploadedFile> GetLogAttachments(int logId)
        {
            var result = new List<MyUploadedFile>();
            string sqlQuery = @"SELECT MA.* FROM tb_MESSAGER_ATTACHMENT MA
                            LEFT OUTER JOIN tb_MESSAGER_LOG_ATTACHMENTS MLA ON MA.ID=MLA.MESSAGER_ATTACHMENT_ID
                            LEFT OUTER JOIN tb_MESSAGER_LOGS ML ON ML.ID=MLA.MESSAGER_LOG_ID
                            WHERE ML.ID=@ID";

            using (SqlConnection conn = new SqlConnection(connString))
            {

                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                    {
                        using (SqlDataAdapter adap = new SqlDataAdapter(cmd))
                        {
                            cmd.Parameters.AddWithValue("@ID", logId);
                            DataTable dt = new DataTable();
                            adap.Fill(dt);
                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    result.Add(new MyUploadedFile()
                                    {
                                        Id = Convert.ToInt32(row["ID"]),
                                        Code = row["CODE"].ToString(),
                                        Info = row["INFO"].ToString(),
                                        Data = ((byte[])row["DATA"]).ToArray(),
                                        CreatedBy = row["CREATED_BY"].ToString(),
                                        CreatedDate = Convert.ToDateTime(row["CREATED_DATE"]),
                                    });
                                }
                            }
                        }
                    }

                }
                catch (Exception)
                {
                }
                finally
                {
                    conn.Close();
                }
            }

            return result;
        }

        public static void InsertKepLogControl(string refCode, List<int> createdRefIds)
        {
            string sqlstart = @"INSERT INTO tb_KEP_RECORD_CONTROL VALUES";
            List<string> values = new List<string>();
            foreach (var item in createdRefIds)
            {
                values.Add("('" + refCode + "'," + item + ")");
            }
            string sqlQuery = sqlstart + string.Join(",", values);
            using (SqlConnection conn = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }

        }

        public static bool JobDelete(int Id)
        {
            var result = false;
            string sqlQuery = @"UPDATE tb_MESSAGER_JOBS SET 
                            CANCELLED=@cancel
                            WHERE ID=@Id";
            using (SqlConnection conn = new SqlConnection(connString))
            {

                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", Id);
                        cmd.Parameters.AddWithValue("@cancel", true);
                        cmd.ExecuteNonQuery();
                        result = true;
                    }
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    conn.Close();
                }
            }
            return result;
        }

        public static string JobInsert(Job job)
        {
            string result = "";

            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string cmdSql = @"
INSERT INTO tb_MESSAGER_JOBS (CODE,INFO,INTERVAL_TYPE,INTERVAL,RECEIVER_TYPE,AUTO_SEND,SEND_TYPE,RECIEVER_ID,REPORT_ID,START_TIME,ACTIVE,RELEVANT_MAIL,CREATED_BY,CREATED_DATE) 
                  VALUES(@CODE,@INFO,@INTERVAL_TYPE,@INTERVAL,@RECEIVER_TYPE,@AUTO_SEND,@SEND_TYPE,@RECIEVER_ID,@REPORT_ID,@START_TIME,@ACTIVE,@RELEVANT_MAIL,@CREATED_BY,@CREATED_DATE)";
                    using (SqlCommand cmd = new SqlCommand(cmdSql, conn))
                    {

                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@CODE", job.Code ?? "");
                        cmd.Parameters.AddWithValue("@INFO", job.Info ?? "");
                        cmd.Parameters.AddWithValue("@INTERVAL_TYPE", (int)job.IntervalType);
                        cmd.Parameters.AddWithValue("@INTERVAL", job.Interval);
                        cmd.Parameters.AddWithValue("@RECEIVER_TYPE", (int)job.ReceiverType);
                        //cmd.Parameters.AddWithValue("@RECEIVER_GSMS", job.ReceiverGSMs);
                        //cmd.Parameters.AddWithValue("@RECEIVER_EMAILS", job.ReceiverEmails);
                        cmd.Parameters.AddWithValue("@AUTO_SEND", job.AutoSend);
                        cmd.Parameters.AddWithValue("@SEND_TYPE", (int)job.MessageChannels);
                        cmd.Parameters.AddWithValue("@RECIEVER_ID", job.ReceiverId);
                        cmd.Parameters.AddWithValue("@REPORT_ID", job.ReportId);
                        cmd.Parameters.AddWithValue("@START_TIME", job.StartTime);
                        cmd.Parameters.AddWithValue("@ACTIVE", job.Active);
                        cmd.Parameters.AddWithValue("@RELEVANT_MAIL", job.RelevantAddress ?? "");
                        cmd.Parameters.AddWithValue("@CREATED_BY", Session.Username);
                        cmd.Parameters.AddWithValue("@CREATED_DATE", DateTime.Now);
                        int lastInserted = Convert.ToInt32(cmd.ExecuteScalar());
                        result = "Kayıt başarılı Kayıt ID= #" + lastInserted;
                    }
                }
                catch (Exception ex)
                {
                    result = "Kayıt başarısız\n" + ex.Message;
                    //throw ex;
                }
                finally
                {
                    conn.Close();
                }
            }
            return result;
        }

        public static Job JobGetById(int Id)
        {
            Job result = null;
            using (SqlConnection conn = new SqlConnection(connString))
            {

                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("Select * from tb_MESSAGER_JOBS where ID=@Id and CANCELLED=0", conn))
                    {
                        using (SqlDataAdapter adap = new SqlDataAdapter(cmd))
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@Id", Id);
                            DataTable dt = new DataTable();
                            adap.Fill(dt);
                            if (dt.Rows.Count > 0)
                            {
                                result = new Job
                                {
                                    Id = Id,
                                    Code = dt.Rows[0]["CODE"].ToString(),
                                    Info = dt.Rows[0]["INFO"].ToString(),
                                    IntervalType = (IntervalTypes)Convert.ToInt32(dt.Rows[0]["INTERVAL_TYPE"]),
                                    Interval = Convert.ToInt32(dt.Rows[0]["INTERVAL"]),
                                    ReceiverId = Convert.ToInt32(dt.Rows[0]["RECIEVER_ID"]),
                                    ReportId = Convert.ToInt32(dt.Rows[0]["REPORT_ID"]),
                                    StartTime = Convert.ToDateTime(dt.Rows[0]["START_TIME"]),
                                    Active = Convert.ToBoolean(dt.Rows[0]["ACTIVE"]),
                                    Created_By = dt.Rows[0]["CREATED_BY"].ToString(),
                                    Created_Date = Convert.ToDateTime(dt.Rows[0]["CREATED_DATE"]),
                                    RelevantAddress = dt.Rows[0]["RELEVANT_MAIL"].ToString(),
                                    AutoSend = Convert.ToBoolean(dt.Rows[0]["AUTO_SEND"]),
                                    ReceiverType = (ReceiverType)Convert.ToInt32(dt.Rows[0]["RECEIVER_TYPE"]),
                                    MessageChannels = (MessageChannels)Convert.ToInt32(dt.Rows[0]["SEND_TYPE"]),
                                    //ReceiverEmails = dt.Rows[0]["RECEIVER_EMAILS"].ToString(),
                                    //ReceiverGSMs = dt.Rows[0]["RECEIVER_GSMS"].ToString(),
                                };
                            }
                        }
                    }

                }
                catch (Exception)
                {
                }
                finally
                {
                    conn.Close();
                }
            }
            return result;
        }
        public static void JobUpdate(Job job)
        {

            string cmdSql = @"
                        UPDATE tb_MESSAGER_JOBS
                        SET 
                        CODE=@CODE,
                        INFO=@INFO,
                        INTERVAL_TYPE=@INTERVAL_TYPE,
                        INTERVAL=@INTERVAL,
                        RECEIVER_TYPE=@RECEIVER_TYPE,
                        AUTO_SEND=@AUTO_SEND,
                        SEND_TYPE=@SEND_TYPE,
                        RECIEVER_ID=@RECIEVER_ID,
                        REPORT_ID=@REPORT_ID,
                        START_TIME=@START_TIME,
                        ACTIVE=@ACTIVE,
                        LAST_EXECUTED_TIME=@LAST_EXECUTED_TIME,
                        RELEVANT_MAIL=@RELEVANT_MAIL
                         WHERE 
                         ID=@ID
                        ";//RECEIVER_GSMS=@RECEIVER_GSMS,
                          //RECEIVER_EMAILS = @RECEIVER_EMAILS,
            using (SqlConnection conn = new SqlConnection(connString))
            {

                using (SqlCommand cmd = new SqlCommand(cmdSql, conn))
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@ID", job.Id);
                    cmd.Parameters.AddWithValue("@CODE", job.Code ?? "");
                    cmd.Parameters.AddWithValue("@INFO", job.Info ?? "");
                    cmd.Parameters.AddWithValue("@INTERVAL_TYPE", (int)job.IntervalType);
                    cmd.Parameters.AddWithValue("@INTERVAL", job.Interval);
                    cmd.Parameters.AddWithValue("@RECEIVER_TYPE", (int)job.ReceiverType);
                    //cmd.Parameters.AddWithValue("@RECEIVER_GSMS", job.ReceiverGSMs);
                    //cmd.Parameters.AddWithValue("@RECEIVER_EMAILS", job.ReceiverEmails);
                    cmd.Parameters.AddWithValue("@AUTO_SEND", job.AutoSend);
                    cmd.Parameters.AddWithValue("@SEND_TYPE", (int)job.MessageChannels);
                    cmd.Parameters.AddWithValue("@RECIEVER_ID", job.ReceiverId);
                    cmd.Parameters.AddWithValue("@REPORT_ID", job.ReportId);
                    cmd.Parameters.AddWithValue("@START_TIME", job.StartTime);
                    cmd.Parameters.AddWithValue("@ACTIVE", job.Active);
                    cmd.Parameters.AddWithValue("@RELEVANT_MAIL", job.RelevantAddress ?? "");

                    if (job.LastExecutedTime != default || job.LastExecutedTime != null)
                    {
                        cmd.Parameters.AddWithValue("@LAST_EXECUTED_TIME", job.LastExecutedTime);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@LAST_EXECUTED_TIME", DBNull.Value);
                    }
                    try
                    {
                        conn.Open();
                        var res = cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                    }
                    finally
                    {
                        conn.Close();
                    }
                }

            }
        }

        public static void MessageLogUpdate(List<MessageLog> logs)
        {
            string cmdSql = @"UPDATE tb_MESSAGER_LOGS SET 
                            STATUS_CODE=@statuscode,
                            STATUS_TYPE=@statustype,
                            SENT_DATETIME=@sentdatetime
                            WHERE ID=@id";
            using (SqlConnection conn = new SqlConnection(connString))
            {
                foreach (var log in logs)
                {

                    using (SqlCommand cmd = new SqlCommand(cmdSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@statuscode", log.StatusCode);
                        cmd.Parameters.AddWithValue("@statustype", log.StatusType);
                        cmd.Parameters.AddWithValue("@id", log.Id);
                        if (log.SentDatetime != default && log.SentDatetime != null)
                        {
                            cmd.Parameters.AddWithValue("@sentdatetime", log.SentDatetime);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@sentdatetime", DBNull.Value);
                        }
                        try
                        {
                            conn.Open();
                            var res = cmd.ExecuteNonQuery(); ;
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        finally
                        {
                            conn.Close();
                        }

                    }
                }
            }
        }

        public static void LogSentErrorInsert(string Code, string Info, string Type, MessageLog kepLog)
        {
            string sqlStr = @"INSERT INTO tb_KEP_ERRORS(
                            CODE
                            ,INFO
                            ,ERROR_TYPE
                            ,ERROR_TIME
                            ,LOG_ID)
                             VALUES
                            (@CODE 
                            ,@INFO 
                            ,@ERROR_TYPE
                            ,@ERROR_TIME
                            ,@LOG_ID)";
            using (SqlConnection conn = new SqlConnection(connString))
            {

                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sqlStr, conn))
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@CODE", Code);
                        cmd.Parameters.AddWithValue("@INFO", Info ?? "");
                        cmd.Parameters.AddWithValue("@ERROR_TYPE", Type);
                        cmd.Parameters.AddWithValue("@ERROR_TIME", kepLog.SentDatetime);
                        cmd.Parameters.AddWithValue("@LOG_ID", kepLog.Id);
                        cmd.ExecuteNonQuery();
                        int insertId = TemplateGetLastInserted();
                    }
                }
                catch (Exception ex)
                {
                    //throw ex;
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        public static IEnumerable<Datasets> GetDatasets()
        {
            var result = new List<Datasets>();

            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string sql = "Select * from tb_KEP_DATASETS where CANCELLED=0";
                    //if(datasetType!=DatasetType.All)
                    //{
                    //    sql += " WHERE DATASET_TYPE=" + ((int)datasetType);
                    //}
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        using (SqlDataAdapter adap = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adap.Fill(dt);
                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {

                                    result.Add(new Datasets()
                                    {
                                        Id = Convert.ToInt32(row["ID"]),
                                        Code = row["CODE"].ToString(),
                                        Info = row["INFO"].ToString(),
                                        ReferanceTable = row["REFERANCE_TABLE"].ToString(),
                                        Dataset = row["DATASET"].ToString(),
                                        Created_Date = Convert.ToDateTime(row["CREATED_DATE"])
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    conn.Close();
                }
            }
            return result;
        }

        public static bool TemplateDelete(int Id)
        {
            var result = false;
            using (SqlConnection conn = new SqlConnection(connString))
            {

                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"UPDATE tb_KEP_TEMPLATES SET CANCELLED=1 WHERE ID=@Id", conn);
                    cmd.Parameters.AddWithValue("@Id", Id);
                    cmd.ExecuteNonQuery();
                    result = true;
                    Session.Reports.Remove(Id);
                }
                catch (Exception)
                {

                }
                finally
                {
                    conn.Close();
                }
            }
            return result;
        }
        public static string TemplateInsert(Template ds)
        {
            string result = "";
            using (SqlConnection conn = new SqlConnection(connString))
            {

                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(@"INSERT INTO tb_KEP_TEMPLATES VALUES(@code,@info,@content,@params,@reftable,@cancelled,@date,@user)", conn))
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@code", ds.Code);
                        cmd.Parameters.AddWithValue("@info", ds.Info ?? "");
                        cmd.Parameters.AddWithValue("@content", ds.Content);
                        cmd.Parameters.AddWithValue("@params", ds.Params ?? "");
                        cmd.Parameters.AddWithValue("@reftable", ds.RefTable ?? "");
                        cmd.Parameters.AddWithValue("@cancelled", false);
                        cmd.Parameters.AddWithValue("@user", Session.Username);
                        cmd.Parameters.AddWithValue("@date", DateTime.Now);
                        cmd.ExecuteNonQuery();
                        int insertId = TemplateGetLastInserted();
                        result = "Kayıt başarılı Kayıt ID= #" + insertId;
                        Session.Reports.Add(insertId, ds.Code);
                    }
                }
                catch (Exception ex)
                {
                    result = "Kayıt başarısız\n" + ex.Message;
                    //throw ex;
                }
                finally
                {
                    conn.Close();
                }
            }
            return result;
        }
        private static int TemplateGetLastInserted()
        {
            int result = 0;
            using (SqlConnection conn = new SqlConnection(connString))
            {

                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(@"SELECT TOP 1 ID FROM tb_KEP_TEMPLATES ORDER BY ID DESC", conn))
                    {
                        result = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    conn.Close();
                }
            }
            return result;
        }
        public static Template TemplateGetById(int Id)
        {
            Template result = null;

            using (SqlConnection conn = new SqlConnection(connString))
            {

                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("Select * from tb_KEP_TEMPLATES where ID=@Id", conn))
                    {
                        using (SqlDataAdapter adap = new SqlDataAdapter(cmd))
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@Id", Id);
                            DataTable dt = new DataTable();
                            adap.Fill(dt);
                            if (dt.Rows.Count > 0)
                            {
                                result = new Template();
                                result.Id = Id;
                                result.Code = dt.Rows[0]["CODE"].ToString();
                                result.Info = dt.Rows[0]["INFO"].ToString();
                                result.RefTable = dt.Rows[0]["REF_TABLE"].ToString();
                                result.Content = ((byte[])dt.Rows[0]["CONTENT"]).ToArray();
                                result.Created_By = dt.Rows[0]["CREATED_BY"].ToString();
                                result.Created_Date = Convert.ToDateTime(dt.Rows[0]["CREATED_DATE"]);
                            }
                        }
                    }

                }
                catch (Exception)
                {
                }
                finally
                {
                    conn.Close();
                }
            }
            return result;
        }

        public static Template TemplateGetByCode(string Code)
        {
            Template result = null;
            using (SqlConnection conn = new SqlConnection(connString))
            {

                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("Select * from tb_KEP_TEMPLATES where CODE=@Code AND CANCELLED=0", conn))
                    {
                        using (SqlDataAdapter adap = new SqlDataAdapter(cmd))
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@Code", Code);
                            DataTable dt = new DataTable();
                            adap.Fill(dt);
                            if (dt.Rows.Count > 0)
                            {
                                result = new Template();
                                result.Id = Convert.ToInt32(dt.Rows[0]["ID"]);
                                result.Code = dt.Rows[0]["CODE"].ToString();
                                result.Info = dt.Rows[0]["INFO"].ToString();
                                result.RefTable = dt.Rows[0]["REF_TABLE"].ToString();
                                result.Content = ((byte[])dt.Rows[0]["CONTENT"]).ToArray();
                                result.Created_By = dt.Rows[0]["CREATED_BY"].ToString();
                                result.Created_Date = Convert.ToDateTime(dt.Rows[0]["CREATED_DATE"]);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    conn.Close();
                }
            }
            return result;
        }
        public static void TemplateUpdate(int id, byte[] vs, string refTable = "")
        {

            string cmdSql = "UPDATE tb_KEP_TEMPLATES SET CONTENT=@content , REF_TABLE=@reftable WHERE ID=@id";
            using (SqlConnection conn = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(cmdSql, conn))
                {
                    cmd.Parameters.AddWithValue("@content", vs);
                    cmd.Parameters.AddWithValue("@reftable", refTable ?? "");
                    cmd.Parameters.AddWithValue("@id", id);
                    try
                    {
                        conn.Open();
                        var res = cmd.ExecuteNonQuery(); ;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                    }

                }
            }
        }
        public static void TemplateUpdate(string code, byte[] vs, string refTable = "")
        {

            string cmdSql = "UPDATE tb_KEP_TEMPLATES SET CONTENT=@content , REF_TABLE=@reftable WHERE CODE=@code";
            using (SqlConnection conn = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(cmdSql, conn))
                {
                    cmd.Parameters.AddWithValue("@content", vs);
                    cmd.Parameters.AddWithValue("@reftable", refTable ?? "");
                    cmd.Parameters.AddWithValue("@code", code);
                    try
                    {
                        conn.Open();
                        var res = cmd.ExecuteNonQuery(); ;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                    }

                }
            }
        }


        public static Datasets DatasetGetById(int Id)
        {
            Datasets result = null;
            using (SqlConnection conn = new SqlConnection(connString))
            {

                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("Select * from tb_KEP_DATASETS where ID=@Id", conn))
                    {
                        using (SqlDataAdapter adap = new SqlDataAdapter(cmd))
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@Id", Id);
                            DataTable dt = new DataTable();
                            adap.Fill(dt);
                            if (dt.Rows.Count > 0)
                            {
                                result = new Datasets();
                                result.Id = Id;
                                result.Code = dt.Rows[0]["CODE"].ToString();
                                result.Info = dt.Rows[0]["INFO"].ToString();
                                result.Dataset = dt.Rows[0]["DATASET"].ToString();
                                result.ReferanceTable = dt.Rows[0]["REFERANCE_TABLE"].ToString();
                                result.Created_By = dt.Rows[0]["CREATED_BY"].ToString();
                                result.Created_Date = Convert.ToDateTime(dt.Rows[0]["CREATED_DATE"]);

                            }
                        }
                    }

                }
                catch (Exception)
                {
                }
                finally
                {
                    conn.Close();
                }
            }
            return result;
        }

        public static IEnumerable<string> FindParameters(string command)
        {
            var pattern = "(@[A-Z,a-z,0-9])\\w+";
            var matches = Regex.Matches(command, pattern);
            return matches.Cast<Match>().Select(m => m.Value);
        }

        public static bool DatasetDelete(int Id)
        {
            var result = false;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(@"UPDATE tb_KEP_DATASETS SET CANCELLED=1 WHERE ID=@Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", Id);
                        cmd.ExecuteNonQuery();
                        result = true;
                    }
                }
                catch (Exception)
                {

                }
                finally
                {
                    conn.Close();
                }
            }
            return result;
        }

        public static string DatasetInsert(Datasets ds)
        {
            string result = "";
            using (SqlConnection conn = new SqlConnection(connString))
            {

                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(@"INSERT INTO tb_KEP_DATASETS VALUES(@code,@info,@dataset@referance,@cancelled,@user,@date)", conn))
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@code", ds.Code);
                        cmd.Parameters.AddWithValue("@info", ds.Info);
                        //cmd.Parameters.AddWithValue("@dataset_type", ds.DatasetType);
                        cmd.Parameters.AddWithValue("@dataset", ds.Dataset);
                        cmd.Parameters.AddWithValue("@referance", ds.ReferanceTable);
                        cmd.Parameters.AddWithValue("@cancelled", ds.Cancelled);
                        cmd.Parameters.AddWithValue("@user", Session.Username);
                        cmd.Parameters.AddWithValue("@date", DateTime.Now);
                        cmd.ExecuteNonQuery();
                        result = "Kayıt başarılı Kayıt ID= #" + DatasetGetLastInserted();
                    }
                }
                catch (Exception ex)
                {
                    result = "Kayıt başarısız\n" + ex.Message;
                    //throw ex;
                }
                finally
                {
                    conn.Close();
                }
            }
            return result;
        }
        public static int DatasetGetLastInserted()
        {
            int result = 0;
            using (SqlConnection conn = new SqlConnection(connString))
            {

                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(@"SELECT TOP 1 ID FROM tb_KEP_DATASETS ORDER BY ID DESC", conn))
                    {
                        result = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    conn.Close();
                }
            }
            return result;
        }
        public static void DatasetUpdate(Datasets ds)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string cmdSql = @"UPDATE tb_KEP_DATASETS 
                        SET CODE=@code,INFO=@info,DATASET=@dataset,REFERANCE_TABLE=@referance
                        WHERE ID=@id";
                    using (SqlCommand cmd = new SqlCommand(cmdSql, conn))
                    {

                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@code", ds.Code);
                        cmd.Parameters.AddWithValue("@info", ds.Info);
                        cmd.Parameters.AddWithValue("@referance", ds.ReferanceTable);
                        //cmd.Parameters.AddWithValue("@dataset_type", ds.DatasetType);
                        cmd.Parameters.AddWithValue("@dataset", ds.Dataset);
                        cmd.Parameters.AddWithValue("@id", ds.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception)
                {
                    //throw ex;
                }
                finally
                {
                    conn.Close();
                }
            }
            return;
        }

        public static List<Template> GetTemplates()
        {
            var result = new List<Template>();
            using (SqlConnection conn = new SqlConnection(connString))
            {

                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("Select * from tb_KEP_TEMPLATES WHERE CANCELLED=0", conn))
                    {
                        using (SqlDataAdapter adap = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adap.Fill(dt);
                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {

                                    result.Add(new Template()
                                    {
                                        Id = Convert.ToInt32(row["ID"]),
                                        Code = row["CODE"].ToString(),
                                        Info = row["INFO"].ToString(),
                                        Created_Date = Convert.ToDateTime(row["CREATED_DATE"])
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    conn.Close();
                }
            }
            return result;
        }
        public static Dictionary<int, string> GetTemplateCodes()
        {
            var result = new Dictionary<int, string>();
            var temp = GetTemplates();
            foreach (var item in temp)
            {
                result.Add(item.Id, item.Code);
            }

            return result;
        }
        public static List<BaseModel> GetInvoiceDepartments()
        {
            List<BaseModel> result = new List<BaseModel>();
            string sqlQuery = @"
                                SELECT ID, CODE FROM tb_DEPARTMENTS
                                WHERE ISNULL(TAXNUM,'')<>''
";
            using (SqlConnection conn = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    DataTable dt = new DataTable();
                    SqlDataAdapter adap = new SqlDataAdapter(cmd);
                    try
                    {
                        conn.Open();
                        adap.Fill(dt);
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                result.Add(new BaseModel()
                                {
                                    Id = Convert.ToInt32(row["ID"]),
                                    Code = row["CODE"].ToString(),
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            return result;
        }


        public static List<Receiver> GetRecievers(DatasetType refTable, List<int> recieverIds = null, string departmentId = "", bool all = false)
        {
            var result = new List<Receiver>();
            try
            {
                DataTable dt = GetRecieversDataTable(refTable, recieverIds, departmentId, all);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var rec = new Receiver()
                        {
                            Id = Convert.ToInt32(row["ID"]),
                            Code = row["CODE"].ToString(),
                            Info = row["INFO"].ToString(),
                            KepMail = row["KEP_MAIL"].ToString(),
                            Email = row["EMail"].ToString(),
                            ManagementGsm = row["Gsm"].ToString()
                        };
                        if (refTable == DatasetType.Abone)
                        {
                            rec.AccountGsm = row["Gsm2"].ToString();
                            rec.TechnicGsm = row["Gsm3"].ToString();
                            rec.AccountEmail = row["ACCOUNT_EMAIL"].ToString();
                        }
                        result.Add(rec);
                    }
                }
            }
            catch (Exception)
            {
            }
            return result;
        }
        public static DataTable GetRecieversDataTable(DatasetType refTable, List<int> subscriberIds = null, string department = "", bool all = false)
        {
            string tableName = "";
            var cmdSql = @"SELECT
                         ID ID,
                         CODE CODE,
                         INFO INFO,
                         --ACCOUNT_EMAIL,
                         EMail,
                         Gsm,
                         --Gsm2,
                         --Gsm3,
                         KEP_MAIL
                         FROM ~#table_name#~ 
                         WHERE
                         ACTIVE_STATUS=0";

            var allQuery = @" AND (ISNULL(EMail,'')<>'' OR
                         ISNULL(Gsm,'')<>'' OR
                         --ISNULL(Gsm2,'')<>'' OR
                         --ISNULL(Gsm3,'')<>'' OR
                         ISNULL(KEP_MAIL,'')<>'') ";
            switch (refTable)
            {
                case DatasetType.Firma:
                    tableName = "tb_ACCOUNTS";
                    break;
                case DatasetType.Abone:
                    tableName = "tb_SUBSCRIPTION";
                    cmdSql = cmdSql.Replace("--", "");
                    allQuery = allQuery.Replace("--", "");
                    break;
            }
            cmdSql += all ? "" : allQuery;
            cmdSql += (subscriberIds == null ? "" : (" AND ID IN(" + string.Join(",", subscriberIds) + ")"));
            cmdSql += string.IsNullOrEmpty(department) ? "" : $" AND DEPARTMENT_ID={department}";
            cmdSql = cmdSql.Replace("~#table_name#~", tableName);
            var result = new DataTable(refTable.ToString());
            using (SqlConnection conn = new SqlConnection(connString))
            {

                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(cmdSql, conn))
                    {
                        using (SqlDataAdapter adap = new SqlDataAdapter(cmd))
                        {
                            adap.Fill(result);
                        }
                    }
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    conn.Close();
                }
            }
            return result;
        }

        public static DataTable GetRecieversDatatable(string dataset)
        {
            DataTable result = null;
            using (SqlConnection conn = new SqlConnection(connString))
            {

                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(dataset, conn))
                    {
                        using (SqlDataAdapter adap = new SqlDataAdapter(cmd))
                        {
                            result = new DataTable();
                            adap.Fill(result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    conn.Close();
                }
            }
            return result;
        }


        public static int MessageLogInsert(MessageLog ds, string createdBy)
        {
            int insertedId = 0;
            using (SqlConnection conn = new SqlConnection(connString))
            {

                try
                {
                    conn.Open();
                    string cmdSql = @"
                 INSERT INTO tb_MESSAGER_LOGS(RECEIVER_TYPE,RECEIVER,REF_TABLE,REF_ID,JOB_ID,TOKEN,SUBJECT,CONTENT,HASATTACHMENTS,STATUS_TYPE,STATUS_CODE,CREATED_BY,CREATED_DATE)
                    VALUES(@RECEIVER_TYPE,@RECEIVER,@REF_TABLE,@REF_ID,@JOB_ID,@TOKEN,@SUBJECT,@CONTENT,@HASATTACHMENTS,@STATUS_TYPE,@STATUS_CODE,@CREATED_BY,@CREATED_DATE)";
                    using (SqlCommand cmd = new SqlCommand(cmdSql, conn))
                    {

                        cmd.Parameters.Clear();
                        ds.CreatedDate = DateTime.Now;
                        ds.CreatedBy = createdBy;
                        cmd.Parameters.AddWithValue("@RECEIVER_TYPE", (int)ds.MessageChannel);
                        cmd.Parameters.AddWithValue("@RECEIVER", ds.Receiver ?? "");
                        cmd.Parameters.AddWithValue("@REF_TABLE", ds.RefTable ?? "");
                        cmd.Parameters.AddWithValue("@REF_ID", ds.RefId);
                        cmd.Parameters.AddWithValue("@SUBJECT", ds.Subject);
                        cmd.Parameters.AddWithValue("@CONTENT", ds.Content);
                        cmd.Parameters.AddWithValue("@HASATTACHMENTS", (ds.HasAttachments == default) ? false : ds.HasAttachments);
                        cmd.Parameters.AddWithValue("@STATUS_TYPE", (int)ds.StatusType);
                        cmd.Parameters.AddWithValue("@STATUS_CODE", ds.StatusCode);
                        cmd.Parameters.AddWithValue("@CREATED_BY", ds.CreatedBy);
                        cmd.Parameters.AddWithValue("@CREATED_DATE", ds.CreatedDate);
                        cmd.Parameters.AddWithValue("@TOKEN", Guid.NewGuid().ToString("N"));
                        if (ds.JobId == null)
                        {
                            cmd.Parameters.AddWithValue("@JOB_ID", DBNull.Value);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@JOB_ID", ds.JobId);
                        }
                        insertedId = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    conn.Close();
                }
            }
            return insertedId;
        }

        public static List<MessageLog> GetMessageLogs(JobStatus? jobStatus = null)
        {
            var res = new List<MessageLog>();

            string cmdSql = @"
SELECT 
ML.ID ID
,ML.HASATTACHMENTS
,ML.REF_ID REF_ID
,S.CODE REF_CODE
,S.INFO REF_INFO
,ISNULL(KJ.INFO,'El İle Oluşturuldu') JOB_INFO
,ML.RECEIVER RECEIVER
,ML.RECEIVER_TYPE
,ML.CONTENT CONTENT
,ML.SIGNED_CONTENT SIGNED_CONTENT
,ML.STATUS_TYPE STATUS_TYPE
,ML.STATUS_CODE STATUS_CODE
,ML.SENT_DATETIME SENT_DATE
,ML.CREATED_DATE CREATED_DATE
,KJ.REPORT_ID
,ML.SUBJECT
,REF_TABLE
,ML.KEP_STATUS KEPSTATUS_ID
,ML.KEP_STATUS_DATETIME 
FROM tb_MESSAGER_LOGS ML
LEFT OUTER JOIN tb_SUBSCRIPTION S WITH(NOLOCK) ON ML.REF_ID=S.ID
LEFT OUTER JOIN tb_MESSAGER_JOBS KJ ON ML.JOB_ID = KJ.ID
WHERE
REF_TABLE='tb_SUBSCRIPTION'" + (jobStatus == null ? "" : @"

AND
    ML.STATUS_TYPE=@STATUS_TYPE
") +
@"
UNION ALL(
SELECT 
ML.ID ID
,ML.HASATTACHMENTS
,ML.REF_ID REF_ID
,S.CODE REF_CODE
,S.INFO REF_INFO
,ISNULL(KJ.INFO,'El İle Oluşturuldu') JOB_INFO
,ML.RECEIVER RECEIVER
,ML.RECEIVER_TYPE
,ML.CONTENT CONTENT
,ML.SIGNED_CONTENT SIGNED_CONTENT
,ML.STATUS_TYPE STATUS_TYPE
,ML.STATUS_CODE STATUS_CODE
,ML.SENT_DATETIME SENT_DATE
,ML.CREATED_DATE CREATED_DATE
,KJ.REPORT_ID
,ML.SUBJECT
,REF_TABLE
,ML.KEP_STATUS KEPSTATUS_ID
,ML.KEP_STATUS_DATETIME 
FROM tb_MESSAGER_LOGS ML
LEFT OUTER JOIN tb_ACCOUNTS S WITH(NOLOCK) ON ML.REF_ID=S.ID
LEFT OUTER JOIN tb_MESSAGER_JOBS KJ ON ML.JOB_ID = KJ.ID
WHERE 
REF_TABLE LIKE 'tb_ACCOUNTS'" + (jobStatus == null ? "" : @"

AND
    ML.STATUS_TYPE=@STATUS_TYPE
") +
@"
)
";
            using (SqlConnection conn = new SqlConnection(connString))
            {

                using (SqlCommand cmd = new SqlCommand(cmdSql, conn))
                {
                    if (jobStatus != null)
                    {
                        cmd.Parameters.AddWithValue("@STATUS_TYPE", (int)jobStatus);
                    }
                    using (SqlDataAdapter adab = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        try
                        {
                            conn.Open();
                            adab.Fill(dt);
                        }
                        catch (Exception ex)
                        {

                        }
                        finally
                        {
                            conn.Close();
                        }
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                MessageLog temp = new MessageLog()
                                {
                                    Id = Convert.ToInt32(row["ID"]),
                                    JobInfo = row["JOB_INFO"].ToString() ?? "Elle(Manuel) Oluşturuldu",
                                    Receiver = row["RECEIVER"].ToString(),
                                    MessageChannel = (MessageChannels)Convert.ToInt32(row["RECEIVER_TYPE"]),
                                    SignedContent = row["SIGNED_CONTENT"].ToString() ?? "",
                                    StatusType = (JobStatus)Convert.ToInt32(row["STATUS_TYPE"]),
                                    RefCode = row["REF_CODE"].ToString(),
                                    RefInfo = row["REF_INFO"].ToString(),
                                    Content = row["CONTENT"].ToString(),
                                    HasAttachments = Convert.ToBoolean(row["HASATTACHMENTS"]),
                                    CreatedDate = Convert.ToDateTime(row["CREATED_DATE"]),
                                    RefId = Convert.ToInt32(row["REF_ID"]),
                                    Subject = row["SUBJECT"].ToString() ?? "",
                                    RefTable = row["REF_TABLE"].ToString(),

                                };
                                temp.TemplateId = 0;
                                if (row["REPORT_ID"] != DBNull.Value)
                                {
                                    temp.TemplateId = Convert.ToInt32(row["REPORT_ID"]);
                                }
                                res.Add(temp);
                                if (row["SENT_DATE"] != DBNull.Value)
                                {
                                    temp.SentDatetime = Convert.ToDateTime(row["SENT_DATE"]);
                                }
                                if (row["KEPSTATUS_ID"] != DBNull.Value)
                                {
                                    temp.KepStatus = (DelilTipi)Convert.ToInt32(row["KEPSTATUS_ID"]);
                                }
                                if (row["KEP_STATUS_DATETIME"] != DBNull.Value)
                                {
                                    temp.KepStatusDatetime = Convert.ToDateTime(row["KEP_STATUS_DATETIME"]);
                                }

                            }
                        }
                    }
                }
            }
            return res;
        }
        public static MessageLog GetMessageLogByTOKEN(string TOKEN)
        {
            MessageLog res = null;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT ID FROM tb_MESSAGER_LOGS WHERE TOKEN=@TOKEN"))
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@TOKEN", TOKEN);
                    try
                    {
                        conn.Open();
                        int resId = Convert.ToInt32(cmd.ExecuteScalar());
                        res = GetMessageLogById(resId, true);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }
            }

            return res;
        }
        public static MessageLog GetMessageLogById(int logId, bool withAttachments = false)
        {
            var res = new MessageLog();

            string cmdSql = @"
                            SELECT 
                            ML.SUBJECT
                            ,ML.CONTENT CONTENT
                            ,ML.HASATTACHMENTS
                            ,MA.CODE
                            ,MA.DATA
                            ,ML.KEP_STATUS
                            ,ML.STATUS_TYPE
                            FROM tb_MESSAGER_LOGS ML
                            LEFT OUTER JOIN tb_MESSAGER_JOBS KJ ON ML.JOB_ID = KJ.ID
                            LEFT OUTER JOIN tb_MESSAGER_LOG_ATTACHMENTS MLA ON MLA.MESSAGER_LOG_ID=ML.ID
                            LEFT OUTER JOIN tb_MESSAGER_ATTACHMENT MA ON MLA.MESSAGER_ATTACHMENT_ID=MA.ID
                            WHERE ML.ID=@ID
                            ";
            using (SqlConnection conn = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(cmdSql, conn))
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@ID", logId);
                    using (SqlDataAdapter adab = new SqlDataAdapter(cmd))
                    {
                        conn.Open();
                        DataTable dt = new DataTable();
                        try
                        {
                            adab.Fill(dt);
                        }
                        finally
                        {
                            conn.Close();
                        }
                        if (dt.Rows.Count > 0)
                        {
                            res.StatusType = (JobStatus)Convert.ToInt32(dt.Rows[0]["STATUS_TYPE"]);
                            res.Content = dt.Rows[0]["CONTENT"].ToString();
                            res.Subject = dt.Rows[0]["SUBJECT"].ToString() ?? "";
                            res.HasAttachments = Convert.ToBoolean(dt.Rows[0]["HASATTACHMENTS"]);

                            if (dt.Rows[0]["KEP_STATUS"] != DBNull.Value)
                            {
                                res.KepStatus = (DelilTipi)Convert.ToInt32(dt.Rows[0]["KEP_STATUS"]);
                            }
                            if (res.HasAttachments && withAttachments)
                            {
                                res.Attachments = new List<MyUploadedFile>();
                                foreach (DataRow row in dt.Rows)
                                {
                                    res.Attachments.Add(new MyUploadedFile()
                                    {
                                        Code = row["CODE"].ToString(),
                                        Data = (byte[])row["DATA"]
                                    });

                                }
                            }
                        }
                    }
                }
                return res;
            }
        }
        public static bool MessageLogUpdateStatus(int[] Ids, JobStatus status)
        {
            bool result = false;
            if (Ids.Length < 0)
            {
                return result;
            }
            string sqlStr = "UPDATE tb_MESSAGER_LOGS SET STATUS_TYPE=@statusType, STATUS_CODE=@statusCode WHERE ID IN(" + string.Join(",", Ids) + ")";
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sqlStr, conn))
                {
                    cmd.Parameters.AddWithValue("statusType", status);
                    cmd.Parameters.AddWithValue("statusCode", EnumHelper<JobStatus>.GetDisplayValue(status));
                    try
                    {
                        cmd.ExecuteNonQuery();
                        result = true;
                    }
                    catch (Exception)
                    {
                        result = false;
                    }
                    finally
                    {
                        conn.Close();
                    }

                }
            }
            return result;
        }
        public static int GetUserRole(string username)
        {
            int result = 0;
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                string sqlStr = "SELECT ADMIN FROM def_USERS U WHERE U.CODE = @CODE";
                using (SqlCommand cmd = new SqlCommand(sqlStr, conn))
                {
                    cmd.Parameters.AddWithValue("@CODE", username);
                    try
                    {

                        result = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    finally
                    {
                        conn.Close();
                    }
                }

            }
            return result;
        }

        public static void RecieverUpdateMail(Receiver sub, DatasetType dataset)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sqlQuery = "UPDATE " + (dataset == DatasetType.Abone ? "tb_SUBSCRIPTION" : "tb_ACCOUNTS") +
                    " SET KEP_MAIL=@KEP_MAIL, EMail=@EMail, Gsm=@Gsm " +
                    (dataset == DatasetType.Abone ? ", Gsm2=@Gsm2 , Gsm3=@Gsm3, ACCOUNT_EMAIL=@ACCOUNT_EMAIL " : "") +
                     " WHERE ID = @ID";

                /*
                  var rec = new Receiver()
                        {
                            Id = Convert.ToInt32(row["ID"]),
                            Code = row["CODE"].ToString(),
                            Info = row["INFO"].ToString(),
                            KepMail = row["KEP_MAIL"].ToString(),
                            Email = row["EMail"].ToString(),
                            ManagementGsm = row["Gsm"].ToString()
                        };
                        if (refTable == DatasetType.Abone)
                        {
                            rec.AccountGsm = row["Gsm2"].ToString();
                            rec.TechnicGsm = row["Gsm3"].ToString();
                            rec.AccountEmail = row["ACCOUNT_EMAIL"].ToString(); 
                        }
                */

                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@ID", sub.Id);
                    cmd.Parameters.AddWithValue("@KEP_MAIL", sub.KepMail ?? "");
                    cmd.Parameters.AddWithValue("@EMail", sub.Email ?? "");
                    cmd.Parameters.AddWithValue("@Gsm", sub.ManagementGsm ?? "");
                    if (dataset == DatasetType.Abone)
                    {
                        cmd.Parameters.AddWithValue("@Gsm2", sub.AccountGsm ?? "");
                        cmd.Parameters.AddWithValue("@Gsm3", sub.TechnicGsm ?? "");
                        cmd.Parameters.AddWithValue("@ACCOUNT_EMAIL", sub.AccountEmail ?? "");
                    }


                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }
        public static long GetKepUIDLast()
        {
            long result = 0;
            string sqlQuery = @"SELECT VALUE FROM tb_KEP_PARAMETERS WHERE CODE = 'KepUIDStartFrom'";
            using (SqlConnection conn = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    try
                    {
                        conn.Open();
                        result = Convert.ToInt64(cmd.ExecuteScalar());
                    }
                    catch (Exception ex)
                    {

                    }
                    finally
                    {
                        conn.Close();
                    }
                }

            }
            return result;
        }
        public static void SetKepUIDLast(string param)
        {
            string sqlQuery = @"UPDATE tb_KEP_PARAMETERS SET VALUE=@param WHERE CODE='KepUIDStartFrom'";
            using (SqlConnection conn = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@param", param);
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {

                    }
                    finally
                    {
                        conn.Close();
                    }
                }

            }
        }
        public static void UpdateKepStatusByMessageId(string messageId, int status, DateTime statusDate)
        {
            string sqlQuery = @"UPDATE tb_MESSAGER_LOGS SET KEP_STATUS=@status,KEP_STATUS_DATETIME=@date WHERE MESSAGE_ID=@messageId";
            using (SqlConnection conn = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@status", status);
                        cmd.Parameters.AddWithValue("@messageId", messageId);
                        cmd.Parameters.AddWithValue("@date", statusDate);

                        cmd.ExecuteNonQuery();
                    }
                    finally
                    {
                        conn.Close();
                    }
                }

            }
        }
        public static int GetKepStatusByMessageId(string messageId)
        {
            int result = 0;
            string sqlQuery = @"SELECT KEP_STATUS tb_MESSAGER_LOGS WHERE MESSAGE_ID=@messageId";
            using (SqlConnection conn = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@messageId", messageId);
                        try
                        {

                            result = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                        catch (Exception)
                        {
                            result = 0;
                        }
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            return result;
        }

        public static DataTable GetJobNotifyMailUser()
        {
            string cmdSql = @"SELECT * FROM tb_KEP_PARAMETERS WHERE CODE LIKE '%JobEmail%'";
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(cmdSql, conn))
                {
                    using (SqlDataAdapter adab = new SqlDataAdapter(cmd))
                    {
                        conn.Open();
                        try
                        {
                            adab.Fill(dt);
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }
                return dt;
            }
        }
    }
}