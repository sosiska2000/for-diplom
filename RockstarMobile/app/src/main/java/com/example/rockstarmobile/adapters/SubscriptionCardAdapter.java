package com.example.rockstarmobile.adapters;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ProgressBar;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.cardview.widget.CardView;
import androidx.core.content.ContextCompat;
import androidx.recyclerview.widget.RecyclerView;

import com.example.rockstarmobile.R;
import com.example.rockstarmobile.models.UserSubscriptionDto;

import java.util.ArrayList;
import java.util.Date;
import java.util.List;

public class SubscriptionCardAdapter extends RecyclerView.Adapter<SubscriptionCardAdapter.ViewHolder> {

    private final Context context;
    private List<UserSubscriptionDto> subscriptions;
    private List<UserSubscriptionDto> activeSubscriptions;

    public SubscriptionCardAdapter(Context context, List<UserSubscriptionDto> subscriptions) {
        this.context = context;
        this.subscriptions = subscriptions != null ? subscriptions : new ArrayList<>();
        this.activeSubscriptions = filterActiveSubscriptions(this.subscriptions);
    }

    private List<UserSubscriptionDto> filterActiveSubscriptions(List<UserSubscriptionDto> all) {
        List<UserSubscriptionDto> active = new ArrayList<>();
        Date now = new Date();

        for (UserSubscriptionDto sub : all) {
            if (sub.isActive()) {
                active.add(sub);
            }
        }
        return active;
    }

    public void updateSubscriptions(List<UserSubscriptionDto> newSubscriptions) {
        this.subscriptions = newSubscriptions != null ? newSubscriptions : new ArrayList<>();
        this.activeSubscriptions = filterActiveSubscriptions(this.subscriptions);
        notifyDataSetChanged();
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(context).inflate(R.layout.item_subscription_card, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        UserSubscriptionDto sub = activeSubscriptions.get(position);

        holder.tvSubscriptionName.setText(sub.getSubscriptionName());
        holder.tvDirectionName.setText(sub.getDirectionName());

        String priceText = String.format(java.util.Locale.getDefault(), "%.0f ₽", sub.getPrice());
        holder.tvPrice.setText(priceText);

        int used = sub.getSessionsUsed();
        int total = sub.getTotalSessions();
        int remaining = sub.getSessionsRemaining();

        String usedText = String.format(java.util.Locale.getDefault(), "Использовано: %d / %d", used, total);
        holder.tvUsed.setText(usedText);

        String remainingText = String.format(java.util.Locale.getDefault(), "Осталось: %d", remaining);
        holder.tvRemaining.setText(remainingText);

        // 👇 ДАТА ОКОНЧАНИЯ (1 год)
        String expiryText = "Действует до: " + sub.getFormattedExpiryDate();
        holder.tvExpiryDate.setText(expiryText);
        holder.tvExpiryDate.setVisibility(View.VISIBLE);

        // 👇 ДНИ ДО ОКОНЧАНИЯ
        if (sub.getDaysLeft() > 0 && sub.getDaysLeft() <= 365) {
            String daysLeftText = getDaysLeftText(sub.getDaysLeft());
            holder.tvDaysLeft.setText(daysLeftText);
            holder.tvDaysLeft.setVisibility(View.VISIBLE);

            // Если осталось меньше 30 дней - выделяем оранжевым, меньше 7 - красным
            if (sub.getDaysLeft() <= 7) {
                holder.tvDaysLeft.setTextColor(ContextCompat.getColor(context, android.R.color.holo_red_dark));
                holder.tvExpiryDate.setTextColor(ContextCompat.getColor(context, android.R.color.holo_red_dark));
            } else if (sub.getDaysLeft() <= 30) {
                holder.tvDaysLeft.setTextColor(ContextCompat.getColor(context, R.color.warning));
                holder.tvExpiryDate.setTextColor(ContextCompat.getColor(context, R.color.warning));
            } else {
                holder.tvDaysLeft.setTextColor(ContextCompat.getColor(context, R.color.fitness));
                holder.tvExpiryDate.setTextColor(ContextCompat.getColor(context, R.color.text_hint));
            }
        } else {
            holder.tvDaysLeft.setVisibility(View.GONE);
        }

        // Прогресс бар
        int progress = total > 0 ? (used * 100 / total) : 0;
        holder.progressBar.setProgress(progress);

        // Цвет карточки в зависимости от статуса
        if (sub.isExpired()) {
            holder.cardView.setCardBackgroundColor(ContextCompat.getColor(context, R.color.gray_light));
            holder.tvRemaining.setTextColor(ContextCompat.getColor(context, R.color.text_hint));
        } else if (sub.getDaysLeft() <= 7 && sub.getDaysLeft() > 0) {
            holder.cardView.setCardBackgroundColor(ContextCompat.getColor(context, R.color.warning_light));
            holder.tvRemaining.setTextColor(ContextCompat.getColor(context, R.color.warning));
        } else if ("active".equals(sub.getStatus())) {
            holder.cardView.setCardBackgroundColor(ContextCompat.getColor(context, R.color.white));
            holder.tvRemaining.setTextColor(ContextCompat.getColor(context, R.color.fitness));
        } else {
            holder.cardView.setCardBackgroundColor(ContextCompat.getColor(context, R.color.gray_light));
            holder.tvRemaining.setTextColor(ContextCompat.getColor(context, R.color.text_hint));
        }
    }

    private String getDaysLeftText(int days) {
        if (days == 1) return "Остался 1 день";
        if (days >= 2 && days <= 4) return "Осталось " + days + " дня";
        return "Осталось " + days + " дней";
    }

    @Override
    public int getItemCount() {
        return activeSubscriptions != null ? activeSubscriptions.size() : 0;
    }

    public static class ViewHolder extends RecyclerView.ViewHolder {
        public final CardView cardView;
        public final TextView tvSubscriptionName;
        public final TextView tvDirectionName;
        public final TextView tvPrice;
        public final TextView tvUsed;
        public final TextView tvRemaining;
        public final ProgressBar progressBar;
        public final TextView tvExpiryDate;
        public final TextView tvDaysLeft;

        public ViewHolder(@NonNull View itemView) {
            super(itemView);
            cardView = itemView.findViewById(R.id.cardView);
            tvSubscriptionName = itemView.findViewById(R.id.tvSubscriptionName);
            tvDirectionName = itemView.findViewById(R.id.tvDirectionName);
            tvPrice = itemView.findViewById(R.id.tvPrice);
            tvUsed = itemView.findViewById(R.id.tvUsed);
            tvRemaining = itemView.findViewById(R.id.tvRemaining);
            progressBar = itemView.findViewById(R.id.progressBar);
            tvExpiryDate = itemView.findViewById(R.id.tvExpiryDate);
            tvDaysLeft = itemView.findViewById(R.id.tvDaysLeft);
        }
    }
}