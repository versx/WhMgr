CREATE TABLE `subscriptions` (
   `id` int(11) NOT NULL AUTO_INCREMENT,
   `guild_id` bigint(20) DEFAULT NULL,
   `userId` bigint(20) DEFAULT NULL,
   `enabled` int(11) DEFAULT 1,
   `distance` int(11) DEFAULT 0,
   `latitude` double DEFAULT NULL,
   `longitude` double DEFAULT NULL,
   `icon_style` text DEFAULT NULL,
   PRIMARY KEY (`id`)
);

CREATE TABLE `pokemon` (
   `id` int(11) NOT NULL AUTO_INCREMENT,
   `subscription_id` int(11) NOT NULL DEFAULT 0,
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
   `stamina` int(11) DEFAULT NULL,
   PRIMARY KEY (`id`)
);

CREATE TABLE `raids` (
   `id` int(11) NOT NULL AUTO_INCREMENT,
   `subscription_id` int(11) NOT NULL DEFAULT 0,
   `guild_id` bigint(20) DEFAULT NULL,
   `userId` bigint(20) DEFAULT NULL,
   `pokemon_id` int(11) DEFAULT NULL,
   `form` text DEFAULT NULL,
   `city` text DEFAULT NULL,
   PRIMARY KEY (`id`)
);

CREATE TABLE `quests` (
   `id` int(11) NOT NULL AUTO_INCREMENT,
   `subscription_id` int(11) NOT NULL DEFAULT 0,
   `guild_id` bigint(20) DEFAULT NULL,
   `userId` bigint(20) DEFAULT NULL,
   `reward` text DEFAULT NULL,
   `city` text DEFAULT NULL,
   PRIMARY KEY (`id`)
);

CREATE TABLE `gyms` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `subscription_id` int(11) NOT NULL DEFAULT 0,
  `guild_id` bigint(20) DEFAULT NULL,
  `userId` bigint(20) DEFAULT NULL,
  `name` text DEFAULT NULL,
  PRIMARY KEY (`id`)
);

CREATE TABLE `invasions` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `subscription_id` int(11) NOT NULL DEFAULT 0,
  `guild_id` bigint(20) DEFAULT NULL,
  `userId` bigint(20) DEFAULT NULL,
  `grunt_type` text DEFAULT NULL,
  `reward_pokemon_id` int(11) DEFAULT NULL,
  `city` text DEFAULT NULL,
  PRIMARY KEY (`id`)
);

CREATE TABLE `pvp` (
   `id` int(11) NOT NULL AUTO_INCREMENT,
   `guild_id` bigint(20) NOT NULL,
   `userId` bigint(20) NOT NULL,
   `subscription_id` int(11) NOT NULL,
   `pokemon_id` int(11) NOT NULL,
   `form` varchar(255) DEFAULT NULL,
   `league` varchar(255) NOT NULL,
   `miv_rank` int(11) NOT NULL DEFAULT 25,
   `min_percent` double NOT NULL DEFAULT 90,
   PRIMARY KEY (`id`),
   KEY `FK_pvp_subscriptions_subscription_id` (`subscription_id`),
   CONSTRAINT `FK_pvp_subscriptions_subscription_id` FOREIGN KEY (`subscription_id`) REFERENCES `subscriptions` (`id`)
);
