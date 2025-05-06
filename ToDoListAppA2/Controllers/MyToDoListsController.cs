using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Elfie.Serialization;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using ToDoListAppA2.Data;
using ToDoListAppA2.Models;
using iText.StyledXmlParser.Jsoup.Nodes;


namespace ToDoListAppA2.Controllers
{
    public class MyToDoListsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        // Inject ApplicationDbContent and UserManager into controller
        public MyToDoListsController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _context = context;
            _userManager = userManager;
        }

        // Export current users To-Do Lists as a PDF, Premium only
        public async Task<IActionResult> ExportToPDF()
        {
            // Load fonts for PDF formatting
            PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            PdfFont regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            // Get the current users To-Do Lists
            var currentUserId = _userManager.GetUserId(User);
            var toDoLists = await _context.ToDoLists
                .Where(t => t.UserId == currentUserId)
                .Include(t => t.ToDoListNodes)
                .Include(t => t.SharedWith)
                    .ThenInclude(ts => ts.SharedWithUser)
                .ToListAsync();

            byte[] pdfBytes;

            using (var stream = new MemoryStream())
            {
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var doc = new iText.Layout.Document(pdf);

                // To-Do List main title
                doc.Add(new Paragraph("My To-Do Lists").SetFont(boldFont).SetFontSize(32));

                foreach (var list in toDoLists)
                {
                    // To-Do List title and description
                    doc.Add(new Paragraph()
                        .Add(new Text("Title: ").SetFont(boldFont))
                        .Add(new Text(list.Title).SetFont(regularFont)));

                    doc.Add(new Paragraph()
                        .Add(new Text("Description: ").SetFont(boldFont))
                        .Add(new Text(list.Description).SetFont(regularFont)));

                    // Shared users
                    if (list.SharedWith?.Any() == true)
                    {
                        var userEmails = list.SharedWith
                            .Select(ts => ts.SharedWithUser?.Email ?? "Unknown user");

                        string emailList = string.Join(", ", userEmails);
                        doc.Add(new Paragraph()
                            .Add(new Text("Shared With: ").SetFont(boldFont))
                            .Add(new Text(emailList).SetFont(regularFont)));
                    }
                    else
                    {
                        doc.Add(new Paragraph()
                            .Add(new Text("Shared With: ").SetFont(boldFont))
                            .Add(new Text("None").SetFont(regularFont)));
                    }

                    // To-Do List nodes
                    if (list.ToDoListNodes.Any())
                    {
                        foreach (var item in list.ToDoListNodes)
                        {
                            doc.Add(new Paragraph()
                                .Add(new Text("Title: ").SetFont(boldFont))
                                .Add(new Text(item.Title ?? "No Title").SetFont(regularFont))
                                .SetMarginLeft(20));

                            doc.Add(new Paragraph()
                                .Add(new Text("Description: ").SetFont(boldFont))
                                .Add(new Text(item.Description ?? "No Description").SetFont(regularFont))
                                .SetMarginLeft(20));

                            doc.Add(new Paragraph()
                                .Add(new Text("Due Date: ").SetFont(boldFont))
                                .Add(new Text(item.DueDate?.ToString("dd-MM-yy") ?? "No Due Date").SetFont(regularFont))
                                .SetMarginLeft(20));

                            doc.Add(new Paragraph()
                                .Add(new Text("Status: ").SetFont(boldFont))
                                .Add(new Text(item.Status ?? "No Status").SetFont(regularFont))
                                .SetMarginLeft(20));

                            doc.Add(new Paragraph("\n"));
                        }
                    }
                    else
                    {
                        doc.Add(new Paragraph("No To-Do List items.").SetMarginLeft(20));
                    }
                    doc.Add(new Paragraph("\n"));
                }
                doc.Close();
                pdfBytes = stream.ToArray();
            }
            return File(pdfBytes, "application/pdf", "ToDoLists.pdf");
        }

        // GET: ToDoLists
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var myToDoLists = await _context.ToDoLists
                .Where(t => t.UserId == userId)
                .Include(t => t.User)
                .Include(t => t.SharedWith)
                .ThenInclude(ts => ts.SharedWithUser)
                .ToListAsync();

            return View(myToDoLists);
        }

        // GET: ToDoLists/Create
        public async Task<IActionResult> Create()
        {
            var currentUserId = _userManager.GetUserId(User);

            // Restrict normal user to create only 5 To-Do lists
            if (User.IsInRole("Normal"))
            {
                var amountOfToDoLists = await _context.ToDoLists
                    .CountAsync(t => t.UserId == currentUserId);

                if (amountOfToDoLists >= 5)
                {
                    TempData["CreateError"] = "Normal users can only create up to 5 To-Do Lists.";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View();
        }

        // POST: ToDoLists/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description")] ToDoList toDoList)
        {
            if (ModelState.IsValid)
            {
                toDoList.UserId = _userManager.GetUserId(User);
                _context.Add(toDoList);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(toDoList);
        }

        // GET: ToDoLists/Edit
        public async Task<IActionResult> Edit(int? id, string source)
        {
            if (id == null)
            {
                return NotFound();
            }

            var toDoList = await _context.ToDoLists.FindAsync(id);
            if (toDoList == null)
            {
                return NotFound();
            }
            ViewBag.Source = source;

            return View(toDoList);
        }

        // POST: ToDoLists/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Title,Description")] ToDoList toDoList, string source)
        {
            if (!ModelState.IsValid)
            {
                return View(toDoList);
            }

            var existingToDoList = await _context.ToDoLists.FindAsync(id);
            if (existingToDoList == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);

            // If the user is the owner of the ToDoList, allow edits
            if (existingToDoList.UserId == currentUserId)
            {
                existingToDoList.Title = toDoList.Title;
                existingToDoList.Description = toDoList.Description;
            } else
            {
                // if the user is a shared user, allow edits
                bool isSharedWithUser = await _context.ToDoListShares
                    .AnyAsync(ts => ts.ToDoListId == id && ts.SharedWithUserId == currentUserId);
                if (isSharedWithUser)
                {
                    existingToDoList.Title = toDoList.Title;
                    existingToDoList.Description = toDoList.Description;
                }
                // Do not allow edits from any other type of user
                else
                {
                    return Forbid();
                }
            }

            try
            {
                _context.Update(existingToDoList);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ToDoListExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            ViewBag.Source = source;
            return RedirectToAction("Index", source ?? "MyToDoLists");
        }

        // GET: ToDoLists/Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var toDoList = await _context.ToDoLists
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (toDoList == null)
            {
                return NotFound();
            }

            return View(toDoList);
        }

        // POST: ToDoLists/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var toDoList = await _context.ToDoLists.FindAsync(id);
            if (toDoList != null)
            {
                _context.ToDoLists.Remove(toDoList);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: ToDoList/Share
        public async Task<IActionResult> Share(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var toDoList = await _context.ToDoLists
                .FirstOrDefaultAsync(t => t.Id == id);

            if (toDoList == null)
            {
                return NotFound();
            }

            return View(toDoList);
        }

        // POST: ToDoList/Share
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Share(int id, string email)
        {
            // For displaying Title label in Share view
            var toDoList = await _context.ToDoLists.FirstOrDefaultAsync(t => t.Id == id);
            
            // Ensure To-Do List exists
            if (toDoList == null)
            {
                return NotFound();
            }

            // Check if email is null
            if (string.IsNullOrEmpty(email))
            {
                
                ModelState.AddModelError("Email", "The Email field is required.");
                return View(toDoList);
            }

            // Check if user with email exists
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError("Email", "Could not find a User with that Email.");
                return View(toDoList);
            }

            var currentUserEmail = User.Identity.Name;

            // Ensure user can't share a To-Do List with themselves
            // Compare entered email and current users email with case insensitive match
            if (string.Equals(email, currentUserEmail, StringComparison.OrdinalIgnoreCase))
            {
                // Error messages for user feedback, sent to <span> in Share view
                ModelState.AddModelError("Email", "You can't share a To-Do List with yourself!");
                return View(toDoList);
            }

            // Check if the To-Do list has already been shared with an inputted email
            var isAlreadySharedWithUser = await _context.ToDoListShares
               .AnyAsync(s => s.ToDoListId == id && s.SharedWithUserId == user.Id);

            if (isAlreadySharedWithUser)
            {
                ModelState.AddModelError("Email", "This To-Do List is already shared with that User.");
                return View(toDoList);
            }

            // If checks pass, create a new ToDoListShare object
            var toDoListShare = new ToDoListShare
            {
                ToDoListId = id,
                SharedWithUserId = user.Id
            };

            // Add new ToDoListShare to database
            _context.ToDoListShares.Add(toDoListShare);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: ToDoList/Unshare
        public async Task<IActionResult> Unshare(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var toDoList = await _context.ToDoLists
                .Include(t => t.SharedWith)
                    .ThenInclude(ts => ts.SharedWithUser)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (toDoList == null)
            {
                return NotFound();
            }

            return View(toDoList);
        }

        // POST: ToDoList/Unshare
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unshare(int id, string sharedWithUserId)
        {
            var toDoList = await _context.ToDoLists
                .Include(t => t.SharedWith)
                    .ThenInclude(ts => ts.SharedWithUser)
                .FirstOrDefaultAsync(t => t.Id == id);

            // Ensure To-Do List exists
            if (toDoList == null)
            {
                return NotFound();
            }

            // Ensure User Id of user to remove is not null
            if (string.IsNullOrEmpty(sharedWithUserId))
            {
                ModelState.AddModelError("Email", "Please select a user to unshare the To-Do list from");


                return View(toDoList);
            }

            // Try to obtain the share entry pertaining to the selected ToDoList, and User to remove
            var userShareEntry = await _context.ToDoListShares
                .FirstOrDefaultAsync(ts => ts.ToDoListId == id && ts.SharedWithUserId == sharedWithUserId);

            // Ensure the ToDoListShare entry exists
            if (userShareEntry == null) 
            {
                ModelState.AddModelError("Email", "This To-Do list is not shared with that user!");
                return View(toDoList);
            }

            // If validation is successful, remove the share entry from the selected user
            _context.ToDoListShares.Remove(userShareEntry);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



        private bool ToDoListExists(int id)
        {
            return _context.ToDoLists.Any(t => t.Id == id);
        }
    }
}
