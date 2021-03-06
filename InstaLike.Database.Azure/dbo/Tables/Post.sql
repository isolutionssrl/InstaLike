﻿CREATE TABLE [dbo].[Post] (
    [ID]        INT                        IDENTITY (1, 1) NOT NULL,
    [UserID]    INT                        NOT NULL,
    [Picture]   VARBINARY (MAX)            NOT NULL,
    [Text]      VARCHAR (500)              CONSTRAINT [DF_Post_Text] DEFAULT ('') NOT NULL,
    [PostDate]  DATETIMEOFFSET (7)         CONSTRAINT [DF_Post_PostDate] DEFAULT (SYSDATETIMEOFFSET()) NOT NULL,
    CONSTRAINT [PK_Post] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_User_Post] FOREIGN KEY ([UserID]) REFERENCES [dbo].[User] ([ID]),
);

GO

