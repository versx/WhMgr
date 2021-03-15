ALTER TABLE `gyms` 
ADD COLUMN `min_level` tinyint(1) unsigned DEFAULT 0;

ALTER TABLE `gyms` 
ADD COLUMN `max_level` tinyint(1) unsigned DEFAULT 0;

ALTER TABLE `gyms` 
ADD COLUMN `pokemon_ids` text DEFAULT NULL;
