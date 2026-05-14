package com.example.rockstarmobile.activities;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.example.rockstarmobile.R;
import com.example.rockstarmobile.adapters.ServicesAdapter;
import com.example.rockstarmobile.adapters.SubscriptionsAdapter;
import com.example.rockstarmobile.models.Service;
import com.example.rockstarmobile.models.Subscription;
import com.example.rockstarmobile.utils.ApiClient;

import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class DirectionDetailActivity extends AppCompatActivity {

    private ImageView ivBack, ivDirectionIcon;
    private TextView tvDirectionTitle, tvDirectionDescription;
    private RecyclerView rvServices, rvSubscriptions;
    private ProgressBar progressBar;
    private LinearLayout llSubscriptionInfo;

    private ApiClient apiClient;
    private int directionId;
    private String directionName;
    private String directionKey;
    private String directionDescription;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_direction_detail);

        apiClient = ApiClient.getInstance(this);

        directionId = getIntent().getIntExtra("direction_id", 0);
        directionName = getIntent().getStringExtra("direction_name");
        directionKey = getIntent().getStringExtra("direction_key");
        directionDescription = getIntent().getStringExtra("direction_description");

        initViews();
        setupData();
        setupListeners();
        loadServices();
        loadSubscriptions();
    }

    private void initViews() {
        ivBack = findViewById(R.id.ivBack);
        ivDirectionIcon = findViewById(R.id.ivDirectionIcon);
        tvDirectionTitle = findViewById(R.id.tvDirectionTitle);
        tvDirectionDescription = findViewById(R.id.tvDirectionDescription);
        rvServices = findViewById(R.id.rvServices);
        rvSubscriptions = findViewById(R.id.rvSubscriptions);
        progressBar = findViewById(R.id.progressBar);
        llSubscriptionInfo = findViewById(R.id.llSubscriptionInfo);

        rvServices.setLayoutManager(new LinearLayoutManager(this));
        rvSubscriptions.setLayoutManager(new LinearLayoutManager(this));
    }

    private void setupData() {
        tvDirectionTitle.setText(directionName);
        tvDirectionDescription.setText(directionDescription);
        setDirectionIcon();
    }

    private void setupListeners() {
        ivBack.setOnClickListener(v -> finish());
    }

    private void loadServices() {
        apiClient.getApiService().getServices(directionId).enqueue(new Callback<List<Service>>() {
            @Override
            public void onResponse(Call<List<Service>> call, Response<List<Service>> response) {
                if (response.isSuccessful() && response.body() != null) {
                    ServicesAdapter adapter = new ServicesAdapter(DirectionDetailActivity.this, response.body());
                    rvServices.setAdapter(adapter);
                }
            }

            @Override
            public void onFailure(Call<List<Service>> call, Throwable t) {
                Toast.makeText(DirectionDetailActivity.this, "Ошибка загрузки услуг", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void loadSubscriptions() {
        progressBar.setVisibility(View.VISIBLE);

        int dirId = directionId;

        apiClient.getApiService().getSubscriptions(dirId).enqueue(new Callback<List<Subscription>>() {
            @Override
            public void onResponse(Call<List<Subscription>> call, Response<List<Subscription>> response) {
                progressBar.setVisibility(View.GONE);

                if (response.isSuccessful() && response.body() != null) {
                    List<Subscription> subs = response.body();

                    if (!subs.isEmpty()) {
                        // 👇 СОЗДАЕМ АДАПТЕР
                        SubscriptionsAdapter adapter = new SubscriptionsAdapter(
                                DirectionDetailActivity.this,
                                subs,
                                sub -> showPurchaseInGymMessage(sub)
                        );
                        rvSubscriptions.setAdapter(adapter);
                        llSubscriptionInfo.setVisibility(View.VISIBLE);
                    } else {
                        llSubscriptionInfo.setVisibility(View.GONE);
                        Toast.makeText(DirectionDetailActivity.this, "Нет абонементов", Toast.LENGTH_SHORT).show();
                    }
                } else {
                    Toast.makeText(DirectionDetailActivity.this, "Ошибка", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<List<Subscription>> call, Throwable t) {
                progressBar.setVisibility(View.GONE);
                Toast.makeText(DirectionDetailActivity.this, "Ошибка: " + t.getMessage(), Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void showPurchaseInGymMessage(Subscription subscription) {
        new androidx.appcompat.app.AlertDialog.Builder(this)
                .setTitle("Информация")
                .setMessage("Абонемент \"" + subscription.getName() + "\"\n\n" +
                        "Стоимость: " + subscription.getPriceDisplay() + "\n" +
                        "Занятий: " + subscription.getSessionsDisplay() + "\n\n" +
                        "Приобрести абонемент можно только в клубе.\n\n" +
                        "Адрес: ул. Спортивная, д. 15\n" +
                        "Телефон: +7 (999) 123-45-67")
                .setPositiveButton("OK", null)
                .show();
    }

    private void setDirectionIcon() {
        if (directionKey == null) return;
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