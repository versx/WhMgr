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

DROP TABLE IF EXISTS `metadata`;
CREATE TABLE `metadata` (
    `key` varchar(255) NOT NULL,
    `value` longtext DEFAULT NULL,
    PRIMARY KEY (`key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
INSERT IGNORE INTO `metadata` (`key`, `value`) VALUES
('LAST_MODIFIED', '1643133618.555');


ALTER TABLE subscriptions MODIFY COLUMN `status` tinyint(3) unsigned NOT NULL;
ALTER TABLE subscriptions MODIFY COLUMN `icon_style` longtext DEFAULT NULL;
ALTER TABLE subscriptions MODIFY COLUMN `phone_number` longtext DEFAULT NULL;
ALTER TABLE subscriptions MODIFY COLUMN `location` longtext DEFAULT NULL;


ALTER TABLE gyms MODIFY COLUMN `name` longtext DEFAULT NULL;
ALTER TABLE gyms MODIFY COLUMN `min_level` smallint(5) unsigned NOT NULL;
ALTER TABLE gyms MODIFY COLUMN `max_level` smallint(5) unsigned NOT NULL;
ALTER TABLE gyms MODIFY COLUMN `pokemon_ids` longtext DEFAULT NULL;
ALTER TABLE gyms MODIFY COLUMN `location` longtext DEFAULT NULL;
ALTER TABLE gyms ADD COLUMN `ex_eligible` tinyint(1) NOT NULL DEFAULT 0;


ALTER TABLE raids DROP INDEX `ix_pokemon_id`;
ALTER TABLE raids DROP INDEX `ix_form`;
ALTER TABLE raids MODIFY COLUMN `pokemon_id` longtext DEFAULT NULL;
ALTER TABLE raids CHANGE COLUMN `form` `forms` longtext DEFAULT NULL;
ALTER TABLE raids MODIFY COLUMN `location` longtext DEFAULT NULL;
ALTER TABLE raids CHANGE COLUMN `city` `areas` longtext DEFAULT NULL;
ALTER TABLE raids ADD COLUMN `ex_eligible` tinyint(1) NOT NULL DEFAULT 0;
UPDATE raids SET `pokemon_id` = CONCAT('[', pokemon_id, ']');
UPDATE raids SET forms=NULL WHERE forms = '' AND forms IS NOT NULL OR forms = ',';


ALTER TABLE invasions DROP INDEX `ix_reward_pokemon_id`;
ALTER TABLE invasions MODIFY COLUMN `reward_pokemon_id` longtext DEFAULT NULL;
ALTER TABLE invasions MODIFY COLUMN `grunt_type` longtext DEFAULT NULL;
ALTER TABLE invasions MODIFY COLUMN `pokestop_name` longtext DEFAULT NULL;
ALTER TABLE invasions MODIFY COLUMN `location` longtext DEFAULT NULL;
ALTER TABLE invasions CHANGE COLUMN `city` `areas` longtext DEFAULT NULL;
UPDATE invasions SET `grunt_type` = CONCAT('[', reward_pokemon_id, ']') WHERE grunt_type IS NOT NULL;
UPDATE invasions SET `reward_pokemon_id` = CONCAT('[', reward_pokemon_id, ']') WHERE reward_pokemon_id IS NOT NULL;


ALTER TABLE lures MODIFY COLUMN `pokestop_name` longtext DEFAULT NULL;
ALTER TABLE lures MODIFY COLUMN `lure_type` longtext DEFAULT NULL;
ALTER TABLE lures MODIFY COLUMN `location` longtext DEFAULT NULL;
ALTER TABLE lures CHANGE COLUMN `city` `areas` longtext DEFAULT NULL;
UPDATE lures SET `lure_type` = CONCAT('[', lure_type, ']') WHERE lure_type IS NOT NULL;


ALTER TABLE pokemon DROP INDEX `ix_form`;
ALTER TABLE pokemon MODIFY COLUMN `pokemon_id` longtext NOT NULL;
ALTER TABLE pokemon CHANGE COLUMN `form` `forms` longtext DEFAULT NULL;
ALTER TABLE pokemon MODIFY COLUMN `min_cp` int(11) NOT NULL;
ALTER TABLE pokemon MODIFY COLUMN `min_iv` int(11) NOT NULL;
ALTER TABLE pokemon MODIFY COLUMN `iv_list` longtext DEFAULT NULL;
ALTER TABLE pokemon MODIFY COLUMN `min_lvl` int(11) NOT NULL;
ALTER TABLE pokemon MODIFY COLUMN `max_lvl` int(11) NOT NULL;
ALTER TABLE pokemon MODIFY COLUMN `gender` longtext DEFAULT NULL;
ALTER TABLE pokemon MODIFY COLUMN `size` tinyint(3) unsigned NOT NULL;
ALTER TABLE pokemon MODIFY COLUMN `location` longtext DEFAULT NULL;
ALTER TABLE pokemon CHANGE COLUMN `city` `areas` longtext DEFAULT NULL;
UPDATE pokemon SET `pokemon_id` = CONCAT('[', pokemon_id, ']');
UPDATE pokemon SET forms=NULL WHERE forms = '' AND forms IS NOT NULL OR forms = ',';


ALTER TABLE pvp DROP INDEX `ix_form`;
ALTER TABLE pvp MODIFY COLUMN `pokemon_id` longtext NOT NULL;
ALTER TABLE pvp CHANGE COLUMN `form` `forms` longtext DEFAULT NULL;
ALTER TABLE pvp MODIFY COLUMN `league` longtext NOT NULL;
ALTER TABLE pvp MODIFY COLUMN `min_rank` int(11) NOT NULL;
ALTER TABLE pvp MODIFY COLUMN `min_percent` double NOT NULL;
ALTER TABLE pvp MODIFY COLUMN `location` longtext DEFAULT NULL;
ALTER TABLE pvp CHANGE COLUMN `city` `areas` longtext DEFAULT NULL;
UPDATE pvp SET `pokemon_id` = CONCAT('[', pokemon_id, ']');
UPDATE pvp SET forms=NULL WHERE forms = '' AND forms IS NOT NULL OR forms = ',';


ALTER TABLE quests DROP INDEX `ix_reward`;
ALTER TABLE quests MODIFY COLUMN `pokestop_name` longtext DEFAULT NULL;
ALTER TABLE quests MODIFY COLUMN `reward` longtext DEFAULT NULL;
ALTER TABLE quests MODIFY COLUMN `location` longtext DEFAULT NULL;
ALTER TABLE quests CHANGE COLUMN `city` `areas` longtext DEFAULT NULL;
