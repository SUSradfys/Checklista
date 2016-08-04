CREATE TABLE [dbo].[ChecklistItem] (
    [ChecklistItemSer]				BIGINT			IDENTITY (1, 1) NOT NULL,
	[ChecklistSer]					BIGINT			NOT NULL,
	[ShortInfo]						VARCHAR (500)	NOT NULL,
	[DetailedInfo]					VARCHAR (2000)	NULL,
	[ShortResult]					VARCHAR (500)	NULL,
	[DetailedResult]				VARCHAR (8000)	NULL,
	[CheckStatus]					BIT				NOT NULL,
	[AutoCheckStatus]				CHAR (7)		NOT NULL,
	[BinaryData]					VARBINARY (MAX)	NULL,
    PRIMARY KEY CLUSTERED ([ChecklistItemSer] ASC),
	CONSTRAINT fk_Checklist_CheckListItem FOREIGN KEY ([ChecklistSer]) REFERENCES Checklist([ChecklistSer])
);