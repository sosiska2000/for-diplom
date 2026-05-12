package com.example.rockstarmobile.utils;

import android.content.Context;
import android.content.SharedPreferences;

import com.example.rockstarmobile.models.User;
import com.google.gson.Gson;

public class SessionManager {
    private static final String PREF_NAME = "RockstarPref";
    private static final String KEY_USER = "user";
    private static final String KEY_TOKEN = "token";
    private static final String KEY_IS_LOGGED_IN = "isLoggedIn";

    private final SharedPreferences pref;
    private final SharedPreferences.Editor editor;
    private final Gson gson;

    public SessionManager(Context context) {
        pref = context.getSharedPreferences(PREF_NAME, Context.MODE_PRIVATE);
        editor = pref.edit();
        gson = new Gson();
    }

    // Метод для сохранения при входе/регистрации (с токеном)
    public void saveUser(User user, String token) {
        String userJson = gson.toJson(user);
        editor.putString(KEY_USER, userJson);
        editor.putString(KEY_TOKEN, token);
        editor.putBoolean(KEY_IS_LOGGED_IN, true);
        editor.apply();
    }

    // Метод для сохранения при обновлении профиля (без токена)
    public void saveUser(User user) {
        String userJson = gson.toJson(user);
        editor.putString(KEY_USER, userJson);
        editor.putBoolean(KEY_IS_LOGGED_IN, true);
        editor.apply();
    }

    // Обновление только токена (например, при refresh)
    public void updateToken(String newToken) {
        editor.putString(KEY_TOKEN, newToken);
        editor.apply();
    }

    public User getUser() {
        String userJson = pref.getString(KEY_USER, null);
        if (userJson == null) {
            return null;
        }
        return gson.fromJson(userJson, User.class);
    }

    public String getToken() {
        return pref.getString(KEY_TOKEN, null);
    }

    public boolean isLoggedIn() {
        return pref.getBoolean(KEY_IS_LOGGED_IN, false);
    }

    public void logout() {
        editor.clear();
        editor.apply();
    }
}