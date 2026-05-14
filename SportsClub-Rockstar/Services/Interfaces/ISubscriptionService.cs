using Rockstar.Admin.WPF.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rockstar.Admin.WPF.Services.Interfaces
{
    public interface ISubscriptionService
    {
        Task<List<Subscription>> GetAllAsync();
        Task<List<Subscription>> GetByDirectionIdAsync(int directionId);
        Task<Subscription?> GetByIdAsync(int id);
        Task<bool> CreateAsync(Subscription subscription);
        Task<bool> UpdateAsync(Subscription subscription);
        Task<bool> DeleteAsync(int id);
        Task<List<SubscriptionPurchaseDto>> GetUserSubscriptionsAsync(int userId);
        Task<bool> PurchaseSubscriptionAsync(int userId, int subscriptionId, DateTime? expiryDate = null);
        Task<bool> UseSessionAsync(int purchaseId);
        Task<bool> AddSessionAsync(int purchaseId);
    }
}