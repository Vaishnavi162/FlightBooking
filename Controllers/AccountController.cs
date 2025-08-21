using FlightBooking.Models;
using Microsoft.Owin.Security.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FlightBooking.Controllers
{
    public class AccountController : Controller
    {
        UserDataEntities db = new UserDataEntities();
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(UserRegister ur)
        {
            if (ModelState.IsValid)
            {
                using (UserDataEntities db = new UserDataEntities())
                {
                    // check for duplicate email
                    if (db.UserRegisters.Any(x => x.Email == ur.Email))
                    {
                        ViewBag.Message = "Email already registered";
                        return View(ur);  // return View with model
                    }

                    db.UserRegisters.Add(ur);
                    db.SaveChanges();
                    Response.Write("<script>alert('Registration Successful')</script>");
                    return RedirectToAction("Login"); // go to Login after success
                }
            }

            
            return View(ur);
        }
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(UserRegister model)
        {
            using (UserDataEntities db = new UserDataEntities())
            {
                var user = db.UserRegisters
                             .FirstOrDefault(x => x.Email == model.Email && x.Password == model.Password);

                if (user != null)
                {
                    // ✅ Session set
                    Session["UserName"] = user.UserName;

                    // ✅ TempData me message save
                    TempData["SuccessMessage"] = "Login Successful! Welcome, " + user.UserName;

                    // ✅ Redirect to Home/Index
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.Message = "Invalid Email or Password";
                    return View();
                }
            }
        }



        public ActionResult Logout()
        {
            // ✅ Username remove only
            Session.Remove("UserName");

            // ✅ Store logout message in Session
            Session["LogoutMessage"] = "You have been logged out";

            return RedirectToAction("Index", "Home");
        }




    }
}
 

