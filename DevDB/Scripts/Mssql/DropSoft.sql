DECLARE @sql NVARCHAR(2000)

--------------------------------------------------
-- Drop all views, except those related to built-in tools (like e.g. sys.database_firewall_rules, sys.event_log, etc.)
--------------------------------------------------
WHILE EXISTS (
	SELECT *
	FROM sys.views V
	WHERE
		V.is_ms_shipped = 0
)
BEGIN
	SELECT TOP 1 @sql = 'DROP VIEW [' + S.[name] + '].[' + V.[name] + ']'
	FROM sys.views V
		INNER JOIN sys.schemas S
		ON V.[schema_id] = S.[schema_id]
	WHERE
		V.is_ms_shipped = 0

	EXECUTE sp_executesql @sql
END

--------------------------------------------------
-- Drop all functions, except those related to built-in tools (like e.g. sysdiagrams)
--------------------------------------------------
WHILE EXISTS (
	SELECT *
	FROM sys.objects O
		LEFT JOIN sys.extended_properties EP
		ON EP.major_id = O.[object_id]
	WHERE
		O.[type] IN ('AF', 'FN', 'FS', 'FT', 'IF', 'TF')
		AND (EP.class_desc IS NULL
			OR (EP.class_desc <> 'OBJECT_OR_COLUMN' AND EP.[name] <> 'microsoft_database_tools_support'))
)
BEGIN
	SELECT TOP 1 @sql = 'DROP FUNCTION [' + S.[name] + '].[' + O.[name] + ']'
	FROM sys.objects O
		INNER JOIN sys.schemas S
		ON O.[schema_id] = S.[schema_id]
		LEFT JOIN sys.extended_properties EP
		ON EP.major_id = O.[object_id]
	WHERE
		O.[type] IN ('AF', 'FN', 'FS', 'FT', 'IF', 'TF')
		AND (EP.class_desc IS NULL
			OR (EP.class_desc <> 'OBJECT_OR_COLUMN' AND EP.[name] <> 'microsoft_database_tools_support'))

	EXECUTE sp_executesql @sql
END

--------------------------------------------------
-- Drop all procedures, except those related to built-in tools (like e.g. sysdiagrams)
--------------------------------------------------
WHILE EXISTS (
	SELECT *
	FROM sys.procedures P
		LEFT JOIN sys.extended_properties EP
		ON EP.major_id = P.[object_id]
	WHERE
		P.[type] IN ('P', 'PC')
		AND (EP.class_desc IS NULL
			OR (EP.class_desc <> 'OBJECT_OR_COLUMN' AND EP.[name] <> 'microsoft_database_tools_support'))
)
BEGIN
	SELECT TOP 1 @sql = 'DROP PROCEDURE [' + S.[name] + '].[' + P.[name] + ']'
	FROM sys.procedures P
		INNER JOIN sys.schemas S
		ON P.[schema_id] = S.[schema_id]
		LEFT JOIN sys.extended_properties EP
		ON EP.major_id = P.[object_id]
	WHERE
		P.[type] IN ('P', 'PC')
		AND (EP.class_desc IS NULL
			OR (EP.class_desc <> 'OBJECT_OR_COLUMN' AND EP.[name] <> 'microsoft_database_tools_support'))

	EXECUTE sp_executesql @sql
END

--------------------------------------------------
-- Drop all user-defined types
--------------------------------------------------
WHILE EXISTS (
	SELECT *
	FROM sys.types T
	WHERE
		T.is_user_defined = 1
)
BEGIN
	SELECT TOP 1 @sql = 'DROP TYPE [' + S.[name] + '].[' + T.[name] + ']'
	FROM sys.types T
		INNER JOIN sys.schemas S
		ON T.[schema_id] = S.[schema_id]
	WHERE
		T.is_user_defined = 1

	EXECUTE sp_executesql @sql
END
