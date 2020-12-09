--First Create Database with name Architecture.WebMaster and then execute this script.
USE [Architecture.WebMaster]
GO
/****** Object:  Table [dbo].[table1]    Script Date: 09-12-2020 21:03:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[table1](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[col1] [varchar](200) NULL,
	[col2] [varchar](200) NULL,
	[col3] [varchar](200) NULL,
 CONSTRAINT [PK_table1] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[table2]    Script Date: 09-12-2020 21:03:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[table2](
	[column1] [varchar](200) NULL,
	[column2] [varchar](200) NULL,
	[column3] [varchar](200) NULL,
	[column4] [varchar](200) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserTables]    Script Date: 09-12-2020 21:03:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserTables](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmailId] [varchar](50) NOT NULL,
	[TableName] [varchar](50) NOT NULL,
	[Stamp] [datetime] NULL,
 CONSTRAINT [PK_UserTables] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[GetTablesByUserId]    Script Date: 09-12-2020 21:03:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[GetTablesByUserId]
(
	@EmailId VARCHAR(50)
)
AS
BEGIN
	SELECT * FROM [Architecture.WebMaster]..UserTables
	where EmailId = @EmailId
END
GO
USE [master]
GO
ALTER DATABASE [Architecture.WebMaster] SET  READ_WRITE 
GO
