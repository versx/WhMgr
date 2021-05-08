ALTER TABLE `invasions`
ADD COLUMN `pokestop_name` VARCHAR(255) DEFAULT NULL;

ALTER TABLE `invasions`
ADD COLUMN `grunt_type` TINYINT(2) DEFAULT NULL;
