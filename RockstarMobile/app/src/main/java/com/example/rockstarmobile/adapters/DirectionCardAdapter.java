package com.example.rockstarmobile.adapters;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.cardview.widget.CardView;
import androidx.recyclerview.widget.RecyclerView;

import com.example.rockstarmobile.R;
import com.example.rockstarmobile.models.Direction;

import java.util.List;

public class DirectionCardAdapter extends RecyclerView.Adapter<DirectionCardAdapter.ViewHolder> {

    private Context context;
    private List<Direction> directions;
    private OnDirectionClickListener listener;

    public interface OnDirectionClickListener {
        void onDirectionClick(Direction direction);
    }

    public DirectionCardAdapter(Context context, List<Direction> directions, OnDirectionClickListener listener) {
        this.context = context;
        this.directions = directions;
        this.listener = listener;
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(context).inflate(R.layout.item_direction_card, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        Direction direction = directions.get(position);

        holder.tvDirectionName.setText(direction.getName());
        holder.tvDirectionDescription.setText(direction.getDescription());

        // Устанавливаем иконку в зависимости от направления
        setDirectionIcon(holder.ivDirectionIcon, direction.getNameKey());

        // Устанавливаем цвет фона
        setDirectionColor(holder.cardView, direction.getNameKey());

        holder.itemView.setOnClickListener(v -> {
            if (listener != null) {
                listener.onDirectionClick(direction);
            }
        });
    }

    private void setDirectionIcon(ImageView imageView, String nameKey) {
        if (nameKey == null) return;

        switch (nameKey) {
            case "yoga":
                imageView.setImageResource(R.drawable.ic_yoga);
                break;
            case "fitness":
                imageView.setImageResource(R.drawable.ic_fitness);
                break;
            case "climbing":
                imageView.setImageResource(R.drawable.ic_climbing);
                break;
            default:
                imageView.setImageResource(R.drawable.ic_yoga);
                break;
        }
    }

    private void setDirectionColor(CardView cardView, String nameKey) {
        int color;
        if (nameKey == null) {
            color = 0xFFFF4D00;
        } else {
            switch (nameKey) {
                case "yoga":
                    color = 0xFFFF9F4D;
                    break;
                case "fitness":
                    color = 0xFF4CAF50;
                    break;
                case "climbing":
                    color = 0xFF2196F3;
                    break;
                default:
                    color = 0xFFFF4D00;
                    break;
            }
        }
        cardView.setCardBackgroundColor(color);
    }

    @Override
    public int getItemCount() {
        return directions != null ? directions.size() : 0;
    }

    static class ViewHolder extends RecyclerView.ViewHolder {
        CardView cardView;
        ImageView ivDirectionIcon;
        TextView tvDirectionName, tvDirectionDescription;

        ViewHolder(@NonNull View itemView) {
            super(itemView);
            cardView = itemView.findViewById(R.id.cardView);
            ivDirectionIcon = itemView.findViewById(R.id.ivDirectionIcon);
            tvDirectionName = itemView.findViewById(R.id.tvDirectionName);
            tvDirectionDescription = itemView.findViewById(R.id.tvDirectionDescription);
        }
    }
}