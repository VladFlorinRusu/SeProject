using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace SEProject.Models
{
    public class Announcement
    {
        [DisplayName("Category")]
        public String category { get; set; }

        [DisplayName("Announcement")]
        public String text { get; set; }

        [DisplayName("Price")]
        public float price { get; set; }

        [DisplayName("City")]
        public String city { get; set; }

        public User user { get; set; }

        public string announcementID { get; set; }
    }
}
