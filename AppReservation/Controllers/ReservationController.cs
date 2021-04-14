using AppReservation.Data;
using AppReservation.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppReservation.ViewModels;
using NToastNotify;

namespace AppReservation.Controllers
{
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _data;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IToastNotification _notification;

        public ReservationController(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IToastNotification toastNotification)
        {
            _data = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _notification = toastNotification;
        }

        //affichage tout les reservation avec filtre de rescount
        
        public IActionResult Idx()
        {

            var list = _data.Reservations.Include(s => s.Student).Include(rt => rt.Reserv)
                
                .OrderBy(c => c.Student.ResCount);
            
            ViewBag.role = new IdentityRole();
            return View(list.ToList().Where(d => d.Date >= DateTime.Today||d.Date.DayOfWeek == DayOfWeek.Saturday || d.Date.DayOfWeek == DayOfWeek.Sunday));
                
        }

        //reservation approuved
        
        public IActionResult Idxx()
        {

            var list = _data.Reservations.Include(s => s.Student).Include(rt => rt.Reserv);

            ViewBag.role = new IdentityRole();
            return View(list.ToList().Where(d => d.Status == "Approved" && d.Date >= DateTime.Today));
        }

        // reservation declined
        public IActionResult Idxxx()
        {
            var list = _data.Reservations.Include(s => s.Student).Include(rt => rt.Reserv);

            ViewBag.role = new IdentityRole();
            return View(list.ToList().Where(d => d.Status == "Declined" && d.Date >= DateTime.Today));
        }

        //reservation pend
        public IActionResult Pend()
        {
            var list = _data.Reservations.Include(s => s.Student).Include(rt => rt.Reserv);

            ViewBag.role = new IdentityRole();
            return View(list.ToList().Where(d => d.Status == "pending" && d.Date >= DateTime.Today));
        }


        //chercher par date
        [HttpPost]
        public ActionResult Index(DateTime? dates)
        {
            var tb_visitas = _data.Reservations
                .Include(s => s.Student)
                .Include(rt => rt.Reserv)
              .Where(t => t.Date == dates);
            return View(tb_visitas.ToList());
        }


        public async Task<IActionResult> Approuved()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Student"))
                {
                    await UserApproved();
                }
                else if (User.IsInRole("Admin"))
                {
                    Res_Approved();
                }
                else
                {
                    return NotFound();
                }
            }
            return View();

        }

        public async Task<IActionResult> Declined()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Student"))
                {
                    await GetDataByUser();
                }
                else if (User.IsInRole("Admin"))
                {
                    Idxxx();
                }
                else
                {
                    return NotFound();
                }
            }
            return View();

        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Student"))
                {
                    await GetDataByUser();
                }
                else if (User.IsInRole("Admin"))
                {
                    Idx();
                }
                else
                {
                    return NotFound();
                }
            }
            return View();
        }

        public async Task<IActionResult> GetDataByUser()
        {
            var student = await _userManager.GetUserAsync(HttpContext.User);
            var list = _data.Reservations
                .Include(s => s.Student)
                .Include(rt => rt.Reserv).Where(s => s.StudentId == student.Id);
            return View("Index", list.ToList().OrderBy(d=>d.Date));
        }

        public async Task<IActionResult> UserApproved()
        {
            var student = await _userManager.GetUserAsync(HttpContext.User);
            var list = _data.Reservations
                .Include(s => s.Student)
                .Include(rt => rt.Reserv).Where(s => s.StudentId == student.Id && s.Status== "Approved");
            return View("Index", list.ToList());
        }


        //affichage de tout les reservation approuvé


        public IActionResult Res_Approved()
        {

            var list = _data.Reservations.Include(s => s.Student).Include(rt => rt.Reserv);

            ViewBag.role = new IdentityRole();
            return View(list.ToList().Where(d => d.Status == "Approved" && d.Date >= DateTime.Today));

        }


        // GET: ReservationController/Create
        public ActionResult Create()
        {
            var list = _data.TypeReservations;
            ViewBag.types = list.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Reservation reservation)
        {

            if (ModelState.IsValid)
            {
                var type = _data.TypeReservations.Where(r => r.Id == reservation.ReservId).FirstOrDefault();
                var student = await _userManager.GetUserAsync(HttpContext.User);

                var reser = new Reservation();
                reser.Status = reservation.Status;
                reser.Date = reservation.Date;
                reser.Cause = reservation.Cause;
                reser.StudentId = student.Id;
                reser.ReservId = type.Id;


                _data.Add(reser);

                await _data.SaveChangesAsync();
                return RedirectToAction("index");
            }

            return View(reservation);
        }

        
        public ActionResult Edit(int? id)
        {
            var getid = _data.Reservations.Find(id);
            ViewBag.gettype = _data.TypeReservations.ToList();
            return View(getid);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Reservation reservation)
        {

            if (ModelState.IsValid)
            {
                var type = _data.TypeReservations.Where(r => r.Id == reservation.ReservId).FirstOrDefault();
                //reservation.ReservationType.Id = type.ToString();
                var student = await _userManager.GetUserAsync(HttpContext.User);
                //var studentId = student.Id;


                reservation.StudentId = student.Id;
                reservation.ReservId = type.Id;

                _data.Update(reservation);
                await _data.SaveChangesAsync();
                return RedirectToAction("index");
            }

            return View(reservation);
        }

        public IActionResult Delete(int? id)
        {
            var list = _data.Reservations.Include(s => s.Student).Include(rt => rt.Reserv);
            ViewBag.data = list.AsEnumerable();
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            var del = _data.Reservations.Find(id);
            return View(del);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {

            var del = _data.Reservations.Find(id);
            _data.Reservations.Remove(del);
            _data.SaveChanges();
            return RedirectToAction("Index");
        }

        public void Increment(int id)
        {
            var usr = _data.Reservations.Find(id);

            //var student = await _userManager.GetUserAsync(HttpContext.User);
            var u = _data.Students.FirstOrDefault(s => s.Id == usr.StudentId);
            var inc = usr.Student.ResCount;
            u.ResCount = inc + 1;

            _data.Update(usr);
            _data.Update(u);
            _data.SaveChanges();
        }

        public async Task<IActionResult> Confirm(int id)
        {
            var resr = _data.Reservations.Find(id);
            if(resr.Status != "Approved")
            {
                Increment(id);
                //var app = new Reservation();
                resr.Status = "Approved";
                _data.Update(resr);
                await _data.SaveChangesAsync();
                _notification.AddSuccessToastMessage("Reservation approved");
            }
            else
            {
                _notification.AddErrorToastMessage("Reservation already approved");
            }
            
            return RedirectToAction("index");
        }
    }
}
