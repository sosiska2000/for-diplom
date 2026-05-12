package com.example.rockstarmobile.activities;

import android.app.DatePickerDialog;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.provider.MediaStore;
import android.text.InputFilter;
import android.text.TextUtils;
import android.util.Base64;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.app.NotificationCompat;
import androidx.core.app.NotificationManagerCompat;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.example.rockstarmobile.R;
import com.example.rockstarmobile.adapters.SubscriptionCardAdapter;
import com.example.rockstarmobile.models.UpdateProfileDto;
import com.example.rockstarmobile.models.User;
import com.example.rockstarmobile.models.UserSubscriptionDto;
import com.example.rockstarmobile.utils.ApiClient;
import com.example.rockstarmobile.utils.PhoneInputFilter;
import com.example.rockstarmobile.utils.SessionManager;

import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;
import java.util.List;
import java.util.Locale;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class ProfileActivity extends AppCompatActivity {

    private static final int PICK_IMAGE_REQUEST = 1;
    private static final String PREFS_NAME = "notifications_prefs";
    private static final String KEY_LAST_NOTIFICATION_DATE = "last_notification_date";
    private static final String CHANNEL_ID = "subscription_expiring_channel";

    private ImageView ivBack, ivProfilePhoto, ivEditPhoto;
    private TextView tvEmail, tvAge;
    private EditText etFirstName, etLastName, etBirthDate, etPhone;
    private Button btnSave, btnChangePassword, btnLogout;
    private ProgressBar progressBar;
    private RecyclerView rvSubscriptions;
    private LinearLayout llSubscriptionsEmpty;

    private SessionManager sessionManager;
    private ApiClient apiClient;
    private User currentUser;
    private byte[] newPhotoData;

    private SubscriptionCardAdapter subscriptionAdapter;
    private List<UserSubscriptionDto> userSubscriptions = new ArrayList<>();

    private Calendar selectedBirthDate = Calendar.getInstance();
    private SimpleDateFormat dateFormat = new SimpleDateFormat("dd.MM.yyyy", Locale.getDefault());

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_profile);

        sessionManager = new SessionManager(this);
        apiClient = ApiClient.getInstance(this);
        currentUser = sessionManager.getUser();

        initViews();
        setupData();
        setupListeners();
        setupDatePicker();
        createNotificationChannel();

        loadProfileFromServer();
        loadSubscriptions();
        checkExpiringSubscriptions();
    }

    private void initViews() {
        ivBack = findViewById(R.id.ivBack);
        ivProfilePhoto = findViewById(R.id.ivProfilePhoto);
        ivEditPhoto = findViewById(R.id.ivEditPhoto);
        tvEmail = findViewById(R.id.tvEmail);
        tvAge = findViewById(R.id.tvAge);
        etFirstName = findViewById(R.id.etFirstName);
        etLastName = findViewById(R.id.etLastName);
        etBirthDate = findViewById(R.id.etBirthDate);
        etPhone = findViewById(R.id.etPhone);
        btnSave = findViewById(R.id.btnSave);
        btnChangePassword = findViewById(R.id.btnChangePassword);
        btnLogout = findViewById(R.id.btnLogout);
        progressBar = findViewById(R.id.progressBar);
        rvSubscriptions = findViewById(R.id.rvSubscriptions);
        llSubscriptionsEmpty = findViewById(R.id.llSubscriptionsEmpty);

        etPhone.setFilters(new InputFilter[] { new PhoneInputFilter() });
        rvSubscriptions.setLayoutManager(new LinearLayoutManager(this));
    }

    private void setupData() {
        if (currentUser != null) {
            tvEmail.setText(currentUser.getEmail());
            etFirstName.setText(currentUser.getFirstName());
            etLastName.setText(currentUser.getLastName());

            if (currentUser.getAge() > 0) {
                tvAge.setText(currentUser.getAge() + " " + getAgeWord(currentUser.getAge()));
            }

            // Восстанавливаем дату рождения из currentUser
            if (currentUser.getBirthDate() != null) {
                selectedBirthDate.setTime(currentUser.getBirthDate());
                etBirthDate.setText(dateFormat.format(selectedBirthDate.getTime()));
            } else if (currentUser.getAge() > 0) {
                // Если нет даты, но есть возраст, вычисляем примерную дату
                Calendar approxBirthDate = Calendar.getInstance();
                approxBirthDate.add(Calendar.YEAR, -currentUser.getAge());
                selectedBirthDate.setTime(approxBirthDate.getTime());
                etBirthDate.setText(dateFormat.format(selectedBirthDate.getTime()));
            }

            etPhone.setText(currentUser.getPhone());

            if (currentUser.getPhoto() != null && currentUser.getPhoto().length > 0) {
                Bitmap bitmap = BitmapFactory.decodeByteArray(currentUser.getPhoto(), 0, currentUser.getPhoto().length);
                ivProfilePhoto.setImageBitmap(bitmap);
            }
        }
    }

    private void setupDatePicker() {
        Calendar maxDate = Calendar.getInstance();
        maxDate.add(Calendar.YEAR, -14);

        etBirthDate.setOnClickListener(v -> {
            DatePickerDialog datePickerDialog = new DatePickerDialog(
                    ProfileActivity.this,
                    (view, year, month, dayOfMonth) -> {
                        selectedBirthDate.set(year, month, dayOfMonth);
                        etBirthDate.setText(dateFormat.format(selectedBirthDate.getTime()));

                        int age = calculateAge(selectedBirthDate);
                        tvAge.setText(age + " " + getAgeWord(age));
                    },
                    selectedBirthDate.get(Calendar.YEAR),
                    selectedBirthDate.get(Calendar.MONTH),
                    selectedBirthDate.get(Calendar.DAY_OF_MONTH)
            );
            datePickerDialog.getDatePicker().setMaxDate(maxDate.getTimeInMillis());
            datePickerDialog.show();
        });

        etBirthDate.setFocusable(false);
        etBirthDate.setClickable(true);
    }

    private int calculateAge(Calendar birthDate) {
        Calendar today = Calendar.getInstance();
        int age = today.get(Calendar.YEAR) - birthDate.get(Calendar.YEAR);
        if (today.get(Calendar.DAY_OF_YEAR) < birthDate.get(Calendar.DAY_OF_YEAR)) {
            age--;
        }
        return age;
    }

    private String getAgeWord(int age) {
        if (age % 10 == 1 && age % 100 != 11) return "год";
        if (age % 10 >= 2 && age % 10 <= 4 && (age % 100 < 10 || age % 100 >= 20)) return "года";
        return "лет";
    }

    private void createNotificationChannel() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            NotificationChannel channel = new NotificationChannel(
                    CHANNEL_ID,
                    "Уведомления об абонементах",
                    NotificationManager.IMPORTANCE_HIGH
            );
            channel.setDescription("Уведомления о скором окончании абонементов");
            channel.enableVibration(true);

            NotificationManager notificationManager = getSystemService(NotificationManager.class);
            if (notificationManager != null) {
                notificationManager.createNotificationChannel(channel);
            }
        }
    }

    private void showExpiringNotification(String subscriptionName, int daysLeft) {
        String daysWord = getDaysWord(daysLeft);

        NotificationCompat.Builder builder = new NotificationCompat.Builder(this, CHANNEL_ID)
                .setSmallIcon(R.drawable.ic_notification)
                .setContentTitle("Абонемент истекает!")
                .setContentText("Абонемент \"" + subscriptionName + "\" истекает через " + daysLeft + " " + daysWord)
                .setPriority(NotificationCompat.PRIORITY_HIGH)
                .setAutoCancel(true);

        NotificationManagerCompat notificationManager = NotificationManagerCompat.from(this);
        try {
            notificationManager.notify(subscriptionName.hashCode(), builder.build());
        } catch (SecurityException e) {
            Toast.makeText(this,
                    "Абонемент \"" + subscriptionName + "\" истекает через " + daysLeft + " " + daysWord,
                    Toast.LENGTH_LONG).show();
        }
    }

    private String getDaysWord(int days) {
        if (days % 10 == 1 && days % 100 != 11) return "день";
        if (days % 10 >= 2 && days % 10 <= 4 && (days % 100 < 10 || days % 100 >= 20)) return "дня";
        return "дней";
    }

    private boolean shouldShowNotification() {
        SharedPreferences prefs = getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE);
        String lastDate = prefs.getString(KEY_LAST_NOTIFICATION_DATE, "");
        String today = new SimpleDateFormat("yyyy-MM-dd", Locale.getDefault()).format(new Date());

        if (lastDate.equals(today)) {
            return false;
        }

        prefs.edit().putString(KEY_LAST_NOTIFICATION_DATE, today).apply();
        return true;
    }

    private void checkExpiringSubscriptions() {
        apiClient.getApiService().getExpiringSubscriptions(30).enqueue(new Callback<List<UserSubscriptionDto>>() {
            @Override
            public void onResponse(@NonNull Call<List<UserSubscriptionDto>> call, @NonNull Response<List<UserSubscriptionDto>> response) {
                if (response.isSuccessful() && response.body() != null && !response.body().isEmpty()) {
                    boolean shouldNotify = shouldShowNotification();

                    for (UserSubscriptionDto sub : response.body()) {
                        String daysWord = getDaysWord(sub.getDaysLeft());
                        Toast.makeText(ProfileActivity.this,
                                "⚠️ Абонемент \"" + sub.getSubscriptionName() + "\" истекает через " +
                                        sub.getDaysLeft() + " " + daysWord + "!",
                                Toast.LENGTH_LONG).show();

                        if (shouldNotify) {
                            showExpiringNotification(sub.getSubscriptionName(), sub.getDaysLeft());
                        }
                    }
                }
            }

            @Override
            public void onFailure(@NonNull Call<List<UserSubscriptionDto>> call, @NonNull Throwable t) {
                // Не показываем ошибку
            }
        });
    }

    private void loadProfileFromServer() {
        progressBar.setVisibility(View.VISIBLE);
        apiClient.getApiService().getProfile().enqueue(new Callback<User>() {
            @Override
            public void onResponse(@NonNull Call<User> call, @NonNull Response<User> response) {
                progressBar.setVisibility(View.GONE);
                if (response.isSuccessful() && response.body() != null) {
                    currentUser = response.body();
                    sessionManager.saveUser(currentUser);
                    setupData();
                }
            }

            @Override
            public void onFailure(@NonNull Call<User> call, @NonNull Throwable t) {
                progressBar.setVisibility(View.GONE);
                Toast.makeText(ProfileActivity.this, "Ошибка загрузки профиля", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void loadSubscriptions() {
        progressBar.setVisibility(View.VISIBLE);

        apiClient.getApiService().getMySubscriptions().enqueue(new Callback<List<UserSubscriptionDto>>() {
            @Override
            public void onResponse(@NonNull Call<List<UserSubscriptionDto>> call, @NonNull Response<List<UserSubscriptionDto>> response) {
                progressBar.setVisibility(View.GONE);

                if (response.isSuccessful() && response.body() != null) {
                    userSubscriptions = response.body();

                    List<UserSubscriptionDto> activeSubscriptions = new ArrayList<>();
                    for (UserSubscriptionDto sub : userSubscriptions) {
                        if (sub.isActive()) {
                            activeSubscriptions.add(sub);
                        }
                    }

                    if (activeSubscriptions.isEmpty()) {
                        llSubscriptionsEmpty.setVisibility(View.VISIBLE);
                        rvSubscriptions.setVisibility(View.GONE);
                    } else {
                        llSubscriptionsEmpty.setVisibility(View.GONE);
                        rvSubscriptions.setVisibility(View.VISIBLE);
                        subscriptionAdapter = new SubscriptionCardAdapter(ProfileActivity.this, activeSubscriptions);
                        rvSubscriptions.setAdapter(subscriptionAdapter);
                    }
                } else {
                    llSubscriptionsEmpty.setVisibility(View.VISIBLE);
                    rvSubscriptions.setVisibility(View.GONE);
                }
            }

            @Override
            public void onFailure(@NonNull Call<List<UserSubscriptionDto>> call, @NonNull Throwable t) {
                progressBar.setVisibility(View.GONE);
                llSubscriptionsEmpty.setVisibility(View.VISIBLE);
                rvSubscriptions.setVisibility(View.GONE);
            }
        });
    }

    private void setupListeners() {
        ivBack.setOnClickListener(v -> finish());
        ivEditPhoto.setOnClickListener(v -> openImagePicker());
        btnSave.setOnClickListener(v -> saveProfile());
        btnChangePassword.setOnClickListener(v -> showChangePasswordDialog());
        btnLogout.setOnClickListener(v -> logout());
    }

    private void openImagePicker() {
        Intent intent = new Intent(Intent.ACTION_PICK, MediaStore.Images.Media.EXTERNAL_CONTENT_URI);
        startActivityForResult(intent, PICK_IMAGE_REQUEST);
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, @Nullable Intent data) {
        super.onActivityResult(requestCode, resultCode, data);

        if (requestCode == PICK_IMAGE_REQUEST && resultCode == RESULT_OK && data != null) {
            Uri imageUri = data.getData();
            try {
                Bitmap bitmap = MediaStore.Images.Media.getBitmap(this.getContentResolver(), imageUri);
                ivProfilePhoto.setImageBitmap(bitmap);

                ByteArrayOutputStream stream = new ByteArrayOutputStream();
                bitmap.compress(Bitmap.CompressFormat.PNG, 100, stream);
                newPhotoData = stream.toByteArray();

            } catch (IOException e) {
                Toast.makeText(this, "Ошибка загрузки фото", Toast.LENGTH_SHORT).show();
            }
        }
    }

    private String cleanPhoneNumber(String phone) {
        if (TextUtils.isEmpty(phone)) return "";

        String cleaned = phone.replaceAll("[^\\d+]", "");

        if (cleaned.startsWith("8") && cleaned.length() == 11) {
            cleaned = "7" + cleaned.substring(1);
        }

        if (cleaned.startsWith("9") && cleaned.length() == 10) {
            cleaned = "7" + cleaned;
        }

        return cleaned;
    }

    private boolean isValidPhoneNumber(String phone) {
        String cleaned = cleanPhoneNumber(phone);

        if (cleaned.length() != 11) {
            return false;
        }

        if (!cleaned.startsWith("7")) {
            return false;
        }

        return cleaned.matches("\\d+");
    }

    private void saveProfile() {
        String firstName = etFirstName.getText().toString().trim();
        String lastName = etLastName.getText().toString().trim();
        String phoneRaw = etPhone.getText().toString().trim();
        String birthDateStr = etBirthDate.getText().toString().trim();

        if (firstName.isEmpty() || lastName.isEmpty()) {
            Toast.makeText(this, "Заполните имя и фамилию", Toast.LENGTH_SHORT).show();
            return;
        }

        if (!TextUtils.isEmpty(phoneRaw)) {
            String phone = cleanPhoneNumber(phoneRaw);
            if (!isValidPhoneNumber(phone)) {
                etPhone.setError("Введите корректный номер телефона");
                Toast.makeText(this, "Номер должен содержать 10-11 цифр", Toast.LENGTH_LONG).show();
                return;
            }
        }

        progressBar.setVisibility(View.VISIBLE);
        btnSave.setEnabled(false);

        UpdateProfileDto dto = new UpdateProfileDto();
        dto.setFirstName(firstName);
        dto.setLastName(lastName);
        dto.setPhone(cleanPhoneNumber(phoneRaw));

        if (!birthDateStr.isEmpty()) {
            int age = calculateAge(selectedBirthDate);
            dto.setAge(age);
            // 👇 ОТПРАВЛЯЕМ ДАТУ РОЖДЕНИЯ НА СЕРВЕР
            dto.setBirthDate(selectedBirthDate.getTime());
        }

        if (newPhotoData != null && newPhotoData.length > 0) {
            String photoBase64 = Base64.encodeToString(newPhotoData, Base64.DEFAULT);
            dto.setPhotoBase64(photoBase64);
        }

        apiClient.getApiService().updateProfile(dto).enqueue(new Callback<User>() {
            @Override
            public void onResponse(@NonNull Call<User> call, @NonNull Response<User> response) {
                progressBar.setVisibility(View.GONE);
                btnSave.setEnabled(true);

                if (response.isSuccessful() && response.body() != null) {
                    currentUser = response.body();
                    sessionManager.saveUser(currentUser);
                    Toast.makeText(ProfileActivity.this, "Профиль обновлен", Toast.LENGTH_SHORT).show();
                    newPhotoData = null;

                    setupData();

                    if (currentUser.getAge() > 0) {
                        tvAge.setText(currentUser.getAge() + " " + getAgeWord(currentUser.getAge()));
                    }
                } else {
                    String errorMsg = "Ошибка обновления";
                    try {
                        if (response.errorBody() != null) {
                            errorMsg = response.errorBody().string();
                        }
                    } catch (Exception e) {
                        e.printStackTrace();
                    }
                    Toast.makeText(ProfileActivity.this, errorMsg, Toast.LENGTH_LONG).show();
                }
            }

            @Override
            public void onFailure(@NonNull Call<User> call, @NonNull Throwable t) {
                progressBar.setVisibility(View.GONE);
                btnSave.setEnabled(true);
                Toast.makeText(ProfileActivity.this, "Ошибка сети: " + t.getMessage(), Toast.LENGTH_LONG).show();
            }
        });
    }

    private void showChangePasswordDialog() {
        AlertDialog.Builder builder = new AlertDialog.Builder(this);
        builder.setTitle("Изменение пароля");

        View dialogView = getLayoutInflater().inflate(R.layout.dialog_change_password, null);
        builder.setView(dialogView);

        EditText etOldPassword = dialogView.findViewById(R.id.etOldPassword);
        EditText etNewPassword = dialogView.findViewById(R.id.etNewPassword);
        EditText etConfirmPassword = dialogView.findViewById(R.id.etConfirmPassword);

        builder.setPositiveButton("Сохранить", (dialog, which) -> {
            String oldPass = etOldPassword.getText().toString().trim();
            String newPass = etNewPassword.getText().toString().trim();
            String confirmPass = etConfirmPassword.getText().toString().trim();

            if (oldPass.isEmpty() || newPass.isEmpty() || confirmPass.isEmpty()) {
                Toast.makeText(this, "Заполните все поля", Toast.LENGTH_SHORT).show();
                return;
            }

            if (newPass.length() < 6) {
                Toast.makeText(this, "Новый пароль должен быть не менее 6 символов", Toast.LENGTH_SHORT).show();
                return;
            }

            if (!newPass.equals(confirmPass)) {
                Toast.makeText(this, "Пароли не совпадают", Toast.LENGTH_SHORT).show();
                return;
            }

            changePassword(oldPass, newPass);
        });

        builder.setNegativeButton("Отмена", null);
        builder.show();
    }

    private void changePassword(String oldPassword, String newPassword) {
        progressBar.setVisibility(View.VISIBLE);

        ApiClient.ChangePasswordRequest request = new ApiClient.ChangePasswordRequest(oldPassword, newPassword);

        apiClient.getApiService().changePassword(request).enqueue(new Callback<Void>() {
            @Override
            public void onResponse(@NonNull Call<Void> call, @NonNull Response<Void> response) {
                progressBar.setVisibility(View.GONE);
                if (response.isSuccessful()) {
                    Toast.makeText(ProfileActivity.this, "Пароль успешно изменен", Toast.LENGTH_SHORT).show();
                } else {
                    Toast.makeText(ProfileActivity.this, "Ошибка смены пароля", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(@NonNull Call<Void> call, @NonNull Throwable t) {
                progressBar.setVisibility(View.GONE);
                Toast.makeText(ProfileActivity.this, "Ошибка сети: " + t.getMessage(), Toast.LENGTH_LONG).show();
            }
        });
    }

    private void logout() {
        new AlertDialog.Builder(this)
                .setTitle("Выход")
                .setMessage("Вы уверены, что хотите выйти?")
                .setPositiveButton("Выйти", (dialog, which) -> {
                    sessionManager.logout();
                    Intent intent = new Intent(ProfileActivity.this, LoginActivity.class);
                    intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
                    startActivity(intent);
                    finish();
                })
                .setNegativeButton("Отмена", null)
                .show();
    }
}