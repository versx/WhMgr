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