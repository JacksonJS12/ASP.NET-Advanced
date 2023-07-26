﻿using HouseRentingSystem.Web.ViewModels.House;

namespace HouseRentingSystem.Service.Data.Models.House
{
    public class AllHousesFilteredAndPagedServiceModel
    {
        public AllHousesFilteredAndPagedServiceModel()
        {
            this.Houses = new HashSet<HouseAllViewModel>();
        }
        public int TotalHousesCount { get; set; }
        public IEnumerable<HouseAllViewModel> Houses { get; set; }
    }
}
