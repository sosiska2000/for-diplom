using Rockstar.Admin.WPF.Models;
using Rockstar.Admin.WPF.Services.Interfaces;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Rockstar.Admin.WPF.Services
{
    public class ApiServiceTypeService : IServiceTypeService
    {
        private readonly IApiService _apiService;

        public ApiServiceTypeService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<ServiceType>> GetAllAsync()
        {
            try
            {
                Debug.WriteLine("📥 Fetching all service types");
                return await _apiService.GetAsync<List<ServiceType>>("servicetypes") ?? new();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 Error in GetAllAsync: {ex.Message}");
                return new();
            }
        }

        public async Task<List<ServiceType>> GetByDirectionIdAsync(int directionId)
        {
            try
            {
                Debug.WriteLine($"📥 Fetching service types for direction {directionId}");
                return await _apiService.GetAsync<List<ServiceType>>($"servicetypes/by-direction/{directionId}") ?? new();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 Error in GetByDirectionIdAsync: {ex.Message}");
                return new();
            }
        }

        public async Task<ServiceType?> GetByIdAsync(int id)
        {
            try
            {
                Debug.WriteLine($"📥 Fetching service type {id}");
                return await _apiService.GetAsync<ServiceType>($"servicetypes/{id}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 Error in GetByIdAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CreateAsync(ServiceType serviceType)
        {
            try
            {
                Debug.WriteLine($"📤 Creating service type: {serviceType.Name}");

                var createDto = new
                {
                    serviceType.DirectionId,
                    serviceType.Name,
                    serviceType.Description,
                    DefaultDuration = serviceType.DefaultDuration
                };

                var result = await _apiService.PostAsync<ServiceType>("servicetypes", createDto);
                return result != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 Error in CreateAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateAsync(ServiceType serviceType)
        {
            try
            {
                Debug.WriteLine($"📤 Updating service type: {serviceType.Id}");

                var updateDto = new
                {
                    serviceType.Id,
                    serviceType.DirectionId,
                    serviceType.Name,
                    serviceType.Description,
                    DefaultDuration = serviceType.DefaultDuration
                };

                var result = await _apiService.PutAsync<ServiceType>($"servicetypes/{serviceType.Id}", updateDto);
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
                Debug.WriteLine($"📤 Deleting service type: {id}");
                return await _apiService.DeleteAsync($"servicetypes/{id}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 Error in DeleteAsync: {ex.Message}");
                return false;
            }
        }
    }
}