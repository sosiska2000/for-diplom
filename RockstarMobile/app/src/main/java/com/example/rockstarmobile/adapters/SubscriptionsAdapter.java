package com.example.rockstarmobile.adapters;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.cardview.widget.CardView;
import androidx.recyclerview.widget.RecyclerView;

import com.example.rockstarmobile.R;
import com.example.rockstarmobile.models.Subscription;

import java.util.List;

public class SubscriptionsAdapter extends RecyclerView.Adapter<SubscriptionsAdapter.SubscriptionViewHolder> {

    private Context context;
    private List<Subscription> subscriptions;
    private OnSubscriptionClickListener listener;

    public interface OnSubscriptionClickListener {
        void onSubscriptionClick(Subscription subscription);
    }

    public SubscriptionsAdapter(Context context, List<Subscription> subscriptions, OnSubscriptionClickListener listener) {
        this.context = context;
        this.subscriptions = subscriptions;
        this.listener = listener;
    }

    @NonNull
    @Override
    public SubscriptionViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(context).inflate(R.layout.item_subscription, parent, false);
        return new SubscriptionViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull SubscriptionViewHolder holder, int position) {
        if (subscriptions != null && position < subscriptions.size()) {
            Subscription subscription = subscriptions.get(position);

            holder.tvSubscriptionName.setText(subscription.getName());
            holder.tvDirectionName.setText(subscription.getDirectionDisplay());
            holder.tvPrice.setText(subscription.getPriceDisplay());
            holder.tvSessionsCount.setText(subscription.getSessionsDisplay());

            // Проверяем, что cardView не null перед установкой слушателя
            if (holder.cardView != null) {
                holder.cardView.setOnClickListener(v -> {
                    if (listener != null) {
                        listener.onSubscriptionClick(subscription);
                    }
                });
            }
        }
    }

    @Override
    public int getItemCount() {
        return subscriptions != null ? subscriptions.size() : 0;
    }

    static class SubscriptionViewHolder extends RecyclerView.ViewHolder {
        CardView cardView;
        TextView tvSubscriptionName, tvDirectionName, tvPrice, tvSessionsCount;

        SubscriptionViewHolder(@NonNull View itemView) {
            super(itemView);
            cardView = itemView.findViewById(R.id.cardView);
            tvSubscriptionName = itemView.findViewById(R.id.tvSubscriptionName);
            tvDirectionName = itemView.findViewById(R.id.tvDirectionName);
            tvPrice = itemView.findViewById(R.id.tvPrice);
            tvSessionsCount = itemView.findViewById(R.id.tvSessionsCount);
        }
    }
}