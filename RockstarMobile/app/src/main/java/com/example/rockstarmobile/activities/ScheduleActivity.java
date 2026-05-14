package com.example.rockstarmobile.activities;

import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Build;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.app.NotificationCompat;
import androidx.core.app.NotificationManagerCompat;
import androidx.core.content.ContextCompat;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.example.rockstarmobile.R;
import com.example.rockstarmobile.adapters.ScheduleAdapter;
import com.example.rockstarmobile.models.Schedule;
import com.example.rockstarmobile.utils.ApiClient;
import com.google.gson.Gson;
import com.google.gson.reflect.TypeToken;

import java.lang.reflect.Type;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.Set;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class ScheduleActivity extends AppCompatActivity {

    private static final String TAG = "ScheduleDebug";
    private static final String PREFS_NAME = "schedule_prefs";
    private static final String KEY_MY_TRAININGS_IDS = "my_trainings_ids";
    private static final String KEY_MY_TRAININGS_DETAILS = "my_trainings_details";
    private static final String CHANNEL_ID = "training_cancelled_channel";

    private ImageView ivBack;
    private TextView tvEmptyState;
    private LinearLayout llEmptyState;
    private RecyclerView rvSchedule;
    private ProgressBar progressBar;
    private Button btnRetry;

    private Button btnDay1, btnDay2, btnDay3, btnDay4, btnDay5;
    private Button selectedDateButton;

    private String selectedDate;
    private ScheduleAdapter adapter;
    private List<Schedule> allScheduleList = new ArrayList<>();
    private ApiClient apiClient;
    private Gson gson = new Gson();

    private boolean hasShownCancellationDialog = false;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_schedule);

        apiClient = ApiClient.getInstance(this);

        createNotificationChannel();

        initViews();
        generateDateButtons();
        setupListeners();
        loadAllSchedule();
    }

    private void createNotificationChannel() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            NotificationChannel channel = new NotificationChannel(
                    CHANNEL_ID,
                    "Отмена тренировок",
                    NotificationManager.IMPORTANCE_HIGH
            );
            channel.setDescription("Уведомления об отмене тренировок администратором");
            channel.enableVibration(true);
            channel.setVibrationPattern(new long[]{0, 500, 200, 500});

            NotificationManager manager = getSystemService(NotificationManager.class);
            if (manager != null) {
                manager.createNotificationChannel(channel);
            }
        }
    }

    private void initViews() {
        ivBack = findViewById(R.id.ivBack);
        tvEmptyState = findViewById(R.id.tvEmptyState);
        llEmptyState = findViewById(R.id.llEmptyState);
        rvSchedule = findViewById(R.id.rvSchedule);
        progressBar = findViewById(R.id.progressBar);
        btnRetry = findViewById(R.id.btnRetry);

        btnDay1 = findViewById(R.id.btnDay1);
        btnDay2 = findViewById(R.id.btnDay2);
        btnDay3 = findViewById(R.id.btnDay3);
        btnDay4 = findViewById(R.id.btnDay4);
        btnDay5 = findViewById(R.id.btnDay5);

        rvSchedule.setLayoutManager(new LinearLayoutManager(this));
    }

    private void generateDateButtons() {
        SimpleDateFormat sdf = new SimpleDateFormat("dd MMM", Locale.getDefault());
        Calendar calendar = Calendar.getInstance();

        String todayDate = getDateString(calendar.getTime());
        btnDay1.setText(sdf.format(calendar.getTime()));
        btnDay1.setTag(todayDate);

        calendar.add(Calendar.DAY_OF_MONTH, 1);
        String tomorrowDate = getDateString(calendar.getTime());
        btnDay2.setText(sdf.format(calendar.getTime()));
        btnDay2.setTag(tomorrowDate);

        calendar.add(Calendar.DAY_OF_MONTH, 1);
        String day3Date = getDateString(calendar.getTime());
        btnDay3.setText(sdf.format(calendar.getTime()));
        btnDay3.setTag(day3Date);

        calendar.add(Calendar.DAY_OF_MONTH, 1);
        String day4Date = getDateString(calendar.getTime());
        btnDay4.setText(sdf.format(calendar.getTime()));
        btnDay4.setTag(day4Date);

        calendar.add(Calendar.DAY_OF_MONTH, 1);
        String day5Date = getDateString(calendar.getTime());
        btnDay5.setText(sdf.format(calendar.getTime()));
        btnDay5.setTag(day5Date);
    }

    private String getDateString(Date date) {
        SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd", Locale.getDefault());
        return sdf.format(date);
    }

    private void setupListeners() {
        ivBack.setOnClickListener(v -> finish());
        btnRetry.setOnClickListener(v -> loadAllSchedule());

        btnDay1.setOnClickListener(v -> selectDate(btnDay1));
        btnDay2.setOnClickListener(v -> selectDate(btnDay2));
        btnDay3.setOnClickListener(v -> selectDate(btnDay3));
        btnDay4.setOnClickListener(v -> selectDate(btnDay4));
        btnDay5.setOnClickListener(v -> selectDate(btnDay5));

        selectDate(btnDay1);
    }

    private void selectDate(Button button) {
        if (selectedDateButton != null) {
            selectedDateButton.setBackgroundResource(R.drawable.btn_outline_small);
            selectedDateButton.setTextColor(ContextCompat.getColor(this, R.color.text_secondary));
        }

        button.setBackgroundResource(R.drawable.btn_primary_small);
        button.setTextColor(ContextCompat.getColor(this, android.R.color.white));
        selectedDateButton = button;

        selectedDate = (String) button.getTag();
        filterScheduleByDate();
    }

    private void loadAllSchedule() {
        progressBar.setVisibility(View.VISIBLE);
        rvSchedule.setVisibility(View.GONE);
        llEmptyState.setVisibility(View.GONE);
        btnRetry.setVisibility(View.GONE);

        apiClient.getApiService().getGroupSchedule().enqueue(new Callback<List<Schedule>>() {
            @Override
            public void onResponse(Call<List<Schedule>> call, Response<List<Schedule>> response) {
                progressBar.setVisibility(View.GONE);

                if (response.isSuccessful() && response.body() != null) {
                    allScheduleList = response.body();

                    // Проверяем отменённые занятия
                    checkForCancelledTrainings(allScheduleList);

                    filterScheduleByDate();
                } else {
                    showEmptyState("Ошибка загрузки расписания");
                }
            }

            @Override
            public void onFailure(Call<List<Schedule>> call, Throwable t) {
                progressBar.setVisibility(View.GONE);
                showEmptyState("Ошибка сети: " + t.getMessage());
            }
        });
    }

    private void showEmptyState(String message) {
        tvEmptyState.setText(message);
        llEmptyState.setVisibility(View.VISIBLE);
        rvSchedule.setVisibility(View.GONE);
        btnRetry.setVisibility(View.VISIBLE);
    }

    // Сохраняем детали занятий в SharedPreferences
    private void saveScheduleDetails(List<Schedule> schedules) {
        SharedPreferences prefs = getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE);

        // Сохраняем ID занятий
        Set<String> ids = new HashSet<>();
        for (Schedule schedule : schedules) {
            ids.add(String.valueOf(schedule.getId()));
        }
        prefs.edit().putStringSet(KEY_MY_TRAININGS_IDS, ids).apply();

        // Сохраняем детали занятий (название, дата, время)
        Map<String, String> details = new HashMap<>();
        for (Schedule schedule : schedules) {
            String detail = schedule.getDirectionName() + "|" +
                    schedule.getServiceName() + "|" +
                    schedule.getDateDisplay() + "|" +
                    schedule.getTimeRange();
            details.put(String.valueOf(schedule.getId()), detail);
        }

        String detailsJson = gson.toJson(details);
        prefs.edit().putString(KEY_MY_TRAININGS_DETAILS, detailsJson).apply();
    }

    // Проверяем отменённые занятия
    private void checkForCancelledTrainings(List<Schedule> currentSchedules) {
        SharedPreferences prefs = getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE);

        // Получаем сохранённые ID занятий
        Set<String> previousIds = prefs.getStringSet(KEY_MY_TRAININGS_IDS, new HashSet<>());

        if (previousIds.isEmpty()) {
            saveScheduleDetails(currentSchedules);
            return;
        }

        // Получаем сохранённые детали занятий
        String detailsJson = prefs.getString(KEY_MY_TRAININGS_DETAILS, "");
        Type type = new TypeToken<Map<String, String>>(){}.getType();
        Map<String, String> previousDetails = gson.fromJson(detailsJson, type);
        if (previousDetails == null) {
            previousDetails = new HashMap<>();
        }

        // Создаём множество текущих ID
        Set<String> currentIds = new HashSet<>();
        for (Schedule schedule : currentSchedules) {
            currentIds.add(String.valueOf(schedule.getId()));
        }

        // Находим удалённые занятия и собираем информацию о них
        List<CancelledTraining> cancelledTrainings = new ArrayList<>();
        for (String oldId : previousIds) {
            if (!currentIds.contains(oldId)) {
                String detail = previousDetails.get(oldId);
                if (detail != null) {
                    String[] parts = detail.split("\\|");
                    if (parts.length >= 4) {
                        CancelledTraining ct = new CancelledTraining();
                        ct.id = oldId;
                        ct.directionName = parts[0];
                        ct.serviceName = parts[1];
                        ct.date = parts[2];
                        ct.time = parts[3];
                        cancelledTrainings.add(ct);
                    }
                }
            }
        }

        // Если есть отменённые занятия - показываем уведомление с деталями
        if (!cancelledTrainings.isEmpty() && !hasShownCancellationDialog) {
            hasShownCancellationDialog = true;

            // Формируем сообщение с деталями отменённых занятий
            StringBuilder messageBuilder = new StringBuilder();
            if (cancelledTrainings.size() == 1) {
                CancelledTraining ct = cancelledTrainings.get(0);
                messageBuilder.append("Занятие \"").append(ct.directionName)
                        .append(" - ").append(ct.serviceName)
                        .append("\" (").append(ct.date).append(" ").append(ct.time)
                        .append(") отменено администратором. Приносим извинения за неудобства!");
            } else {
                messageBuilder.append("Следующие занятия отменены администратором:\n\n");
                for (int i = 0; i < cancelledTrainings.size(); i++) {
                    CancelledTraining ct = cancelledTrainings.get(i);
                    messageBuilder.append(i + 1).append(". ")
                            .append(ct.directionName).append(" - ").append(ct.serviceName)
                            .append("\n   ").append(ct.date).append(" ").append(ct.time);
                    if (i < cancelledTrainings.size() - 1) {
                        messageBuilder.append("\n\n");
                    }
                }
                messageBuilder.append("\n\nПриносим извинения за неудобства!");
            }

            String message = messageBuilder.toString();

            // Показываем push-уведомление
            showPushNotification(cancelledTrainings);

            // Показываем Toast
            Toast.makeText(this, "⚠️ " + (cancelledTrainings.size() == 1 ?
                    cancelledTrainings.get(0).directionName + " отменено" :
                    cancelledTrainings.size() + " занятия отменены"), Toast.LENGTH_LONG).show();

            // Показываем диалог с деталями
            new AlertDialog.Builder(this)
                    .setTitle("Внимание")
                    .setMessage(message)
                    .setPositiveButton("Понятно", null)
                    .setIcon(android.R.drawable.ic_dialog_alert)
                    .show();
        }

        saveScheduleDetails(currentSchedules);
    }

    // Показываем push-уведомление с деталями
    private void showPushNotification(List<CancelledTraining> cancelledTrainings) {
        if (cancelledTrainings.isEmpty()) return;

        String title;
        String content;

        if (cancelledTrainings.size() == 1) {
            CancelledTraining ct = cancelledTrainings.get(0);
            title = "⚠️ Занятие отменено";
            content = ct.directionName + " - " + ct.serviceName +
                    "\n" + ct.date + " " + ct.time;
        } else {
            title = "⚠️ " + cancelledTrainings.size() + " занятия отменены";
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Math.min(cancelledTrainings.size(), 3); i++) {
                CancelledTraining ct = cancelledTrainings.get(i);
                if (i > 0) sb.append("\n");
                sb.append("• ").append(ct.directionName).append(" - ").append(ct.serviceName)
                        .append(" (").append(ct.date).append(")");
            }
            if (cancelledTrainings.size() > 3) {
                sb.append("\nи ещё ").append(cancelledTrainings.size() - 3).append(" занятия");
            }
            content = sb.toString();
        }

        NotificationCompat.Builder builder = new NotificationCompat.Builder(this, CHANNEL_ID)
                .setSmallIcon(R.drawable.ic_notification)
                .setContentTitle(title)
                .setContentText(content)
                .setPriority(NotificationCompat.PRIORITY_HIGH)
                .setAutoCancel(true)
                .setVibrate(new long[]{0, 500, 200, 500});

        NotificationManagerCompat notificationManager = NotificationManagerCompat.from(this);
        try {
            notificationManager.notify((int) System.currentTimeMillis(), builder.build());
        } catch (SecurityException e) {
            // Игнорируем
        }
    }

    private void filterScheduleByDate() {
        if (allScheduleList.isEmpty()) {
            showEmptyState("Нет доступных занятий");
            return;
        }

        List<Schedule> filteredList = new ArrayList<>();
        for (Schedule schedule : allScheduleList) {
            String scheduleDate = schedule.getDateDisplay();
            if (scheduleDate.equals(selectedDate)) {
                filteredList.add(schedule);
            }
        }

        if (filteredList.isEmpty()) {
            tvEmptyState.setText("Нет занятий на выбранную дату");
            llEmptyState.setVisibility(View.VISIBLE);
            rvSchedule.setVisibility(View.GONE);
        } else {
            llEmptyState.setVisibility(View.GONE);
            rvSchedule.setVisibility(View.VISIBLE);
            adapter = new ScheduleAdapter(ScheduleActivity.this, filteredList,
                    schedule -> {
                        Intent intent = new Intent(ScheduleActivity.this, BookingActivity.class);
                        intent.putExtra("schedule", schedule);
                        startActivity(intent);
                    });
            rvSchedule.setAdapter(adapter);
        }
    }

    public void refreshSchedule() {
        hasShownCancellationDialog = false;
        loadAllSchedule();
    }

    @Override
    protected void onResume() {
        super.onResume();
        refreshSchedule();
    }

    // Внутренний класс для хранения информации об отменённом занятии
    private static class CancelledTraining {
        String id;
        String directionName;
        String serviceName;
        String date;
        String time;
    }
}