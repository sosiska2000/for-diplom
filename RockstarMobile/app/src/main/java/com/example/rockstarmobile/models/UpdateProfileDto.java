package com.example.rockstarmobile.models;

import java.util.Date;

public class UpdateProfileDto {
    private String firstName;
    private String lastName;
    private String phone;
    private Integer age;
    private String photoBase64;
    private Date birthDate;

    // Конструктор по умолчанию
    public UpdateProfileDto() {}

    // Конструктор со всеми полями
    public UpdateProfileDto(String firstName, String lastName, String phone, Integer age, String photoBase64) {
        this.firstName = firstName;
        this.lastName = lastName;
        this.phone = phone;
        this.age = age;
        this.photoBase64 = photoBase64;
    }

    // Геттеры и сеттеры
    public String getFirstName() {
        return firstName;
    }

    public void setFirstName(String firstName) {
        this.firstName = firstName;
    }

    public String getLastName() {
        return lastName;
    }

    public void setLastName(String lastName) {
        this.lastName = lastName;
    }

    public String getPhone() {
        return phone;
    }

    public void setPhone(String phone) {
        this.phone = phone;
    }

    public Integer getAge() {
        return age;
    }

    public void setAge(Integer age) {
        this.age = age;
    }

    public String getPhotoBase64() {
        return photoBase64;
    }

    public void setPhotoBase64(String photoBase64) {
        this.photoBase64 = photoBase64;
    }

    public Date getBirthDate() {
        return birthDate;
    }

    public void setBirthDate(Date birthDate) {
        this.birthDate = birthDate;
    }
}