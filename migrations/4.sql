CREATE TABLE IF NOT EXISTS `lures` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `subscription_id` int(11) NOT NULL DEFAULT 0,
  `guild_id` bigint(20) DEFAULT NULL,
  `user_id` bigint(20) DEFAULT NULL,
  `lure_type` varchar(20) NOT NULL,
  `city` text DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `FK_lure_subscriptions_subscription_id` (`subscription_id`),
  CONSTRAINT `FK_lure_subscriptions_subscription_id` FOREIGN KEY (`subscription_id`) REFERENCES `subscriptions` (`id`)
);


ALTER TABLE subscriptions ADD KEY ix_server (guild_id, user_id);
ALTER TABLE subscriptions ADD KEY ix_enabled (enabled);

ALTER TABLE pokemon MODIFY COLUMN pokemon_id smallint(5) unsigned NOT NULL;
ALTER TABLE pokemon MODIFY COLUMN iv_list text DEFAULT NULL;
ALTER TABLE pokemon MODIFY COLUMN city text DEFAULT NULL;
ALTER TABLE pokemon ADD KEY ix_server (guild_id, user_id);
ALTER TABLE pokemon ADD KEY ix_pokemon_id (pokemon_id);

ALTER TABLE pvp MODIFY COLUMN pokemon_id smallint(5) unsigned NOT NULL;
ALTER TABLE pvp MODIFY COLUMN city text DEFAULT NULL;
ALTER TABLE pvp ADD KEY ix_server (guild_id, user_id);
ALTER TABLE pvp ADD KEY ix_pokemon_id (pokemon_id);

ALTER TABLE raids MODIFY COLUMN pokemon_id smallint(5) unsigned NOT NULL;
ALTER TABLE raids MODIFY COLUMN city text DEFAULT NULL;
ALTER TABLE raids ADD KEY ix_server (guild_id, user_id);
ALTER TABLE raids ADD KEY ix_pokemon_id (pokemon_id);

ALTER TABLE quests MODIFY COLUMN city text DEFAULT NULL;
ALTER TABLE quests ADD KEY ix_server (guild_id, user_id);

ALTER TABLE invasions MODIFY COLUMN city text DEFAULT NULL;
ALTER TABLE invasions MODIFY COLUMN reward_pokemon_id smallint(5) unsigned NOT NULL;
ALTER TABLE invasions ADD KEY ix_server (guild_id, user_id);
ALTER TABLE invasions ADD KEY ix_reward_pokemon_id (reward_pokemon_id);

ALTER TABLE gyms ADD KEY ix_server (guild_id, user_id);
ALTER TABLE gyms ADD KEY ix_name (name);


UPDATE subscriptions SET latitude=0 WHERE latitude IS NULL;
UPDATE subscriptions SET longitude=0 WHERE longitude IS NULL;
UPDATE subscriptions SET phone_number=NULL WHERE phone_number='';
