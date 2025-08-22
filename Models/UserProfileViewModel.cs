using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlightBooking.Models
{
    public class UserProfileViewModel
    {
        public UserRegister User { get; set; }
        public List<Booking> Bookings { get; set; }
    }
}