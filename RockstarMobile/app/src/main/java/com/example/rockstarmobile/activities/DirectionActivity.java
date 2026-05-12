package com.example.rockstarmobile.activities;

import android.os.Bundle;
import android.view.View;
import android.widget.ImageView;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.example.rockstarmobile.R;
import com.example.rockstarmobile.adapters.ServicesAdapter;
import com.example.rockstarmobile.adapters.SubscriptionsAdapter;
import com.example.rockstarmobile.models.Direction;
import com.example.rockstarmobile.models.Service;
import com.example.rockstarmobile.models.Subscription;
import com.example.rockstarmobile.utils.ApiClient;

import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class DirectionActivity extends AppCompatActivity {

    private ImageView ivBack, ivDirectionIcon;
    private TextView tvDirectionTitle, tvDirectionDescription;
    private RecyclerView rvServices, rvSubscriptions;
    private ProgressBar progressBar;

    private String directionKey;
    private ApiClient apiClient;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_direction);

        apiClient = ApiClient.getInstance(this);

        // Получаем ключ направления из Intent
        directionKey = getIntent().getStringExtra("direction_key");
        if (directionKey == null || directionKey.isEmpty()) {
            directionKey = "yoga";
        }

        initViews();
        setupListeners();
        loadDirectionData();
    }

    private void initViews() {
        ivBack = findViewById(R.id.ivBack);
        ivDirectionIcon = findViewById(R.id.ivDirectionIcon);
        tvDirectionTitle = findViewById(R.id.tvDirectionTitle);
        tvDirectionDescription = findViewById(R.id.tvDirectionDescription);
        rvServices = findViewById(R.id.rvServices);
        rvSubscriptions = findViewById(R.id.rvSubscriptions);
        progressBar = findViewById(R.id.progressBar);

        rvServices.setLayoutManager(new LinearLayoutManager(this));
        rvSubscriptions.setLayoutManager(new LinearLayoutManager(this));
    }

    private void setupListeners() {
        ivBack.setOnClickListener(v -> finish());
    }

    private void loadDirectionData() {
        progressBar.setVisibility(View.VISIBLE);

        apiClient.getApiService().getDirectionByKey(directionKey).enqueue(new Callback<Direction>() {
            @Override
            public void onResponse(Call<Direction> call, Response<Direction> response) {
                if (response.isSuccessful() && response.body() != null) {
                    Direction direction = response.body();
                    tvDirectionTitle.setText(direction.getName());
                    tvDirectionDescription.setText(direction.getDescription());
                    setDirectionIcon();

                    // Загружаем услуги и абонементы
                    loadServices(direction.getId());
                    loadSubscriptions(direction.getId());
                } else {
                    progressBar.setVisibility(View.GONE);
                    Toast.makeText(DirectionActivity.this, "Направление не найдено", Toast.LENGTH_SHORT).show();
                    finish();
                }
            }

            @Override
            public void onFailure(Call<Direction> call, Throwable t) {
                progressBar.setVisibility(View.GONE);
                Toast.makeText(DirectionActivity.this, "Ошибка сети: " + t.getMessage(), Toast.LENGTH_SHORT).show();
                finish();
            }
        });
    }

    private void loadServices(int directionId) {
        apiClient.getApiService().getServices(directionId).enqueue(new Callback<List<Service>>() {
            @Override
            public void onResponse(Call<List<Service>> call, Response<List<Service>> response) {
                if (response.isSuccessful() && response.body() != null) {
                    ServicesAdapter adapter = new ServicesAdapter(DirectionActivity.this, response.body());
                    rvServices.setAdapter(adapter);
                }
                // Не скрываем progressBar до загрузки всех данных
            }

            @Override
            public void onFailure(Call<List<Service>> call, Throwable t) {
                Toast.makeText(DirectionActivity.this, "Ошибка загрузки услуг", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void loadSubscriptions(int directionId) {
        apiClient.getApiService().getSubscriptions(directionId).enqueue(new Callback<List<Subscription>>() {
            @Override
            public void onResponse(Call<List<Subscription>> call, Response<List<Subscription>> response) {
                progressBar.setVisibility(View.GONE);
                if (response.isSuccessful() && response.body() != null) {
                    SubscriptionsAdapter adapter = new SubscriptionsAdapter(DirectionActivity.this, response.body(), subscription -> {
                        // Ничего не делаем или показываем детали
                        Toast.makeText(DirectionActivity.this, subscription.getName(), Toast.LENGTH_SHORT).show();
                    });
                    rvSubscriptions.setAdapter(adapter);
                }
            }

            @Override
            public void onFailure(Call<List<Subscription>> call, Throwable t) {
                progressBar.setVisibility(View.GONE);
                Toast.makeText(DirectionActivity.this, "Ошибка загрузки абонементов", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void setDirectionIcon() {
        switch (directionKey) {
            case "yoga":
                ivDirectionIcon.setImageResource(R.drawable.ic_yoga);
                break;
            case "fitness":
                ivDirectionIcon.setImageResource(R.drawable.ic_fitness);
                break;
            case "climbing":
                ivDirectionIcon.setImageResource(R.drawable.ic_climbing);
                break;
            default:
                ivDirectionIcon.setImageResource(R.drawable.ic_yoga);
                break;
        }
    }
}