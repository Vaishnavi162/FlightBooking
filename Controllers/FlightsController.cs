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

            // ✅ Flights list lo DB se
            var flights = db.Flights
                            .Where(f => f.Departure == FromCity &&
                                        f.Destination == ToCity &&
                                        f.Date == Date)
                            .ToList();

            // ✅ Har flight ke liye available seats calculate karo
            foreach (var flight in flights)
            {
                int bookedSeats = db.Bookings
                                    .Where(b => b.FlightId == flight.FlightId)
                                    .Sum(b => (int?)b.Seats) ?? 0;

                flight.TotalSeats = flight.TotalSeats - bookedSeats; // overwrite available seats
            }

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
                         string Airline, decimal Price, int Seats, string FlightNo)
        {
            if (Session["UserName"] == null)
            {
                TempData["ErrorMessage"] = "Please login to book flights.";
                return RedirectToAction("Login", "Account");
            }

            // ✅ Flight object create
            var flight = new Flight
            {
                FlightId = FlightId,
                FlightNo = FlightNo, // अगर auto increment है तो ये remove करो
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
            ViewBag.FlightNo = flight.FlightNo;
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
            int userId = Convert.ToInt32(Session["UserId"]);

            // ✅ Flight record DB से nikaalo
            var flight = db.Flights.FirstOrDefault(f => f.FlightId == FlightId);
            if (flight == null)
            {
                TempData["ErrorMessage"] = "Flight not found!";
                return RedirectToAction("Search", "Flights");
            }

            // ✅ Booking object save
            var booking = new Booking
            {
                UserId = userId,
                FlightId = FlightId,
                Seats = Seats,
                Date = DateTime.Now
            };

            db.Bookings.Add(booking);
            db.SaveChanges();

            decimal totalPrice = Price * Seats;

            // ✅ ViewBag me data
            ViewBag.PassengerName = PassengerName;
            ViewBag.Email = Email;
            ViewBag.Mobile = Mobile;
            ViewBag.FlightId = FlightId;
            ViewBag.FlightNo = flight.FlightNo;   // 👈 ab flight number ayega
            ViewBag.Seats = Seats;
            ViewBag.TotalPrice = totalPrice;

            return RedirectToAction("BookConfirmation", "Flights", new { bookingId = booking.BookingId });
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

        public ActionResult Details(int id)
        {
            var flight = db.Flights.Find(id);
            return View(flight);
        }

        // GET: Flights/EditBooking/5
        [HttpGet]
        public ActionResult EditBooking(int id)
        {
            var booking = db.Bookings.Include("Flight").FirstOrDefault(b => b.BookingId == id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            return View(booking);
        }

        // POST: Flights/EditBooking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditBooking(Booking model)
        {
            var booking = db.Bookings.Find(model.BookingId);
            if (booking == null)
            {
                return HttpNotFound();
            }

            // Seats update
            booking.Seats = model.Seats;
            booking.Date = model.Date;

            db.Entry(booking).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            TempData["SuccessMessage"] = "Booking updated successfully!";
            return RedirectToAction("Profile", "Account");
        }


        [HttpPost]
        public ActionResult CancelBooking(int bookingId)
        {
            var booking = db.Bookings.Find(bookingId);
            if (booking != null)
            {
                db.Bookings.Remove(booking);
                db.SaveChanges();
            }
            return RedirectToAction("Profile", "Account");
        }


    }
}