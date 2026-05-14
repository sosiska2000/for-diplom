using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Rockstar.Admin.WPF.Services.Interfaces;
using System.Diagnostics;

namespace Rockstar.Admin.WPF.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private string? _currentToken;
        private readonly object _tokenLock = new object();
        public string BaseUrl { get; }

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            BaseUrl = _httpClient.BaseAddress?.ToString() ?? "http://localhost:5143/api/";

            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public void SetAuthToken(string? token)
        {
            lock (_tokenLock)
            {
                Debug.WriteLine($"🔑 Setting auth token: {(token != null ? "Token provided" : "Token cleared")}");

                _currentToken = token;

                if (string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = null;
                    Debug.WriteLine("🔑 Auth token cleared");
                }
                else
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                    Debug.WriteLine($"🔑 Auth token set. Header: {_httpClient.DefaultRequestHeaders.Authorization}");
                }
            }
        }

        public string? GetAuthToken()
        {
            lock (_tokenLock)
            {
                return _currentToken;
            }
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                lock (_tokenLock)
                {
                    Debug.WriteLine($"📥 GET {BaseUrl}{endpoint}");
                    Debug.WriteLine($"Current token in ApiService: {(string.IsNullOrEmpty(_currentToken) ? "NULL" : "PRESENT")}");
                    Debug.WriteLine($"🔑 Authorization Header: {_httpClient.DefaultRequestHeaders.Authorization}");
                }

                var response = await _httpClient.GetAsync(endpoint);

                Debug.WriteLine($"📊 Response Status: {(int)response.StatusCode} {response.StatusCode}");

                var json = await response.Content.ReadAsStringAsync();

                if (!string.IsNullOrEmpty(json))
                {
                    Debug.WriteLine($"📄 Response Body: {json}");
                }

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"❌ GET failed: {response.StatusCode}");

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        Debug.WriteLine("🔴 Token expired or invalid!");
                    }

                    return default;
                }

                if (string.IsNullOrEmpty(json))
                {
                    Debug.WriteLine("⚠️ Empty response body");
                    return default;
                }

                return JsonSerializer.Deserialize<T>(json, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"💥 GET HttpRequestException: {ex.Message}");
                return default;
            }
            catch (TaskCanceledException ex)
            {
                Debug.WriteLine($"💥 GET Timeout: {ex.Message}");
                return default;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 GET error: {ex.Message}");
                Debug.WriteLine($"💥 Stack trace: {ex.StackTrace}");
                return default;
            }
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                lock (_tokenLock)
                {
                    Debug.WriteLine($"📤 POST {BaseUrl}{endpoint}");
                    Debug.WriteLine($"Current token in ApiService: {(string.IsNullOrEmpty(_currentToken) ? "NULL" : "PRESENT")}");
                    Debug.WriteLine($"🔑 Authorization Header: {_httpClient.DefaultRequestHeaders.Authorization}");
                }

                var json = JsonSerializer.Serialize(data, _jsonOptions);
                Debug.WriteLine($"📄 Request Body: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);

                Debug.WriteLine($"📊 Response Status: {(int)response.StatusCode} {response.StatusCode}");

                var responseJson = await response.Content.ReadAsStringAsync();

                if (!string.IsNullOrEmpty(responseJson))
                {
                    Debug.WriteLine($"📄 Response Body: {responseJson}");
                }

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"❌ POST failed: {response.StatusCode}");

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        Debug.WriteLine("🔴 Token expired or invalid!");
                    }

                    // 👇 ВЫБРАСЫВАЕМ ИСКЛЮЧЕНИЕ С ТЕКСТОМ ОШИБКИ ОТ СЕРВЕРА
                    throw new HttpRequestException($"Ошибка API: {response.StatusCode} - {responseJson}");
                }

                if (string.IsNullOrEmpty(responseJson))
                {
                    Debug.WriteLine("⚠️ Empty response body");
                    return default;
                }

                return JsonSerializer.Deserialize<T>(responseJson, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"💥 POST HttpRequestException: {ex.Message}");
                throw; // 👇 ПРОБРАСЫВАЕМ ДАЛЬШЕ
            }
            catch (TaskCanceledException ex)
            {
                Debug.WriteLine($"💥 POST Timeout: {ex.Message}");
                throw new Exception("Время ожидания ответа от сервера истекло", ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 POST error: {ex.Message}");
                Debug.WriteLine($"💥 Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<T?> PutAsync<T>(string endpoint, object data)
        {
            try
            {
                lock (_tokenLock)
                {
                    Debug.WriteLine($"📤 PUT {BaseUrl}{endpoint}");
                    Debug.WriteLine($"Current token in ApiService: {(string.IsNullOrEmpty(_currentToken) ? "NULL" : "PRESENT")}");
                    Debug.WriteLine($"🔑 Authorization Header: {_httpClient.DefaultRequestHeaders.Authorization}");
                }

                var json = JsonSerializer.Serialize(data, _jsonOptions);
                Debug.WriteLine($"📄 Request Body: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(endpoint, content);

                Debug.WriteLine($"📊 Response Status: {(int)response.StatusCode} {response.StatusCode}");

                var responseJson = await response.Content.ReadAsStringAsync();

                if (!string.IsNullOrEmpty(responseJson))
                {
                    Debug.WriteLine($"📄 Response Body: {responseJson}");
                }

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"❌ PUT failed: {response.StatusCode}");

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        Debug.WriteLine("🔴 Token expired or invalid!");
                    }

                    // 👇 ВЫБРАСЫВАЕМ ИСКЛЮЧЕНИЕ С ТЕКСТОМ ОШИБКИ ОТ СЕРВЕРА
                    throw new HttpRequestException($"Ошибка API: {response.StatusCode} - {responseJson}");
                }

                if (string.IsNullOrEmpty(responseJson))
                {
                    Debug.WriteLine("⚠️ Empty response body");
                    return default;
                }

                return JsonSerializer.Deserialize<T>(responseJson, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"💥 PUT HttpRequestException: {ex.Message}");
                throw;
            }
            catch (TaskCanceledException ex)
            {
                Debug.WriteLine($"💥 PUT Timeout: {ex.Message}");
                throw new Exception("Время ожидания ответа от сервера истекло", ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 PUT error: {ex.Message}");
                Debug.WriteLine($"💥 Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                lock (_tokenLock)
                {
                    Debug.WriteLine($"📤 DELETE {BaseUrl}{endpoint}");
                    Debug.WriteLine($"Current token in ApiService: {(string.IsNullOrEmpty(_currentToken) ? "NULL" : "PRESENT")}");
                    Debug.WriteLine($"🔑 Authorization Header: {_httpClient.DefaultRequestHeaders.Authorization}");
                }

                var response = await _httpClient.DeleteAsync(endpoint);

                Debug.WriteLine($"📊 Response Status: {(int)response.StatusCode} {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"❌ DELETE failed: {response.StatusCode}");

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        Debug.WriteLine("🔴 Token expired or invalid!");
                    }

                    var errorBody = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Ошибка API: {response.StatusCode} - {errorBody}");
                }

                return true;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"💥 DELETE HttpRequestException: {ex.Message}");
                throw;
            }
            catch (TaskCanceledException ex)
            {
                Debug.WriteLine($"💥 DELETE Timeout: {ex.Message}");
                throw new Exception("Время ожидания ответа от сервера истекло", ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 DELETE error: {ex.Message}");
                Debug.WriteLine($"💥 Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
} 