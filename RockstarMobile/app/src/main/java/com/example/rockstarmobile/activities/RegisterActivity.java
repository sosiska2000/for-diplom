package com.example.rockstarmobile.activities;

import android.app.DatePickerDialog;
import android.content.Intent;
import android.os.Bundle;
import android.text.InputFilter;
import android.text.TextUtils;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import com.example.rockstarmobile.R;
import com.example.rockstarmobile.models.AuthResponse;
import com.example.rockstarmobile.models.RegisterRequest;
import com.example.rockstarmobile.utils.ApiClient;
import com.example.rockstarmobile.utils.PhoneInputFilter;
import com.example.rockstarmobile.utils.SessionManager;

import java.io.IOException;
import java.text.SimpleDateFormat;
import java.util.Calendar;
import java.util.Locale;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class RegisterActivity extends AppCompatActivity {

    private EditText etLastName, etFirstName, etBirthDate, etPhone, etEmail, etPassword, etConfirmPassword;
    private Button btnRegister;
    private TextView tvLogin;
    private ProgressBar progressBar;

    private SessionManager sessionManager;
    private ApiClient apiClient;

    private Calendar selectedBirthDate = Calendar.getInstance();
    private SimpleDateFormat dateFormat = new SimpleDateFormat("dd.MM.yyyy", Locale.getDefault());

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_register);

        sessionManager = new SessionManager(this);
        apiClient = ApiClient.getInstance(this);

        initViews();
        setupListeners();
        setupDatePicker();
    }

    private void initViews() {
        etLastName = findViewById(R.id.etLastName);
        etFirstName = findViewById(R.id.etFirstName);
        etBirthDate = findViewById(R.id.etBirthDate);
        etPhone = findViewById(R.id.etPhone);
        etEmail = findViewById(R.id.etEmail);
        etPassword = findViewById(R.id.etPassword);
        etConfirmPassword = findViewById(R.id.etConfirmPassword);
        btnRegister = findViewById(R.id.btnRegister);
        tvLogin = findViewById(R.id.tvLogin);
        progressBar = findViewById(R.id.progressBar);

        // Применяем фильтр для телефона
        etPhone.setFilters(new InputFilter[] { new PhoneInputFilter() });
    }

    private void setupListeners() {
        btnRegister.setOnClickListener(v -> attemptRegister());
        tvLogin.setOnClickListener(v -> {
            Intent intent = new Intent(RegisterActivity.this, LoginActivity.class);
            startActivity(intent);
            finish();
        });
    }

    private void setupDatePicker() {
        Calendar maxDate = Calendar.getInstance();
        maxDate.add(Calendar.YEAR, -14);

        Calendar minDate = Calendar.getInstance();
        minDate.add(Calendar.YEAR, -120);

        etBirthDate.setOnClickListener(v -> {
            DatePickerDialog datePickerDialog = new DatePickerDialog(
                    RegisterActivity.this,
                    (view, year, month, dayOfMonth) -> {
                        selectedBirthDate.set(year, month, dayOfMonth);
                        etBirthDate.setText(dateFormat.format(selectedBirthDate.getTime()));
                    },
                    selectedBirthDate.get(Calendar.YEAR),
                    selectedBirthDate.get(Calendar.MONTH),
                    selectedBirthDate.get(Calendar.DAY_OF_MONTH)
            );
            datePickerDialog.getDatePicker().setMaxDate(maxDate.getTimeInMillis());
            datePickerDialog.getDatePicker().setMinDate(minDate.getTimeInMillis());
            datePickerDialog.show();
        });

        etBirthDate.setFocusable(false);
        etBirthDate.setClickable(true);
    }

    private int calculateAge() {
        Calendar today = Calendar.getInstance();
        int age = today.get(Calendar.YEAR) - selectedBirthDate.get(Calendar.YEAR);
        if (today.get(Calendar.DAY_OF_YEAR) < selectedBirthDate.get(Calendar.DAY_OF_YEAR)) {
            age--;
        }
        return age;
    }

    // 👇 НОВЫЙ МЕТОД: валидация и очистка номера телефона
    private String cleanPhoneNumber(String phone) {
        if (TextUtils.isEmpty(phone)) return "";

        // Оставляем только цифры и +
        String cleaned = phone.replaceAll("[^\\d+]", "");

        // Если номер начинается с 8, заменяем на 7
        if (cleaned.startsWith("8") && cleaned.length() == 11) {
            cleaned = "7" + cleaned.substring(1);
        }

        // Если номер без кода страны, добавляем 7
        if (cleaned.startsWith("9") && cleaned.length() == 10) {
            cleaned = "7" + cleaned;
        }

        return cleaned;
    }

    // 👇 НОВЫЙ МЕТОД: проверка корректности номера телефона
    private boolean isValidPhoneNumber(String phone) {
        String cleaned = cleanPhoneNumber(phone);

        // Номер должен содержать 11 цифр (после очистки)
        if (cleaned.length() != 11) {
            return false;
        }

        // Первая цифра должна быть 7
        if (!cleaned.startsWith("7")) {
            return false;
        }

        // Проверяем, что все символы - цифры
        return cleaned.matches("\\d+");
    }

    private void attemptRegister() {
        String lastName = etLastName.getText().toString().trim();
        String firstName = etFirstName.getText().toString().trim();
        String birthDateStr = etBirthDate.getText().toString().trim();
        String phoneRaw = etPhone.getText().toString().trim();
        String email = etEmail.getText().toString().trim();
        String password = etPassword.getText().toString().trim();
        String confirmPassword = etConfirmPassword.getText().toString().trim();

        // Валидация
        if (TextUtils.isEmpty(lastName)) {
            etLastName.setError("Введите фамилию");
            return;
        }
        if (TextUtils.isEmpty(firstName)) {
            etFirstName.setError("Введите имя");
            return;
        }
        if (TextUtils.isEmpty(birthDateStr)) {
            etBirthDate.setError("Выберите дату рождения");
            return;
        }
        if (TextUtils.isEmpty(phoneRaw)) {
            etPhone.setError("Введите телефон");
            return;
        }

        // 👇 ПРОВЕРКА НОМЕРА ТЕЛЕФОНА (запрет букв)
        String phone = cleanPhoneNumber(phoneRaw);
        if (!isValidPhoneNumber(phone)) {
            etPhone.setError("Введите корректный номер телефона (10 или 11 цифр)");
            Toast.makeText(this, "Номер должен содержать 10-11 цифр (например: 9123456789 или 79123456789)", Toast.LENGTH_LONG).show();
            return;
        }

        if (TextUtils.isEmpty(email)) {
            etEmail.setError("Введите email");
            return;
        }
        if (!android.util.Patterns.EMAIL_ADDRESS.matcher(email).matches()) {
            etEmail.setError("Введите корректный email");
            return;
        }
        if (password.length() < 6) {
            etPassword.setError("Пароль должен быть не менее 6 символов");
            return;
        }
        if (!password.equals(confirmPassword)) {
            etConfirmPassword.setError("Пароли не совпадают");
            return;
        }

        int age = calculateAge();

        if (age < 14) {
            etBirthDate.setError("Вам должно быть не менее 14 лет");
            return;
        }

        progressBar.setVisibility(View.VISIBLE);
        btnRegister.setEnabled(false);

        RegisterRequest request = new RegisterRequest(
                email, password, firstName, lastName, phone, age
        );
        request.setBirthDate(selectedBirthDate.getTime());

        apiClient.getApiService().register(request).enqueue(new Callback<AuthResponse>() {
            @Override
            public void onResponse(Call<AuthResponse> call, Response<AuthResponse> response) {
                progressBar.setVisibility(View.GONE);
                btnRegister.setEnabled(true);

                if (response.isSuccessful() && response.body() != null) {
                    AuthResponse authResponse = response.body();

                    if (authResponse.isSuccess() && authResponse.getUser() != null) {
                        sessionManager.saveUser(authResponse.getUser(), authResponse.getToken());

                        Toast.makeText(RegisterActivity.this,
                                "Регистрация успешна!", Toast.LENGTH_SHORT).show();

                        Intent intent = new Intent(RegisterActivity.this, MainActivity.class);
                        intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
                        startActivity(intent);
                        finish();
                    } else {
                        Toast.makeText(RegisterActivity.this,
                                authResponse.getMessage() != null ?
                                        authResponse.getMessage() : "Ошибка регистрации",
                                Toast.LENGTH_LONG).show();
                    }
                } else {
                    try {
                        String errorBody = response.errorBody() != null ?
                                response.errorBody().string() : "Неизвестная ошибка";
                        Toast.makeText(RegisterActivity.this,
                                "Ошибка: " + errorBody, Toast.LENGTH_LONG).show();
                    } catch (IOException e) {
                        Toast.makeText(RegisterActivity.this,
                                "Ошибка регистрации", Toast.LENGTH_LONG).show();
                    }
                }
            }

            @Override
            public void onFailure(Call<AuthResponse> call, Throwable t) {
                progressBar.setVisibility(View.GONE);
                btnRegister.setEnabled(true);
                Toast.makeText(RegisterActivity.this,
                        "Ошибка сети: " + t.getMessage(), Toast.LENGTH_LONG).show();
            }
        });
    }
}