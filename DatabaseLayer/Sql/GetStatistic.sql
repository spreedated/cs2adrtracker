SELECT
	(SELECT count(*) FROM adrs WHERE outcome = 2) AS wins,
	(SELECT count(*) FROM adrs WHERE outcome = 3) AS draws,
	(SELECT count(*) FROM adrs WHERE outcome = 1) AS losses;