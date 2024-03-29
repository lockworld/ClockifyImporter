BEGIN TRANSACTION;
DROP TABLE IF EXISTS "ClockifyImport";
CREATE TABLE IF NOT EXISTS "ClockifyImport" (
	"Id"	TEXT NOT NULL,
	"FilePath"	TEXT,
	"LineNum"	INTEGER,
	"DateRecorded"	INTEGER,
	"Project"	TEXT,
	"Category"	TEXT,
	"Description"	TEXT,
	"Task"	TEXT,
	"User"	TEXT,
	"Group"	TEXT,
	"Email"	TEXT,
	"Tags"	TEXT,
	"Billable"	TEXT,
	"StartDate"	TEXT,
	"StartTime"	TEXT,
	"EndDate"	TEXT,
	"EndTime"	TEXT,
	"DurationHours"	TEXT,
	"DurationDecimal"	NUMERIC,
	"BillableRate"	NUMERIC,
	"BillableAmount"	NUMERIC,
	UNIQUE("FilePath","LineNum"),
	PRIMARY KEY("Id")
);
DROP VIEW IF EXISTS "ClockifyImportConversion";
CREATE VIEW ClockifyImportConversion AS
SELECT 
Id, FilePath, LineNum, datetime(DateRecorded, 'unixepoch', 'localtime') AS DateRecorded,
Project, Category, Description, Task, User, `Group`,	Email, Tags, Billable, 
datetime(substr(StartDate, 7, 4) || '-' || substr(StartDate, 1, 2) || '-' || substr(StartDate, 4, 2)) AS StartDate,
datetime(substr(StartDate, 7, 4) || '-' || substr(StartDate, 1, 2) || '-' || substr(StartDate, 4, 2) || ' ' || CASE WHEN substr(StartTime,10,2)='PM' THEN CAST(CAST(substr(StartTime,1,2) AS INTEGER)+12 AS VarChar) ELSE substr(StartTime,1,2) END || ':' || substr(StartTime,4,2) || ':' || substr(StartTime,7,2)) AS StartDateTime,
datetime(substr(EndDate, 7, 4) || '-' || substr(EndDate, 1, 2) || '-' || substr(EndDate, 4, 2)) AS EndDate,
datetime(substr(EndDate, 7, 4) || '-' || substr(EndDate, 1, 2) || '-' || substr(EndDate, 4, 2) || ' ' || CASE WHEN substr(EndTime,10,2)='PM' THEN CAST(CAST(substr(EndTime,1,2) AS INTEGER)+12 AS VarChar) ELSE substr(EndTime,1,2) END || ':' || substr(EndTime,4,2) || ':' || substr(EndTime,7,2)) AS EndDateTime,
DurationHours, DurationDecimal, BillableRate, BillableAmount
FROM ClockifyImport;
DROP VIEW IF EXISTS "HoursByDateProjectTask";
CREATE VIEW HoursByDateProjectTask AS
SELECT StartDate, Project, Task, SUM(DurationDecimal) FROM ClockifyImportConversion
GROUP BY StartDate, Project, Task;
DROP VIEW IF EXISTS "TotalHoursByProjectTask";
CREATE VIEW TotalHoursByProjectTask AS
SELECT Project, Task, SUM(DurationDecimal) FROM ClockifyImportConversion
GROUP BY Project, Task;
DROP VIEW IF EXISTS "PossibleDublicates";
CREATE VIEW PossibleDublicates AS
SELECT StartDateTime, COUNT(*) FROM ClockifyImportConversion
GROUP BY StartDateTime
HAVING COUNT(*)>1;
DROP VIEW IF EXISTS "TotalHoursByProject";
CREATE VIEW TotalHoursByProject AS
SELECT Project, SUM(DurationDecimal) FROM ClockifyImportConversion
GROUP BY Project;
COMMIT;
