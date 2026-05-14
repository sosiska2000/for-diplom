package com.example.rockstarmobile.utils;

import android.text.InputFilter;
import android.text.Spanned;

public class NameInputFilter implements InputFilter {

    private final boolean allowSpaces;

    public NameInputFilter() {
        this.allowSpaces = true;
    }

    public NameInputFilter(boolean allowSpaces) {
        this.allowSpaces = allowSpaces;
    }

    @Override
    public CharSequence filter(CharSequence source, int start, int end, Spanned dest, int dstart, int dend) {
        StringBuilder filtered = new StringBuilder();

        for (int i = start; i < end; i++) {
            char c = source.charAt(i);

            // Разрешаем буквы (русские и английские), дефис и апостроф
            if (Character.isLetter(c) || c == '-' || c == '\'' || c == ' ') {
                // Если символ - пробел, проверяем разрешены ли пробелы
                if (c == ' ') {
                    if (allowSpaces) {
                        filtered.append(c);
                    }
                } else {
                    filtered.append(c);
                }
            }
            // Все остальные символы (цифры, спецсимволы) игнорируются
        }

        return filtered.toString();
    }
}