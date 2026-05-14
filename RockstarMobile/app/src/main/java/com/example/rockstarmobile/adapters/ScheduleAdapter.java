package com.example.rockstarmobile.adapters;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.cardview.widget.CardView;
import androidx.core.content.ContextCompat;
import androidx.recyclerview.widget.RecyclerView;

import com.example.rockstarmobile.R;
import com.example.rockstarmobile.models.Schedule;

import java.util.List;

public class ScheduleAdapter extends RecyclerView.Adapter<ScheduleAdapter.ScheduleViewHolder> {

    private final Context context;
    private final List<Schedule> schedules;
    private final OnItemClickListener listener;

    public interface OnItemClickListener {
        void onItemClick(Schedule schedule);
    }

    public ScheduleAdapter(Context context, List<Schedule> schedules, OnItemClickListener listener) {
        this.context = context;
        this.schedules = schedules;
        this.listener = listener;
    }

    @NonNull
    @Override
    public ScheduleViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(context).inflate(R.layout.item_schedule, parent, false);
        return new ScheduleViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ScheduleViewHolder holder, int position) {
        Schedule schedule = schedules.get(position);

        // Устанавливаем время начала и окончания
        holder.tvStartTime.setText(schedule.getStartTime());
        holder.tvEndTime.setText(schedule.getEndTime());

        holder.tvDirection.setText(schedule.getDirectionName());
        holder.tvService.setText(schedule.getServiceName());
        holder.tvTrainer.setText(schedule.getTrainerName());

        // Форматируем дату
        holder.tvDate.setText(schedule.getFormattedDate());

        holder.tvPrice.setText(schedule.getPriceDisplay());
        holder.tvParticipants.setText(schedule.getParticipantsDisplay());

        holder.itemView.setOnClickListener(v -> {
            if (listener != null) {
                listener.onItemClick(schedule);
            }
        });

        // Если мест нет, показываем другим цветом
        if (!schedule.isAvailable()) {
            holder.tvParticipants.setTextColor(ContextCompat.getColor(context, android.R.color.holo_red_dark));
        } else {
            holder.tvParticipants.setTextColor(ContextCompat.getColor(context, R.color.fitness));
        }
    }

    @Override
    public int getItemCount() {
        return schedules.size();
    }

    public static class ScheduleViewHolder extends RecyclerView.ViewHolder {
        public CardView cardView;
        public TextView tvStartTime, tvEndTime;
        public TextView tvDirection, tvService, tvTrainer, tvDate, tvPrice, tvParticipants;

        public ScheduleViewHolder(@NonNull View itemView) {
            super(itemView);
            cardView = itemView.findViewById(R.id.cardView);
            tvStartTime = itemView.findViewById(R.id.tvStartTime);
            tvEndTime = itemView.findViewById(R.id.tvEndTime);
            tvDirection = itemView.findViewById(R.id.tvDirection);
            tvService = itemView.findViewById(R.id.tvService);
            tvTrainer = itemView.findViewById(R.id.tvTrainer);
            tvDate = itemView.findViewById(R.id.tvDate);
            tvPrice = itemView.findViewById(R.id.tvPrice);
            tvParticipants = itemView.findViewById(R.id.tvParticipants);
        }
    }
}