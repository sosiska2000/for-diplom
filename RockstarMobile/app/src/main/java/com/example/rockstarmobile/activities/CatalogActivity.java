package com.example.rockstarmobile.activities;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.ProgressBar;
import android.widget.TextView;

import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.example.rockstarmobile.R;
import com.example.rockstarmobile.adapters.DirectionCardAdapter;
import com.example.rockstarmobile.models.Direction;
import com.example.rockstarmobile.utils.ApiClient;

import java.util.ArrayList;
import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class CatalogActivity extends AppCompatActivity {

    private ImageView ivBack;
    private ProgressBar progressBar;
    private RecyclerView rvDirections;
    private TextView tvEmptyState;
    private LinearLayout llEmptyState;
    private Button btnRetry;

    private ApiClient apiClient;
    private List<Direction> directions = new ArrayList<>();
    private DirectionCardAdapter adapter;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_catalog);

        apiClient = ApiClient.getInstance(this);

        initViews();
        setupListeners();
        loadDirections();
    }

    private void initViews() {
        ivBack = findViewById(R.id.ivBack);
        progressBar = findViewById(R.id.progressBar);
        rvDirections = findViewById(R.id.rvDirections);
        tvEmptyState = findViewById(R.id.tvEmptyState);
        llEmptyState = findViewById(R.id.llEmptyState);
        btnRetry = findViewById(R.id.btnRetry);

        rvDirections.setLayoutManager(new LinearLayoutManager(this));
    }

    private void setupListeners() {
        ivBack.setOnClickListener(v -> finish());
        btnRetry.setOnClickListener(v -> loadDirections());
    }

    private void loadDirections() {
        progressBar.setVisibility(View.VISIBLE);
        rvDirections.setVisibility(View.GONE);
        llEmptyState.setVisibility(View.GONE);
        btnRetry.setVisibility(View.GONE);

        apiClient.getApiService().getDirections().enqueue(new Callback<List<Direction>>() {
            @Override
            public void onResponse(Call<List<Direction>> call, Response<List<Direction>> response) {
                progressBar.setVisibility(View.GONE);

                if (response.isSuccessful() && response.body() != null) {
                    directions = response.body();
                    if (directions.isEmpty()) {
                        showEmptyState("Нет доступных направлений");
                    } else {
                        llEmptyState.setVisibility(View.GONE);
                        rvDirections.setVisibility(View.VISIBLE);
                        adapter = new DirectionCardAdapter(CatalogActivity.this, directions, direction -> {
                            openDirectionDetail(direction);
                        });
                        rvDirections.setAdapter(adapter);
                    }
                } else {
                    showEmptyState("Ошибка загрузки направлений");
                }
            }

            @Override
            public void onFailure(Call<List<Direction>> call, Throwable t) {
                progressBar.setVisibility(View.GONE);
                showEmptyState("Ошибка сети: " + t.getMessage());
            }
        });
    }

    private void openDirectionDetail(Direction direction) {
        Intent intent = new Intent(CatalogActivity.this, DirectionDetailActivity.class);
        intent.putExtra("direction_id", direction.getId());
        intent.putExtra("direction_name", direction.getName());
        intent.putExtra("direction_key", direction.getNameKey());
        intent.putExtra("direction_description", direction.getDescription());
        startActivity(intent);
    }
    private void showEmptyState(String message) {
        tvEmptyState.setText(message);
        llEmptyState.setVisibility(View.VISIBLE);
        rvDirections.setVisibility(View.GONE);
        btnRetry.setVisibility(View.VISIBLE);
    }
}