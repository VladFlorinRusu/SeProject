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

            Server server = new Server();
            Contract contract = new Contract(server, CurrentUser.Instance.User, 1);
            server.action();

            if (announcement != null)
            {
                CurrentUser.Instance.User.action();
                announcement.user = CurrentUser.Instance.User;
                announcement.announcementID = AddToFirebase("Announcements/", announcement);
                SetResponse setResponse = client.Set("Announcements/" + announcement.announcementID, announcement);

            }

            return RedirectToAction("LoggedInHome", "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Create(Models.User user)
        {
            client = new FirebaseClient(config);
            FirebaseResponse response = client.Get("Users/");
            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);
            var list = new List<User>();

            if(data != null)
            {
                foreach (dynamic item in data)
                {
                    list.Add((User)JsonConvert.DeserializeObject<User>(((JProperty)item).Value.ToString()));
                }
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
                        user.userID = AddToFirebase("Users/", user);
                        SetResponse setResponse = client.Set("Users/" + user.userID, user);
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
                if (u.email.StartsWith("admin@"))
                {
                    if (u.password.Equals(password) && u.email.Equals(email))
                    {
                        ModelState.AddModelError(string.Empty, "Userul exista");
                        loggedInUser = u;
                        CurrentUser.Instance.User = u;
                        CurrentUser.Instance.IsLogged = true;
                        u.isAuthenticated = true;
                        return RedirectToAction("Admin", "Account");
                    }
                }
                else if (u.password.Equals(password) && u.email.Equals(email))
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

        private String AddToFirebase<T>(String path, T a)
        {
            client = new FirebaseClient(config);
            var data1 = a;

            PushResponse responses = client.Push(path, data1);

            return responses.Result.name;
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

        public ActionResult Search(string searching)
        {
            int addInHistory = 0;
            client = new FirebaseClient(config);
            FirebaseResponse response = client.Get("Announcements");
            var list = new List<Announcement>();
            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);

            if (data != null)
                foreach (dynamic item in data)
                {
                    Announcement a = (Announcement)JsonConvert.DeserializeObject<Announcement>(((JProperty)item).Value.ToString());
                    if (!CurrentUser.Instance.User.userID.Equals(a.user.userID))
                    {
                        if ((string.Equals(a.category, searching, StringComparison.OrdinalIgnoreCase) || (string.Equals(a.text, searching, StringComparison.OrdinalIgnoreCase))
                            && (searching != null)))
                        {
                            list.Add(a);

                            if (addInHistory == 0)
                            {
                                addInHistory = 1;
                                CurrentUser.Instance.User.addNew(a.category);
                            }
                        }
                    }
                }

            if (CurrentUser.Instance.User.history.Count != 0)
            {
                client.Set("Users/" + CurrentUser.Instance.User.userID + "/" + "history/", CurrentUser.Instance.User.history);
            }
            return View(list);
        }

        [HttpGet]
        public ActionResult Request(string id)
        {
            client = new FirebaseClient(config);
            FirebaseResponse response = client.Get("Announcements/" + id);
            Announcement announcement = JsonConvert.DeserializeObject<Announcement>(response.Body);
            Order order = new Order(announcement.user, CurrentUser.Instance.User, announcement, "waiting...");
            //AddToFirebase("Requests/", order);
            order.orderID = AddToFirebase("Requests/", order);
            SetResponse setResponse = client.Set("Requests/" + order.orderID, order);

            return View(announcement);
        }


        [HttpGet]
        public ActionResult ViewRequests()
        {
            client = new FirebaseClient(config);
            FirebaseResponse response = client.Get("Requests/");
            var list = new List<Order>();
            var myReq = new List<Order>();
            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);
            foreach (dynamic item in data)
            {
                list.Add((Order)JsonConvert.DeserializeObject<Order>(((JProperty)item).Value.ToString()));
            }
            foreach (Order order in list)
            {
                if ((order.seller.userID).Equals(CurrentUser.Instance.User.userID))
                {
                    myReq.Add(order);
                }
            }
            return View(myReq);
        }

        [HttpGet]
        public ActionResult EditStatus(string id)
        {
            client = new FirebaseClient(config);
            FirebaseResponse response = client.Get("Requests/" + id);
            Order data = JsonConvert.DeserializeObject<Order>(response.Body);
            return View(data);
        }
        
        [HttpPost]
        public ActionResult EditStatus(Order order)
        {
            client = new FirebaseClient(config);
            SetResponse response = client.Set("Requests/" + order.orderID + "/status/", order.status);
            return RedirectToAction("Home", "Home");
        }

        public ActionResult Admin()
        {
            client = new FirebaseClient(config);
            FirebaseResponse response = client.Get("Announcements/");
            var list = new List<Announcement>();
            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);

            if (data != null)
                foreach (dynamic item in data)
                {
                    Announcement a = (Announcement)JsonConvert.DeserializeObject<Announcement>(((JProperty)item).Value.ToString());
                    list.Add(a);
                }

            FirebaseResponse response2 = client.Get("Users/");
            var list2 = new List<User>();
            dynamic data2 = JsonConvert.DeserializeObject<dynamic>(response2.Body);

            if (data2 != null)
                foreach (dynamic item in data2)
                {
                    User a = (User)JsonConvert.DeserializeObject<User>(((JProperty)item).Value.ToString());
                    list2.Add(a);
                }

            var TupleModel = new Tuple<List<Announcement>, List<User>>(list, list2);

            return View(TupleModel);
        }


        public ActionResult Delete(String userID)
        {

            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Delete("Users/" + userID);

            return RedirectToAction("Admin", "Account");
        }

        public ActionResult Delete2(String annID)
        {
            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Delete("Announcements/" + annID);
            return RedirectToAction("Admin", "Account");
        }

        public ActionResult Delete3(String annID)
        {
            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Delete("Announcements/" + annID);
            return RedirectToAction("MyAccount", "Account");
        }


        public ActionResult MyAccount()
        {
            client = new FirebaseClient(config);

            FirebaseResponse response = client.Get("Announcements/");
            var list = new List<Announcement>();
            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);

            if (data != null)
                foreach (dynamic item in data)
                {
                    Announcement a = (Announcement)JsonConvert.DeserializeObject<Announcement>(((JProperty)item).Value.ToString());
                    list.Add(a);
                }

            var list2 = new List<Announcement>();

            foreach (dynamic i in list)
            {
                if (CurrentUser.Instance.User.email.Equals(i.user.email))
                {
                    list2.Add(i);
                }
            }


            FirebaseResponse response2 = client.Get("Requests/");
            var reqs = new List<Order>();
            dynamic data2 = JsonConvert.DeserializeObject<dynamic>(response2.Body);

            if (data2 != null)
                foreach (dynamic item in data2)
                {
                    Order a = (Order)JsonConvert.DeserializeObject<Order>(((JProperty)item).Value.ToString());
                    reqs.Add(a);
                }


            var reqs2 = new List<Order>();

            foreach (Order i in reqs)
            {
                if (CurrentUser.Instance.User.email.Equals(i.buyer.email))
                {
                    reqs2.Add(i);
                }
            }

            var myTuple = new Tuple<User,List<Announcement>, List<Order>>(CurrentUser.Instance.User,list2, reqs2);

            return View(myTuple);
        }

    }
}