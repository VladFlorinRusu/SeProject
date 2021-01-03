using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SEProject.Models
{
    public class CurrentUser
    {

        private static readonly CurrentUser instance = new CurrentUser();
        private CurrentUser() { }
        public static CurrentUser Instance = instance;
        public User User { get; set; }
        public Boolean IsLogged = false;


    }
}