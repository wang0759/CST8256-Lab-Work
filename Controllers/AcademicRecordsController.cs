using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Lab8.Models.DataAccess;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;

namespace Lab8.Controllers
{
    public class AcademicRecordsController : Controller
    {
        private readonly StudentRecordContext _context;

        public AcademicRecordsController(StudentRecordContext context)
        {
            _context = context;
        }
        // Retrieve and sort Academic Records for display:
        private IQueryable<AcademicRecord> sortRecords(string clickedColumn, string previousColumn)
        { // Operate on Iqueryable to export as List or Array
            IQueryable<AcademicRecord> studentRecordContext = _context.AcademicRecord
                .Include(a => a.CourseCodeNavigation)
                .Include(a => a.Student);
            bool reverse = false;
            Expression<Func<AcademicRecord, string>> orderByString = null;
            Expression<Func<AcademicRecord, int>> orderByInt = null;
            switch (clickedColumn)
            {
                case "course":
                    orderByString = r => r.CourseCode;
                    break;
                case "student":
                    orderByString = r => r.StudentId;
                    break;
                case "grade":
                    orderByInt = r => (int)r.Grade;
                    break;
                default:
                    break;
            }
            if (previousColumn != null && previousColumn == clickedColumn)
            { // Reverse record list if necessary
                reverse = true;
                HttpContext.Session.Remove("previousColumn");
            } else if (clickedColumn != null)
            {
                HttpContext.Session.SetString("previousColumn", clickedColumn);
            }
            if (orderByInt != null)
            { // IQueryable.Reverse() struggles with async tasks in this instance
             // Use OrderByDescending to control sort direction instead
                studentRecordContext = reverse
                    ? studentRecordContext.OrderByDescending(orderByInt)
                    : studentRecordContext.OrderBy(orderByInt);
            } else if (orderByString != null)
            { // Sort Grade as type INT but other columns as type STRING
                studentRecordContext = reverse
                    ? studentRecordContext.OrderByDescending(orderByString)
                    : studentRecordContext.OrderBy(orderByString);
            }
            return studentRecordContext;
        }

        // GET: AcademicRecords
        public async Task<IActionResult> Index()
        {
            return View(await sortRecords(
                    HttpContext.Request.Query["clickedColumn"],
                    HttpContext.Session.GetString("previousColumn")
                ).ToListAsync());
        }

        // GET: AcademicRecords/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var academicRecord = await _context.AcademicRecord
                .Include(a => a.CourseCodeNavigation)
                .Include(a => a.Student)
                .FirstOrDefaultAsync(m => m.StudentId == id);
            if (academicRecord == null)
            {
                return NotFound();
            }

            return View(academicRecord);
        }

        // Custom dropdown lists to include course and student names on Create View
        private void populateCreateViewData()
        {
            ViewData["CourseSelectItems"] = new SelectList (
                from c in _context.Course select new {
                    Value = c.Code,
                    Text = c.Code + " - " + c.Title },
                "Value",
                "Text"
            );
            ViewData["StudentSelectItems"] = new SelectList (
                from s in _context.Student select new {
                    Value = s.Id,
                    Text = s.Id + " - " + s.Name },
                "Value",
                "Text"
            );
        }
        // Display student name and course title on Edit View
        private void populateEditViewData(string studentId, string courseCode)
        {
            ViewData["StudentString"] = studentId + " - " + (
                    from s in _context.Student
                    where s.Id.Equals(studentId)
                    select s.Name
                ).FirstOrDefault();
            ViewData["CourseString"] = courseCode + " - " + (
                    from c in _context.Course
                    where c.Code.Equals(courseCode)
                    select c.Title
                ).FirstOrDefault();
        }

        // GET: AcademicRecords/Create
        public IActionResult Create()
        {
            // Include course titles and student names in drop down lists
            populateCreateViewData();

            return View();
        }

        // POST: AcademicRecords/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseCode,StudentId,Grade")] AcademicRecord academicRecord)
        {
            // Check for duplicate records:
            if ((from a in _context.AcademicRecord
                 where a.CourseCode.Equals(academicRecord.CourseCode)
                    && a.StudentId.Equals(academicRecord.StudentId)
                 select a).FirstOrDefault() != null)
            { // Duplicate record exists
                ViewData["Error"] = "Student already has an academic record for the course.";
            }
            else if (ModelState.IsValid)
            {
                _context.Add(academicRecord);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // Include course titles and student names in drop down lists
            populateCreateViewData();

            return View(academicRecord);
        }

        // GET: AcademicRecords/Edit/5
        public async Task<IActionResult> Edit(string studentId, string courseCode)
        {
            if (studentId == null || courseCode == null)
            {
                return NotFound();
            }

            var academicRecord = await _context.AcademicRecord.FindAsync(studentId, courseCode);
            if (academicRecord == null)
            {
                return NotFound();
            }

            // Pass student name and course title to view
            populateEditViewData(studentId, courseCode);

            return View(academicRecord);
        }

        // POST: AcademicRecords/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string studentId, string courseCode, [Bind("CourseCode,StudentId,Grade")] AcademicRecord academicRecord)
        {
            if (studentId != academicRecord.StudentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(academicRecord);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AcademicRecordExists(academicRecord.StudentId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // Pass student name and course title to view
            populateEditViewData(studentId, courseCode);
            
            return View(academicRecord);
        }

        private bool AcademicRecordExists(string id)
        {
            return _context.AcademicRecord.Any(e => e.StudentId == id);
        }
        // GET: AcademicRecords/EditAll
        public async Task<IActionResult> EditAll()
        {
            return View(await sortRecords(
                    HttpContext.Request.Query["clickedColumn"],
                    HttpContext.Session.GetString("previousColumn")
                ).ToArrayAsync());
        }
        // POST: AcademicRecords/EditAll
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAll(AcademicRecord[] academicRecords)
        {
            if (academicRecords != null
                && academicRecords.Length > 0
                && ModelState.IsValid)
            {
                foreach (AcademicRecord record in academicRecords)
                {
                    try
                    {
                        _context.Update(record);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        throw;
                    }
                }
            }
            return View(await sortRecords(
                    HttpContext.Request.Query["clickedColumn"],
                    HttpContext.Session.GetString("previousColumn")
                ).ToArrayAsync());
        }
    }
}
