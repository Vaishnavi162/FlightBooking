using FlightBooking.Models;
using Microsoft.Owin.Security.Provider;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
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
        public ActionResult Register(UserRegister model, HttpPostedFileBase ProfilePicture)
        {
            if (ModelState.IsValid)
            {
                if (ProfilePicture != null && ProfilePicture.ContentLength > 0)
                {
                    // ✅ Safe filename (extension included)
                    string fileName = Path.GetFileName(ProfilePicture.FileName);

                    // ✅ Upload folder ka path nikaalo
                    string folderPath = Server.MapPath("~/Uploads/Profiles/");

                    // ✅ Agar folder exist nahi karta to create karo
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    // ✅ File save ka full path
                    string filePath = Path.Combine(folderPath, fileName);

                    // ✅ File save karo
                    ProfilePicture.SaveAs(filePath);

                    // ✅ DB me sirf relative path save karo (for <img src>)
                    model.ProfilePicture = "/Uploads/Profiles/" + fileName;
                }

                // ✅ Save user to DB
                db.UserRegisters.Add(model);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Registration successful!";
                return RedirectToAction("Login");
            }

            return View(model);
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
                    // AccountController.cs - Login Success ke baad
                    Session["UserId"] = user.UserId;   // 👈 UserId from UserRegister table
                    Session["UserName"] = user.UserName;

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

        public ActionResult Profile()
        {
            if (Session["UserId"] == null)
            {
                TempData["ErrorMessage"] = "Please login first.";
                return RedirectToAction("Login");
            }

            int userId = Convert.ToInt32(Session["UserId"]);

            var user = db.UserRegisters.Find(userId);

            var bookings = db.Bookings
                             .Include("Flight")
                             .Where(b => b.UserId == userId)
                             .ToList();

            var model = new UserProfileViewModel
            {
                User = user,
                Bookings = bookings
            };

            return View(model);
        }


        [HttpGet]
        public ActionResult EditProfile()
        {
            if (Session["UserId"] == null)
            {
                TempData["ErrorMessage"] = "Please login first.";
                return RedirectToAction("Login");
            }

            int userId = Convert.ToInt32(Session["UserId"]);
            var user = db.UserRegisters.FirstOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found!";
                return RedirectToAction("Login");
            }

            return View(user); // strongly typed model
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile(UserRegister model, HttpPostedFileBase ProfilePicture)
        {
            var userId = Convert.ToInt32(Session["UserId"]);
            var user = db.UserRegisters.Find(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login");
            }

            // Preserve required fields
            user.UserName = string.IsNullOrWhiteSpace(model.UserName) ? user.UserName : model.UserName;
            user.Email = string.IsNullOrWhiteSpace(model.Email) ? user.Email : model.Email;
            user.Password = string.IsNullOrWhiteSpace(model.Password) ? user.Password : model.Password;

            // Optional fields – avoid nulls if DB doesn't allow null
            user.Gender = string.IsNullOrWhiteSpace(model.Gender) ? (user.Gender ?? "") : model.Gender;
            user.Mobile = string.IsNullOrWhiteSpace(model.Mobile) ? (user.Mobile ?? "") : model.Mobile;
            user.State = string.IsNullOrWhiteSpace(model.State) ? (user.State ?? "") : model.State;
            user.City = string.IsNullOrWhiteSpace(model.City) ? (user.City ?? "") : model.City;

            // Profile picture (make sure folder exists)
            if (ProfilePicture != null && ProfilePicture.ContentLength > 0)
            {
                var dir = Server.MapPath("~/Uploads/Profiles");
                if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);

                var original = System.IO.Path.GetFileName(ProfilePicture.FileName);
                var safeName = System.IO.Path.GetFileNameWithoutExtension(original);
                var ext = System.IO.Path.GetExtension(original);
                var final = $"{safeName}_{DateTime.UtcNow.Ticks}{ext}";
                var fullPath = System.IO.Path.Combine(dir, final);

                ProfilePicture.SaveAs(fullPath);
                user.ProfilePicture = "/Uploads/Profiles/" + final;
            }

            // Mark modified (optional if tracking proxies are enabled)
            db.Entry(user).State = System.Data.Entity.EntityState.Modified;

            try
            {
                db.SaveChanges();
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                // Show exactly which property failed and why
                var msgs = ex.EntityValidationErrors
                             .SelectMany(e => e.ValidationErrors)
                             .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
                             .ToList();

                foreach (var m in msgs) ModelState.AddModelError("", m);

                TempData["ErrorMessage"] = "Validation error: " + string.Join(" | ", msgs);
                return View(model);  // show errors on the form
            }
        }

       


    }
}
 

