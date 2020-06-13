﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GraduationProject.Repositry;
using GraduationProject.Models;
using GraduationProject.Areas.Api.VIewModels;

namespace GraduationProject.Areas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepositry _CategoryRepository;
        private readonly IFIlterRepository _FIlterRepository;

        public CategoriesController(ICategoryRepositry categoryRepositry, IFIlterRepository fIlterRepository)
        {
            _CategoryRepository = categoryRepositry;
            _FIlterRepository = fIlterRepository;
        }

        [HttpGet]
        public IActionResult GetCategories()
        {
            var AllCategories = _CategoryRepository.GetAll();
            return Ok(AllCategories);
        }

        [HttpPost]
        public IActionResult GetCategory(int id)
        {
            Category category = _CategoryRepository.Get(id);
            List<Dictionary<string, Object>> filters = _FIlterRepository.GetFilterByCategory(id);
            return Ok(new
            {
                CategoryId = category.Id,
                CategoryName = category.Name,
                AvailableFilters = filters
            });
        }

        [HttpGet]
        [Route("GetFilterProducts")]
        public IActionResult GetFilterProducts([FromQuery] FilterProductViewModel filterProductViewModel)
        {
            string method = " select p.* from UserProduct p ";
            if (filterProductViewModel.Brand != null)
            {
                method = method + " inner join Product g " +
                                  " on p.ProductId = g.Id " +
                                  " inner join dbo.Brand b " +
                                  " on b.Id = g.BrandId and";
                for (int i = 1; i <=  filterProductViewModel.Brand.Count(); i++)
                {
                    method = method + " b.Name='" + filterProductViewModel.Brand[i-1] + "' ";
                    if (filterProductViewModel.Brand.Count() > 1 && i != filterProductViewModel.Brand.Count())
                    {
                        method = method + " or ";
                    }
                }
            }
            if (filterProductViewModel.Condition != null || filterProductViewModel.FromPrice != 0 || filterProductViewModel.ToPrice != 0)
            {
                method = method + " where ";
                if (filterProductViewModel.Condition != null)
                {
                    method = method + " p.condition =";
                    for (int i = 1; i <= filterProductViewModel.Condition.Count(); i++)
                    {
                        if (filterProductViewModel.Condition[i-1] == "New")
                        {
                            method = method + " 0 ";
                        }
                        if (filterProductViewModel.Condition[i-1] == "With Box")
                        {
                            method = method + " 1 ";
                        }
                        if (filterProductViewModel.Condition[i-1] == "Without Box")
                        {
                            method = method + " 2 ";
                        }
                        if (filterProductViewModel.Condition.Count() > 1 && i != filterProductViewModel.Condition.Count())
                        {
                            method = method + " and ";
                        }
                    }
                }
                if (filterProductViewModel.FromPrice != 0)
                {
                    method = method + " and ";
                    method = method + "p.price >" + filterProductViewModel.FromPrice;
                }
                else
                {
                    method = method + " and ";

                    method = method + "p.price > 0";
                }
                if (filterProductViewModel.ToPrice != 0)
                {
                    method = method + " and p.price <" + filterProductViewModel.ToPrice;
                }
            }
            List<UserProduct> products = _FIlterRepository.GetFilterdProducts(method);
            return Ok(products);
        }
    }
}