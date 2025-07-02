SELECT
	SUM(outcome = 2) AS wins,
	SUM(outcome = 3) AS draws,
	SUM(outcome = 1) AS losses
FROM
	adrs;