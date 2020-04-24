DECLARE @sql NVARCHAR(2000)

--------------------------------------------------
-- Drop all views
--------------------------------------------------
WHILE EXISTS (
	SELECT *
	FROM sys.views V
)
BEGIN
	SELECT TOP 1 @sql = 'DROP VIEW [' + S.[name] + '].[' + V.[name] + ']'
	FROM sys.views V
		INNER JOIN sys.schemas S
		ON V.[schema_id] = S.[schema_id]

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

--------------------------------------------------
-- Turn off system versioning from all tables which are using it
--------------------------------------------------
WHILE EXISTS (
	SELECT *
	FROM sys.tables T
	WHERE
		T.temporal_type_desc = 'SYSTEM_VERSIONED_TEMPORAL_TABLE'
)
BEGIN
	SELECT TOP 1 @sql = 'ALTER TABLE [' + S.[name] + '].[' + T.[name] + '] SET (SYSTEM_VERSIONING = OFF)'
	FROM sys.tables T
		INNER JOIN sys.schemas S
		ON T.[schema_id] = S.[schema_id]
	WHERE
		T.temporal_type_desc = 'SYSTEM_VERSIONED_TEMPORAL_TABLE'

	EXECUTE sp_executesql @sql
END

--------------------------------------------------
-- Drop all foreign keys
--------------------------------------------------
WHILE EXISTS (
	SELECT *
	FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS TC
	WHERE
		TC.CONSTRAINT_CATALOG = DB_NAME()
		AND TC.CONSTRAINT_TYPE = 'FOREIGN KEY'
)
BEGIN
	SELECT TOP 1 @sql = 'ALTER TABLE [' + TC.TABLE_SCHEMA + '].[' + TC.TABLE_NAME + '] DROP CONSTRAINT [' + TC.CONSTRAINT_NAME + ']'
	FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS TC
	WHERE
		TC.CONSTRAINT_CATALOG = DB_NAME()
		AND TC.CONSTRAINT_TYPE = 'FOREIGN KEY'

	EXECUTE sp_executesql @sql
END

--------------------------------------------------
-- Drop all tables, except those related to built-in tools (like e.g. sysdiagrams)
--------------------------------------------------
WHILE EXISTS (
	SELECT *
	FROM sys.tables T
		LEFT JOIN sys.extended_properties EP
		ON EP.major_id = T.[object_id]
	WHERE
		EP.class_desc IS NULL
			OR (EP.class_desc <> 'OBJECT_OR_COLUMN' AND EP.[name] <> 'microsoft_database_tools_support')
)
BEGIN
	SELECT TOP 1 @sql = 'DROP TABLE [' + S.[name] + '].[' + T.[name] + ']'
	FROM sys.tables T
		INNER JOIN sys.schemas S
		ON T.[schema_id] = S.[schema_id]
		LEFT JOIN sys.extended_properties EP
		ON EP.major_id = T.[object_id]
	WHERE
		EP.class_desc IS NULL
			OR (EP.class_desc <> 'OBJECT_OR_COLUMN' AND EP.[name] <> 'microsoft_database_tools_support')

	EXECUTE sp_executesql @sql
END

--------------------------------------------------
-- Drop all sequences
--------------------------------------------------
WHILE EXISTS (
	SELECT *
	FROM sys.sequences Q
)
BEGIN
	SELECT TOP 1 @sql = 'DROP SEQUENCE [' + S.[name] + '].[' + Q.[name] + ']'
	FROM sys.sequences Q
		INNER JOIN sys.schemas S
		ON Q.[schema_id] = S.[schema_id]

	EXECUTE sp_executesql @sql
END

--------------------------------------------------
-- Drop all user-defined schemas (i.e. will not delete dbo, sys, INFORMATION_SCHEMA, db_* etc.)
--------------------------------------------------
WHILE EXISTS (
	SELECT *
	FROM sys.schemas S
	WHERE
		S.[schema_id] <> S.[principal_id]
)
BEGIN
	SELECT TOP 1 @sql = 'DROP SCHEMA [' + S.[name] + ']'
	FROM sys.schemas S
	WHERE
		S.[schema_id] <> S.[principal_id]

	EXECUTE sp_executesql @sql
END
