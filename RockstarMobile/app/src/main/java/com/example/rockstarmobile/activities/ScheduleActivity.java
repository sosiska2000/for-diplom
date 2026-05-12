package com.example.rockstarmobile.activities;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;
import androidx.core.content.ContextCompat;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.example.rockstarmobile.R;
import com.example.rockstarmobile.adapters.ScheduleAdapter;
import com.example.rockstarmobile.models.Schedule;
import com.example.rockstarmobile.utils.ApiClient;

import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;
import java.util.List;
import java.util.Locale;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class ScheduleActivity extends AppCompatActivity {

    private static final String TAG = "ScheduleDebug";

    private ImageView ivBack;
    private TextView tvEmptyState;
    private LinearLayout llEmptyState;
    private RecyclerView rvSchedule;
    private ProgressBar progressBar;
    private Button btnRetry;

    // Кнопки для дней
    private Button btnDay1, btnDay2, btnDay3, btnDay4, btnDay5;
    private Button selectedDateButton;

    private String selectedDate;
    private ScheduleAdapter adapter;
    private List<Schedule> allScheduleList = new ArrayList<>();
    private ApiClient apiClient;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_schedule);

        apiClient = ApiClient.getInstance(this);

        initViews();
        generateDateButtons();
        setupListeners();
        loadAllSchedule();
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

        // Сегодня
        String todayDate = getDateString(calendar.getTime());
        btnDay1.setText(sdf.format(calendar.getTime()));
        btnDay1.setTag(todayDate);
        Log.d(TAG, "Day1: text=" + btnDay1.getText() + ", tag=" + todayDate);

        // Завтра
        calendar.add(Calendar.DAY_OF_MONTH, 1);
        String tomorrowDate = getDateString(calendar.getTime());
        btnDay2.setText(sdf.format(calendar.getTime()));
        btnDay2.setTag(tomorrowDate);
        Log.d(TAG, "Day2: text=" + btnDay2.getText() + ", tag=" + tomorrowDate);

        // Послезавтра
        calendar.add(Calendar.DAY_OF_MONTH, 1);
        String day3Date = getDateString(calendar.getTime());
        btnDay3.setText(sdf.format(calendar.getTime()));
        btnDay3.setTag(day3Date);
        Log.d(TAG, "Day3: text=" + btnDay3.getText() + ", tag=" + day3Date);

        // +3 дня
        calendar.add(Calendar.DAY_OF_MONTH, 1);
        String day4Date = getDateString(calendar.getTime());
        btnDay4.setText(sdf.format(calendar.getTime()));
        btnDay4.setTag(day4Date);
        Log.d(TAG, "Day4: text=" + btnDay4.getText() + ", tag=" + day4Date);

        // +4 дня
        calendar.add(Calendar.DAY_OF_MONTH, 1);
        String day5Date = getDateString(calendar.getTime());
        btnDay5.setText(sdf.format(calendar.getTime()));
        btnDay5.setTag(day5Date);
        Log.d(TAG, "Day5: text=" + btnDay5.getText() + ", tag=" + day5Date);
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

        // По умолчанию выбираем сегодня
        selectDate(btnDay1);
    }

    private void selectDate(Button button) {
        Log.d(TAG, "=== selectDate ===");
        Log.d(TAG, "Button tag: " + button.getTag());

        // Сбрасываем стиль предыдущей выбранной кнопки
        if (selectedDateButton != null) {
            selectedDateButton.setBackgroundResource(R.drawable.btn_outline_small);
            selectedDateButton.setTextColor(ContextCompat.getColor(this, R.color.text_secondary));
        }

        // Устанавливаем стиль для выбранной кнопки
        button.setBackgroundResource(R.drawable.btn_primary_small);
        button.setTextColor(ContextCompat.getColor(this, android.R.color.white));
        selectedDateButton = button;

        selectedDate = (String) button.getTag();
        Log.d(TAG, "Selected date: " + selectedDate);
        filterScheduleByDate();
    }

    private void loadAllSchedule() {
        Log.d(TAG, "=== loadAllSchedule START ===");
        progressBar.setVisibility(View.VISIBLE);
        rvSchedule.setVisibility(View.GONE);
        llEmptyState.setVisibility(View.GONE);
        btnRetry.setVisibility(View.GONE);

        apiClient.getApiService().getGroupSchedule().enqueue(new Callback<List<Schedule>>() {
            @Override
            public void onResponse(Call<List<Schedule>> call, Response<List<Schedule>> response) {
                progressBar.setVisibility(View.GONE);
                Log.d(TAG, "onResponse: response.isSuccessful() = " + response.isSuccessful());

                if (response.isSuccessful() && response.body() != null) {
                    allScheduleList = response.body();
                    Log.d(TAG, "Loaded " + allScheduleList.size() + " schedules");

                    for (Schedule s : allScheduleList) {
                        Log.d(TAG, "Schedule: id=" + s.getId() +
                                ", direction=" + s.getDirectionName() +
                                ", date=" + s.getDateDisplay() +
                                ", time=" + s.getTimeRange());
                    }
                    filterScheduleByDate();
                } else {
                    Log.e(TAG, "Response not successful");
                    showEmptyState("Ошибка загрузки расписания");
                }
            }

            @Override
            public void onFailure(Call<List<Schedule>> call, Throwable t) {
                progressBar.setVisibility(View.GONE);
                Log.e(TAG, "onFailure: " + t.getMessage());
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

    private void filterScheduleByDate() {
        Log.d(TAG, "=== filterScheduleByDate ===");
        Log.d(TAG, "selectedDate = '" + selectedDate + "'");
        Log.d(TAG, "allScheduleList size = " + allScheduleList.size());

        if (allScheduleList.isEmpty()) {
            showEmptyState("Нет доступных занятий");
            return;
        }

        List<Schedule> filteredList = new ArrayList<>();
        for (Schedule schedule : allScheduleList) {
            String scheduleDate = schedule.getDateDisplay();
            Log.d(TAG, "Schedule date: '" + scheduleDate + "', comparing with: '" + selectedDate + "'");
            Log.d(TAG, "Equals: " + scheduleDate.equals(selectedDate));
            if (scheduleDate.equals(selectedDate)) {
                filteredList.add(schedule);
                Log.d(TAG, "  -> MATCH! Added to filteredList");
            }
        }

        Log.d(TAG, "Filtered list size: " + filteredList.size());

        if (filteredList.isEmpty()) {
            tvEmptyState.setText("Нет занятий на выбранную дату");
            llEmptyState.setVisibility(View.VISIBLE);
            rvSchedule.setVisibility(View.GONE);
        } else {
            llEmptyState.setVisibility(View.GONE);
            rvSchedule.setVisibility(View.VISIBLE);
            adapter = new ScheduleAdapter(ScheduleActivity.this, filteredList,
                    schedule -> {
                        Log.d(TAG, "Schedule clicked: " + schedule.getId());
                        Intent intent = new Intent(ScheduleActivity.this, BookingActivity.class);
                        intent.putExtra("schedule", schedule);
                        startActivity(intent);
                    });
            rvSchedule.setAdapter(adapter);
        }
    }
}