using Rockstar.Admin.WPF.Models;
using Rockstar.Admin.WPF.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rockstar.Admin.WPF.Services
{
    public class ApiSubscriptionService : ISubscriptionService
    {
        private readonly IApiService _apiService;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiSubscriptionService(IApiService apiService)
        {
            _apiService = apiService;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<List<Subscription>> GetAllAsync()
        {
            try
            {
                Debug.WriteLine("📥 Fetching all subscriptions");
                return await _apiService.GetAsync<List<Subscription>>("subscriptions") ?? new List<Subscription>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 Error in GetAllAsync: {ex.Message}");
                return new List<Subscription>();
            }
        }

        public async Task<List<Subscription>> GetByDirectionIdAsync(int directionId)
        {
            try
            {
                Debug.WriteLine($"📥 Fetching subscriptions for direction {directionId}");
                return await _apiService.GetAsync<List<Subscription>>($"subscriptions/by-direction/{directionId}")
                       ?? new List<Subscription>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 Error in GetByDirectionIdAsync: {ex.Message}");
                return new List<Subscription>();
            }
        }

        public async Task<Subscription?> GetByIdAsync(int id)
        {
            try
            {
                Debug.WriteLine($"📥 Fetching subscription {id}");
                return await _apiService.GetAsync<Subscription>($"subscriptions/{id}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 Error in GetByIdAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CreateAsync(Subscription subscription)
        {
            try
            {
                Debug.WriteLine($"📤 Creating subscription: {subscription.Name}");

                var createDto = new
                {
                    subscription.Name,
                    subscription.DirectionId,
                    subscription.Price,
                    subscription.SessionsCount,
                    subscription.Description
                };

                var result = await _apiService.PostAsync<Subscription>("subscriptions", createDto);
                return result != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 Error in CreateAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Subscription subscription)
        {
            try
            {
                Debug.WriteLine($"📤 Updating subscription: {subscription.Id}");

                var updateDto = new
                {
                    subscription.Name,
                    subscription.DirectionId,
                    subscription.Price,
                    subscription.SessionsCount,
                    subscription.Description
                };

                var result = await _apiService.PutAsync<Subscription>($"subscriptions/{subscription.Id}", updateDto);
                return result != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 Error in UpdateAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                Debug.WriteLine($"📤 Deleting subscription: {id}");
                return await _apiService.DeleteAsync($"subscriptions/{id}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 Error in DeleteAsync: {ex.Message}");
                return false;
            }
        }

        // ========== НОВЫЕ МЕТОДЫ ==========

        public async Task<List<SubscriptionPurchaseDto>> GetUserSubscriptionsAsync(int userId)
        {
            try
            {
                Debug.WriteLine($"📥 Fetching subscriptions for user {userId}");
                return await _apiService.GetAsync<List<SubscriptionPurchaseDto>>($"subscriptions/user/{userId}")
                       ?? new List<SubscriptionPurchaseDto>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 Error in GetUserSubscriptionsAsync: {ex.Message}");
                return new List<SubscriptionPurchaseDto>();
            }
        }

        public async Task<bool> PurchaseSubscriptionAsync(int userId, int subscriptionId, DateTime? expiryDate = null)
        {
            try
            {
                Debug.WriteLine($"📤 Purchasing subscription {subscriptionId} for user {userId}");
                var dto = new { UserId = userId, SubscriptionId = subscriptionId, ExpiryDate = expiryDate };
                var result = await _apiService.PostAsync<object>("subscriptions/purchase", dto);
                return result != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 Error in PurchaseSubscriptionAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UseSessionAsync(int purchaseId)
        {
            try
            {
                Debug.WriteLine($"📤 Using session for purchase {purchaseId}");
                var result = await _apiService.PostAsync<object>($"subscriptions/use-session/{purchaseId}", null);
                return result != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 Error in UseSessionAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AddSessionAsync(int purchaseId)
        {
            try
            {
                Debug.WriteLine($"📤 Adding session for purchase {purchaseId}");
                var result = await _apiService.PostAsync<object>($"subscriptions/add-session/{purchaseId}", null);
                return result != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 Error in AddSessionAsync: {ex.Message}");
                return false;
            }
        }
    }
}