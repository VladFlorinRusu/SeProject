using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections;

namespace SEProject.Models
{
    public class User
    {
        public bool isAuthenticated { get; set; }

        public String userID { get; set; }
        public String email { get; set; }
        public String fullName { get; set; }
        public String city { get; set; }
        public String username { get; set; }
        [DataType(DataType.Password)]
        public String password { get; set; }

        [System.ComponentModel.DataAnnotations.Compare("password")]
        [DataType(DataType.Password)]
        public String confirmPassword { get; set; }

        public List<String> history = new List<String>();//put the categories

        public List<String> messages = new List<String>();

        public List<Announcement> announcements = new List<Announcement>();
        
        public Boolean hasCommitted { get; set; } = false;

        public void action()
        {
            this.hasCommitted = true;
        }

        public void addNew(String s)
        {
            if (history.Count == 5)
            {
                history.RemoveAt(0);
                history.Add(s);
            }
            else
            {
                history.Add(s);
            }
        }

        public void addAnnouncement(Announcement a)
        {
            this.announcements.Add(a);
        }

        public void receiveMessage(String newMessage)
        {
            messages.Add(newMessage);
        }

        public void logIn()
        {
            isAuthenticated = true;
        }

        public int computePoints(Announcement a)
        {
            int i = 0;
            if (this.city.Equals(a.city))
                i += 3;
            if(history.Count!=0) //de schimbat
            foreach (String categ in history)
            {
                    if (a.category.Equals(categ))
                    i++;
            }

            return i;
        }
        
    }
}