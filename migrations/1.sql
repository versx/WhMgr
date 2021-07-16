CREATE TABLE `subscriptions` (
   `id` int(11) NOT NULL AUTO_INCREMENT,
   `guild_id` bigint(20) DEFAULT NULL,
   `user_id` bigint(20) DEFAULT NULL,
   `enabled` int(11) DEFAULT 1,
   `distance` int(11) DEFAULT 0,
   `latitude` double DEFAULT NULL,
   `longitude` double DEFAULT NULL,
   `icon_style` text DEFAULT NULL,
   PRIMARY KEY (`id`)
);

CREATE TABLE IF NOT EXISTS `pokemon` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `subscription_id` int(11) NOT NULL DEFAULT 0,
  `guild_id` bigint(20) DEFAULT NULL,
  `user_id` bigint(20) DEFAULT NULL,
  `pokemon_id` int(11) DEFAULT NULL,
  `form` text DEFAULT NULL,
  `min_cp` int(11) DEFAULT 0,
  `min_iv` int(11) DEFAULT NULL,
  `min_lvl` int(11) DEFAULT 0,
  `max_lvl` int(11) DEFAULT 35,
  `gender` varchar(1) DEFAULT '*',
  `iv_list` longtext DEFAULT NULL,
  `city` varchar(64) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `FK_pokemon_subscriptions_subscription_id` (`subscription_id`),
  CONSTRAINT `FK_pokemon_subscriptions_subscription_id` FOREIGN KEY (`subscription_id`) REFERENCES `subscriptions` (`id`)
);

CREATE TABLE IF NOT EXISTS `raids` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `subscription_id` int(11) NOT NULL DEFAULT 0,
  `guild_id` bigint(20) DEFAULT NULL,
  `user_id` bigint(20) DEFAULT NULL,
  `pokemon_id` int(11) DEFAULT NULL,
  `form` text DEFAULT NULL,
  `city` varchar(64) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `FK_raid_subscriptions_subscription_id` (`subscription_id`),
  CONSTRAINT `FK_raid_subscriptions_subscription_id` FOREIGN KEY (`subscription_id`) REFERENCES `subscriptions` (`id`)
);

CREATE TABLE IF NOT EXISTS `quests` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `subscription_id` int(11) NOT NULL DEFAULT 0,
  `guild_id` bigint(20) DEFAULT NULL,
  `user_id` bigint(20) DEFAULT NULL,
  `reward` text DEFAULT NULL,
  `city` varchar(64) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `FK_quest_subscriptions_subscription_id` (`subscription_id`),
  CONSTRAINT `FK_quest_subscriptions_subscription_id` FOREIGN KEY (`subscription_id`) REFERENCES `subscriptions` (`id`)
);

CREATE TABLE IF NOT EXISTS `gyms` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `subscription_id` int(11) NOT NULL DEFAULT 0,
  `guild_id` bigint(20) DEFAULT NULL,
  `user_id` bigint(20) DEFAULT NULL,
  `name` varchar(128) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `FK_gym_subscriptions_subscription_id` (`subscription_id`),
  CONSTRAINT `FK_gym_subscriptions_subscription_id` FOREIGN KEY (`subscription_id`) REFERENCES `subscriptions` (`id`)
);

CREATE TABLE IF NOT EXISTS `invasions` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `subscription_id` int(11) NOT NULL DEFAULT 0,
  `guild_id` bigint(20) DEFAULT NULL,
  `user_id` bigint(20) DEFAULT NULL,
  `reward_pokemon_id` int(11) DEFAULT NULL,
  `city` varchar(64) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `FK_invasion_subscriptions_subscription_id` (`subscription_id`),
  CONSTRAINT `FK_invasion_subscriptions_subscription_id` FOREIGN KEY (`subscription_id`) REFERENCES `subscriptions` (`id`)
);

CREATE TABLE IF NOT EXISTS `pvp` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `guild_id` bigint(20) NOT NULL,
  `user_id` bigint(20) NOT NULL,
  `subscription_id` int(11) NOT NULL,
  `pokemon_id` int(11) NOT NULL,
  `form` varchar(255) DEFAULT NULL,
  `league` varchar(255) NOT NULL,
  `min_rank` int(11) NOT NULL DEFAULT 25,
  `min_percent` double NOT NULL DEFAULT 90,
  `city` varchar(64) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `FK_pvp_subscriptions_subscription_id` (`subscription_id`),
  CONSTRAINT `FK_pvp_subscriptions_subscription_id` FOREIGN KEY (`subscription_id`) REFERENCES `subscriptions` (`id`)
);