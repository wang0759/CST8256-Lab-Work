using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Lab8.Models.DataAccess;
using Lab8.Models;

namespace Lab8.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly StudentRecordContext _context;

        public EmployeesController(StudentRecordContext context)
        {
            _context = context;
        }

        // GET: Employees
        public async Task<IActionResult> Index()
        {
            return View(await _context.Employee.ToListAsync());
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employee
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Employees/Create
        public IActionResult Create()
        { // Return EmployeeRoleSelections model to View
            EmployeeRoleSelections employeeRoleSelections = new EmployeeRoleSelections();
            return View(employeeRoleSelections);
        }

        // POST: Employees/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(EmployeeRoleSelections employeeRoleSelections)
        {
            if (!employeeRoleSelections.roleSelections.Any(m => m.Selected))
            { // Validate RoleSelections
                ModelState.AddModelError("roleSelections", "You must select at least one role!");
            }
            if (_context.Employee.Any(e => e.UserName == employeeRoleSelections.employee.UserName))
            { // Validate user name
                ModelState.AddModelError("employee.UserName", "This user name already exists!");
            }
            if (ModelState.IsValid)
            {
                _context.Add(employeeRoleSelections.employee);
                _context.SaveChanges();
                foreach (RoleSelection roleSelection in employeeRoleSelections.roleSelections)
                { // Add Role Selections
                    if (roleSelection.Selected)
                    {
                        EmployeeRole employeeRole = new EmployeeRole
                        {
                            RoleId = roleSelection.role.Id,
                            EmployeeId = employeeRoleSelections.employee.Id
                        };
                        _context.EmployeeRole.Add(employeeRole);
                    }
                }
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(employeeRoleSelections);
        }
        // Default Async Create Task:
        /*public async Task<IActionResult> Create([Bind("Id,Name,UserName,Password")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }*/

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employee.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            // Return EmployeeRoleSelections model to View
            EmployeeRoleSelections employeeRoleSelections = new EmployeeRoleSelections();
            employeeRoleSelections.employee = employee;
            var ERTable = _context.EmployeeRole
                    .Include(er => er.Role)
                    .Where(er => er.EmployeeId == id);
            // Set Job Title selections
            foreach (RoleSelection roleSelection in employeeRoleSelections.roleSelections)
            {
                if (ERTable.Any(er => er.Role.Id == roleSelection.role.Id))
                {
                    roleSelection.Selected = true;
                }
            }
            return View(employeeRoleSelections);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EmployeeRoleSelections employeeRoleSelections)
        {
            if (!employeeRoleSelections.roleSelections.Any(m => m.Selected))
            { // Validate RoleSelections
                ModelState.AddModelError("roleSelections", "You must select at least one role!");
            }
            if (_context.Employee.Any(e => e.UserName == employeeRoleSelections.employee.UserName
                    && e.Id != employeeRoleSelections.employee.Id))
            { // Validate user name
                ModelState.AddModelError("employee.UserName", "This user name already exists!");
            }
            if (ModelState.IsValid)
            { // Update DB context:
                try
                {
                    _context.Update(employeeRoleSelections.employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employeeRoleSelections.employee.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Reset EmployeeRoles
                var ERTable = _context.EmployeeRole
                    .Where(er => er.EmployeeId == employeeRoleSelections.employee.Id);
                foreach (var roleRow in ERTable)
                {
                    _context.EmployeeRole.Remove(roleRow);
                }
                foreach (RoleSelection roleSelection in employeeRoleSelections.roleSelections)
                { // Add EmployeeRoles
                    if (roleSelection.Selected)
                    {
                        EmployeeRole employeeRole = new EmployeeRole
                        {
                            RoleId = roleSelection.role.Id,
                            EmployeeId = employeeRoleSelections.employee.Id
                        };
                        _context.EmployeeRole.Add(employeeRole);
                    }
                }
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(employeeRoleSelections);
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var employee = await _context.Employee
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employee.FindAsync(id);
            var ERTable = _context.EmployeeRole
                    .Where(er => er.EmployeeId == id);
            foreach (var roleRow in ERTable)
            { // Remove EmployeeRoles to satisfy foreign key constraints
                _context.EmployeeRole.Remove(roleRow);
            }
            _context.Employee.Remove(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employee.Any(e => e.Id == id);
        }
    }
}
