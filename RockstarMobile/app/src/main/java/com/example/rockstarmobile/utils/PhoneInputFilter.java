package com.example.rockstarmobile.utils;
import android.text.InputFilter;
import android.text.Spanned;
public class PhoneInputFilter implements InputFilter {
    @Override
    public CharSequence filter(CharSequence source, int start, int end, Spanned dest, int dstart, int dend) {
        StringBuilder filtered = new StringBuilder();
        for (int i = start; i < end; i++) {
            char c = source.charAt(i);
            if (Character.isDigit(c) ||
                    c == '+' ||
                    c == '-' ||
                    c == ' ' ||
                    c == '(' ||
                    c == ')' ||
                    c == '*' ||
                    c == '#') {
                filtered.append(c);
            }
        }
        return filtered.toString();
    }
}