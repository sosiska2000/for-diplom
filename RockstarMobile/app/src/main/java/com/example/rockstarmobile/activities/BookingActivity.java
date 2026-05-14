package com.example.rockstarmobile.activities;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;

import com.example.rockstarmobile.R;
import com.example.rockstarmobile.models.EnrollResponse;
import com.example.rockstarmobile.models.Schedule;
import com.example.rockstarmobile.models.User;
import com.example.rockstarmobile.models.UserSubscriptionDto;
import com.example.rockstarmobile.utils.ApiClient;
import com.example.rockstarmobile.utils.SessionManager;

import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class BookingActivity extends AppCompatActivity {

    private ImageView ivBack;
    private TextView tvDirectionName, tvServiceName, tvTrainerName, tvDateTime, tvDuration;
    private TextView tvPrice, tvParticipants, tvAvailableSpots;
    private TextView tvSubscriptionInfo;  // 👈 НОВОЕ ПОЛЕ
    private Button btnBook, btnCancel;
    private ProgressBar progressBar;

    private Schedule currentSchedule;
    private SessionManager sessionManager;
    private User currentUser;
    private ApiClient apiClient;

    private List<UserSubscriptionDto> userSubscriptions;
    private UserSubscriptionDto applicableSubscription;  // 👈 Подходящий абонемент

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_booking);

        sessionManager = new SessionManager(this);
        apiClient = ApiClient.getInstance(this);
        currentUser = sessionManager.getUser();

        currentSchedule = (Schedule) getIntent().getSerializableExtra("schedule");
        if (currentSchedule == null) {
            Toast.makeText(this, "Ошибка загрузки данных", Toast.LENGTH_SHORT).show();
            finish();
            return;
        }

        initViews();
        setupData();
        setupListeners();
        loadUserSubscriptions();  // 👈 Загружаем абонементы пользователя
    }

    private void initViews() {
        ivBack = findViewById(R.id.ivBack);
        tvDirectionName = findViewById(R.id.tvDirectionName);
        tvServiceName = findViewById(R.id.tvServiceName);
        tvTrainerName = findViewById(R.id.tvTrainerName);
        tvDateTime = findViewById(R.id.tvDateTime);
        tvDuration = findViewById(R.id.tvDuration);
        tvPrice = findViewById(R.id.tvPrice);
        tvParticipants = findViewById(R.id.tvParticipants);
        tvAvailableSpots = findViewById(R.id.tvAvailableSpots);
        tvSubscriptionInfo = findViewById(R.id.tvSubscriptionInfo);  // 👈 НОВОЕ
        btnBook = findViewById(R.id.btnBook);
        btnCancel = findViewById(R.id.btnCancel);
        progressBar = findViewById(R.id.progressBar);
    }

    private void setupData() {
        tvDirectionName.setText(currentSchedule.getDirectionName());
        tvServiceName.setText(currentSchedule.getServiceName());
        tvTrainerName.setText("Тренер: " + currentSchedule.getTrainerName());
        tvDateTime.setText(currentSchedule.getDateDisplay() + " " + currentSchedule.getTimeRange());
        tvDuration.setText("Длительность: " + currentSchedule.getDurationMinutes() + " мин");
        tvPrice.setText(currentSchedule.getPriceDisplay());
        tvParticipants.setText("Записано: " + currentSchedule.getParticipantsDisplay());

        int available = currentSchedule.getMaxParticipants() - currentSchedule.getCurrentParticipants();
        tvAvailableSpots.setText("Осталось мест: " + available);

        if (available <= 0) {
            btnBook.setEnabled(false);
            btnBook.setText("Мест нет");
            btnBook.setAlpha(0.5f);
        }
    }

    // 👇 НОВЫЙ МЕТОД: загрузка абонементов пользователя
    private void loadUserSubscriptions() {
        progressBar.setVisibility(View.VISIBLE);

        apiClient.getApiService().getMySubscriptions().enqueue(new Callback<List<UserSubscriptionDto>>() {
            @Override
            public void onResponse(Call<List<UserSubscriptionDto>> call, Response<List<UserSubscriptionDto>> response) {
                progressBar.setVisibility(View.GONE);

                if (response.isSuccessful() && response.body() != null) {
                    userSubscriptions = response.body();
                    checkApplicableSubscription();
                }
            }

            @Override
            public void onFailure(Call<List<UserSubscriptionDto>> call, Throwable t) {
                progressBar.setVisibility(View.GONE);
                // Не показываем ошибку, просто запись будет платной
            }
        });
    }
    // В BookingActivity добавьте обработчик для btnCancel
    private void setupListeners() {
        ivBack.setOnClickListener(v -> finish());
        btnBook.setOnClickListener(v -> showConfirmationDialog());
        btnCancel.setOnClickListener(v -> cancelBooking());  // 👈 ДОБАВЬТЕ
    }

    private void cancelBooking() {
        new AlertDialog.Builder(this)
                .setTitle("Отмена записи")
                .setMessage("Вы уверены, что хотите отменить запись на занятие?")
                .setPositiveButton("Отменить", (dialog, which) -> performCancel())
                .setNegativeButton("Оставить", null)
                .show();
    }

    private void performCancel() {
        progressBar.setVisibility(View.VISIBLE);

        apiClient.getApiService().cancelEnrollment(currentSchedule.getId()).enqueue(new Callback<Void>() {
            @Override
            public void onResponse(Call<Void> call, Response<Void> response) {
                progressBar.setVisibility(View.GONE);

                if (response.isSuccessful()) {
                    Toast.makeText(BookingActivity.this,
                            "Запись отменена!", Toast.LENGTH_LONG).show();
                    finish();
                } else {
                    Toast.makeText(BookingActivity.this,
                            "Ошибка отмены записи", Toast.LENGTH_LONG).show();
                }
            }

            @Override
            public void onFailure(Call<Void> call, Throwable t) {
                progressBar.setVisibility(View.GONE);
                Toast.makeText(BookingActivity.this,
                        "Ошибка сети: " + t.getMessage(), Toast.LENGTH_LONG).show();
            }
        });
    }

    // 👇 НОВЫЙ МЕТОД: проверка подходящего абонемента
    private void checkApplicableSubscription() {
        if (userSubscriptions == null || userSubscriptions.isEmpty()) {
            tvSubscriptionInfo.setVisibility(View.GONE);
            return;
        }

        // Ищем активный абонемент на это направление
        for (UserSubscriptionDto sub : userSubscriptions) {
            if (sub.getStatus().equals("active")
                    && sub.getDirectionId() == currentSchedule.getDirectionId()
                    && sub.getSessionsRemaining() > 0) {
                applicableSubscription = sub;
                break;
            }
        }

        if (applicableSubscription != null) {
            // Показываем информацию об абонементе
            tvSubscriptionInfo.setVisibility(View.VISIBLE);
            tvSubscriptionInfo.setText(
                    "✓ У вас есть абонемент \"" + applicableSubscription.getSubscriptionName() + "\"\n" +
                            "  Осталось занятий: " + applicableSubscription.getSessionsRemaining() + "\n" +
                            "  Занятие будет списано с абонемента"
            );

            // Меняем текст кнопки
            btnBook.setText("ЗАПИСАТЬСЯ ПО АБОНЕМЕНТУ");

            // Обновляем отображение цены
            tvPrice.setText("0 ₽ (по абонементу)");
            tvPrice.setTextColor(getColor(R.color.fitness));
        } else {
            tvSubscriptionInfo.setVisibility(View.GONE);
            btnBook.setText("ЗАПИСАТЬСЯ");
        }
    }


    private void showConfirmationDialog() {
        String message;
        if (applicableSubscription != null) {
            message = "Вы уверены, что хотите записаться на занятие?\n\n" +
                    currentSchedule.getDirectionName() + " - " + currentSchedule.getServiceName() + "\n" +
                    currentSchedule.getDateDisplay() + " " + currentSchedule.getTimeRange() + "\n\n" +
                    "✅ Занятие будет списано с абонемента \"" + applicableSubscription.getSubscriptionName() + "\"\n" +
                    "Останется занятий: " + (applicableSubscription.getSessionsRemaining() - 1);
        } else {
            message = "Вы уверены, что хотите записаться на занятие?\n\n" +
                    currentSchedule.getDirectionName() + " - " + currentSchedule.getServiceName() + "\n" +
                    currentSchedule.getDateDisplay() + " " + currentSchedule.getTimeRange() + "\n\n" +
                    "💰 Стоимость: " + currentSchedule.getPriceDisplay() + "\n" +
                    "Оплата будет произведена при посещении";
        }

        new AlertDialog.Builder(this)
                .setTitle("Подтверждение записи")
                .setMessage(message)
                .setPositiveButton("Записаться", (dialog, which) -> performBooking())
                .setNegativeButton("Отмена", null)
                .show();
    }

    private void performBooking() {
        progressBar.setVisibility(View.VISIBLE);
        btnBook.setEnabled(false);
        btnCancel.setEnabled(false);

        apiClient.getApiService().enrollToSchedule(currentSchedule.getId()).enqueue(new Callback<EnrollResponse>() {
            @Override
            public void onResponse(Call<EnrollResponse> call, Response<EnrollResponse> response) {
                progressBar.setVisibility(View.GONE);
                btnBook.setEnabled(true);
                btnCancel.setEnabled(true);

                if (response.isSuccessful() && response.body() != null) {
                    EnrollResponse result = response.body();

                    String successMessage;
                    if (result.isUsedSubscription()) {
                        successMessage = "Вы записаны на занятие по абонементу!\n" +
                                "Осталось занятий: " + (applicableSubscription != null ?
                                (applicableSubscription.getSessionsRemaining() - 1) : "?");
                    } else {
                        successMessage = "Вы успешно записаны на занятие!\n" +
                                "Оплата будет произведена при посещении.";
                    }

                    Toast.makeText(BookingActivity.this, successMessage, Toast.LENGTH_LONG).show();

                    Intent resultIntent = new Intent();
                    resultIntent.putExtra("schedule_id", currentSchedule.getId());
                    setResult(RESULT_OK, resultIntent);
                    finish();
                } else {
                    String errorMsg = "Ошибка записи";
                    try {
                        if (response.errorBody() != null) {
                            errorMsg = response.errorBody().string();
                        }
                    } catch (Exception e) {
                        e.printStackTrace();
                    }
                    Toast.makeText(BookingActivity.this, errorMsg, Toast.LENGTH_LONG).show();
                }
            }

            @Override
            public void onFailure(Call<EnrollResponse> call, Throwable t) {
                progressBar.setVisibility(View.GONE);
                btnBook.setEnabled(true);
                btnCancel.setEnabled(true);
                Toast.makeText(BookingActivity.this,
                        "Ошибка сети: " + t.getMessage(), Toast.LENGTH_LONG).show();
            }
        });
    }
}