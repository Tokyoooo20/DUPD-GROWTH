using Microsoft.AspNetCore.Mvc;
using Models;
using System;
using System.Collections.Generic;

namespace Models
{
    public class Projects
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int Priority { get; set; }
        public DateTime Deadline { get; set; }
        public List<string> Responsibilities { get; set; }
        public List<string> TeamMembers { get; set; }
    }

    public class MyDbContext
    {
        // Implementation of context
    }

    public class Controller
    {
        [HttpPost]
        public IActionResult CreateNewProgram(Projects projects)
        {
            if (ModelState.IsValid)
            {
                MyDbContext db = new MyDbContext();
                db.Projects.Add(projects);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}