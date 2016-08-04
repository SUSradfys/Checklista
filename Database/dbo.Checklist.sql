CREATE TABLE [dbo].[Checklist] (
    [ChecklistSer]                BIGINT       IDENTITY (1, 1) NOT NULL,
    [Status]                      NCHAR (15)   NOT NULL,
    [DateTime]                    DATETIME     NOT NULL,
	[UserId]                      VARCHAR (50) NOT NULL,
    [PatientId]                   VARCHAR (50) NOT NULL,
    [FirstName]                   VARCHAR (50) NOT NULL,
    [LastName]                    VARCHAR (50) NOT NULL,
    [PatientSer]                  BIGINT       NOT NULL,
    [CourseId]                    VARCHAR (50) NOT NULL,
    [CourseSer]                   BIGINT       NOT NULL,
    [PlanSetupId]                 VARCHAR (50) NOT NULL,
    [PlanSetupSer]                BIGINT       NOT NULL,   
    PRIMARY KEY CLUSTERED ([ChecklistSer] ASC)
);