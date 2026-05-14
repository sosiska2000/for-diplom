package com.example.rockstarmobile.utils;

import android.content.Context;
import android.util.Log;

import com.example.rockstarmobile.models.AuthResponse;
import com.example.rockstarmobile.models.Direction;
import com.example.rockstarmobile.models.EnrollResponse;
import com.example.rockstarmobile.models.Enrollment;
import com.example.rockstarmobile.models.LoginRequest;
import com.example.rockstarmobile.models.RegisterRequest;
import com.example.rockstarmobile.models.Schedule;
import com.example.rockstarmobile.models.Service;
import com.example.rockstarmobile.models.Subscription;
import com.example.rockstarmobile.models.Trainer;
import com.example.rockstarmobile.models.UpdateProfileDto;
import com.example.rockstarmobile.models.User;
import com.example.rockstarmobile.models.UserSubscriptionDto;
import com.google.gson.Gson;
import com.google.gson.GsonBuilder;

import java.util.List;
import java.util.concurrent.TimeUnit;

import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.logging.HttpLoggingInterceptor;
import retrofit2.Call;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;
import retrofit2.http.Body;
import retrofit2.http.DELETE;
import retrofit2.http.GET;
import retrofit2.http.POST;
import retrofit2.http.PUT;
import retrofit2.http.Path;
import retrofit2.http.Query;

public class ApiClient {
    // ВНИМАНИЕ: порт 5143, НЕ 5000!
    private static final String BASE_URL = "http://10.0.2.2:5143/api/";
    // Для реального устройства: "http://192.168.1.xxx:5143/api/"

    private static final String TAG = "ApiClient";
    private static volatile ApiClient instance;
    private final ApiService apiService;
    private final SessionManager sessionManager;

    public interface ApiService {
        // ============= АУТЕНТИФИКАЦИЯ =============
        @POST("auth/login")
        Call<AuthResponse> login(@Body LoginRequest loginRequest);
        // В интерфейсе ApiService добавьте:
        @GET("schedule/deleted-schedule-ids")
        Call<List<Integer>> getDeletedScheduleIds(@Query("lastChecked") long lastChecked);
        @GET("users/expiring-subscriptions")
        Call<List<UserSubscriptionDto>> getExpiringSubscriptions(@Query("daysThreshold") int daysThreshold);
        @POST("auth/register")
        Call<AuthResponse> register(@Body RegisterRequest registerRequest);

        @GET("auth/check-email")
        Call<Boolean> checkEmail(@Query("email") String email);

        // ============= ПОЛЬЗОВАТЕЛИ =============
        @GET("users/profile")
        Call<User> getProfile();
        // В интерфейсе ApiService добавьте:

        @PUT("users/profile")
        Call<User> updateProfile(@Body UpdateProfileDto dto);

        @POST("users/change-password")
        Call<Void> changePassword(@Body ChangePasswordRequest request);

        @GET("users/my-schedule")
        Call<List<Schedule>> getMySchedule();

        @GET("users/my-subscriptions")
        Call<List<UserSubscriptionDto>> getMySubscriptions();

        // ============= НАПРАВЛЕНИЯ =============
        @GET("directions")
        Call<List<Direction>> getDirections();

        @GET("directions/key/{key}")
        Call<Direction> getDirectionByKey(@Path("key") String key);

        @GET("directions/{id}/services")
        Call<List<Service>> getServices(@Path("id") int directionId);

        @GET("directions/{id}/subscriptions")
        Call<List<Subscription>> getSubscriptions(@Path("id") int directionId);

        @GET("directions/{id}/trainers")
        Call<List<Trainer>> getTrainersByDirection(@Path("id") int directionId);

        // ============= ТРЕНЕРЫ =============
        @GET("trainers")
        Call<List<Trainer>> getTrainers();

        @GET("trainers/{id}")
        Call<Trainer> getTrainer(@Path("id") int id);

        @GET("trainers/{id}/schedule")
        Call<List<Schedule>> getTrainerSchedule(@Path("id") int id);

        // ============= РАСПИСАНИЕ =============
        @GET("schedule")
        Call<List<Schedule>> getSchedule();

        @GET("schedule/group")
        Call<List<Schedule>> getGroupSchedule();

        @GET("schedule/by-date")
        Call<List<Schedule>> getScheduleByDate(@Query("date") String date);

        @GET("schedule/{id}")
        Call<Schedule> getScheduleItem(@Path("id") int id);

        // 👇 ТОЛЬКО ОДИН МЕТОД enrollToSchedule (с возвращаемым типом EnrollResponse)
        @POST("schedule/{id}/enroll")
        Call<EnrollResponse> enrollToSchedule(@Path("id") int scheduleId);

        @DELETE("schedule/{id}/cancel")
        Call<Void> cancelEnrollment(@Path("id") int scheduleId);

        // ============= ЗАПИСИ =============
        @GET("enrollments/my")
        Call<List<Enrollment>> getMyEnrollments();
    }

    // Вспомогательный класс для смены пароля
    public static class ChangePasswordRequest {
        private String currentPassword;
        private String newPassword;

        public ChangePasswordRequest(String currentPassword, String newPassword) {
            this.currentPassword = currentPassword;
            this.newPassword = newPassword;
        }

        public String getCurrentPassword() { return currentPassword; }
        public void setCurrentPassword(String currentPassword) { this.currentPassword = currentPassword; }
        public String getNewPassword() { return newPassword; }
        public void setNewPassword(String newPassword) { this.newPassword = newPassword; }
    }

    private ApiClient(Context context) {
        this.sessionManager = new SessionManager(context);

        // Логирование запросов для отладки
        HttpLoggingInterceptor loggingInterceptor = new HttpLoggingInterceptor();
        loggingInterceptor.setLevel(HttpLoggingInterceptor.Level.BODY);

        OkHttpClient client = new OkHttpClient.Builder()
                .connectTimeout(30, TimeUnit.SECONDS)
                .readTimeout(30, TimeUnit.SECONDS)
                .writeTimeout(30, TimeUnit.SECONDS)
                .addInterceptor(loggingInterceptor)
                .addInterceptor(chain -> {
                    Request original = chain.request();
                    Request.Builder requestBuilder = original.newBuilder()
                            .header("Content-Type", "application/json")
                            .header("Accept", "application/json");

                    // Добавляем токен если пользователь авторизован
                    if (sessionManager.isLoggedIn()) {
                        String token = sessionManager.getToken();
                        if (token != null && !token.isEmpty()) {
                            requestBuilder.header("Authorization", "Bearer " + token);
                            Log.d(TAG, "Added Authorization header with token");
                        }
                    }

                    Request request = requestBuilder.build();
                    Log.d(TAG, "Request URL: " + request.url());
                    return chain.proceed(request);
                })
                .build();

        Gson gson = new GsonBuilder()
                .setLenient()
                .setDateFormat("yyyy-MM-dd'T'HH:mm:ss")
                .create();

        Retrofit retrofit = new Retrofit.Builder()
                .baseUrl(BASE_URL)
                .client(client)
                .addConverterFactory(GsonConverterFactory.create(gson))
                .build();

        this.apiService = retrofit.create(ApiService.class);
    }

    public static synchronized ApiClient getInstance(Context context) {
        if (instance == null) {
            instance = new ApiClient(context.getApplicationContext());
        }
        return instance;
    }

    public ApiService getApiService() {
        return apiService;
    }

    public void refreshToken(String newToken) {
        sessionManager.updateToken(newToken);
    }
}