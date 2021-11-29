DROP TABLE IF EXISTS [dbo].[HTMLCode];
DROP TABLE IF EXISTS [dbo].[Users];

CREATE TABLE [dbo].[Users] (
    [Id]       INT           IDENTITY (1, 1) NOT NULL,
    [Username] VARCHAR (255) NOT NULL,
    [Password] VARCHAR (255) NOT NULL,
    [IsAdmin]  BIT           NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[HTMLCode] (
    [Id]      INT            IDENTITY (1, 1) NOT NULL,
    [HTML]    NVARCHAR (MAX) NOT NULL,
    [Created] DATETIME       NOT NULL,
    [Edited]  DATETIME       NULL,
    [UserId]  INT            NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_HTMLCode_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id])
);