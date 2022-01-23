CREATE TABLE `__EFMigrationsHistory` (
   `MigrationId` varchar(150) NOT NULL,
   `ProductVersion` varchar(32) NOT NULL,
   PRIMARY KEY (`MigrationId`)
);

INSERT INTO `__EFMigrationsHistory` VALUES
(
    '20210707002802_InitialCreate',
    5.0.7
),
(
    '20210707043736_AddMultiRaidSubSupport',
    5.0.7
),
(
    '20210714054610_AddExRaidGymSubFilter',
    5.0.7
),
(
    '20210909003442_AddMultiInvasionSubSupport',
    5.0.8
),
(
    '20210909012222_AddMultiLureSubSupport',
    5.0.8
),
(
    '20220109043031_ModifyFormsToList',
    5.0.10
),
(
    '20220109043806_RenameCityToAreas',
    5.0.10
);

UPDATE raids ADD COLUMN `ex_eligible` tinyint(1) NOT NULL DEFAULT 0;
UPDATE gyms ADD COLUMN `ex_eligible` tinyint(1) NOT NULL DEFAULT 0;

UPDATE invasions MODIFY COLUMN `grunt_type` longtext DEFAULT NULL;
UPDATE invasions SET `grunt_type` = CONCAT('[', reward_pokemon_id, ']') WHERE grunt_type IS NOT NULL;
UPDATE invasions SET `reward_pokemon_id` = CONCAT('[', reward_pokemon_id, ']') WHERE reward_pokemon_id IS NOT NULL;

UPDATE lures MODIFY COLUMN `lure_type` longtext NOT NULL;
UPDATE lures SET `lure_type` = CONCAT('[', lure_type, ']') WHERE lure_type IS NOT NULL;

UPDATE pokemon SET `pokemon_id` = CONCAT('[', pokemon_id, ']');

UPDATE pvp SET `pokemon_id` = CONCAT('[', pokemon_id, ']');

UPDATE raids MODIFY COLUMN `pokemon_id` longtext NOT NULL;
UPDATE raids SET `pokemon_id` = CONCAT('[', pokemon_id, ']');

UPDATE pokemon SET form=NULL WHERE form = '' AND form IS NOT NULL OR form = ',';
UPDATE pvp SET form=NULL WHERE form = '' AND form IS NOT NULL OR form = ',';
UPDATE raids SET form=NULL WHERE form = '' AND form IS NOT NULL OR form = ',';


UPDATE pokemon MODIFY COLUMN `city` `areas` longtext DEFAULT NULL;
UPDATE pvp MODIFY COLUMN `city` `areas` longtext DEFAULT NULL;
UPDATE raids MODIFY COLUMN `city` `areas` longtext DEFAULT NULL;
UPDATE quests MODIFY COLUMN `city` `areas` longtext DEFAULT NULL;
UPDATE lures MODIFY COLUMN `city` `areas` longtext DEFAULT NULL;
UPDATE invasions MODIFY COLUMN `city` `areas` longtext DEFAULT NULL;