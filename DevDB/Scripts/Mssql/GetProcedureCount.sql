SELECT COUNT(*)
FROM sys.procedures P
	LEFT JOIN sys.extended_properties EP
	ON EP.major_id = P.[object_id]
WHERE
	EP.class_desc IS NULL
		OR (EP.class_desc <> 'OBJECT_OR_COLUMN' AND EP.[name] <> 'microsoft_database_tools_support')
