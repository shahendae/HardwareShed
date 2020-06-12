﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GraduationProject.Areas.Api.ViewModels;
using GraduationProject.ExtenstionMethods;
using GraduationProject.Models;
using GraduationProject.Repositry;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.HttpSys;

namespace GraduationProject.Areas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuyerController : ControllerBase
    {
        private readonly IBuyerRepository _buyerRepository;
        private readonly IMapper _mapper;

        public BuyerController(IBuyerRepository buyerRepository,IMapper mapper)
        {
            this._buyerRepository = buyerRepository;
            this._mapper = mapper;
        }
        [HttpPost(template: "{productId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult AddBuyer(int productId)
        {
            string UserID = User.GetUserIdToken();
          var model =  _buyerRepository.GetByCondition(a => a.UserId == UserID && a.UserProductId == productId);
            if(model.Count() == 0)
            {
                _buyerRepository.Add(new Buys()
                {
                    UserId = UserID,
                    IsSold = false,
                    UserProductId = productId

                    
                });
                _buyerRepository.SaveAll();
                return Ok();
            }
            return NoContent();
            
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet(template: "{productId}")]
        public IActionResult GetBuyersForProduct(int productId)
        {
          
            var model =  _buyerRepository.GetProductBuyers(productId);
            if(model == null)
            {
                return NotFound("Product is not Found !");
            }
            var result = model.Buys.Where(a => a.IsSold == true).Count() == 0 ?
             model.Buys.ToList() : null;
            if (result != null)
            {
            
                return Ok(_mapper.Map<IEnumerable<Buys>, IEnumerable<ProductBuyersViewModel>>(result));
            }
            return BadRequest("The product is sold out .");
            
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut]
        public IActionResult ProductSold(ProductIsSoldViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                _buyerRepository.UserSold(viewModel.ProductID, viewModel.UserID);
                _buyerRepository.SaveAll();
                return Ok();
            }
            return BadRequest("Model is not Valid");

        }

      
    }
}