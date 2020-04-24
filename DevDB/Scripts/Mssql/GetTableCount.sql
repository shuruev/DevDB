SELECT COUNT(*)
FROM sys.tables T
	LEFT JOIN sys.extended_properties EP
	ON EP.major_id = T.[object_id]
WHERE
	EP.class_desc IS NULL
		OR (EP.class_desc <> 'OBJECT_OR_COLUMN' AND EP.[name] <> 'microsoft_database_tools_support')
