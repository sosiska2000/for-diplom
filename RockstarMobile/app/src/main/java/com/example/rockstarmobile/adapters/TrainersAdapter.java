package com.example.rockstarmobile.adapters;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.cardview.widget.CardView;
import androidx.core.content.ContextCompat;
import androidx.recyclerview.widget.RecyclerView;

import com.example.rockstarmobile.R;
import com.example.rockstarmobile.models.Trainer;

import java.util.List;

public class TrainersAdapter extends RecyclerView.Adapter<TrainersAdapter.TrainerViewHolder> {

    private Context context;
    private List<Trainer> trainers;
    private OnTrainerClickListener listener;

    public interface OnTrainerClickListener {
        void onTrainerClick(Trainer trainer);
    }

    public TrainersAdapter(Context context, List<Trainer> trainers) {
        this.context = context;
        this.trainers = trainers;
    }

    public TrainersAdapter(Context context, List<Trainer> trainers, OnTrainerClickListener listener) {
        this.context = context;
        this.trainers = trainers;
        this.listener = listener;
    }

    @NonNull
    @Override
    public TrainerViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(context).inflate(R.layout.item_trainer, parent, false);
        return new TrainerViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull TrainerViewHolder holder, int position) {
        Trainer trainer = trainers.get(position);

        holder.tvName.setText(trainer.getFullName());

        // 👇 ОТОБРАЖАЕМ ВСЕ НАПРАВЛЕНИЯ
        holder.tvDirection.setText(trainer.getDirectionsDisplay());
        holder.tvExperience.setText(trainer.getExperienceDisplay());
        holder.tvDescription.setText(trainer.getDescription());

        // 👇 ЗАГРУЗКА ФОТО ТРЕНЕРА
        loadTrainerPhoto(holder.ivPhoto, trainer);

        // Цвет карточки в зависимости от основного направления
        int cardColor;
        int textColor = android.graphics.Color.WHITE;
        int descriptionColor = android.graphics.Color.parseColor("#F0F0F0");

        String direction = trainer.getDirectionName();
        if (direction != null) {
            switch (direction) {
                case "Йога":
                    cardColor = ContextCompat.getColor(context, R.color.yoga);
                    break;
                case "Фитнес":
                    cardColor = ContextCompat.getColor(context, R.color.fitness);
                    break;
                case "Скалолазание":
                    cardColor = ContextCompat.getColor(context, R.color.climbing);
                    break;
                default:
                    cardColor = ContextCompat.getColor(context, R.color.primary);
                    break;
            }
        } else {
            cardColor = ContextCompat.getColor(context, R.color.primary);
        }

        holder.cardView.setCardBackgroundColor(cardColor);

        // Устанавливаем белый цвет для всех текстовых элементов
        holder.tvName.setTextColor(textColor);
        holder.tvDirection.setTextColor(textColor);
        holder.tvExperience.setTextColor(textColor);
        holder.tvDescription.setTextColor(descriptionColor);

        // Обработчик клика
        if (listener != null) {
            holder.itemView.setOnClickListener(v -> listener.onTrainerClick(trainer));
        }
    }

    // 👇 НОВЫЙ МЕТОД: загрузка фото тренера
    private void loadTrainerPhoto(ImageView imageView, Trainer trainer) {
        // Сначала пробуем загрузить из байтов
        if (trainer.getPhoto() != null && trainer.getPhoto().length > 0) {
            Bitmap bitmap = BitmapFactory.decodeByteArray(trainer.getPhoto(), 0, trainer.getPhoto().length);
            if (bitmap != null) {
                imageView.setImageBitmap(bitmap);
                return;
            }
        }

        // Пробуем загрузить из Base64
        if (trainer.getPhotoBase64() != null && !trainer.getPhotoBase64().isEmpty()) {
            try {
                byte[] decodedBytes = android.util.Base64.decode(trainer.getPhotoBase64(), android.util.Base64.DEFAULT);
                Bitmap bitmap = BitmapFactory.decodeByteArray(decodedBytes, 0, decodedBytes.length);
                if (bitmap != null) {
                    imageView.setImageBitmap(bitmap);
                    return;
                }
            } catch (Exception e) {
                e.printStackTrace();
            }
        }

        // Если нет фото - ставим заглушку с инициалами
        imageView.setImageResource(R.drawable.default_avatar);

        // Можно также установить текст с инициалами поверх заглушки
        // Для этого нужен специальный layout, но пока оставляем так
    }

    @Override
    public int getItemCount() {
        return trainers.size();
    }

    static class TrainerViewHolder extends RecyclerView.ViewHolder {
        CardView cardView;
        ImageView ivPhoto;
        TextView tvName, tvDirection, tvExperience, tvDescription;

        TrainerViewHolder(@NonNull View itemView) {
            super(itemView);
            cardView = itemView.findViewById(R.id.cardView);
            ivPhoto = itemView.findViewById(R.id.ivPhoto);
            tvName = itemView.findViewById(R.id.tvName);
            tvDirection = itemView.findViewById(R.id.tvDirection);
            tvExperience = itemView.findViewById(R.id.tvExperience);
            tvDescription = itemView.findViewById(R.id.tvDescription);
        }
    }
}