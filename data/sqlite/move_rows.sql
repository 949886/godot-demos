-- move rows 108-145 and insert them before row 197
CREATE TEMP TABLE IF NOT EXISTS temp_rows AS SELECT * FROM questions WHERE id BETWEEN 108 AND 146;
DELETE FROM questions WHERE id BETWEEN 108 AND 146;
UPDATE questions SET id = id - 39 WHERE id BETWEEN 147 AND 197;
UPDATE temp_rows SET id = rowid + 197 - 39 WHERE TRUE;
INSERT INTO questions SELECT * FROM temp_rows;
DROP TABLE temp_rows;
