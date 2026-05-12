package com.example.rockstarmobile.activities;

import android.os.Bundle;
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
import com.example.rockstarmobile.adapters.TrainersAdapter;
import com.example.rockstarmobile.models.Trainer;
import com.example.rockstarmobile.utils.ApiClient;

import java.util.ArrayList;
import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class TrainersActivity extends AppCompatActivity {

    private ImageView ivBack;
    private TextView tvEmptyState;
    private LinearLayout llEmptyState;
    private RecyclerView rvTrainers;
    private ProgressBar progressBar;
    private Button btnRetry;

    // Кнопки фильтров
    private Button btnAll, btnYoga, btnFitness, btnClimbing;
    private Button selectedFilterButton;

    private List<Trainer> allTrainers = new ArrayList<>();
    private String currentFilter = "all";
    private ApiClient apiClient;
    private TrainersAdapter adapter;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_trainers);

        apiClient = ApiClient.getInstance(this);

        initViews();
        setupListeners();
        loadTrainers();
    }

    private void initViews() {
        ivBack = findViewById(R.id.ivBack);
        tvEmptyState = findViewById(R.id.tvEmptyState);
        llEmptyState = findViewById(R.id.llEmptyState);
        rvTrainers = findViewById(R.id.rvTrainers);
        progressBar = findViewById(R.id.progressBar);
        btnRetry = findViewById(R.id.btnRetry);

        btnAll = findViewById(R.id.btnAll);
        btnYoga = findViewById(R.id.btnYoga);
        btnFitness = findViewById(R.id.btnFitness);
        btnClimbing = findViewById(R.id.btnClimbing);

        rvTrainers.setLayoutManager(new LinearLayoutManager(this));
    }

    private void setupListeners() {
        ivBack.setOnClickListener(v -> finish());
        btnRetry.setOnClickListener(v -> loadTrainers());

        btnAll.setOnClickListener(v -> selectFilter(btnAll, "all"));
        btnYoga.setOnClickListener(v -> selectFilter(btnYoga, "yoga"));
        btnFitness.setOnClickListener(v -> selectFilter(btnFitness, "fitness"));
        btnClimbing.setOnClickListener(v -> selectFilter(btnClimbing, "climbing"));

        selectFilter(btnAll, "all");
    }

    private void selectFilter(Button button, String filter) {
        if (selectedFilterButton != null) {
            selectedFilterButton.setBackgroundResource(R.drawable.btn_outline_small);
            selectedFilterButton.setTextColor(ContextCompat.getColor(this, R.color.text_secondary));
        }

        button.setBackgroundResource(R.drawable.btn_primary_small);
        button.setTextColor(ContextCompat.getColor(this, android.R.color.white));
        selectedFilterButton = button;

        currentFilter = filter;
        filterTrainers();
    }

    private void loadTrainers() {
        progressBar.setVisibility(View.VISIBLE);
        rvTrainers.setVisibility(View.GONE);
        llEmptyState.setVisibility(View.GONE);
        btnRetry.setVisibility(View.GONE);

        apiClient.getApiService().getTrainers().enqueue(new Callback<List<Trainer>>() {
            @Override
            public void onResponse(Call<List<Trainer>> call, Response<List<Trainer>> response) {
                progressBar.setVisibility(View.GONE);

                if (response.isSuccessful() && response.body() != null) {
                    allTrainers = response.body();
                    filterTrainers();
                } else {
                    showEmptyState("Ошибка загрузки тренеров");
                }
            }

            @Override
            public void onFailure(Call<List<Trainer>> call, Throwable t) {
                progressBar.setVisibility(View.GONE);
                showEmptyState("Ошибка сети: " + t.getMessage());
            }
        });
    }

    private void showEmptyState(String message) {
        tvEmptyState.setText(message);
        llEmptyState.setVisibility(View.VISIBLE);
        rvTrainers.setVisibility(View.GONE);
        btnRetry.setVisibility(View.VISIBLE);
    }

    private void filterTrainers() {
        if (allTrainers.isEmpty()) {
            showEmptyState("Нет тренеров");
            return;
        }

        List<Trainer> filteredList = new ArrayList<>();

        if (currentFilter.equals("all")) {
            filteredList.addAll(allTrainers);
        } else {
            for (Trainer trainer : allTrainers) {
                // Проверяем по основному направлению
                String directionKey = trainer.getDirectionKey();
                if (directionKey != null && directionKey.equalsIgnoreCase(currentFilter)) {
                    filteredList.add(trainer);
                }
                // Также проверяем по списку направлений
                else if (trainer.getDirections() != null) {
                    for (com.example.rockstarmobile.models.Direction dir : trainer.getDirections()) {
                        if (dir.getNameKey() != null && dir.getNameKey().equalsIgnoreCase(currentFilter)) {
                            filteredList.add(trainer);
                            break;
                        }
                    }
                }
            }
        }

        if (filteredList.isEmpty()) {
            tvEmptyState.setText("Нет тренеров по выбранному направлению");
            llEmptyState.setVisibility(View.VISIBLE);
            rvTrainers.setVisibility(View.GONE);
            btnRetry.setVisibility(View.VISIBLE);
        } else {
            llEmptyState.setVisibility(View.GONE);
            rvTrainers.setVisibility(View.VISIBLE);
            btnRetry.setVisibility(View.GONE);

            // 👇 ОБНОВЛЯЕМ АДАПТЕР С ФОТО
            adapter = new TrainersAdapter(TrainersActivity.this, filteredList, trainer -> {
                // Обработка клика по тренеру (можно открыть детали)
                Toast.makeText(TrainersActivity.this, trainer.getFullName(), Toast.LENGTH_SHORT).show();
            });
            rvTrainers.setAdapter(adapter);
        }
    }
}