﻿using Microsoft.IdentityModel.Tokens;

namespace HouseRentingSystem.Services.Data.Interfaces
{
    using HouseRentingSystem.Service.Data.Models.House;
    using HouseRentingSystem.Web.ViewModels.House;
    using Web.ViewModels.Home;

    public interface IHouseService
    {
        Task<IEnumerable<IndexViewModel>> LastThreeHousesAsync();

        Task<string> CreateAndReturnIdAsync(HouseFormModel formModel, string agentId);

        Task<AllHousesFilteredAndPagedServiceModel> AllAsync(AllHousesQueryModel queryModel);

        Task<IEnumerable<HouseAllViewModel>> AllByAgentIdAsync(string agentId);

        Task<IEnumerable<HouseAllViewModel>> AllByUserIdAsync(string userId);

        Task<bool> ExistsByIdAsync(string houseId);

        Task<HouseDetailsViewModel?> GetDetailsByIdAsync(string houseId);

        Task<HouseFormModel> GetHouseForEditByIdAsync(string houseId);

        Task<bool> IsAgentWithIdOwnerOfHouseWithIdAsync(string houseId, string agentId);

        Task EditHouseByIdAndFormModel(string houseId, HouseFormModel formModel);
    }
}
