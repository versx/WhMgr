CREATE TABLE `subscriptions` (
  `id` int(11) PRIMARY KEY AUTO_INCREMENT,
  `subscription_id` int(11) DEFAULT 0,
  `guild_id` bigint(20) DEFAULT NULL,
  `userId` bigint(20) DEFAULT NULL,
  `enabled` int(11) DEFAULT 1,
  `distance` int(11) DEFAULT 0,
  `latitude` double DEFAULT NULL,
  `longitude` double DEFAULT NULL,
  `icon_style` text DEFAULT 'Default'
)

CREATE TABLE `pokemon` (
  `id` int(11) PRIMARY KEY AUTO_INCREMENT,
  `subscription_id` int(11) DEFAULT 0,
  `guild_id` bigint(20) DEFAULT NULL,
  `userId` bigint(20) DEFAULT NULL,
  `pokemon_id` int(11) DEFAULT NULL,
  `form` text DEFAULT NULL,
  `min_cp` int(11) DEFAULT NULL,
  `miv_iv` int(11) DEFAULT NULL,
  `min_lvl` int(11) DEFAULT NULL,
  `gender` text DEFAULT NULL,
  `attack` int(11) DEFAULT NULL,
  `defense` int(11) DEFAULT NULL,
  `stamina` int(11) DEFAULT NULL
)

CREATE TABLE `raids` (
  `id` int(11) PRIMARY KEY AUTO_INCREMENT,
  `guild_id` bigint(20) DEFAULT NULL,
  `userId` bigint(20) DEFAULT NULL,
  `pokemon_id` int(11) DEFAULT NULL,
  `form` text DEFAULT NULL,
  `city` text DEFAULT NULL
)

CREATE TABLE `quests` (
  `id` int(11) PRIMARY KEY AUTO_INCREMENT,
  `guild_id` bigint(20) DEFAULT NULL,
  `userId` bigint(20) DEFAULT NULL,
  `reward` text DEFAULT NULL,
  `city` text DEFAULT NULL
)

CREATE TABLE `invasions` (
  `id` int(11) PRIMARY KEY AUTO_INCREMENT,
  `guild_id` bigint(20) DEFAULT NULL,
  `userId` bigint(20) DEFAULT NULL,
  `grunt_type` text DEFAULT NULL,
  `city` text DEFAULT NULL
)

CREATE TABLE `gyms` (
  `id` int(11) PRIMARY KEY AUTO_INCREMENT,
  `guild_id` bigint(20) DEFAULT NULL,
  `userId` bigint(20) DEFAULT NULL,
  `name` text DEFAULT NULL
)