using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KepNotificationDev.Models
{
    public class Datasets
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Kod")]
        public string Code { get; set; }
        [Display(Name = "Açıklama")]
        public string Info { get; set; }
        [Display(Name = "Veri Kümesi")]
        public string Dataset { get; set; }
        public string ReferanceTable { get; set; }
        public bool Cancelled { get; set; } = false;
        public string DatasetParameters { get; set; }
        public string Created_By { get; set; }
        public DateTime? Created_Date { get; set; }

        public Helpers.Service.DatasetType DatasetType { get; set; }
        //public List<string> Columns { get; set; } = new List<string>();
    }
}