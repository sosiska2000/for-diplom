package com.example.rockstarmobile.models;

import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.content.Context;
import android.os.Build;

import androidx.annotation.NonNull;
import androidx.core.app.NotificationCompat;
import androidx.work.Worker;
import androidx.work.WorkerParameters;

import com.example.rockstarmobile.R;
import com.example.rockstarmobile.utils.ApiClient;
import com.example.rockstarmobile.utils.SessionManager;

import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.List;
import java.util.Locale;

import retrofit2.Call;
import retrofit2.Response;

public class ReminderWorker extends Worker {

    private static final String CHANNEL_ID = "training_reminder_channel";
    private static final int NOTIFICATION_ID = 2000;

    public ReminderWorker(@NonNull Context context, @NonNull WorkerParameters params) {
        super(context, params);
    }

    @NonNull
    @Override
    public Result doWork() {
        try {
            SessionManager sessionManager = new SessionManager(getApplicationContext());

            // Проверяем, авторизован ли пользователь
            if (!sessionManager.isLoggedIn()) {
                return Result.success();
            }

            // Получаем расписание пользователя
            ApiClient apiClient = ApiClient.getInstance(getApplicationContext());
            Call<List<Schedule>> call = apiClient.getApiService().getMySchedule();
            Response<List<Schedule>> response = call.execute();

            if (response.isSuccessful() && response.body() != null) {
                Date now = new Date();

                for (Schedule schedule : response.body()) {
                    Date scheduleDateTime = parseDateTime(schedule.getDateTime());

                    if (scheduleDateTime != null) {
                        // Проверяем, что занятие в будущем
                        if (scheduleDateTime.after(now)) {
                            long diffMs = scheduleDateTime.getTime() - now.getTime();
                            long diffHours = diffMs / (60 * 60 * 1000);

                            // Если до занятия осталось 4 часа (± 30 минут)
                            if (diffHours >= 3.5 && diffHours <= 4.5) {
                                sendReminderNotification(schedule);
                            }
                        }
                    }
                }
            }

            return Result.success();
        } catch (Exception e) {
            e.printStackTrace();
            return Result.retry();
        }
    }

    private Date parseDateTime(String dateTimeStr) {
        try {
            // Формат: "2026-05-15T10:00:00" или "2026-05-15 10:00:00"
            String cleaned = dateTimeStr.replace("T", " ");
            SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.getDefault());
            return sdf.parse(cleaned);
        } catch (Exception e) {
            return null;
        }
    }

    private void sendReminderNotification(Schedule schedule) {
        createNotificationChannel();

        String title = "⏰ Напоминание о тренировке";
        String content = schedule.getDirectionName() + " - " + schedule.getServiceName() +
                "\nТренер: " + schedule.getTrainerName() +
                "\nНачало: " + schedule.getTimeRange();

        NotificationCompat.Builder builder = new NotificationCompat.Builder(getApplicationContext(), CHANNEL_ID)
                .setSmallIcon(R.drawable.ic_notification)
                .setContentTitle(title)
                .setContentText(content)
                .setPriority(NotificationCompat.PRIORITY_HIGH)
                .setAutoCancel(true)
                .setVibrate(new long[]{0, 500, 200, 500});

        NotificationManager notificationManager =
                (NotificationManager) getApplicationContext().getSystemService(Context.NOTIFICATION_SERVICE);

        if (notificationManager != null) {
            notificationManager.notify(schedule.getId(), builder.build());
        }
    }

    private void createNotificationChannel() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            NotificationChannel channel = new NotificationChannel(
                    CHANNEL_ID,
                    "Напоминания о тренировках",
                    NotificationManager.IMPORTANCE_HIGH
            );
            channel.setDescription("Уведомления о предстоящих тренировках за 4 часа до начала");
            channel.enableVibration(true);
            channel.setVibrationPattern(new long[]{0, 500, 200, 500});

            NotificationManager notificationManager =
                    (NotificationManager) getApplicationContext().getSystemService(Context.NOTIFICATION_SERVICE);

            if (notificationManager != null) {
                notificationManager.createNotificationChannel(channel);
            }
        }
    }
}