﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AppReservation.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public string Cause { get; set; }
        
        [ForeignKey("StudentId,ReservId")]
        public string StudentId { get; set; }
        public Student Student { get; set; }
        
        public TypeReservation Reserv { get; set; }
        [DisplayName("Type of reservation")]
        public int ReservId { get; set; }

    }
}
