using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.Interface;
using Bulky.Model;
using Bulky.Model.ViewModel;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Drawing;

namespace BulkyWeb.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UsersController : Controller
    {

        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public UsersController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
            _db = db;
        }

        public IActionResult Index()
        {
            var listUsers = _db.applicationUsers.Include("Company").ToList();

            var Role = _db.Roles.ToList();
            var UserRoles = _db.UserRoles.ToList();

            foreach (var user in listUsers)
            {
                var roleId = UserRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                var roleName = Role.FirstOrDefault(u => u.Id == roleId).Name;
                user.Role = roleName;
                if (user.Company == null)
                {
                    user.Company = new Company()
                    {
                        Name = ""
                    };
                }
            }
            return View(listUsers);
        }
        [HttpPost]
        public IActionResult LockandUnlock(string id)
        {
            var user = _db.applicationUsers.FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }
            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.Now)
            {
                //unlock user
                user.LockoutEnd = DateTime.Now;
                TempData["Success"] = "User Unlocked Successfully";
            }
            else
            {
                //lock user
                user.LockoutEnd = DateTime.Now.AddYears(1000);
                TempData["Success"] = "User Locked Successfully";
            }
            // TempData["Success"] = "User Locked/Unlocked Successfully";
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult RoleManagement(string id)
        {

            var RoleId = _db.UserRoles.FirstOrDefault(u => u.UserId == id).RoleId;
            var roleName = _db.Roles.FirstOrDefault(u => u.Id == RoleId).Name;
            RoleManagementVM roleManagementVM = new RoleManagementVM()
            {
                applicationUsers = _db.applicationUsers.Include("Company").FirstOrDefault(u => u.Id == id),

                RolesList = _db.Roles.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name

                }),
                CompanyList = _db.companys.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.CompanyId.ToString()
                })
            };
            roleManagementVM.applicationUsers.Role = roleName;
            return View(roleManagementVM);
        }

        [HttpPost]
        [ActionName("RoleManagement")]
        public IActionResult RoleManagementPost(RoleManagementVM roleManagementVM)
        {
            var RoleId = _db.UserRoles.FirstOrDefault(u => u.UserId == roleManagementVM.applicationUsers.Id).RoleId;
            var oldRoleName = _db.Roles.FirstOrDefault(u => u.Id == RoleId).Name;
            if (roleManagementVM.applicationUsers.Role != oldRoleName)
            {
                // role has changed
                var user = _db.applicationUsers.FirstOrDefault(u => u.Id == roleManagementVM.applicationUsers.Id);
                if (roleManagementVM.applicationUsers.Role == SD.Role_Company)
                {
                    user.CompanyId = roleManagementVM.applicationUsers.CompanyId;
                }
                if (oldRoleName == SD.Role_Company)
                {
                    user.CompanyId = null;
                }
                _db.SaveChanges();

                _userManager.RemoveFromRoleAsync(user, oldRoleName).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(user, roleManagementVM.applicationUsers.Role).GetAwaiter().GetResult();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

