package com.example.rockstarmobile.models;

import java.io.Serializable;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Locale;

public class UserSubscriptionDto implements Serializable {
    private int purchaseId;
    private int subscriptionId;
    private String subscriptionName;
    private double price;
    private int totalSessions;
    private int sessionsUsed;
    private String purchaseDate;
    private String expiryDate;      // Дата окончания (1 год)
    private String status;
    private int directionId;
    private String directionName;
    private int daysLeft;            // дней до окончания

    // Геттеры
    public int getPurchaseId() { return purchaseId; }
    public int getSubscriptionId() { return subscriptionId; }
    public String getSubscriptionName() { return subscriptionName; }
    public double getPrice() { return price; }
    public int getTotalSessions() { return totalSessions; }
    public int getSessionsUsed() { return sessionsUsed; }
    public int getSessionsRemaining() { return totalSessions - sessionsUsed; }
    public String getPurchaseDate() { return purchaseDate; }
    public String getExpiryDate() { return expiryDate; }
    public String getStatus() { return status; }
    public int getDirectionId() { return directionId; }
    public String getDirectionName() { return directionName; }
    public int getDaysLeft() { return daysLeft; }

    // Сеттеры
    public void setPurchaseId(int purchaseId) { this.purchaseId = purchaseId; }
    public void setSubscriptionId(int subscriptionId) { this.subscriptionId = subscriptionId; }
    public void setSubscriptionName(String subscriptionName) { this.subscriptionName = subscriptionName; }
    public void setPrice(double price) { this.price = price; }
    public void setTotalSessions(int totalSessions) { this.totalSessions = totalSessions; }
    public void setSessionsUsed(int sessionsUsed) { this.sessionsUsed = sessionsUsed; }
    public void setPurchaseDate(String purchaseDate) { this.purchaseDate = purchaseDate; }
    public void setExpiryDate(String expiryDate) { this.expiryDate = expiryDate; }
    public void setStatus(String status) { this.status = status; }
    public void setDirectionId(int directionId) { this.directionId = directionId; }
    public void setDirectionName(String directionName) { this.directionName = directionName; }
    public void setDaysLeft(int daysLeft) { this.daysLeft = daysLeft; }

    // Форматированная дата окончания для отображения
    public String getFormattedExpiryDate() {
        if (expiryDate == null || expiryDate.isEmpty()) {
            return "Без ограничения";
        }
        try {
            SimpleDateFormat inputFormat = new SimpleDateFormat("yyyy-MM-dd", Locale.getDefault());
            SimpleDateFormat outputFormat = new SimpleDateFormat("dd.MM.yyyy", Locale.getDefault());
            Date date = inputFormat.parse(expiryDate);
            return outputFormat.format(date);
        } catch (Exception e) {
            return expiryDate;
        }
    }

    // Проверка активности абонемента
    public boolean isActive() {
        if (!"active".equals(status)) return false;
        if (getSessionsRemaining() <= 0) return false;

        // Проверка по дате окончания
        if (expiryDate != null && !expiryDate.isEmpty()) {
            try {
                SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd", Locale.getDefault());
                Date expiry = sdf.parse(expiryDate);
                if (expiry != null && expiry.before(new Date())) {
                    return false;
                }
            } catch (Exception e) {
                // ignore
            }
        }
        return true;
    }

    public boolean isExpired() {
        if (expiryDate == null || expiryDate.isEmpty()) return false;
        try {
            SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd", Locale.getDefault());
            Date expiry = sdf.parse(expiryDate);
            return expiry != null && expiry.before(new Date());
        } catch (Exception e) {
            return false;
        }
    }

    public String getStatusDisplay() {
        if (isExpired()) return "Истек";
        if (!isActive()) return "Неактивен";
        if (getSessionsRemaining() <= 0) return "Использован";
        return "Активен";
    }
}