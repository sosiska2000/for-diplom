package com.example.rockstarmobile.activities;

import android.content.Intent;
import android.os.Build;
import android.os.Bundle;
import android.Manifest;
import android.content.pm.PackageManager;
import android.widget.Button;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;
import androidx.cardview.widget.CardView;
import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;

import com.example.rockstarmobile.R;
import com.example.rockstarmobile.models.User;
import com.example.rockstarmobile.utils.ReminderScheduler;
import com.example.rockstarmobile.utils.SessionManager;

public class MainActivity extends AppCompatActivity {

    private CardView cardCatalog, cardSchedule, cardTrainers, cardProfile;
    private Button btnLogout;
    private TextView tvWelcome;

    private SessionManager sessionManager;
    private User currentUser;

    private static final int NOTIFICATION_PERMISSION_REQUEST_CODE = 100;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        sessionManager = new SessionManager(this);
        currentUser = sessionManager.getUser();

        // Запрашиваем разрешение на уведомления для Android 13+
        requestNotificationPermission();

        // Запускаем планировщик напоминаний
        ReminderScheduler.scheduleReminders(this);

        initViews();
        setupListeners();
        setupUserInfo();
    }

    private void requestNotificationPermission() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
            if (ContextCompat.checkSelfPermission(this, Manifest.permission.POST_NOTIFICATIONS)
                    != PackageManager.PERMISSION_GRANTED) {
                ActivityCompat.requestPermissions(
                        this,
                        new String[]{Manifest.permission.POST_NOTIFICATIONS},
                        NOTIFICATION_PERMISSION_REQUEST_CODE
                );
            }
        }
    }

    @Override
    public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults);
        if (requestCode == NOTIFICATION_PERMISSION_REQUEST_CODE) {
            if (grantResults.length > 0 && grantResults[0] == PackageManager.PERMISSION_GRANTED) {
                // Разрешение получено
            } else {
                // Разрешение не получено - уведомления не будут работать
            }
        }
    }

    private void initViews() {
        cardCatalog = findViewById(R.id.cardCatalog);
        cardSchedule = findViewById(R.id.cardSchedule);
        cardTrainers = findViewById(R.id.cardTrainers);
        cardProfile = findViewById(R.id.cardProfile);
        btnLogout = findViewById(R.id.btnLogout);
        tvWelcome = findViewById(R.id.tvWelcome);
    }

    private void setupListeners() {
        cardCatalog.setOnClickListener(v -> openCatalog());
        cardSchedule.setOnClickListener(v -> openSchedule());
        cardTrainers.setOnClickListener(v -> openTrainers());
        cardProfile.setOnClickListener(v -> openProfile());
        btnLogout.setOnClickListener(v -> logout());
    }

    private void setupUserInfo() {
        if (currentUser != null) {
            tvWelcome.setText("Привет, " + currentUser.getFirstName() + "!");
        } else {
            tvWelcome.setText("Привет, Гость!");
        }
    }

    private void openCatalog() {
        Intent intent = new Intent(MainActivity.this, CatalogActivity.class);
        startActivity(intent);
    }

    private void openSchedule() {
        Intent intent = new Intent(MainActivity.this, ScheduleActivity.class);
        startActivity(intent);
    }

    private void openTrainers() {
        Intent intent = new Intent(MainActivity.this, TrainersActivity.class);
        startActivity(intent);
    }

    private void openProfile() {
        Intent intent = new Intent(MainActivity.this, ProfileActivity.class);
        startActivity(intent);
    }

    private void logout() {
        // Отменяем напоминания при выходе
        ReminderScheduler.cancelReminders(this);

        sessionManager.logout();
        Intent intent = new Intent(MainActivity.this, LoginActivity.class);
        intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
        startActivity(intent);
        finish();
    }
}