using DevExpress.DataAccess.Sql;
using DevExpress.Web.Mvc;
using KepNotificationDev.Helpers;
using KepNotificationDev.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KepNotificationDev.Controllers
{
    [Authorize(Roles ="1")]
    public class DatasetController : DevExpress.Web.Mvc.Controllers.QueryBuilderApiController
    {
        public override ActionResult Invoke()
        {
            return base.Invoke();
        }
        public  ActionResult GetQueryBuilder()
        {
            return PartialView("DatasetQueryBuilderPartial");
        }
        //public ActionResult Save()
        //{
        //    var result = QueryBuilderExtension.GetSaveCallbackResult("DatasetQueryBuilder");
        //    SelectQuery query = result.ResultQuery;
        //    string selectStatement = result.SelectStatement;
        //    // ...
        //    return View();
        //}
        public ActionResult Index()
        {
            //var db = new ABYSIS_DB1Entities();
            //var model=db.tb_KEP_DATASETS.ToList();
            var model = DBHelper.GetDatasets();
            return View(model);
        }

        public ActionResult EditDataset(int Id)
        {
            Datasets editingDataset = DBHelper.DatasetGetById(Id);
            if (editingDataset == null)
            {
                editingDataset = new Datasets();
                editingDataset.Id = -1;
            }
            else
            {
                if(editingDataset.ReferanceTable== "tb_SUBSCRIPTION")
                {
                    editingDataset.DatasetType = Helpers.Service.DatasetType.Abone;
                }
                else if(editingDataset.ReferanceTable== "tb_ACCOUNTS")
                {
                    editingDataset.DatasetType = Helpers.Service.DatasetType.Firma;
                }
                else
                {
                    editingDataset.DatasetType = Helpers.Service.DatasetType.Diğer;
                }
            }
            return View("DatasetEditForm", editingDataset);
        }
        public ActionResult EditDatasetObject(Datasets ds)
        {
            return View("DatasetEditForm", ds);
        }
        public ActionResult AddDataset(Datasets ds)
        {
            if(!string.IsNullOrEmpty(ds.Dataset))
            {
                var query = ds.Dataset.ToUpper();
                if (query.Contains("INSERT") || query.Contains("ALTER") || query.Contains("DELETE") || query.Contains("UPDATE"))
                {
                    return RedirectToAction("EditDatasetObject", ds);
                }
            }
            string res = "";
            if (ds.Id > 0)
            {
                DBHelper.DatasetUpdate(ds);
                return RedirectToAction("EditDataset", new { Id = ds.Id });
            }
            else
            {
                res = DBHelper.DatasetInsert(ds);
            }
            if (res.IndexOf("başarılı") > 0)
            {
                var t = res.Split('#');
                var addedId=Convert.ToInt32(t[1]);
                return RedirectToAction("EditDataset", new { Id = addedId });
            }
            return View();
        }
        public ActionResult DeleteDataset(int Id)
        {
            DBHelper.DatasetDelete(Id);
            return RedirectToAction("Index");
        }
        
        public ActionResult SaveQuery()
        {
            //var result = QueryBuilderExtension.GetSaveCallbackResult("DatasetQueryBuilder");
            //SelectQuery query = result.ResultQuery;
            //string selectStatement = result.SelectStatement;


            var result = QueryBuilderExtension.GetSaveCallbackResult("DatasetQueryBuilder");
            var filterstring = result.ResultQuery.FilterString;
            if (!string.IsNullOrEmpty(filterstring))
            {
                ProcessFilterString(filterstring); 
            }

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Json(new { queryValidationError = result.ErrorMessage });
            }
            else
            {
                return Json(new { resultQuery = result.SelectStatement });
            }
            //SelectQuery query = result.ResultQuery;
            //string selectStatement = result.SelectStatement;
            //return Content(selectStatement);
        }

        private void ProcessFilterString(string filterstring)
        {
            var res = filterstring.Split(new string[] { "And", "Or" }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}