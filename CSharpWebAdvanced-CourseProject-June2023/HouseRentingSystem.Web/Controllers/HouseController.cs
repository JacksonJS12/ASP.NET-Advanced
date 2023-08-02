﻿using System.Xml;
using HouseRentingSystem.Data.Models;

namespace HouseRentingSystem.Web.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Infrastructure.Extensions;

    using HouseRentingSystem.Services.Data.Interfaces;
    using ViewModels.House;

    using static Common.NotificationMessagesConstants;
    using HouseRentingSystem.Service.Data.Models.House;

    [Authorize]
    public class HouseController : Controller
    {
        private readonly ICategoryService categoryService;
        private readonly IAgentService agentService;
        private readonly IHouseService houseService;

        public HouseController(ICategoryService categoryService, IAgentService agentService, IHouseService houseService)
        {
            this.categoryService = categoryService;
            this.agentService = agentService;
            this.houseService = houseService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> All([FromQuery] AllHousesQueryModel queryModel)
        {
            AllHousesFilteredAndPagedServiceModel serviceModel =
                await this.houseService.AllAsync(queryModel);

            queryModel.Houses = serviceModel.Houses;
            queryModel.TotalHouses = serviceModel.TotalHousesCount;
            queryModel.Categories = await this.categoryService.AllCategoryNamesAsync();

            return this.View(queryModel);
        }

        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> Add()
        {
            bool isAgent =
                await this.agentService.AgentExistsByUserIdAsync(this.User.GetId()!);
            if (!isAgent)
            {
                this.TempData[ErrorMessage] = "You must become an agent in order to add new houses!";
                return this.RedirectToAction("Become", "Agent");
            }

            try
            {
                HouseFormModel formModel = new HouseFormModel()
                {
                    Categories = await this.categoryService.AllCategoriesAsync()
                };
                return View(formModel);
            }
            catch (Exception)
            {
                return this.GeneralError();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add(HouseFormModel model)
        {
            bool isAgent =
                await this.agentService.AgentExistsByUserIdAsync(this.User.GetId()!);
            if (!isAgent)
            {
                this.TempData[ErrorMessage] = "You must become an agent in order to add new houses!";
                return this.RedirectToAction("Become", "Agent");
            }
            bool categoryExists =
                await this.categoryService.ExistByIdAsync(model.CategoryId);
            if (!categoryExists)
            {
                // Adding model error to ModelState automatically makes ModelState Invalid
                this.ModelState.AddModelError(nameof(model.CategoryId), "Selected category does not exist!");
            }
            if (!this.ModelState.IsValid)
            {
                model.Categories = await this.categoryService.AllCategoriesAsync();
                return this.View(model);
            }

            try
            {
                string? agentId =
                    await this.agentService.GetAgentIdByUserIdAsync(this.User.GetId()!);
                string houseId = 
                    await this.houseService.CreateAndReturnIdAsync(model, agentId!);

                this.TempData[SuccessMessage] = "House was added successfully!";
                return this.RedirectToAction("Details", "House", new { id = houseId });
            }
            catch (Exception)
            {
                this.ModelState.AddModelError(string.Empty,
                    "Unexpected error occurred while trying to add your new house! Please try again later or contact administrator!");
                model.Categories = await this.categoryService.AllCategoriesAsync();

                return this.View(model);
            }
           
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Details(string id)
        {
            bool houseExists = await this.houseService
                .ExistsByIdAsync(id);
            if (!houseExists)
            {
                this.TempData[ErrorMessage] = "House with the provided id does not exist!";

                return this.RedirectToAction("All", "House");
            }

            try
            {
                HouseDetailsViewModel viwModel = await this.houseService
                    .GetDetailsByIdAsync(id);

                return this.View(viwModel);
            }
            catch (Exception)
            {
                return this.GeneralError();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            bool houseExists = await this.houseService
                .ExistsByIdAsync(id);
            if (!houseExists)
            {
                this.TempData[ErrorMessage] = "House with the provided id does not exist!";

                return this.RedirectToAction("All", "House");
            }

            bool isUserAgent = await this.agentService
                .AgentExistsByUserIdAsync(this.User.GetId()!);
            if (!isUserAgent)
            {
                this.TempData[ErrorMessage] = "You must become an agent in order to edit house info!";

                return this.RedirectToAction("Become", "Agent");
            }

            string? agentId =
                await this.agentService.GetAgentIdByUserIdAsync(this.User.GetId()!);
            bool isAgentOwner = await this.houseService
                .IsAgentWithIdOwnerOfHouseWithIdAsync(id, agentId!);
            if (!isAgentOwner)
            {
                this.TempData[ErrorMessage] = "You must the agent owner of the house you want to edit!";

                return this.RedirectToAction("Mine", "House");
            }

            try
            {
                HouseFormModel formModel = await this.houseService
                    .GetHouseForEditByIdAsync(id);
                formModel.Categories = await this.categoryService.AllCategoriesAsync();

                return this.View(formModel);
            }
            catch (Exception)
            {
                return this.GeneralError();
            }
        }


        [HttpPost]
        public async Task<IActionResult> Edit(string id, HouseFormModel model)
        {
            if (!this.ModelState.IsValid)
            {
                model.Categories = await this.categoryService.AllCategoriesAsync();

                return this.View(model);
            }

            bool houseExists = await this.houseService
                .ExistsByIdAsync(id);
            if (!houseExists)
            {
                this.TempData[ErrorMessage] = "House with the provided id does not exist!";

                return this.RedirectToAction("All", "House");
            }

            bool isUserAgent = await this.agentService
                .AgentExistsByUserIdAsync(this.User.GetId()!);
            if (!isUserAgent)
            {
                this.TempData[ErrorMessage] = "You must become an agent in order to edit house info!";

                return this.RedirectToAction("Become", "Agent");
            }

            string? agentId =
                await this.agentService.GetAgentIdByUserIdAsync(this.User.GetId()!);
            bool isAgentOwner = await this.houseService
                .IsAgentWithIdOwnerOfHouseWithIdAsync(id, agentId!);
            if (!isAgentOwner)
            {
                this.TempData[ErrorMessage] = "You must the agent owner of the house you want to edit!";

                return this.RedirectToAction("Mine", "House");
            }

            try
            {
                await this.houseService.EditHouseByIdAndFormModel(id, model);
            }
            catch (Exception)
            {
                this.ModelState.AddModelError(string.Empty,
                    "Unexpected error occurred while trying to update the house. PLease try again later or contact the administrator!");
                model.Categories = await this.categoryService.AllCategoriesAsync();

                return this.View(model);
            }

            this.TempData[SuccessMessage] = "House was added successfully!";
            return this.RedirectToAction("Details", "House", new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Mine(HouseFormModel model)
        {
            List<HouseAllViewModel> myHouses =
                new List<HouseAllViewModel>();

            string userId = this.User.GetId()!;
            bool isUserAgent = await this.agentService
                .AgentExistsByUserIdAsync(userId);

            try
            {
                if (isUserAgent)
                {
                    string? agentId =
                        await this.agentService.GetAgentIdByUserIdAsync(userId);

                    myHouses.AddRange(await this.houseService.AllByAgentIdAsync((agentId!)));
                }
                else
                {
                    myHouses.AddRange(await this.houseService.AllByUserIdAsync(userId));
                }

                return this.View(myHouses);
            }
            catch (Exception)
            {
                return this.GeneralError();
            }
        }

        private IActionResult GeneralError()
        {
            this.TempData[ErrorMessage] =
                "Unexpected error occurred! Please try again later on contact the administrator!";

            return this.RedirectToAction("Index", "Home");
        }
    }
}