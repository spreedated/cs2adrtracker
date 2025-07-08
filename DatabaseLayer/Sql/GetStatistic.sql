SELECT
	SUM(outcome = 2) AS victories,
	SUM(outcome = 3) AS ties,
	SUM(outcome = 1) AS defeats
FROM
	adrs;