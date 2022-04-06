USE [master]
GO
/****** Object:  Database [TimaAffiliate]    Script Date: 4/27/2018 9:42:42 AM ******/
CREATE DATABASE [TimaAffiliate]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'Affiliate', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL13.MSSQLSERVER\MSSQL\DATA\Affiliate.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'Affiliate_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL13.MSSQLSERVER\MSSQL\DATA\Affiliate_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
GO
ALTER DATABASE [TimaAffiliate] SET COMPATIBILITY_LEVEL = 130
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [TimaAffiliate].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [TimaAffiliate] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [TimaAffiliate] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [TimaAffiliate] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [TimaAffiliate] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [TimaAffiliate] SET ARITHABORT OFF 
GO
ALTER DATABASE [TimaAffiliate] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [TimaAffiliate] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [TimaAffiliate] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [TimaAffiliate] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [TimaAffiliate] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [TimaAffiliate] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [TimaAffiliate] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [TimaAffiliate] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [TimaAffiliate] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [TimaAffiliate] SET  DISABLE_BROKER 
GO
ALTER DATABASE [TimaAffiliate] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [TimaAffiliate] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [TimaAffiliate] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [TimaAffiliate] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [TimaAffiliate] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [TimaAffiliate] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [TimaAffiliate] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [TimaAffiliate] SET RECOVERY FULL 
GO
ALTER DATABASE [TimaAffiliate] SET  MULTI_USER 
GO
ALTER DATABASE [TimaAffiliate] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [TimaAffiliate] SET DB_CHAINING OFF 
GO
ALTER DATABASE [TimaAffiliate] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [TimaAffiliate] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [TimaAffiliate] SET DELAYED_DURABILITY = DISABLED 
GO
EXEC sys.sp_db_vardecimal_storage_format N'TimaAffiliate', N'ON'
GO
ALTER DATABASE [TimaAffiliate] SET QUERY_STORE = OFF
GO
USE [TimaAffiliate]
GO
ALTER DATABASE SCOPED CONFIGURATION SET LEGACY_CARDINALITY_ESTIMATION = OFF;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET LEGACY_CARDINALITY_ESTIMATION = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET MAXDOP = 0;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET MAXDOP = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET PARAMETER_SNIFFING = ON;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET PARAMETER_SNIFFING = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET QUERY_OPTIMIZER_HOTFIXES = OFF;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET QUERY_OPTIMIZER_HOTFIXES = PRIMARY;
GO
USE [TimaAffiliate]
GO
/****** Object:  Table [dbo].[tblBankAccount]    Script Date: 4/27/2018 9:42:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBankAccount](
	[BankId] [int] IDENTITY(1,1) NOT NULL,
	[BankName] [nvarchar](250) NULL,
	[BankAddress] [nvarchar](max) NULL,
	[BankOwnerName] [nvarchar](50) NULL,
	[BankAccount] [nvarchar](50) NULL,
	[UserId] [int] NULL,
	[Note] [nvarchar](max) NULL,
	[UpdatedDate] [datetime] NULL,
	[CreatedDate] [datetime] NULL,
 CONSTRAINT [PK_tblBankAccount] PRIMARY KEY CLUSTERED 
(
	[BankId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblConfig]    Script Date: 4/27/2018 9:42:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblConfig](
	[ConfigId] [int] IDENTITY(1,1) NOT NULL,
	[PercentAmount] [float] NOT NULL,
	[PercentRefer] [float] NOT NULL,
	[ApplyDate] [datetime] NULL,
 CONSTRAINT [PK_tblConfig] PRIMARY KEY CLUSTERED 
(
	[ConfigId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblLoan]    Script Date: 4/27/2018 9:42:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblLoan](
	[LoanId] [bigint] NOT NULL,
	[FullName] [nvarchar](250) NULL,
	[Address] [nvarchar](max) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[AffCode] [nvarchar](50) NOT NULL,
	[Amount] [bigint] NULL,
	[PhoneNumber] [nvarchar](50) NULL,
	[UserId] [int] NULL,
	[Status] [int] NULL,
	[CityId] [int] NULL,
	[DistrictId] [int] NULL,
	[ProductCreditId] [int] NULL,
 CONSTRAINT [PK_tblLoan] PRIMARY KEY CLUSTERED 
(
	[LoanId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblPayment]    Script Date: 4/27/2018 9:42:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblPayment](
	[PaymentId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[AmountEarning] [bigint] NOT NULL,
	[PaymentStatus] [int] NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[VerifyStatus] [int] NULL,
	[Comment1] [nvarchar](500) NULL,
	[Comment2] [nvarchar](max) NULL,
	[Comment3] [nvarchar](max) NULL,
	[PaymentMethod] [int] NULL,
	[PaidDate] [datetime] NULL,
	[AccountInfo] [nvarchar](500) NULL,
 CONSTRAINT [PK_tblPayment] PRIMARY KEY CLUSTERED 
(
	[PaymentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblReport]    Script Date: 4/27/2018 9:42:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblReport](
	[ReportId] [bigint] IDENTITY(1,1) NOT NULL,
	[TransactionId] [bigint] NOT NULL,
	[UserId] [int] NOT NULL,
	[CreatedDate] [datetime] NULL,
	[UserReferId] [int] NULL,
	[TransactionAmount] [bigint] NULL,
	[PercentAmount] [float] NULL,
	[Level] [int] NULL,
	[Amount] [bigint] NULL,
	[LoanId] [bigint] NULL,
	[LenderId] [bigint] NULL,
 CONSTRAINT [PK_tblReport] PRIMARY KEY CLUSTERED 
(
	[ReportId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblTransaction]    Script Date: 4/27/2018 9:42:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblTransaction](
	[TransactionId] [bigint] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NULL,
	[LoanId] [bigint] NULL,
	[LenderId] [bigint] NULL,
	[Status] [int] NULL,
	[AffCode] [nvarchar](50) NULL,
	[PercentAmount] [float] NULL,
	[TotalAmount] [bigint] NULL,
	[FullName] [nvarchar](250) NULL,
	[CreatedDate] [datetime] NULL,
	[Address] [nvarchar](250) NULL,
	[PhoneNumber] [nvarchar](50) NULL,
	[CityId] [int] NULL,
	[DistrictId] [int] NULL,
	[ProductCreditId] [int] NULL,
 CONSTRAINT [PK_tblTransaction] PRIMARY KEY CLUSTERED 
(
	[TransactionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblUser]    Script Date: 4/27/2018 9:42:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblUser](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [nvarchar](50) NULL,
	[PassWord] [nvarchar](250) NOT NULL,
	[Email] [nvarchar](250) NOT NULL,
	[FullName] [nvarchar](250) NULL,
	[Phone] [nvarchar](50) NULL,
	[RoleType] [int] NULL,
	[CreatedDate] [datetime] NULL,
	[IsActive] [bit] NOT NULL,
	[AffCode] [nvarchar](50) NULL,
	[Address] [nvarchar](max) NULL,
	[Company] [nvarchar](max) NULL,
	[Avatar] [nvarchar](max) NULL,
	[TotalAmountEarning] [bigint] NULL,
	[AvailableBalance] [bigint] NULL,
	[UpdatedDate] [datetime] NULL,
	[EndMatchingDate] [datetime] NULL,
	[EndPaymentDate] [datetime] NULL,
	[PercentAmount] [float] NULL,
	[PercentRefer] [float] NULL,
 CONSTRAINT [PK_tblUser] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblUserReference]    Script Date: 4/27/2018 9:42:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblUserReference](
	[UserId] [int] NOT NULL,
	[UserReferenceId] [int] NOT NULL,
	[AffReferenceCode] [nvarchar](50) NOT NULL,
	[Level] [int] NULL,
	[UserCode] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_tblUserReference] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[UserReferenceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblUserReport]    Script Date: 4/27/2018 9:42:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblUserReport](
	[UserReportId] [bigint] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[FromDate] [datetime] NULL,
	[ToDate] [datetime] NULL,
	[TotalAvailableAmount] [bigint] NULL,
	[TranferAmount] [bigint] NULL,
	[CreatedDate] [datetime] NULL,
	[Note] [nvarchar](500) NULL,
	[DataOfMonth] [nvarchar](50) NULL,
 CONSTRAINT [PK_tblUserReport] PRIMARY KEY CLUSTERED 
(
	[UserReportId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblViewCount]    Script Date: 4/27/2018 9:42:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblViewCount](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[AffCode] [nvarchar](50) NOT NULL,
	[IpAddress] [nvarchar](50) NOT NULL,
	[Count] [int] NULL,
	[CreatedDate] [datetime] NULL,
 CONSTRAINT [PK_tblViewCount] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[tblConfig] ON 

INSERT [dbo].[tblConfig] ([ConfigId], [PercentAmount], [PercentRefer], [ApplyDate]) VALUES (1, 50, 10, CAST(N'2018-04-04T00:00:00.000' AS DateTime))
SET IDENTITY_INSERT [dbo].[tblConfig] OFF
SET IDENTITY_INSERT [dbo].[tblUser] ON 

INSERT [dbo].[tblUser] ([UserId], [UserName], [PassWord], [Email], [FullName], [Phone], [RoleType], [CreatedDate], [IsActive], [AffCode], [Address], [Company], [Avatar], [TotalAmountEarning], [AvailableBalance], [UpdatedDate], [EndMatchingDate], [EndPaymentDate], [PercentAmount], [PercentRefer]) VALUES (1, N'admin', N'K3pRqgtrHPRgdkRJABHF8Q==', N'admin@tima.vn', N'admin', N'234234234243', 0, CAST(N'2018-04-27T09:41:21.920' AS DateTime), 1, N'd9ab19', NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL)
SET IDENTITY_INSERT [dbo].[tblUser] OFF
ALTER TABLE [dbo].[tblBankAccount]  WITH CHECK ADD  CONSTRAINT [FK_tblBankAccount_tblUser] FOREIGN KEY([UserId])
REFERENCES [dbo].[tblUser] ([UserId])
GO
ALTER TABLE [dbo].[tblBankAccount] CHECK CONSTRAINT [FK_tblBankAccount_tblUser]
GO
ALTER TABLE [dbo].[tblLoan]  WITH CHECK ADD  CONSTRAINT [FK_tblLoan_tblUser] FOREIGN KEY([UserId])
REFERENCES [dbo].[tblUser] ([UserId])
GO
ALTER TABLE [dbo].[tblLoan] CHECK CONSTRAINT [FK_tblLoan_tblUser]
GO
ALTER TABLE [dbo].[tblPayment]  WITH CHECK ADD  CONSTRAINT [FK_tblPayment_tblUser] FOREIGN KEY([UserId])
REFERENCES [dbo].[tblUser] ([UserId])
GO
ALTER TABLE [dbo].[tblPayment] CHECK CONSTRAINT [FK_tblPayment_tblUser]
GO
ALTER TABLE [dbo].[tblReport]  WITH CHECK ADD  CONSTRAINT [FK_tblReport_tblTransaction1] FOREIGN KEY([TransactionId])
REFERENCES [dbo].[tblTransaction] ([TransactionId])
GO
ALTER TABLE [dbo].[tblReport] CHECK CONSTRAINT [FK_tblReport_tblTransaction1]
GO
ALTER TABLE [dbo].[tblReport]  WITH CHECK ADD  CONSTRAINT [FK_tblReport_tblUser] FOREIGN KEY([UserId])
REFERENCES [dbo].[tblUser] ([UserId])
GO
ALTER TABLE [dbo].[tblReport] CHECK CONSTRAINT [FK_tblReport_tblUser]
GO
ALTER TABLE [dbo].[tblTransaction]  WITH CHECK ADD  CONSTRAINT [FK_tblTransaction_tblLoan] FOREIGN KEY([LoanId])
REFERENCES [dbo].[tblLoan] ([LoanId])
GO
ALTER TABLE [dbo].[tblTransaction] CHECK CONSTRAINT [FK_tblTransaction_tblLoan]
GO
ALTER TABLE [dbo].[tblTransaction]  WITH CHECK ADD  CONSTRAINT [FK_tblTransaction_tblUser] FOREIGN KEY([UserId])
REFERENCES [dbo].[tblUser] ([UserId])
GO
ALTER TABLE [dbo].[tblTransaction] CHECK CONSTRAINT [FK_tblTransaction_tblUser]
GO
ALTER TABLE [dbo].[tblUserReference]  WITH CHECK ADD  CONSTRAINT [FK_tblUserReference_tblUser] FOREIGN KEY([UserId])
REFERENCES [dbo].[tblUser] ([UserId])
GO
ALTER TABLE [dbo].[tblUserReference] CHECK CONSTRAINT [FK_tblUserReference_tblUser]
GO
ALTER TABLE [dbo].[tblUserReference]  WITH CHECK ADD  CONSTRAINT [FK_tblUserReference_tblUser1] FOREIGN KEY([UserReferenceId])
REFERENCES [dbo].[tblUser] ([UserId])
GO
ALTER TABLE [dbo].[tblUserReference] CHECK CONSTRAINT [FK_tblUserReference_tblUser1]
GO
ALTER TABLE [dbo].[tblUserReport]  WITH CHECK ADD  CONSTRAINT [FK_tblUserReport_tblUser] FOREIGN KEY([UserId])
REFERENCES [dbo].[tblUser] ([UserId])
GO
ALTER TABLE [dbo].[tblUserReport] CHECK CONSTRAINT [FK_tblUserReport_tblUser]
GO
USE [master]
GO
ALTER DATABASE [TimaAffiliate] SET  READ_WRITE 
GO
