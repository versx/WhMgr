CREATE TABLE `__EFMigrationsHistory` (
   `MigrationId` varchar(150) NOT NULL,
   `ProductVersion` varchar(32) NOT NULL,
   PRIMARY KEY (`MigrationId`)
);

INSERT INTO `__EFMigrationsHistory` VALUES
(
    '20210707002802_InitialCreate',
    '5.0.13'
),
(
    '20210707043736_AddMultiRaidSubSupport',
    '5.0.13'
),
(
    '20210714054610_AddExRaidGymSubFilter',
    '5.0.13'
),
(
    '20210909003442_AddMultiInvasionSubSupport',
    '5.0.13'
),
(
    '20210909012222_AddMultiLureSubSupport',
    '5.0.13'
),
(
    '20220109043031_ModifyFormsToList',
    '5.0.13'
),
(
    '20220109043806_RenameCityToAreas',
    '5.0.13'
);


UPDATE subscriptions MODIFY COLUMN `status` tinyint(3) unsigned NOT NULL;
UPDATE subscriptions MODIFY COLUMN `icon_style` longtext DEFAULT NULL;
UPDATE subscriptions MODIFY COLUMN `phone_number` longtext DEFAULT NULL;
UPDATE subscriptions MODIFY COLUMN `location` longtext DEFAULT NULL;


UPDATE gyms MODIFY COLUMN `name` longtext DEFAULT NULL;
UPDATE gyms MODIFY COLUMN `min_level` smallint(5) unsigned NOT NULL;
UPDATE gyms MODIFY COLUMN `max_level` smallint(5) unsigned NOT NULL;
UPDATE gyms MODIFY COLUMN `pokemon_ids` longtext DEFAULT NULL;
UPDATE gyms MODIFY COLUMN `location` longtext DEFAULT NULL;
UPDATE gyms ADD COLUMN `ex_eligible` tinyint(1) NOT NULL DEFAULT 0;


ALTER TABLE raids DROP INDEX `ix_pokemon_id`;
ALTER TABLE raids DROP INDEX `ix_form`;
UPDATE raids MODIFY COLUMN `pokemon_id` longtext DEFAULT NULL;
UPDATE raids MODIFY COLUMN `form` `forms` longtext DEFAULT NULL;
UPDATE raids MODIFY COLUMN `location` longtext DEFAULT NULL;
UPDATE raids MODIFY COLUMN `city` `areas` longtext DEFAULT NULL;
UPDATE raids ADD COLUMN `ex_eligible` tinyint(1) NOT NULL DEFAULT 0;
UPDATE raids SET `pokemon_id` = CONCAT('[', pokemon_id, ']');
UPDATE raids SET forms=NULL WHERE forms = '' AND forms IS NOT NULL OR forms = ',';


ALTER TABLE invasions DROP INDEX `ix_reward_pokemon_id`;
UPDATE invasions MODIFY COLUMN `reward_pokemon_id` longtext DEFAULT NULL;
UPDATE invasions MODIFY COLUMN `grunt_type` longtext DEFAULT NULL;
UPDATE invasions MODIFY COLUMN `pokestop_name` longtext DEFAULT NULL;
UPDATE invasions MODIFY COLUMN `location` longtext DEFAULT NULL;
UPDATE invasions MODIFY COLUMN `city` `areas` longtext DEFAULT NULL;
UPDATE invasions SET `grunt_type` = CONCAT('[', reward_pokemon_id, ']') WHERE grunt_type IS NOT NULL;
UPDATE invasions SET `reward_pokemon_id` = CONCAT('[', reward_pokemon_id, ']') WHERE reward_pokemon_id IS NOT NULL;


UPDATE lures MODIFY COLUMN `pokestop_name` longtext DEFAULT NULL;
UPDATE lures MODIFY COLUMN `lure_type` longtext DEFAULT NULL;
UPDATE lures MODIFY COLUMN `location` longtext DEFAULT NULL;
UPDATE lures MODIFY COLUMN `city` `areas` longtext DEFAULT NULL;
UPDATE lures SET `lure_type` = CONCAT('[', lure_type, ']') WHERE lure_type IS NOT NULL;


ALTER TABLE pokemon DROP INDEX `ix_form`;
UPDATE pokemon MODIFY COLUMN `pokemon_id` longtext NOT NULL;
UPDATE pokemon MODIFY COLUMN `form` `forms` longtext DEFAULT NULL;
UPDATE pokemon MODIFY COLUMN `min_cp` int(11) NOT NULL;
UPDATE pokemon MODIFY COLUMN `min_iv` int(11) NOT NULL;
UPDATE pokemon MODIFY COLUMN `iv_list` longtext DEFAULT NULL;
UPDATE pokemon MODIFY COLUMN `min_lvl` int(11) NOT NULL;
UPDATE pokemon MODIFY COLUMN `max_lvl` int(11) NOT NULL;
UPDATE pokemon MODIFY COLUMN `gender` longtext DEFAULT NULL;
UPDATE pokemon MODIFY COLUMN `size` tinyint(3) unsigned NOT NULL;
UPDATE pokemon MODIFY COLUMN `location` longtext DEFAULT NULL;
UPDATE pokemon MODIFY COLUMN `city` `areas` longtext DEFAULT NULL;
UPDATE pokemon SET `pokemon_id` = CONCAT('[', pokemon_id, ']');
UPDATE pokemon SET forms=NULL WHERE forms = '' AND forms IS NOT NULL OR forms = ',';


ALTER TABLE pvp DROP INDEX `ix_form`;
UPDATE pvp MODIFY COLUMN `pokemon_id` longtext NOT NULL;
UPDATE pvp MODIFY COLUMN `form` `forms` longtext DEFAULT NULL;
UPDATE pvp MODIFY COLUMN `league` longtext NOT NULL;
UPDATE pvp MODIFY COLUMN `min_rank` int(11) NOT NULL;
UPDATE pvp MODIFY COLUMN `min_percent` double NOT NULL;
UPDATE pvp MODIFY COLUMN `location` longtext DEFAULT NULL;
UPDATE pvp MODIFY COLUMN `city` `areas` longtext DEFAULT NULL;
UPDATE pvp SET `pokemon_id` = CONCAT('[', pokemon_id, ']');
UPDATE pvp SET forms=NULL WHERE forms = '' AND forms IS NOT NULL OR forms = ',';


ALTER TABLE quests DROP INDEX `ix_reward`;
UPDATE quests MODIFY COLUMN `pokestop_name` longtext DEFAULT NULL;
UPDATE quests MODIFY COLUMN `reward` longtext DEFAULT NULL;
UPDATE quests MODIFY COLUMN `location` longtext DEFAULT NULL;
UPDATE quests MODIFY COLUMN `city` `areas` longtext DEFAULT NULL;
