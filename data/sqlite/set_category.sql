-- Set `category` column all value of table `questions` to new value if it is NULL

UPDATE `questions`
SET `category` = '衣食住'
WHERE `category` IS NULL
