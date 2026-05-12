package com.example.rockstarmobile.models;

public class EnrollResponse {
    private String message;
    private String paymentType;
    private double price;
    private boolean usedSubscription;

    public EnrollResponse() {}

    public String getMessage() { return message; }
    public void setMessage(String message) { this.message = message; }

    public String getPaymentType() { return paymentType; }
    public void setPaymentType(String paymentType) { this.paymentType = paymentType; }

    public double getPrice() { return price; }
    public void setPrice(double price) { this.price = price; }

    public boolean isUsedSubscription() { return usedSubscription; }
    public void setUsedSubscription(boolean usedSubscription) { this.usedSubscription = usedSubscription; }
}