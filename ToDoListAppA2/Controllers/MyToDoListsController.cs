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
using System.Security.AccessControl;
using ToDoListAppA2.DataAccess.Repository.IRepository;
using ToDoListAppA2.DataAccess.Repository;
using SQLitePCL;
using ToDoListAppA2.Views.ViewModels;


namespace ToDoListAppA2.Controllers
{
    public class MyToDoListsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        // Inject ApplicationDbContent and UserManager into controller
        public MyToDoListsController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
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
            var toDoLists = await _unitOfWork.myToDoLists.GetAllUserToDoListsAsync(currentUserId);

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
                    
                    doc.Add(new Paragraph()
                        .Add(new Text("Archived: ").SetFont(boldFont))
                        .Add(new Text(list.Archived.ToString()).SetFont(regularFont)));

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
            var currentUserId = _userManager.GetUserId(User);

            var unarchivedLists = await _unitOfWork.myToDoLists.GetUnarchivedUserToDoListsAsync(currentUserId);
            var archivedLists = await _unitOfWork.myToDoLists.GetArchivedUserToDoListsAsync(currentUserId);

            var viewModel = new ToDoListIndexViewModel
            {
                UnarchivedLists = unarchivedLists,
                ArchivedLists = archivedLists
            };

            return View(viewModel);
        }

        // GET: ToDoLists/Create
        public async Task<IActionResult> Create()
        {
            var currentUserId = _userManager.GetUserId(User);

            // Restrict normal user to create only 5 To-Do lists
            if (User.IsInRole("Normal"))
            {
                var amountOfToDoLists = await _unitOfWork.myToDoLists.CountUserToDoListsAsync(currentUserId);

                if (amountOfToDoLists >= 5)
                {
                    TempData["ErrorMessage"] = "Normal users can only create up to 5 To-Do Lists.";
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
                await _unitOfWork.myToDoLists.AddAsync(toDoList);
                await _unitOfWork.SaveAsync();
                TempData["SuccessMessage"] = "To-Do List created successfully!";
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

            var toDoList = await _unitOfWork.myToDoLists.GetByIdAsync(id.Value);

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

            var existingToDoList = await _unitOfWork.myToDoLists.GetByIdAsync(id);

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
            } 
            else
            {
                // if the user is a shared user, allow edits
                bool isSharedWithUser = await _unitOfWork.myToDoLists.IsSharedWithUserAsync(id, currentUserId);

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
                await _unitOfWork.myToDoLists.UpdateAsync(toDoList);
                await _unitOfWork.SaveAsync();
                TempData["SuccessMessage"] = "To-Do List updated successfully!";

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _unitOfWork.myToDoLists.ExistsAsync(id))
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

            var toDoList = await _unitOfWork.myToDoLists.GetByIdAsync(id.Value);

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
            var toDoList = await _unitOfWork.myToDoLists.GetByIdAsync(id);

            if (toDoList != null)
            {
                await _unitOfWork.myToDoLists.DeleteAsync(toDoList);
                await _unitOfWork.SaveAsync();
                TempData["SuccessMessage"] = "To-Do List deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: ToDoLists/Archive
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var toDoList = await _unitOfWork.myToDoLists.GetByIdAsync(id.Value);

            if (toDoList == null)
            {
                return NotFound();
            }

            return View(toDoList);
        }

        // POST: ToDoLists/Archive
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            var toDoList = await _unitOfWork.myToDoLists.GetByIdAsync(id);

            if (toDoList != null)
            {
                toDoList.Archived = true;
                await _unitOfWork.SaveAsync();
                TempData["SuccessMessage"] = "To-Do List successfully archived!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: ToDoLists/Unarchive
        public async Task<IActionResult> Unarchive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var toDoList = await _unitOfWork.myToDoLists.GetByIdAsync(id.Value);

            if (toDoList == null)
            {
                return NotFound();
            }

            return View(toDoList);
        }

        // POST: ToDoLists/Archive
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnarchiveConfirmed(int id)
        {
            var toDoList = await _unitOfWork.myToDoLists.GetByIdAsync(id);

            if (toDoList != null)
            {
                toDoList.Archived = false;
                await _unitOfWork.SaveAsync();
                TempData["SuccessMessage"] = "To-Do List successfully unarchived!";
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
