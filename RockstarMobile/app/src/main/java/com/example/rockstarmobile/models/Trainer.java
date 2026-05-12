package com.example.rockstarmobile.models;

import java.io.Serializable;
import java.util.List;

public class Trainer implements Serializable {
    private int id;
    private String firstName;
    private String lastName;
    private Integer directionId;
    private String directionName;
    private String directionKey;
    private String email;
    private String passwordHash;
    private String photoBase64;  // 👈 НОВОЕ ПОЛЕ (Base64 строка)
    private byte[] photo;        // 👈 НОВОЕ ПОЛЕ (байты для bitmap)
    private int experience;
    private String description;
    private boolean isActive;
    private String createdAt;
    private List<Direction> directions;  // 👈 НОВОЕ: несколько направлений

    // Конструктор по умолчанию
    public Trainer() {}

    // Геттеры и сеттеры
    public int getId() { return id; }
    public void setId(int id) { this.id = id; }

    public String getFirstName() { return firstName; }
    public void setFirstName(String firstName) { this.firstName = firstName; }

    public String getLastName() { return lastName; }
    public void setLastName(String lastName) { this.lastName = lastName; }

    public Integer getDirectionId() { return directionId; }
    public void setDirectionId(Integer directionId) { this.directionId = directionId; }

    public String getDirectionName() { return directionName; }
    public void setDirectionName(String directionName) { this.directionName = directionName; }

    public String getDirectionKey() { return directionKey; }
    public void setDirectionKey(String directionKey) { this.directionKey = directionKey; }

    public String getEmail() { return email; }
    public void setEmail(String email) { this.email = email; }

    public String getPasswordHash() { return passwordHash; }
    public void setPasswordHash(String passwordHash) { this.passwordHash = passwordHash; }

    public String getPhotoBase64() { return photoBase64; }
    public void setPhotoBase64(String photoBase64) {
        this.photoBase64 = photoBase64;
        // Конвертируем Base64 в байты при установке
        if (photoBase64 != null && !photoBase64.isEmpty()) {
            try {
                this.photo = android.util.Base64.decode(photoBase64, android.util.Base64.DEFAULT);
            } catch (Exception e) {
                this.photo = null;
            }
        }
    }

    public byte[] getPhoto() { return photo; }
    public void setPhoto(byte[] photo) { this.photo = photo; }

    public int getExperience() { return experience; }
    public void setExperience(int experience) { this.experience = experience; }

    public String getDescription() { return description; }
    public void setDescription(String description) { this.description = description; }

    public boolean isActive() { return isActive; }
    public void setActive(boolean active) { isActive = active; }

    public String getCreatedAt() { return createdAt; }
    public void setCreatedAt(String createdAt) { this.createdAt = createdAt; }

    public List<Direction> getDirections() { return directions; }
    public void setDirections(List<Direction> directions) { this.directions = directions; }

    public String getFullName() {
        if (firstName == null && lastName == null) return "Тренер";
        if (firstName == null) return lastName;
        if (lastName == null) return firstName;
        return firstName + " " + lastName;
    }

    public String getExperienceDisplay() {
        return "Стаж: " + experience + " " + getExperienceWord(experience);
    }

    private String getExperienceWord(int years) {
        if (years % 10 == 1 && years % 100 != 11) return "год";
        if (years % 10 >= 2 && years % 10 <= 4 && (years % 100 < 10 || years % 100 >= 20)) return "года";
        return "лет";
    }

    // 👇 НОВЫЙ МЕТОД: получение списка направлений для отображения
    public String getDirectionsDisplay() {
        if (directions == null || directions.isEmpty()) {
            return directionName != null ? directionName : "Направление не указано";
        }
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < directions.size(); i++) {
            if (i > 0) sb.append(", ");
            sb.append(directions.get(i).getName());
        }
        return sb.toString();
    }
}