﻿namespace HouseRentingSystem.Services.Data
{
    using Microsoft.EntityFrameworkCore;

    using HouseRentingSystem.Data;
    using Interfaces;
    using Web.ViewModels.Home;
    using HouseRentingSystem.Web.ViewModels.House;
    using HouseRentingSystem.Data.Models;
    using HouseRentingSystem.Service.Data.Models.House;
    using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
    using HouseRentingSystem.Web.ViewModels.House.Enums;

    public class HouseService : IHouseService
    {
        private readonly HouseRentingDbContext dbContext;

        public HouseService(HouseRentingDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IEnumerable<IndexViewModel>> LastThreeHousesAsync()
        {
            IEnumerable<IndexViewModel> lastThreeHouses = await this.dbContext
                .Houses
                .Where(h => h.IsActive)
                .OrderByDescending(h => h.CreatedOn)
                .Take(3)
                .Select(h => new IndexViewModel()
                {
                    Id = h.Id.ToString(),
                    Title = h.Title,
                    ImageUrl = h.ImageUrl
                })
                .ToArrayAsync();

            return lastThreeHouses;
        }

        public async Task CreateAsync(HouseFormModel formModel, string agentId)
        {
            House newHouse = new House
            {
                Title = formModel.Title,
                Address = formModel.Address,
                Description = formModel.Description,
                ImageUrl = formModel.ImageUrl,
                PricePerMonth = formModel.PricePerMonth,
                CategoryId = formModel.CategoryId,
                AgentId = Guid.Parse(agentId)
            };

            await this.dbContext.Houses.AddAsync(newHouse);
            await this.dbContext.SaveChangesAsync();
        }

        public async Task<AllHousesFilteredAndPagedServiceModel> AllAsync(AllHousesQueryModel queryModel)
        {
            IQueryable<House> housesQuery = this.dbContext
                .Houses
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(queryModel.Category))
            {
                housesQuery = housesQuery
                    .Where(h => h.Category.Name == queryModel.Category);
            }

            if (!string.IsNullOrWhiteSpace(queryModel.SearchString))
            {
                string wildCard = $"%{queryModel.SearchString.ToLower()}%";

                housesQuery = housesQuery
                    .Where(h => EF.Functions.Like(h.Title, wildCard) ||
                                EF.Functions.Like(h.Address, wildCard) ||
                                EF.Functions.Like(h.Description, wildCard));
            }

            housesQuery = queryModel.HouseSorting switch
            {
                HouseSorting.Newest => housesQuery
                    .OrderByDescending(h => h.CreatedOn),
                HouseSorting.Oldest => housesQuery
                    .OrderBy(h => h.CreatedOn),
                HouseSorting.PriceAscending => housesQuery
                    .OrderBy(h => h.PricePerMonth),
                HouseSorting.PriceDescending => housesQuery
                    .OrderByDescending(h => h.PricePerMonth),
                _ => housesQuery
                    .OrderBy(h => h.RenterId != null)
                    .ThenByDescending(h => h.CreatedOn)
            };

            IEnumerable<HouseAllViewModel> allHouses = await housesQuery
                .Where(h => h.IsActive)
                .Skip((queryModel.CurrentPage - 1) * queryModel.HousesPerPage)
                .Take(queryModel.HousesPerPage)
                .Select(h => new HouseAllViewModel
                {
                    Id = h.Id.ToString(),
                    Title = h.Title,
                    Address = h.Address,
                    ImageUrl = h.ImageUrl,
                    PricePerMonth = h.PricePerMonth,
                    IsRented = h.RenterId.HasValue
                })
                .ToArrayAsync();
            int totalHouses = housesQuery.Count();

            return new AllHousesFilteredAndPagedServiceModel()
            {
                TotalHousesCount = totalHouses,
                Houses = allHouses
            };
        }

        public async Task<IEnumerable<HouseAllViewModel>> AllByAgentIdAsync(string agentId)
        {
            IEnumerable<HouseAllViewModel> allAgentHouses = await this.dbContext
                .Houses
                .Where(h => h.IsActive && h.AgentId.ToString() == agentId)
                .Select(h => new HouseAllViewModel
                {
                    Id = h.Id.ToString(),
                    Title = h.Title,
                    Address = h.Address,
                    ImageUrl = h.ImageUrl,
                    PricePerMonth = h.PricePerMonth,
                    IsRented = h.RenterId.HasValue
                })
                .ToArrayAsync();

            return allAgentHouses;
        }

        public async Task<IEnumerable<HouseAllViewModel>> AllByUserIdAsync(string userId)
        {
            IEnumerable<HouseAllViewModel> allUserHouses = await this.dbContext
                .Houses
                    .Where(h => h.IsActive &&
                                     h.RenterId.ToString() == userId &&
                                 h.RenterId.HasValue)
                .Select(h => new HouseAllViewModel
                {
                    Id = h.Id.ToString(),
                    Title = h.Title,
                    Address = h.Address,
                    ImageUrl = h.ImageUrl,
                    PricePerMonth = h.PricePerMonth,
                    IsRented = h.RenterId.HasValue
                })
                .ToArrayAsync();

            return allUserHouses;
        }
    }
}
