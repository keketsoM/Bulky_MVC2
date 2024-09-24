using Bulky.DataAccess.Repository.Interface;
using Bulky.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Newtonsoft.Json;

namespace BulkyWeb.Areas.Customer.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public SearchController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var products = _unitOfWork.ProductRepo.GetAll(includeProperties: "Category");
            return Ok(products);
        }

        //[HttpGet("{id}")]
        //public IActionResult Get(int id)
        //{


        //    if (_unitOfWork.ProductRepo.GetAll(includeProperties: "Category").Any(p => p.Id == id))
        //    {
        //        return NotFound();
        //    }
        //    var products = _unitOfWork.ProductRepo.GetAll(includeProperties: "Category").Where(p => p.Id == id);

        //    return Ok(products);
        //}
        //[HttpGet("{searchTitle}")]
        [HttpPost]
        public IActionResult SearchBooks([FromBody] string searchTitle)
        {
            IEnumerable<Product> products = new List<Product>();

            if (!string.IsNullOrEmpty(searchTitle))
            {
                products = _unitOfWork.ProductRepo.SearchProduct(searchTitle);
            }




            var value = JsonConvert.SerializeObject(products);

            return Ok(value);
        }
    }
}
