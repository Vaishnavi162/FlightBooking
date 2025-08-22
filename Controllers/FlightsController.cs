using FlightBooking.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FlightBooking.Controllers
{
    public class FlightsController : Controller
    {
        UserDataEntities db = new UserDataEntities();
        [HttpGet]
        //public ActionResult Search(string FromCity, string ToCity, DateTime? Date)
        //{
        //    // Pass the search input to the view
        //    ViewBag.FromCity = FromCity;
        //    ViewBag.ToCity = ToCity;
        //    ViewBag.Date = Date?.ToShortDateString();

        //    return View("SearchResults");

        //}

        //[HttpGet]
        //public ActionResult Book(string FromCity, string ToCity, string Date, string Airline, int Price)
        //{
        //    ViewBag.FromCity = FromCity;
        //    ViewBag.ToCity = ToCity;
        //    ViewBag.Date = Date;
        //    ViewBag.Airline = Airline;
        //    ViewBag.Price = Price;

        //    return View("Booking");
        //}


        public ActionResult Search(string FromCity, string ToCity, DateTime Date)
        {
            if (Session["UserName"] == null)
            {
                TempData["ErrorMessage"] = "Please login to search flights.";
                return RedirectToAction("Login", "Account");
            }

            var flights = db.Flights
                            .Where(f => f.Departure == FromCity &&
                                        f.Destination == ToCity &&
                                        f.Date == Date)
                            .ToList();

            ViewBag.FromCity = FromCity;
            ViewBag.ToCity = ToCity;
            ViewBag.Date = Date.ToString("dd-MMM-yyyy");

            return View("SearchResults", flights);
        }
        //    public ActionResult Search(string FromCity, string ToCity, DateTime Date)
        //    {
        //        if (Session["UserName"] == null)
        //        {
        //            TempData["ErrorMessage"] = "Please login to search flights.";
        //            return RedirectToAction("Login", "Account");
        //        }

        //        // ✅ Dummy flights list (without DB)
        //        var flights = new List<FlightBooking.Models.Flight>
        //{
        //    new FlightBooking.Models.Flight { FlightId=1, FlightNo="AI101", Airline="Air India", Departure=FromCity, Destination=ToCity, Price=4500 },
        //    new FlightBooking.Models.Flight { FlightId=2, FlightNo="6E202", Airline="IndiGo", Departure=FromCity, Destination=ToCity, Price=4000 }
        //};

        //        // ✅ User selected values forward karna
        //        ViewBag.FromCity = FromCity;
        //        ViewBag.ToCity = ToCity;
        //        ViewBag.Date = Date.ToString("dd-MMM-yyyy");

        //        return View("SearchResults", flights);
        //    }




        // 📌 Book Page
        [HttpGet]
        public ActionResult Book(int FlightId, string FromCity, string ToCity, string Date,
                         string Airline, decimal Price, int Seats)
        {
            if (Session["UserName"] == null)
            {
                TempData["ErrorMessage"] = "Please login to book flights.";
                return RedirectToAction("Login", "Account");
            }

            // ✅ Flight object create
            var flight = new Flight
            {
                FlightId = FlightId,       // अगर auto increment है तो ये remove करो
                Departure = FromCity,
                Destination = ToCity,
                Date = Convert.ToDateTime(Date),
                Airline = Airline,
                Price = Price,
                TotalSeats = Seats
            };

            // ✅ Save flight in DB
            db.Flights.Add(flight);
            db.SaveChanges();

            // ✅ Pass data to Booking.cshtml
            ViewBag.FlightId = flight.FlightId;
            ViewBag.FromCity = FromCity;
            ViewBag.ToCity = ToCity;
            ViewBag.Date = Date;
            ViewBag.Airline = Airline;
            ViewBag.Price = Price;
            ViewBag.Seats = Seats;
            ViewBag.TotalPrice = Price * Seats;

            return View("Booking");
        }





        //[HttpPost]
        //public ActionResult BookConfirm(string PassengerName, string Email, string Mobile, string FromCity, string ToCity, string Date, string Airline, int Price)
        //{
        //    // Save booking details to DB later
        //    ViewBag.Message = $"Booking Confirmed for {PassengerName}! ({Airline} {FromCity} → {ToCity} on {Date})";
        //    return View("BookingConfirmation");
        //}
        [HttpPost]
        public ActionResult BookConfirm(int FlightId, string PassengerName, string Email, string Mobile, int Seats, decimal Price)
        {
            if (Session["UserId"] == null)
            {
                TempData["ErrorMessage"] = "Please login to book flights.";
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserId"]);

            // ✅ Booking save in DB
            var booking = new Booking
            {
                UserId = userId,
                FlightId = FlightId,
                Seats = Seats,
                Date = DateTime.Now
            };

            db.Bookings.Add(booking);
            db.SaveChanges();

            // ✅ Total Price calculate
            decimal totalPrice = Price * Seats;

            // ✅ ViewBag me data bhejna confirmation ke liye
            ViewBag.PassengerName = PassengerName;
            ViewBag.Email = Email;
            ViewBag.Mobile = Mobile;
            ViewBag.FlightId = FlightId;
            ViewBag.Seats = Seats;
            ViewBag.TotalPrice = totalPrice;
            ViewBag.BookingId = booking.BookingId;   // 👈 ab Payment link me BookingId jayega

            // ✅ Direct Confirmation page show karna (RedirectToAction Nahi!)
            return View("BookConfirmation");
        }



        [HttpPost]
        public ActionResult ConfirmPayment(int FlightId, int Seats, string Date)
        {
            if (Session["UserId"] == null)
            {
                TempData["ErrorMessage"] = "Please login first.";
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserId"]);

            using (var db = new UserDataEntities())
            {
                Booking booking = new Booking
                {
                    UserId = userId,
                    FlightId = FlightId,
                    Seats = Seats,
                    Date = Convert.ToDateTime(Date)
                };

                db.Bookings.Add(booking);
                db.SaveChanges();
            }

            TempData["SuccessMessage"] = "Your booking is confirmed!";
            return RedirectToAction("Bookconfirmation", "Flights");
        }

        public ActionResult PaymentSuccess()
        {
            return View();
        }



    }
}