using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository;
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

        private readonly UnitOfWork _unitOfWork;
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly UserManager<IdentityUser> _userManager;

        public UsersController(UnitOfWork unitOfWork, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            var listUsers = _unitOfWork.UserRepo.GetAll(includeProperties: "Company").ToList();



            foreach (var user in listUsers)
            {

                var roleName = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();
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
            var user = _unitOfWork.UserRepo.Get(u => u.Id == id);

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

            _unitOfWork.UserRepo.updated(user);
            _unitOfWork.save();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult RoleManagement(string id)
        {


            RoleManagementVM roleManagementVM = new RoleManagementVM()
            {
                applicationUsers = _unitOfWork.UserRepo.Get(u => u.Id == id, includeProperties: "Company"),

                RolesList = _roleManager.Roles.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name

                }),
                CompanyList = _unitOfWork.CompanyRepo.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.CompanyId.ToString()
                })
            };
            roleManagementVM.applicationUsers.Role = _userManager.GetRolesAsync(_unitOfWork.UserRepo.Get(u => u.Id == id)).GetAwaiter().GetResult().FirstOrDefault();
            return View(roleManagementVM);
        }

        [HttpPost]
        [ActionName("RoleManagement")]
        public IActionResult RoleManagementPost(RoleManagementVM roleManagementVM)
        {
            var oldRoleName = _userManager.GetRolesAsync(_unitOfWork.UserRepo.Get(u => u.Id == roleManagementVM.applicationUsers.Id)).GetAwaiter().GetResult().FirstOrDefault();

            var user = _unitOfWork.UserRepo.Get(u => u.Id == roleManagementVM.applicationUsers.Id);

            if (roleManagementVM.applicationUsers.Role != oldRoleName)
            {
                // role has changed

                if (roleManagementVM.applicationUsers.Role == SD.Role_Company)
                {
                    user.CompanyId = roleManagementVM.applicationUsers.CompanyId;
                }
                if (oldRoleName == SD.Role_Company)
                {
                    user.CompanyId = null;
                }
                _unitOfWork.UserRepo.updated(user);
                _unitOfWork.save();

                _userManager.RemoveFromRoleAsync(user, oldRoleName).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(user, roleManagementVM.applicationUsers.Role).GetAwaiter().GetResult();
            }
            else
            {

                if (oldRoleName == SD.Role_Company && user.CompanyId != roleManagementVM.applicationUsers.CompanyId)
                {
                    user.CompanyId = roleManagementVM.applicationUsers.CompanyId;
                    _unitOfWork.UserRepo.updated(user);
                    _unitOfWork.save();
                }

            }
            return RedirectToAction(nameof(Index));
        }
    }
}

