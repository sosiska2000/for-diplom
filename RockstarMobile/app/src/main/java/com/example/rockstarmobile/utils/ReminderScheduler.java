package com.example.rockstarmobile.utils;

import android.content.Context;

import androidx.work.PeriodicWorkRequest;
import androidx.work.WorkManager;

import com.example.rockstarmobile.models.ReminderWorker;

import java.util.concurrent.TimeUnit;

public class ReminderScheduler {

    private static final long REMINDER_INTERVAL_HOURS = 6; // Проверка каждые 6 часов

    public static void scheduleReminders(Context context) {
        // Создаём периодическую задачу
        PeriodicWorkRequest reminderWork = new PeriodicWorkRequest.Builder(
                ReminderWorker.class,
                REMINDER_INTERVAL_HOURS, TimeUnit.HOURS)
                .build();

        WorkManager.getInstance(context).enqueue(reminderWork);
    }

    public static void cancelReminders(Context context) {
        WorkManager.getInstance(context).cancelAllWorkByTag("reminder_work");
    }
}