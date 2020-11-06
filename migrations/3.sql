ALTER TABLE subscriptions ADD KEY ix_id (id);
ALTER TABLE subscriptions ADD KEY ix_server (guild_id, user_id);
ALTER TABLE subscriptions ADD KEY ix_enabled (enabled);

ALTER TABLE pokemon ADD KEY ix_id (id);
ALTER TABLE pokemon ADD KEY ix_server (guild_id, user_id);
ALTER TABLE pokemon ADD KEY ix_pokemon_id (pokemon_id);
ALTER TABLE pokemon ADD KEY ix_form (form);
ALTER TABLE pokemon ADD KEY ix_city (city);

ALTER TABLE raids ADD KEY ix_id (id);
ALTER TABLE raids ADD KEY ix_server (guild_id, user_id);
ALTER TABLE raids ADD KEY ix_pokemon_id (pokemon_id);
ALTER TABLE raids ADD KEY ix_form (form);
ALTER TABLE raids ADD KEY ix_city (city);

ALTER TABLE quests ADD KEY ix_id (id);
ALTER TABLE quests ADD KEY ix_server (guild_id, user_id);
ALTER TABLE quests ADD KEY ix_reward (reward);
ALTER TABLE quests ADD KEY ix_city (city);

ALTER TABLE invasions ADD KEY ix_id (id);
ALTER TABLE invasions ADD KEY ix_server (guild_id, user_id);
ALTER TABLE invasions ADD KEY ix_reward_pokemon_id (reward_pokemon_id);
ALTER TABLE invasions ADD KEY ix_city (city);

ALTER TABLE gyms ADD KEY ix_id (id);
ALTER TABLE gyms ADD KEY ix_server (guild_id, user_id);
ALTER TABLE gyms ADD KEY ix_name (name);


ALTER TABLE subscriptions MODIFY COLUMN `user_id` bigint(20) unsigned NOT NULL;
ALTER TABLE subscriptions MODIFY COLUMN `guild_id` bigint(20) unsigned NOT NULL;

ALTER TABLE pokemon MODIFY COLUMN `user_id` bigint(20) unsigned NOT NULL;
ALTER TABLE pokemon MODIFY COLUMN `guild_id` bigint(20) unsigned NOT NULL;

ALTER TABLE pvp MODIFY COLUMN `user_id` bigint(20) unsigned NOT NULL;
ALTER TABLE pvp MODIFY COLUMN `guild_id` bigint(20) unsigned NOT NULL;

ALTER TABLE raids MODIFY COLUMN `user_id` bigint(20) unsigned NOT NULL;
ALTER TABLE raids MODIFY COLUMN `guild_id` bigint(20) unsigned NOT NULL;

ALTER TABLE quests MODIFY COLUMN `user_id` bigint(20) unsigned NOT NULL;
ALTER TABLE quests MODIFY COLUMN `guild_id` bigint(20) unsigned NOT NULL;

ALTER TABLE gyms MODIFY COLUMN `user_id` bigint(20) unsigned NOT NULL;
ALTER TABLE gyms MODIFY COLUMN `guild_id` bigint(20) unsigned NOT NULL;

ALTER TABLE invasions MODIFY COLUMN `user_id` bigint(20) unsigned NOT NULL;
ALTER TABLE invasions MODIFY COLUMN `guild_id` bigint(20) unsigned NOT NULL;


UPDATE subscriptions SET latitude=0 WHERE latitude IS NULL;
UPDATE subscriptions SET longitude=0 WHERE longitude IS NULL;

UPDATE pokemon SET form=NULL WHERE form='';
UPDATE pokemon SET city=NULL WHERE city='';