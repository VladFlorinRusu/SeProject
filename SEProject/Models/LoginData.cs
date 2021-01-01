using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SEProject.Models
{
    public class LoginData
    {

        public string email { get; set; }
        [DataType(DataType.Password)]
        public string password { get; set; }



    }
}