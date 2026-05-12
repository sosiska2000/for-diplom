-- phpMyAdmin SQL Dump
-- version 5.2.0
-- https://www.phpmyadmin.net/
--
-- Хост: 127.0.0.1:3306
-- Время создания: Май 12 2026 г., 10:59
-- Версия сервера: 5.7.39
-- Версия PHP: 8.0.22

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- База данных: `rockstar_club`
--

-- --------------------------------------------------------

--
-- Структура таблицы `directions`
--

CREATE TABLE `directions` (
  `id` int(11) NOT NULL,
  `name` varchar(100) NOT NULL,
  `name_key` varchar(50) NOT NULL,
  `description` text,
  `is_active` tinyint(1) DEFAULT '1',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Дамп данных таблицы `directions`
--

INSERT INTO `directions` (`id`, `name`, `name_key`, `description`, `is_active`, `created_at`, `updated_at`) VALUES
(1, 'Йога', 'yoga', 'Практики для гармонии тела и духа. Йога помогает улучшить гибкость, снять стресс и обрести внутренний баланс.', 1, '2026-03-07 06:39:03', '2026-03-07 06:39:03'),
(2, 'Фитнес', 'fitness', 'Тренажерный зал и групповые тренировки для достижения оптимальной физической формы и укрепления здоровья.', 1, '2026-03-07 06:39:03', '2026-03-07 06:39:03'),
(3, 'Скалолазание', 'climbing', 'Скалодром для всех уровней подготовки. Развивает координацию, силу и выносливость.', 1, '2026-03-07 06:39:03', '2026-03-07 06:39:03'),
(4, 'Спортивная стрельба', 'спортивная_стрельба', '', 1, '2026-05-03 15:22:55', '2026-05-03 15:22:55');

-- --------------------------------------------------------

--
-- Структура таблицы `enrollments`
--

CREATE TABLE `enrollments` (
  `id` int(11) NOT NULL,
  `user_id` int(11) NOT NULL,
  `schedule_id` int(11) NOT NULL,
  `enrolled_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `status` enum('enrolled','attended','cancelled','no_show') DEFAULT 'enrolled',
  `PaymentType` varchar(20) DEFAULT 'cash',
  `Price` decimal(10,2) DEFAULT NULL,
  `PurchaseId` int(11) DEFAULT NULL,
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Дамп данных таблицы `enrollments`
--

INSERT INTO `enrollments` (`id`, `user_id`, `schedule_id`, `enrolled_at`, `status`, `PaymentType`, `Price`, `PurchaseId`, `created_at`, `updated_at`) VALUES
(1, 29, 33, '2026-05-11 14:59:20', 'enrolled', 'cash', NULL, NULL, '2026-05-11 17:59:20', '2026-05-11 17:59:20');

-- --------------------------------------------------------

--
-- Структура таблицы `schedule`
--

CREATE TABLE `schedule` (
  `id` int(11) NOT NULL,
  `trainer_id` int(11) DEFAULT NULL,
  `direction_id` int(11) NOT NULL,
  `service_id` int(11) DEFAULT NULL,
  `datetime` datetime NOT NULL,
  `duration_minutes` int(11) NOT NULL,
  `max_participants` int(11) DEFAULT '20',
  `current_participants` int(11) DEFAULT '0',
  `price` decimal(10,2) DEFAULT NULL,
  `is_group` tinyint(1) DEFAULT '1',
  `is_active` tinyint(1) DEFAULT '1',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `RecurringScheduleId` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Дамп данных таблицы `schedule`
--

INSERT INTO `schedule` (`id`, `trainer_id`, `direction_id`, `service_id`, `datetime`, `duration_minutes`, `max_participants`, `current_participants`, `price`, `is_group`, `is_active`, `created_at`, `updated_at`, `RecurringScheduleId`) VALUES
(23, 4, 2, NULL, '2026-04-18 10:00:00', 60, 20, 0, '500.00', 1, 1, '2026-04-17 16:07:12', '2026-04-17 16:07:12', NULL),
(24, 4, 2, NULL, '2026-04-19 10:00:00', 60, 20, 0, '500.00', 1, 1, '2026-04-17 16:07:32', '2026-04-17 16:07:32', NULL),
(25, 4, 2, NULL, '2026-04-20 10:00:00', 60, 20, 1, '500.00', 1, 1, '2026-04-17 16:07:48', '2026-04-17 16:43:10', NULL),
(26, 2, 1, NULL, '2026-04-01 10:00:00', 60, 20, 0, '450.00', 1, 1, '2026-04-20 10:05:03', '2026-04-20 10:05:03', NULL),
(27, 4, 3, NULL, '2026-04-20 10:00:00', 60, 20, 0, '600.00', 1, 1, '2026-04-20 10:06:50', '2026-04-20 10:06:50', NULL),
(28, 4, 1, NULL, '2026-04-21 10:00:00', 60, 20, 2, '600.00', 1, 1, '2026-04-20 10:07:15', '2026-04-20 10:08:59', NULL),
(29, 4, 1, NULL, '2026-04-21 10:00:00', 60, 20, 0, '500.00', 1, 1, '2026-04-20 10:11:59', '2026-04-20 10:11:59', NULL),
(31, 7, 4, 2, '2026-05-16 10:00:00', 60, 20, 2, '999.00', 1, 1, '2026-05-03 15:24:24', '2026-05-03 15:25:44', NULL),
(32, 7, 4, 2, '2026-05-06 10:00:00', 60, 20, 2, '999.00', 1, 1, '2026-05-04 05:56:53', '2026-05-04 06:09:10', NULL),
(33, 7, 1, 3, '2026-05-12 10:00:00', 60, 20, 1, '999.00', 1, 1, '2026-05-11 17:58:49', '2026-05-11 17:59:20', NULL);

-- --------------------------------------------------------

--
-- Структура таблицы `services`
--

CREATE TABLE `services` (
  `id` int(11) NOT NULL,
  `direction_id` int(11) NOT NULL,
  `name` varchar(255) NOT NULL,
  `price` decimal(10,2) NOT NULL,
  `sessions_count` int(11) DEFAULT '1',
  `duration_minutes` int(11) DEFAULT NULL,
  `description` text,
  `is_active` tinyint(1) DEFAULT '1',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Дамп данных таблицы `services`
--

INSERT INTO `services` (`id`, `direction_id`, `name`, `price`, `sessions_count`, `duration_minutes`, `description`, `is_active`, `created_at`, `updated_at`) VALUES
(1, 2, 'Групповая тренировка', '600.00', 1, 120, '', 1, '2026-04-27 09:35:09', '2026-04-27 09:35:09'),
(2, 4, 'Групповая тренировка', '999.00', 14, 120, '', 1, '2026-05-03 15:23:26', '2026-05-03 15:23:26'),
(3, 1, 'групповая тренирка', '999.00', 12, 129, 'тест', 1, '2026-05-11 15:58:21', '2026-05-11 15:58:21');

-- --------------------------------------------------------

--
-- Структура таблицы `service_types`
--

CREATE TABLE `service_types` (
  `id` int(11) NOT NULL,
  `direction_id` int(11) NOT NULL,
  `name` varchar(255) NOT NULL,
  `description` text,
  `default_duration` int(11) DEFAULT '60',
  `is_active` tinyint(1) DEFAULT '1',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Дамп данных таблицы `service_types`
--

INSERT INTO `service_types` (`id`, `direction_id`, `name`, `description`, `default_duration`, `is_active`, `created_at`, `updated_at`) VALUES
(1, 1, 'Хатха-йога', 'Классическая йога, работа с телом и дыханием. Подходит для начинающих.', 60, 1, '2026-03-07 06:39:03', '2026-03-07 06:39:03'),
(2, 1, 'Аштанга-йога', 'Динамическая йога с фиксированной последовательностью асан. Требует хорошей физической подготовки.', 90, 1, '2026-03-07 06:39:03', '2026-03-07 06:39:03'),
(3, 1, 'Функциональная йога', 'Йога для развития силы и выносливости. Сочетает классические асаны с силовыми упражнениями.', 60, 1, '2026-03-07 06:39:03', '2026-03-07 06:39:03'),
(4, 2, 'Силовой тренинг', 'Тренировки с отягощениями для набора мышечной массы и развития силы.', 60, 1, '2026-03-07 06:39:03', '2026-03-07 06:39:03'),
(5, 2, 'Кардио', 'Аэробные нагрузки для укрепления сердечно-сосудистой системы и сжигания калорий.', 45, 1, '2026-03-07 06:39:03', '2026-03-07 06:39:03'),
(6, 2, 'Функциональный тренинг', 'Тренировки для развития функциональных качеств, координации и выносливости.', 60, 1, '2026-03-07 06:39:03', '2026-03-07 06:39:03'),
(7, 3, 'Боулдеринг', 'Лазание на небольшую высоту без страховки. Развивает силу и технику.', 120, 1, '2026-03-07 06:39:03', '2026-03-07 06:39:03'),
(8, 3, 'Трудность', 'Лазание на высоту с верхней страховкой. Требует выносливости и тактического мышления.', 120, 1, '2026-03-07 06:39:03', '2026-03-07 06:39:03'),
(9, 3, 'Скорость', 'Скоростное лазание на время на специальном эталонном маршруте.', 60, 1, '2026-03-07 06:39:03', '2026-03-07 06:39:03');

-- --------------------------------------------------------

--
-- Структура таблицы `subscriptions`
--

CREATE TABLE `subscriptions` (
  `id` int(11) NOT NULL,
  `name` varchar(255) NOT NULL,
  `direction_id` int(11) DEFAULT NULL,
  `price` decimal(10,2) NOT NULL,
  `sessions_count` int(11) NOT NULL,
  `description` text,
  `is_active` tinyint(1) DEFAULT '1',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `validity_days` int(11) NOT NULL DEFAULT '30'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Дамп данных таблицы `subscriptions`
--

INSERT INTO `subscriptions` (`id`, `name`, `direction_id`, `price`, `sessions_count`, `description`, `is_active`, `created_at`, `updated_at`, `validity_days`) VALUES
(1, 'Йога Старт', 1, '3500.00', 8, 'Абонемент на 8 занятий по йоге', 1, '2026-03-07 06:39:03', '2026-03-07 06:39:03', 30),
(2, 'Йога Профи', 1, '4800.00', 12, 'Абонемент на 12 занятий по йоге', 1, '2026-03-07 06:39:03', '2026-03-07 06:39:03', 30),
(3, 'Фитнес Базовый', 2, '3000.00', 10, 'Абонемент на 10 посещений зала', 1, '2026-03-07 06:39:03', '2026-03-07 06:39:03', 30),
(4, 'Фитнес Безлимит', 2, '5000.00', 30, 'Безлимитное посещение на месяц', 1, '2026-03-07 06:39:03', '2026-03-07 06:39:03', 30),
(5, 'Скалолазание Старт', 3, '4000.00', 8, 'Абонемент на 8 посещений скалодрома', 1, '2026-03-07 06:39:03', '2026-03-07 06:39:03', 30),
(6, 'Скалолазание Профи', 3, '5500.00', 12, 'Абонемент на 12 посещений скалодрома', 1, '2026-03-07 06:39:03', '2026-03-07 06:39:03', 30),
(8, 'тестовый абонемент фитнес', 2, '999.00', 4, 'тестовый абонемент фитнес', 1, '2026-05-03 15:21:26', '2026-05-03 15:21:26', 30),
(9, 'тестовый абонемент йога', 1, '999.00', 4, 'тестовый абонемент йога', 1, '2026-05-03 15:21:42', '2026-05-03 15:21:42', 30),
(10, 'тестовый абонемент скалолазание', 3, '999.00', 4, 'тестовый абонемент скалолазание', 1, '2026-05-03 15:22:07', '2026-05-03 15:22:07', 30);

-- --------------------------------------------------------

--
-- Структура таблицы `subscription_purchases`
--

CREATE TABLE `subscription_purchases` (
  `id` int(11) NOT NULL,
  `user_id` int(11) NOT NULL,
  `subscription_id` int(11) NOT NULL,
  `purchase_date` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `expiry_date` datetime DEFAULT NULL,
  `sessions_used` int(11) DEFAULT '0',
  `status` enum('active','expired','used_up','refunded') DEFAULT 'active',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Дамп данных таблицы `subscription_purchases`
--

INSERT INTO `subscription_purchases` (`id`, `user_id`, `subscription_id`, `purchase_date`, `expiry_date`, `sessions_used`, `status`, `created_at`, `updated_at`) VALUES
(9, 29, 9, '2026-05-11 08:04:39', '2026-06-10 11:04:39', 1, 'active', '2026-05-11 11:04:39', '2026-05-11 11:05:36'),
(10, 29, 10, '2026-05-11 08:04:53', '2026-06-10 11:04:53', 1, 'active', '2026-05-11 11:04:52', '2026-05-11 18:36:49'),
(11, 29, 8, '2026-05-11 08:05:05', '2026-06-10 11:05:05', 4, 'used_up', '2026-05-11 11:05:04', '2026-05-11 18:34:16');

-- --------------------------------------------------------

--
-- Структура таблицы `trainers`
--

CREATE TABLE `trainers` (
  `id` int(11) NOT NULL,
  `first_name` varchar(100) NOT NULL,
  `last_name` varchar(100) NOT NULL,
  `direction_id` int(11) DEFAULT NULL,
  `email` varchar(255) DEFAULT NULL,
  `password_hash` varchar(255) DEFAULT NULL,
  `photo` longblob,
  `experience` int(11) DEFAULT '0',
  `description` text,
  `is_active` tinyint(1) DEFAULT '1',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Дамп данных таблицы `trainers`
--

INSERT INTO `trainers` (`id`, `first_name`, `last_name`, `direction_id`, `email`, `password_hash`, `photo`, `experience`, `description`, `is_active`, `created_at`, `updated_at`) VALUES
(1, 'Анна', 'Соколова', 1, 'anna.sokolova@rockstar.ru', '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', NULL, 5, 'Сертифицированный инструктор по Хатха-йоге и Аштанга-йоге. Проводит индивидуальные и групповые занятия.', 1, '2026-03-07 06:39:03', '2026-03-07 06:39:03'),
(2, 'Дмитрий', 'Волков', 2, 'dmitry.volkov@rockstar.ru', '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 0xffd8ffe000104a46494600010101006000600000fffe003b43524541544f523a2067642d6a7065672076312e3020287573696e6720494a47204a50454720763830292c207175616c697479203d2039350affdb0043000201010101010201010102020202020403020202020504040304060506060605060606070908060709070606080b08090a0a0a0a0a06080b0c0b0a0c090a0a0affdb004301020202020202050303050a0706070a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0affc00011080064006403012200021101031101ffc4001f0000010501010101010100000000000000000102030405060708090a0bffc400b5100002010303020403050504040000017d01020300041105122131410613516107227114328191a1082342b1c11552d1f02433627282090a161718191a25262728292a3435363738393a434445464748494a535455565758595a636465666768696a737475767778797a838485868788898a92939495969798999aa2a3a4a5a6a7a8a9aab2b3b4b5b6b7b8b9bac2c3c4c5c6c7c8c9cad2d3d4d5d6d7d8d9dae1e2e3e4e5e6e7e8e9eaf1f2f3f4f5f6f7f8f9faffc4001f0100030101010101010101010000000000000102030405060708090a0bffc400b51100020102040403040705040400010277000102031104052131061241510761711322328108144291a1b1c109233352f0156272d10a162434e125f11718191a262728292a35363738393a434445464748494a535455565758595a636465666768696a737475767778797a82838485868788898a92939495969798999aa2a3a4a5a6a7a8a9aab2b3b4b5b6b7b8b9bac2c3c4c5c6c7c8c9cad2d3d4d5d6d7d8d9dae2e3e4e5e6e7e8e9eaf2f3f4f5f6f7f8f9faffda000c03010002110311003f00fc94f8ada86ab1eb370b65e229a1b4745616c72236665049099e3a9ae423bdbd7b02c24cc9e76018586ec6e1f5a9bc657b777de26b8b463e694e141e401b14f5c1c75acf8ee223672308570b2e1977ede770ee7a5650d20918cf5917ad12fee62f327d52e95b8f9648c1c703ff00afda9d042ab265a5dcd9c10cc47afaf14cd14ea8f0b49a65890a3ae2ed4f3b47a0f4a916eaf1a509751ba92dcee8837ea69b6ee4b562be966596cdde569598498049dfc6d1deafaac9a75a472dd49969250628981e40ea4e7f0a6f82341d47c41aac5a3787b4992e269e50638954b333600c003f0e057d05e19ff82717ed47e308d2faebe0e6b129650210b1aae005dd803b703db9e3926b9f118cc361bf8b351f5674e1b058bc57f069b95bb2b9e3363e2459227b7f22d932a46727355b4e874ad27538af2cda39e74e6357666e7a670072715ddfc49fd9b7c7bf08e2b96f19e837da4bc28e1adb50b5313e403c0278f5ff0026bc9b46593cfb822473b6d4e3332c9d947414a955a55a3cd4ddd0ab52ab467c95159f99dadc78c75d652be4f3927e581bd7dea8dcf8b357dac1d1c70464db7bfbd5b5f0e441994c432b9ce235e7151c9e1f812628215c302bf747f77e94d4a09ec2719f73307882f6e2ee38ccad8670398d57bd74d6430e39ae6ed2ce3b6d4227842a94954825471cd749664060289c93d8229f534e30a10039fc28a2271b3a7eb4565735385d427b4b0d7fc8be91733aab097001f4c1f6e383583ad476de75c4568ea419033294db86cae4574be23fb24d237ca923145dc000485c9fcbd6b9cd4a28ef233a68b8026da3c996403e619076b67bf1c1ade9c9593329ad5a134696dac6d9e19a28f24820a3b7f740f5a2321a7574c63773996a9696e2287ecf2c477ab6d68dad94107038ad164b7d22c5ee25b6569e41f21f2c0d9d39ad1e8ccad73e83ff008272f8235af13fed030ea3a468ab790e90ca2594ce02a4cf1ccf129279e7c86e718e3dc67f6a7e0a78ceded7c3cb65e26f0e5ed9df3da192de269b1e728744f9414063cb30c647435f841ff04f2f8f57ff00077f680b39aeae14e99addf595b6a6177020f9a424801ea577b803d1cf7c57eea1bf8bc45add8ea3e17f0e5ddfdaae871c4971a7c96e537348b2127cc6073f20c918e0e33e9f9df17466b191e7d9ad1fa6e7eb1c05ec6783928bf793d57e5f81f337fc1593e13f8dfe2ffc05b9f11e9bf0d678351d1cdd5c34334aa668ed61b696699c92143a858f208ce7a0e78afc74d08ee86ee5652316ff00c5084ee9e9d6bf65bfe0b65fb50dff00c31f83cdf0f3c1e90c77de2ed0aeed2f5d24ff008f6b66589270a0704bac853d00727ae2bf187c284982f4a8c7fa38e8a57ba7ad7b7c29ed7fb2dca4bdd6f4f43e5f8c9d0fed8e583bc925cdebfd6e7a9c85773b02b8218f03dea85ddca47728dd40932703daac5ddc6c180e07eeff00bd591a8dce496dc3a9e7757baaf7b9f3254cedbc5465e44838fc6b7ed1cee07f4ae4ee358b59f5b8a1b1ba8a5df3aeef2a4c851d715d4d81dc460d5b4d131b335231b901e68a6062a300d159f31b721c5788a3bdd27578c69565131102b06138073939ebf4ac4d7e24d4606d5ad96401462e218db0d1907a8183c67afa7d2b57c5179343a9452bc5f2bc28e8c739c64e7f23c572706a53e9ba8b5d592e03c8c6552c08619eb8ae8a49b8a67354b291a1a7eaf15faacf756f2ab45f2ac8dc6f183807d7eb568a9bab4918a9124c06dda78419c819a66917367a848e2ded912355e54740d9ebfe7d6b5c5922c58c0e5793ef5d94e829a4d984aa34ec8e8ff646d1bc2965fb46f82878cee8269efafda1b8b9b83f2467ce011dbd1431049ec33e95fd017c2cf82da569564ede1cf13cba7c173062e2c05d6d58cfa2ff00b3c763f8d7f3a9225dc1289e0750ca30014c8c7a57eb4ffc1163f6ff00f887f19340d57e01fc54b68f55d5fc2da08bbd2350527cfbeb48c88d849b890f2a16886eeac1b9e412df19c6b96e227878e2a96aa0acd7937bff0099f5bc259ac3075a5879af8ddd3f3b6c70bff05e4f85fe16d1b40f02eb16dabb35ec77f7d65047bffe3e219638da693df6b4708ebfc55f987a7f86ee749175e49df1cb1854001241caf0727d8d7bc7eda7fb647c5ffdb3fe2e49e32f1f34365676024b6d0b47b542134e80b124649f9a46e37b9e4900701540f2eb189db0d2282dfc4477ff000afa2c932dab83caa9d1aefde5dba5ddec7879c63a18ecce75e9ab27fa2b162fefd0c61d64dc3cbc7cac3d4d727e2ad64dc5a1d1a09195a590b4b229e523039c7b9240fc6ba5f105a470698d7d00fba46ec0ec7ffaf8af3f863b9d7bc413c5026e1103e66074009ff015bca97b2a963914f9e243e19b67d23c6b669086115da9203738c64ff4fd6bd534d76dc39ae0238611f102cad210cdf63b76695b6f42c08c7fe3c2bbdd35b1260fa545577b32e9a35558e386a292331ede4d15c6759c3fc5012e9f169f771c1b84e19514b9e30c327d31cd79f5c1934c97edadb637707089f30191d79f7aec3e21bc926b42eaf6ea4964f2132ef8c2e091f2803815cc5dac93afd9a40dbe5742b81f7464f1fad76d0f85238aaeadb3a0f06d9ada68d1c9203b9cee7cf707d7f0ae82ce51247180a5bf76a4f1ed556d628911215e005c2907a715268b23b07460331caca703df23f4af595a2ac71dcb8b6af2ff00cb323ea457db7ff0413d1a19ff006cbd505ec65a1ff840af8cdb41c81f69b4009c723048e7b57c5b03fcfb47a64d7e85ff00c1be36f6b73fb50f892e1747884b67e01bbf32ec3b6f904b7b61b508276803cb7c6003f31af078a2a72e41887fdd3d4c9173e6b49799f19fed2df0f62f007ed1de3cf06da8fdc697e30d4ad6df8eb1a5cc8aa7dfe502b95b2b131a02c8df50b9af77ff008292e8c9a07edd7f13ac228b607f13c93edc018f351253ff00a1e6bc661652bb57b707eb5e9e5f55d6c052a8fed462fef48e3c543d9e2aa43b49afc4a325a2dfd8dcda302a2552b823a1c7f8d7924fa5eb9e14bd9afe22d22cd0b7da3ca3f32b03d3d719c73ee6bd674ebc5b989e449061e462a738e09e2b96f1a20492ee2c311e449c2a83d40e29e2765233a5abb1c9f80d2eee2f9b5e9aeb2b2b04da78dc7b9c7d78fcebd174e94198303dab82f0b437165a3dbc4cc0ab4a190ede76905b9f7e6bb1d3d8fca735c3575675d2d11d1c4e0c6095068aa90cacb18145721d4719f166e6d67d5e3b8b73fb9d84a3471e149673c0e3a0cfbd72f35b4a75bb7b6176ace9282c919ce0024e7d38e3bf7aed7e22c2ad0da43b4a347e60603865f987502b92b422df5bb156400ca832e4fccd956c7f2fd2bb30cf58a38ebe899d1fcd0a000f6e307a543a14ccdae5d5b997e57db22f1df0011fcaacdd42b25848a79fdd9fe559ba1b15bc8eec96c9c13b860904953c7e22bd36df32386da1d558bab5d4919fe1419fc73fe15fa4ff00f06efda69f1fc65f1ceb51dd3492a784e386e22316047bae90ae1b3f36421ec318afcd4d18f9b7774f9e8e17f219feb5fa73ff0006ebe9721f16fc4ed52480f9634dd2e10f8e0ee92e491ff8ed7cef174b97876bfa2ffd291ecf0f2be6d4fe7f933c0bfe0b05a34ba37fc1433c7734b1e16fdb4fba848eead61029fc772b57cc57178d13c902360b444a1f7c57d55ff059b92f5ffe0a0be2cb2bd5ff008f7d3f4c488ff790d9c4f9fcd88fc2be4fd5e3313457383d319ff3f5af4323e6964b876ff923ff00a4a39b33b2cc6b5bf9a5f9999e0e90cfa7adabb8cc1218c00083c7a9e9d2a1f1ee977972c92585dc70916f207c44181031818cd58f0dcaa97570d200b87e060f27f0aa7f10b59934fb347fb32b17475058718c0e95db51de81cb056a873b636daa5a6976665ba81a1648ced5b52180d831f36e23b8ed5d169528295c8e9fe214bbb6834f36a15922450c1cf651ce3fe035d4e8c72bd735e7c99d713a1846630549c514b6db442a379e9dc0a2b94e939ff88eb7f2cd0efb7562d6eeec61524025f2589eddab80b8b92be23d361f39088dd15cac808cee3dfa77e95dc7c4f65bc923b082e76a456cf2322ae578e477c1e8df9579df8a122b4bcb7bab4d830a194b751f3023f4c57561ddac7355576cf478d18ae14f7e06de0fe359b6b6f1c06368d0796c9b4e3a018ffeb0abda2cb71796314cfb06e8c3121bb63f9567e9b3182c441336e6894a31f71c1fe55ebd8f3ac6f7841bceb792e580cbcec71fa7f4afd55ff8370238fed7f17a5963665f2f4253b467a9d43fc2bf2abc2f09b7d2e356fbd8c91f524d7e97ff00c1bf3f12bc33e12f127c48d17c57e35d1f43866d3b4d9d25d4afd2d84be5bdcee258ba6eda25f53806be6b8be129f0ed68c55dfbbffa544f6b2092866b4dbf3fc99e5fff0005bcbdd3ee3fe0a0fe22b6b2243dbe89a547700a1077fd95186723fbac9fa57c937ea925ac7950407afa13fe0aa3e2ef0f78dff6f3f1df88bc2be31b1d7f4e967b15b6d574bbf4b9825db616e8c16446756da5761c31c1420e0822be7e9d37da8e980e0e6bd3c8e0e9e4d878bdd423ff00a4a3933292966155afe67f99ce691227fc243716d6ebf386fe11d3dbad50f8bd6d141a1249214121908502320e369cf39af71fd8d7f62cf89ffb5ff8f750b4f8657da6dac5636fe65f6a3aa4ecb023b160917eed59b79c138c70064e38cf9f7ede3f02bc6bfb347c45ff008575e3a4b969b4f52d33c966634b88e4f97ce89f715963c80015e410c1b04722c7e16a4a587535ceba75b1acb2fc64284715283f66f452b6973c43405dda8c58fee73cff00b35dc686df3609ae7f4bf0e0b5d32cb572a4192056e7fda5ff00ebd6f6862b19ec4c16a7451e4a0e4f4a2a343f2e0f6f6a2b9ae6f731be20c10d96a50585ac4a91084e140fef641fad79b788a43259da9700b6f65ddd0801881fa51457461f58c4c2a7c4cfab7fe0979fb37780bf6a1f14a7863e246a1aa4769676eceaba65ca4664dbd158ba31c76e307deacffc146fe03fc35f801fb42c1e10f85fa44b63a6dce8105d3db4974f2812f9b2c6581725be61182727a9278ce28a2b2f6d5bfb7234f99f2f2bd2fa74e87b1f57a1feabcaaf22e6e74af657b6bd773c8ac00f2f8f4ad9b2b99eca2792de42a6489a2720f546ea3f414515ebe23f8323e7287f150fd2c916cbeee49cfb926addca83665071b885247b9028a2b487f017a13bd4f99fa43ff0443b58749fd9f66d5205df71a8fc49b9b4b99a500b18974d8dd541f661fa9f5af993fe0e07118f8f5e17b96883493786ef1198bb0c049f2bc0383cb1eb9eb4515f99e01bff005a27f3fc8fd7f354bfd43a3e91fccf897c1560977e018758b899da55bb785570a06d0063a0c9ebdcd6d68c002714515f6f3ea7e5d0dd1b89d3f1a28a2b98dd6c7fffd9, 8, 'Эксперт по силовым тренировкам и функциональному фитнесу. Мастер спорта по пауэрлифтингу.', 1, '2026-03-07 06:39:03', '2026-04-01 12:20:25'),
(3, 'Елена', 'Петрова', 3, 'elena.petrova@rockstar.ru', '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', NULL, 6, 'Мастер спорта по скалолазанию. Чемпионка России по боулдерингу.', 1, '2026-03-07 06:39:03', '2026-03-07 06:39:03'),
(4, 'Алексей', 'Иванов', 2, 'alexey.ivanov@rockstar.ru', '$2a$11$WSNaPaPeWE2KqY2y.oBg9Oj9EzeFBj9nP.43RMbxpgxCoY8x8ZKoi', NULL, 4, 'Тренер по функциональному тренингу и кроссфиту.', 1, '2026-03-07 06:39:03', '2026-04-20 10:03:04'),
(5, 'Мария', 'Смирнова', 1, 'maria.smirnova@rockstar.ru', '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', NULL, 3, 'Инструктор по йоге для начинающих и опытных практиков.', 1, '2026-03-07 06:39:03', '2026-03-07 06:39:03'),
(7, 'Константин', 'Овчинников', NULL, NULL, NULL, 0xffd8ffe000104a46494600010101006000600000fffe003b43524541544f523a2067642d6a7065672076312e3020287573696e6720494a47204a50454720763830292c207175616c697479203d2039350affdb0043000201010101010201010102020202020403020202020504040304060506060605060606070908060709070606080b08090a0a0a0a0a06080b0c0b0a0c090a0a0affdb004301020202020202050303050a0706070a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0affc00011080064006403012200021101031101ffc4001f0000010501010101010100000000000000000102030405060708090a0bffc400b5100002010303020403050504040000017d01020300041105122131410613516107227114328191a1082342b1c11552d1f02433627282090a161718191a25262728292a3435363738393a434445464748494a535455565758595a636465666768696a737475767778797a838485868788898a92939495969798999aa2a3a4a5a6a7a8a9aab2b3b4b5b6b7b8b9bac2c3c4c5c6c7c8c9cad2d3d4d5d6d7d8d9dae1e2e3e4e5e6e7e8e9eaf1f2f3f4f5f6f7f8f9faffc4001f0100030101010101010101010000000000000102030405060708090a0bffc400b51100020102040403040705040400010277000102031104052131061241510761711322328108144291a1b1c109233352f0156272d10a162434e125f11718191a262728292a35363738393a434445464748494a535455565758595a636465666768696a737475767778797a82838485868788898a92939495969798999aa2a3a4a5a6a7a8a9aab2b3b4b5b6b7b8b9bac2c3c4c5c6c7c8c9cad2d3d4d5d6d7d8d9dae2e3e4e5e6e7e8e9eaf2f3f4f5f6f7f8f9faffda000c03010002110311003f00fc94f8ada86ab1eb370b65e229a1b4745616c72236665049099e3a9ae423bdbd7b02c24cc9e76018586ec6e1f5a9bc657b777de26b8b463e694e141e401b14f5c1c75acf8ee223672308570b2e1977ede770ee7a5650d20918cf5917ad12fee62f327d52e95b8f9648c1c703ff00afda9d042ab265a5dcd9c10cc47afaf14cd14ea8f0b49a65890a3ae2ed4f3b47a0f4a916eaf1a509751ba92dcee8837ea69b6ee4b562be966596cdde569598498049dfc6d1deafaac9a75a472dd49969250628981e40ea4e7f0a6f82341d47c41aac5a3787b4992e269e50638954b333600c003f0e057d05e19ff82717ed47e308d2faebe0e6b129650210b1aae005dd803b703db9e3926b9f118cc361bf8b351f5674e1b058bc57f069b95bb2b9e3363e2459227b7f22d932a46727355b4e874ad27538af2cda39e74e6357666e7a670072715ddfc49fd9b7c7bf08e2b96f19e837da4bc28e1adb50b5313e403c0278f5ff0026bc9b46593cfb822473b6d4e3332c9d947414a955a55a3cd4ddd0ab52ab467c95159f99dadc78c75d652be4f3927e581bd7dea8dcf8b357dac1d1c70464db7bfbd5b5f0e441994c432b9ce235e7151c9e1f812628215c302bf747f77e94d4a09ec2719f73307882f6e2ee38ccad8670398d57bd74d6430e39ae6ed2ce3b6d4227842a94954825471cd749664060289c93d8229f534e30a10039fc28a2271b3a7eb4565735385d427b4b0d7fc8be91733aab097001f4c1f6e383583ad476de75c4568ea419033294db86cae4574be23fb24d237ca923145dc000485c9fcbd6b9cd4a28ef233a68b8026da3c996403e619076b67bf1c1ade9c9593329ad5a134696dac6d9e19a28f24820a3b7f740f5a2321a7574c63773996a9696e2287ecf2c477ab6d68dad94107038ad164b7d22c5ee25b6569e41f21f2c0d9d39ad1e8ccad73e83ff008272f8235af13fed030ea3a468ab790e90ca2594ce02a4cf1ccf129279e7c86e718e3dc67f6a7e0a78ceded7c3cb65e26f0e5ed9df3da192de269b1e728744f9414063cb30c647435f841ff04f2f8f57ff00077f680b39aeae14e99addf595b6a6177020f9a424801ea577b803d1cf7c57eea1bf8bc45add8ea3e17f0e5ddfdaae871c4971a7c96e537348b2127cc6073f20c918e0e33e9f9df17466b191e7d9ad1fa6e7eb1c05ec6783928bf793d57e5f81f337fc1593e13f8dfe2ffc05b9f11e9bf0d678351d1cdd5c34334aa668ed61b696699c92143a858f208ce7a0e78afc74d08ee86ee5652316ff00c5084ee9e9d6bf65bfe0b65fb50dff00c31f83cdf0f3c1e90c77de2ed0aeed2f5d24ff008f6b66589270a0704bac853d00727ae2bf187c284982f4a8c7fa38e8a57ba7ad7b7c29ed7fb2dca4bdd6f4f43e5f8c9d0fed8e583bc925cdebfd6e7a9c85773b02b8218f03dea85ddca47728dd40932703daac5ddc6c180e07eeff00bd591a8dce496dc3a9e7757baaf7b9f3254cedbc5465e44838fc6b7ed1cee07f4ae4ee358b59f5b8a1b1ba8a5df3aeef2a4c851d715d4d81dc460d5b4d131b335231b901e68a6062a300d159f31b721c5788a3bdd27578c69565131102b06138073939ebf4ac4d7e24d4606d5ad96401462e218db0d1907a8183c67afa7d2b57c5179343a9452bc5f2bc28e8c739c64e7f23c572706a53e9ba8b5d592e03c8c6552c08619eb8ae8a49b8a67354b291a1a7eaf15faacf756f2ab45f2ac8dc6f183807d7eb568a9bab4918a9124c06dda78419c819a66917367a848e2ded912355e54740d9ebfe7d6b5c5922c58c0e5793ef5d94e829a4d984aa34ec8e8ff646d1bc2965fb46f82878cee8269efafda1b8b9b83f2467ce011dbd1431049ec33e95fd017c2cf82da569564ede1cf13cba7c173062e2c05d6d58cfa2ff00b3c763f8d7f3a9225dc1289e0750ca30014c8c7a57eb4ffc1163f6ff00f887f19340d57e01fc54b68f55d5fc2da08bbd2350527cfbeb48c88d849b890f2a16886eeac1b9e412df19c6b96e227878e2a96aa0acd7937bff0099f5bc259ac3075a5879af8ddd3f3b6c70bff05e4f85fe16d1b40f02eb16dabb35ec77f7d65047bffe3e219638da693df6b4708ebfc55f987a7f86ee749175e49df1cb1854001241caf0727d8d7bc7eda7fb647c5ffdb3fe2e49e32f1f34365676024b6d0b47b542134e80b124649f9a46e37b9e4900701540f2eb189db0d2282dfc4477ff000afa2c932dab83caa9d1aefde5dba5ddec7879c63a18ecce75e9ab27fa2b162fefd0c61d64dc3cbc7cac3d4d727e2ad64dc5a1d1a09195a590b4b229e523039c7b9240fc6ba5f105a470698d7d00fba46ec0ec7ffaf8af3f863b9d7bc413c5026e1103e66074009ff015bca97b2a963914f9e243e19b67d23c6b669086115da9203738c64ff4fd6bd534d76dc39ae0238611f102cad210cdf63b76695b6f42c08c7fe3c2bbdd35b1260fa545577b32e9a35558e386a292331ede4d15c6759c3fc5012e9f169f771c1b84e19514b9e30c327d31cd79f5c1934c97edadb637707089f30191d79f7aec3e21bc926b42eaf6ea4964f2132ef8c2e091f2803815cc5dac93afd9a40dbe5742b81f7464f1fad76d0f85238aaeadb3a0f06d9ada68d1c9203b9cee7cf707d7f0ae82ce51247180a5bf76a4f1ed556d628911215e005c2907a715268b23b07460331caca703df23f4af595a2ac71dcb8b6af2ff00cb323ea457db7ff0413d1a19ff006cbd505ec65a1ff840af8cdb41c81f69b4009c723048e7b57c5b03fcfb47a64d7e85ff00c1be36f6b73fb50f892e1747884b67e01bbf32ec3b6f904b7b61b508276803cb7c6003f31af078a2a72e41887fdd3d4c9173e6b49799f19fed2df0f62f007ed1de3cf06da8fdc697e30d4ad6df8eb1a5cc8aa7dfe502b95b2b131a02c8df50b9af77ff008292e8c9a07edd7f13ac228b607f13c93edc018f351253ff00a1e6bc661652bb57b707eb5e9e5f55d6c052a8fed462fef48e3c543d9e2aa43b49afc4a325a2dfd8dcda302a2552b823a1c7f8d7924fa5eb9e14bd9afe22d22cd0b7da3ca3f32b03d3d719c73ee6bd674ebc5b989e449061e462a738e09e2b96f1a20492ee2c311e449c2a83d40e29e2765233a5abb1c9f80d2eee2f9b5e9aeb2b2b04da78dc7b9c7d78fcebd174e94198303dab82f0b437165a3dbc4cc0ab4a190ede76905b9f7e6bb1d3d8fca735c3575675d2d11d1c4e0c6095068aa90cacb18145721d4719f166e6d67d5e3b8b73fb9d84a3471e149673c0e3a0cfbd72f35b4a75bb7b6176ace9282c919ce0024e7d38e3bf7aed7e22c2ad0da43b4a347e60603865f987502b92b422df5bb156400ca832e4fccd956c7f2fd2bb30cf58a38ebe899d1fcd0a000f6e307a543a14ccdae5d5b997e57db22f1df0011fcaacdd42b25848a79fdd9fe559ba1b15bc8eec96c9c13b860904953c7e22bd36df32386da1d558bab5d4919fe1419fc73fe15fa4ff00f06efda69f1fc65f1ceb51dd3492a784e386e22316047bae90ae1b3f36421ec318afcd4d18f9b7774f9e8e17f219feb5fa73ff0006ebe9721f16fc4ed52480f9634dd2e10f8e0ee92e491ff8ed7cef174b97876bfa2ffd291ecf0f2be6d4fe7f933c0bfe0b05a34ba37fc1433c7734b1e16fdb4fba848eead61029fc772b57cc57178d13c902360b444a1f7c57d55ff059b92f5ffe0a0be2cb2bd5ff008f7d3f4c488ff790d9c4f9fcd88fc2be4fd5e3313457383d319ff3f5af4323e6964b876ff923ff00a4a39b33b2cc6b5bf9a5f9999e0e90cfa7adabb8cc1218c00083c7a9e9d2a1f1ee977972c92585dc70916f207c44181031818cd58f0dcaa97570d200b87e060f27f0aa7f10b59934fb347fb32b17475058718c0e95db51de81cb056a873b636daa5a6976665ba81a1648ced5b52180d831f36e23b8ed5d169528295c8e9fe214bbb6834f36a15922450c1cf651ce3fe035d4e8c72bd735e7c99d713a1846630549c514b6db442a379e9dc0a2b94e939ff88eb7f2cd0efb7562d6eeec61524025f2589eddab80b8b92be23d361f39088dd15cac808cee3dfa77e95dc7c4f65bc923b082e76a456cf2322ae578e477c1e8df9579df8a122b4bcb7bab4d830a194b751f3023f4c57561ddac7355576cf478d18ae14f7e06de0fe359b6b6f1c06368d0796c9b4e3a018ffeb0abda2cb71796314cfb06e8c3121bb63f9567e9b3182c441336e6894a31f71c1fe55ebd8f3ac6f7841bceb792e580cbcec71fa7f4afd55ff8370238fed7f17a5963665f2f4253b467a9d43fc2bf2abc2f09b7d2e356fbd8c91f524d7e97ff00c1bf3f12bc33e12f127c48d17c57e35d1f43866d3b4d9d25d4afd2d84be5bdcee258ba6eda25f53806be6b8be129f0ed68c55dfbbffa544f6b2092866b4dbf3fc99e5fff0005bcbdd3ee3fe0a0fe22b6b2243dbe89a547700a1077fd95186723fbac9fa57c937ea925ac7950407afa13fe0aa3e2ef0f78dff6f3f1df88bc2be31b1d7f4e967b15b6d574bbf4b9825db616e8c16446756da5761c31c1420e0822be7e9d37da8e980e0e6bd3c8e0e9e4d878bdd423ff00a4a3933292966155afe67f99ce691227fc243716d6ebf386fe11d3dbad50f8bd6d141a1249214121908502320e369cf39af71fd8d7f62cf89ffb5ff8f750b4f8657da6dac5636fe65f6a3aa4ecb023b160917eed59b79c138c70064e38cf9f7ede3f02bc6bfb347c45ff008575e3a4b969b4f52d33c966634b88e4f97ce89f715963c80015e410c1b04722c7e16a4a587535ceba75b1acb2fc64284715283f66f452b6973c43405dda8c58fee73cff00b35dc686df3609ae7f4bf0e0b5d32cb572a4192056e7fda5ff00ebd6f6862b19ec4c16a7451e4a0e4f4a2a343f2e0f6f6a2b9ae6f731be20c10d96a50585ac4a91084e140fef641fad79b788a43259da9700b6f65ddd0801881fa51457461f58c4c2a7c4cfab7fe0979fb37780bf6a1f14a7863e246a1aa4769676eceaba65ca4664dbd158ba31c76e307deacffc146fe03fc35f801fb42c1e10f85fa44b63a6dce8105d3db4974f2812f9b2c6581725be61182727a9278ce28a2b2f6d5bfb7234f99f2f2bd2fa74e87b1f57a1feabcaaf22e6e74af657b6bd773c8ac00f2f8f4ad9b2b99eca2792de42a6489a2720f546ea3f414515ebe23f8323e7287f150fd2c916cbeee49cfb926addca83665071b885247b9028a2b487f017a13bd4f99fa43ff0443b58749fd9f66d5205df71a8fc49b9b4b99a500b18974d8dd541f661fa9f5af993fe0e07118f8f5e17b96883493786ef1198bb0c049f2bc0383cb1eb9eb4515f99e01bff005a27f3fc8fd7f354bfd43a3e91fccf897c1560977e018758b899da55bb785570a06d0063a0c9ebdcd6d68c002714515f6f3ea7e5d0dd1b89d3f1a28a2b98dd6c7fffd9, 16, 'Универсальный тренер с большим стажем', 1, '2026-05-03 15:16:07', '2026-05-03 15:16:07');

-- --------------------------------------------------------

--
-- Структура таблицы `trainer_directions`
--

CREATE TABLE `trainer_directions` (
  `trainer_id` int(11) NOT NULL,
  `direction_id` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Дамп данных таблицы `trainer_directions`
--

INSERT INTO `trainer_directions` (`trainer_id`, `direction_id`) VALUES
(7, 1),
(7, 2),
(7, 3),
(7, 4);

-- --------------------------------------------------------

--
-- Структура таблицы `users`
--

CREATE TABLE `users` (
  `id` int(11) NOT NULL,
  `email` varchar(255) NOT NULL,
  `password_hash` varchar(255) NOT NULL,
  `first_name` varchar(100) NOT NULL,
  `last_name` varchar(100) NOT NULL,
  `phone` varchar(20) DEFAULT NULL,
  `age` int(11) DEFAULT NULL,
  `photo` longblob,
  `role` enum('admin','client') DEFAULT 'client',
  `is_active` tinyint(1) DEFAULT '1',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `birth_date` date DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Дамп данных таблицы `users`
--

INSERT INTO `users` (`id`, `email`, `password_hash`, `first_name`, `last_name`, `phone`, `age`, `photo`, `role`, `is_active`, `created_at`, `updated_at`, `birth_date`) VALUES
(1, 'adm@mail.ru', 'qweqwe', 'Admin', 'Adminov', '+79991234567', 30, NULL, 'admin', 1, '2026-03-10 08:16:10', '2026-03-18 14:02:11', NULL),
(29, 'sava@mail.ru', '$2a$11$qmPhV..LKOfgo61MLtaW2OcQ0nQtJKTdSBJ0vXEy4yLNHn/rqwF5G', 'Savelyi', 'Komornyi', '79223500530', 19, NULL, 'client', 1, '2026-05-11 10:10:28', '2026-05-11 21:55:57', '2006-07-21');

-- --------------------------------------------------------

--
-- Дублирующая структура для представления `view_schedule_details`
-- (См. Ниже фактическое представление)
--
CREATE TABLE `view_schedule_details` (
`id` int(11)
,`datetime` datetime
,`duration_minutes` int(11)
,`max_participants` int(11)
,`current_participants` int(11)
,`price` decimal(10,2)
,`is_group` tinyint(1)
,`is_active` tinyint(1)
,`direction_id` int(11)
,`direction_name` varchar(100)
,`direction_key` varchar(50)
,`trainer_id` int(11)
,`trainer_first_name` varchar(100)
,`trainer_last_name` varchar(100)
,`trainer_full_name` varchar(201)
,`service_id` int(11)
,`service_name` varchar(255)
);

-- --------------------------------------------------------

--
-- Дублирующая структура для представления `view_user_subscriptions`
-- (См. Ниже фактическое представление)
--
CREATE TABLE `view_user_subscriptions` (
`purchase_id` int(11)
,`user_id` int(11)
,`user_email` varchar(255)
,`user_name` varchar(201)
,`subscription_id` int(11)
,`subscription_name` varchar(255)
,`price` decimal(10,2)
,`total_sessions` int(11)
,`sessions_used` int(11)
,`sessions_remaining` bigint(12)
,`purchase_date` timestamp
,`expiry_date` datetime
,`status` enum('active','expired','used_up','refunded')
,`direction_id` int(11)
,`direction_name` varchar(100)
);

-- --------------------------------------------------------

--
-- Структура таблицы `__EFMigrationsHistory`
--

CREATE TABLE `__EFMigrationsHistory` (
  `MigrationId` varchar(150) NOT NULL,
  `ProductVersion` varchar(32) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- Структура для представления `view_schedule_details`
--
DROP TABLE IF EXISTS `view_schedule_details`;

CREATE ALGORITHM=UNDEFINED DEFINER=`root`@`%` SQL SECURITY DEFINER VIEW `view_schedule_details`  AS SELECT `s`.`id` AS `id`, `s`.`datetime` AS `datetime`, `s`.`duration_minutes` AS `duration_minutes`, `s`.`max_participants` AS `max_participants`, `s`.`current_participants` AS `current_participants`, `s`.`price` AS `price`, `s`.`is_group` AS `is_group`, `s`.`is_active` AS `is_active`, `d`.`id` AS `direction_id`, `d`.`name` AS `direction_name`, `d`.`name_key` AS `direction_key`, `t`.`id` AS `trainer_id`, `t`.`first_name` AS `trainer_first_name`, `t`.`last_name` AS `trainer_last_name`, concat(`t`.`first_name`,' ',`t`.`last_name`) AS `trainer_full_name`, `sv`.`id` AS `service_id`, `sv`.`name` AS `service_name` FROM (((`schedule` `s` left join `directions` `d` on((`s`.`direction_id` = `d`.`id`))) left join `trainers` `t` on((`s`.`trainer_id` = `t`.`id`))) left join `services` `sv` on((`s`.`service_id` = `sv`.`id`)))  ;

-- --------------------------------------------------------

--
-- Структура для представления `view_user_subscriptions`
--
DROP TABLE IF EXISTS `view_user_subscriptions`;

CREATE ALGORITHM=UNDEFINED DEFINER=`root`@`%` SQL SECURITY DEFINER VIEW `view_user_subscriptions`  AS SELECT `sp`.`id` AS `purchase_id`, `u`.`id` AS `user_id`, `u`.`email` AS `user_email`, concat(`u`.`first_name`,' ',`u`.`last_name`) AS `user_name`, `s`.`id` AS `subscription_id`, `s`.`name` AS `subscription_name`, `s`.`price` AS `price`, `s`.`sessions_count` AS `total_sessions`, `sp`.`sessions_used` AS `sessions_used`, (`s`.`sessions_count` - `sp`.`sessions_used`) AS `sessions_remaining`, `sp`.`purchase_date` AS `purchase_date`, `sp`.`expiry_date` AS `expiry_date`, `sp`.`status` AS `status`, `d`.`id` AS `direction_id`, `d`.`name` AS `direction_name` FROM (((`subscription_purchases` `sp` join `users` `u` on((`sp`.`user_id` = `u`.`id`))) join `subscriptions` `s` on((`sp`.`subscription_id` = `s`.`id`))) left join `directions` `d` on((`s`.`direction_id` = `d`.`id`)))  ;

--
-- Индексы сохранённых таблиц
--

--
-- Индексы таблицы `directions`
--
ALTER TABLE `directions`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `name_key` (`name_key`),
  ADD KEY `idx_name_key` (`name_key`),
  ADD KEY `idx_active` (`is_active`);

--
-- Индексы таблицы `enrollments`
--
ALTER TABLE `enrollments`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `unique_enrollment` (`user_id`,`schedule_id`),
  ADD KEY `idx_user` (`user_id`),
  ADD KEY `idx_schedule` (`schedule_id`),
  ADD KEY `idx_status` (`status`),
  ADD KEY `fk_enrollments_purchase` (`PurchaseId`);

--
-- Индексы таблицы `schedule`
--
ALTER TABLE `schedule`
  ADD PRIMARY KEY (`id`),
  ADD KEY `service_id` (`service_id`),
  ADD KEY `idx_datetime` (`datetime`),
  ADD KEY `idx_trainer` (`trainer_id`),
  ADD KEY `idx_direction` (`direction_id`),
  ADD KEY `idx_active` (`is_active`);

--
-- Индексы таблицы `services`
--
ALTER TABLE `services`
  ADD PRIMARY KEY (`id`),
  ADD KEY `idx_direction` (`direction_id`),
  ADD KEY `idx_active` (`is_active`);

--
-- Индексы таблицы `service_types`
--
ALTER TABLE `service_types`
  ADD PRIMARY KEY (`id`),
  ADD KEY `direction_id` (`direction_id`),
  ADD KEY `idx_active` (`is_active`);

--
-- Индексы таблицы `subscriptions`
--
ALTER TABLE `subscriptions`
  ADD PRIMARY KEY (`id`),
  ADD KEY `idx_direction` (`direction_id`),
  ADD KEY `idx_active` (`is_active`);

--
-- Индексы таблицы `subscription_purchases`
--
ALTER TABLE `subscription_purchases`
  ADD PRIMARY KEY (`id`),
  ADD KEY `subscription_id` (`subscription_id`),
  ADD KEY `idx_user` (`user_id`),
  ADD KEY `idx_status` (`status`);

--
-- Индексы таблицы `trainers`
--
ALTER TABLE `trainers`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `email` (`email`),
  ADD KEY `idx_direction` (`direction_id`),
  ADD KEY `idx_active` (`is_active`);

--
-- Индексы таблицы `trainer_directions`
--
ALTER TABLE `trainer_directions`
  ADD PRIMARY KEY (`trainer_id`,`direction_id`),
  ADD KEY `direction_id` (`direction_id`);

--
-- Индексы таблицы `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `email` (`email`),
  ADD KEY `idx_email` (`email`),
  ADD KEY `idx_role` (`role`),
  ADD KEY `idx_active` (`is_active`);

--
-- Индексы таблицы `__EFMigrationsHistory`
--
ALTER TABLE `__EFMigrationsHistory`
  ADD PRIMARY KEY (`MigrationId`);

--
-- AUTO_INCREMENT для сохранённых таблиц
--

--
-- AUTO_INCREMENT для таблицы `directions`
--
ALTER TABLE `directions`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT для таблицы `enrollments`
--
ALTER TABLE `enrollments`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

--
-- AUTO_INCREMENT для таблицы `schedule`
--
ALTER TABLE `schedule`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=34;

--
-- AUTO_INCREMENT для таблицы `services`
--
ALTER TABLE `services`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT для таблицы `service_types`
--
ALTER TABLE `service_types`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=10;

--
-- AUTO_INCREMENT для таблицы `subscriptions`
--
ALTER TABLE `subscriptions`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=11;

--
-- AUTO_INCREMENT для таблицы `subscription_purchases`
--
ALTER TABLE `subscription_purchases`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=12;

--
-- AUTO_INCREMENT для таблицы `trainers`
--
ALTER TABLE `trainers`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=8;

--
-- AUTO_INCREMENT для таблицы `users`
--
ALTER TABLE `users`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=30;

--
-- Ограничения внешнего ключа сохраненных таблиц
--

--
-- Ограничения внешнего ключа таблицы `enrollments`
--
ALTER TABLE `enrollments`
  ADD CONSTRAINT `enrollments_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `enrollments_ibfk_2` FOREIGN KEY (`schedule_id`) REFERENCES `schedule` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `fk_enrollments_purchase` FOREIGN KEY (`PurchaseId`) REFERENCES `subscription_purchases` (`id`) ON DELETE SET NULL;

--
-- Ограничения внешнего ключа таблицы `schedule`
--
ALTER TABLE `schedule`
  ADD CONSTRAINT `schedule_ibfk_1` FOREIGN KEY (`trainer_id`) REFERENCES `trainers` (`id`) ON DELETE SET NULL,
  ADD CONSTRAINT `schedule_ibfk_2` FOREIGN KEY (`direction_id`) REFERENCES `directions` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `schedule_ibfk_3` FOREIGN KEY (`service_id`) REFERENCES `services` (`id`) ON DELETE SET NULL;

--
-- Ограничения внешнего ключа таблицы `services`
--
ALTER TABLE `services`
  ADD CONSTRAINT `services_ibfk_1` FOREIGN KEY (`direction_id`) REFERENCES `directions` (`id`) ON DELETE CASCADE;

--
-- Ограничения внешнего ключа таблицы `service_types`
--
ALTER TABLE `service_types`
  ADD CONSTRAINT `service_types_ibfk_1` FOREIGN KEY (`direction_id`) REFERENCES `directions` (`id`) ON DELETE CASCADE;

--
-- Ограничения внешнего ключа таблицы `subscriptions`
--
ALTER TABLE `subscriptions`
  ADD CONSTRAINT `subscriptions_ibfk_1` FOREIGN KEY (`direction_id`) REFERENCES `directions` (`id`) ON DELETE SET NULL;

--
-- Ограничения внешнего ключа таблицы `subscription_purchases`
--
ALTER TABLE `subscription_purchases`
  ADD CONSTRAINT `subscription_purchases_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `subscription_purchases_ibfk_2` FOREIGN KEY (`subscription_id`) REFERENCES `subscriptions` (`id`) ON DELETE CASCADE;

--
-- Ограничения внешнего ключа таблицы `trainers`
--
ALTER TABLE `trainers`
  ADD CONSTRAINT `trainers_ibfk_1` FOREIGN KEY (`direction_id`) REFERENCES `directions` (`id`) ON DELETE SET NULL;

--
-- Ограничения внешнего ключа таблицы `trainer_directions`
--
ALTER TABLE `trainer_directions`
  ADD CONSTRAINT `trainer_directions_ibfk_1` FOREIGN KEY (`trainer_id`) REFERENCES `trainers` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `trainer_directions_ibfk_2` FOREIGN KEY (`direction_id`) REFERENCES `directions` (`id`) ON DELETE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
