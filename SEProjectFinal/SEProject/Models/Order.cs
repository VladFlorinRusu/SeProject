using System;
using System.ComponentModel;

namespace SEProject.Models
{
    public class Order
    {
        [DisplayName("Seller")]
        public User seller { get; set; }

        [DisplayName("Buyer")]
        public User buyer { get; set; }

        public Announcement announcement;

        [DisplayName("Status")]
        public String status { get; set; }

        public String orderID { get; set; }
        public Order(User u1, User u2, Announcement an, String s)
        {
            this.seller = u1;
            this.buyer = u2;
            this.announcement = an;
            this.status = s;
        }

        public Order(String s)
        {
            this.status = s;
        }

        public Order() { }
    }
}
