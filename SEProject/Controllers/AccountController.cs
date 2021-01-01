using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Firebase;
using FireSharp.Interfaces;
using FireSharp.Config;
using SEProject.Models;
using FireSharp;
using FireSharp.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SEProject.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account

        FirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "uwtQeq7ddob1fsOgus0EOwnPgFjY9UWuIMGMLBuR",
            BasePath = "https://bvlgdatabase.firebaseio.com/"
        };

        IFirebaseClient client;


        public ActionResult Register()
        {
            return View();
        }

        public ActionResult Offers()
        {
            var user = new AccountController();
            return View(user);
        }
        public ActionResult Index()
        {
            client = new FirebaseClient(config);
            FirebaseResponse response = client.Get("Users/");
            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);
            var list = new List<User>();

            foreach (dynamic item in data)
            {
                list.Add(JsonConvert.DeserializeObject<User>(((JProperty)item).Value.ToString()));
            }
            return View();
        }

        public ActionResult Logoff()
        {

            CurrentUser.Instance.User = null;
            CurrentUser.Instance.IsLogged = false;

            return RedirectToAction("Home", "Home");
        }


        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }


        [HttpGet]
        public ActionResult AddAnnouncement()
        {
            return View();
        }


        [HttpPost]
        public ActionResult AddAnnouncement(Models.Announcement announcement)
        {
            client = new FirebaseClient(config);
            FirebaseResponse response = client.Get("Announcements/");
            var list = new List<Announcement>();
            announcement.user = CurrentUser.Instance.User;
            AddToFirebase("Announcements/", announcement);
            CurrentUser.Instance.User.history.Add(announcement.category);
            return View();

        }

        [HttpPost]
        public ActionResult Create(Models.User user)
        {
            client = new FirebaseClient(config);
            FirebaseResponse response = client.Get("Users/");
            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);
            var list = new List<User>();

            foreach (dynamic item in data)
            {
                list.Add((User)JsonConvert.DeserializeObject<User>(((JProperty)item).Value.ToString()));
            }

            int ok = 1;

            foreach (User u in list)
            {
                if (u.email.Equals(user.email))
                {
                    ok = 0;
                }
            }

            if (ok == 1)
            {
                try
                {

                    if (user.password.Equals(user.confirmPassword))
                    {
                        AddToFirebase("Users/", user);
                        ModelState.AddModelError(string.Empty, "User added succesfully");
                    }

                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                return RedirectToAction("Login", "Account");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "User already exists");
                return View();
            }
        }

        public ActionResult Login(String email, String password)
        {
            client = new FirebaseClient(config);
            FirebaseResponse response = client.Get("Users/");
            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);
            var list = new List<User>();

            foreach (dynamic item in data)
            {
                list.Add((User)JsonConvert.DeserializeObject<User>(((JProperty)item).Value.ToString()));
            }

            User loggedInUser;
            /*var ok = 1;*/
            foreach (User u in list)
            {
                if (u.password.Equals(password) && u.email.Equals(email))
                {
                    ModelState.AddModelError(string.Empty, "Userul exista");
                    loggedInUser = u;
                    CurrentUser.Instance.User = u;
                    CurrentUser.Instance.IsLogged = true;
                    u.isAuthenticated = true;
                    return RedirectToAction("Feed", "Account");
                }
            }

            return View();
        }

        private void AddToFirebase<T>(String path, T a)
        {
            client = new FirebaseClient(config);
            var data1 = a;

            PushResponse responses = client.Push(path, data1);

        }

        public ActionResult Account()
        {
            return View();
        }

        public ActionResult Feed()
        {
            client = new FirebaseClient(config);
            FirebaseResponse response = client.Get("Announcements/");
            var list = new List<Announcement>();
            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);

            if (CurrentUser.Instance.IsLogged == false)
            {
                if (data != null)
                    foreach (dynamic item in data)
                    {

                        list.Add((Announcement)JsonConvert.DeserializeObject<Announcement>(((JProperty)item).Value.ToString()));
                    }
            }
            else
                    if (data != null)
                foreach (dynamic item in data)
                {

                    Announcement a = (Announcement)JsonConvert.DeserializeObject<Announcement>(((JProperty)item).Value.ToString());
                    if (multiCriteria(a))
                        list.Add(a);


                }


            return View(list);
        }




        private bool multiCriteria(Announcement a)

        {
            int points;
            points = CurrentUser.Instance.User.computePoints(a);
            if (points >= 3)
                return true;
            return false;




        }


    }




}