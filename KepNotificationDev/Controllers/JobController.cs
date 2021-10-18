using DevExpress.Web.Mvc;
using KepNotificationDev.Helpers;
using KepNotificationDev.Helpers.Service;
using KepNotificationDev.Models;
using KepNotificationDev.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace KepNotificationDev.Controllers
{
    [Authorize(Roles ="1,0")]
    public class JobController : Controller
    {
        // GET: Job
        public ActionResult Index()
        {
            var model = DBHelper.GetJobs();
            return View(model);
        }

        public ActionResult EditJob(int Id)
        {
            JobViewModel jobViewModel = new JobViewModel();
            if (Id > 0)
            {
                jobViewModel.Job = DBHelper.JobGetById(Id);
                //int temp = jobViewModel.Job.DateTimesType;
                //int start = (temp / 10)*10, end = start + 10;
                //jobViewModel.TimesTypes = JobService.TimesTypes.Where(m => m.Key > start && m.Key <= end).ToDictionary(t => t.Key, t => t.Value);
            }
            else
            {
                jobViewModel.Job = new Job();
                jobViewModel.Job.Id = -1;
                //jobViewModel.TimesTypes = new Dictionary<int, string>();
            }
            //jobViewModel.Datasets = DBHelper.GetDatasets(DatasetType.Dataset).ToList();
            jobViewModel.RecieverDatasets = DBHelper.GetDatasets().ToList();
            jobViewModel.Templates = DBHelper.GetTemplates().ToList();


            return View("JobEditForm", jobViewModel);
        }
        public ActionResult AddOrUpdateJob(JobViewModel ds)
        {
            var messageChannelIds = CheckBoxListExtension.GetSelectedValues<int>("cbReceiverChannels");
            if (!ModelState.IsValid)
            {
                return View("JobEditForm");
            }
            if(messageChannelIds.Length>0)
            {
                ds.Job.MessageChannels = (MessageChannels)messageChannelIds.Sum();
            }
            if (ds.Job.Id > 0)
            {
                DBHelper.JobUpdate(ds.Job);
                return RedirectToAction("EditJob", new { Id = ds.Job.Id });
            }
            else
            {
                var res = DBHelper.JobInsert(ds.Job);
                if (res.IndexOf("başarılı") > 0)
                {
                    var t = res.Split('#');
                    var addedId = Convert.ToInt32(t[1]);
                    return RedirectToAction("EditJob", new { Id = addedId });
                }
            }

            return View("JobEditForm", ds);
        }
        public ActionResult DeleteJob(int Id)
        {
            DBHelper.JobDelete(Id);
            return RedirectToAction("Index");
        }
        //public ActionResult TimesTypePartial()
        //{
        //    string enumRes = ComboBoxExtension.GetValue<string>("Job.IntervalType");
        //    var intRes = (IntervalTypes)Enum.Parse(typeof(IntervalTypes), enumRes);
        //    JobViewModel model = new JobViewModel();
        //    model.Job = new Job();
        //    switch (intRes)
        //    {

        //        case IntervalTypes.Zaman:
        //            model.TimesTypes = JobService.TimesTypes.Where(m=>m.Key>20 && m.Key <=30).ToDictionary(t=>t.Key,t=>t.Value);
        //            break;
        //        case IntervalTypes.Dönem:
        //            model.TimesTypes = JobService.TimesTypes.Where(m => m.Key > 0 && m.Key <= 10).ToDictionary(t => t.Key, t=>t.Value);
        //            break;
        //        case IntervalTypes.Özel:
        //            model.TimesTypes = JobService.TimesTypes.Where(m => m.Key > 10 && m.Key <= 20).ToDictionary(t => t.Key,t=>t.Value);
        //            break;
        //        default:
        //            model.TimesTypes = new Dictionary<int, string>();
        //            break;
        //    }

        //    return PartialView("TimesTypePartial", model);
        //}
        public ActionResult ConditionTypePartial()
        {
            return PartialView("ConditionTypePartial");
        }
    }
}