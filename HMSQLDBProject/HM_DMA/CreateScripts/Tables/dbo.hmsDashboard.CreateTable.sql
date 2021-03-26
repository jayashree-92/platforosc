
IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsDashboardPreferenceCodeLkup')
BEGIN
	CREATE TABLE [dbo].[hmsDashboardPreferenceCodeLkup](
	[hmsDashboardPreferenceCodeLkupId] INT IDENTITY(1,1) NOT NULL,
	[PreferenceName] VARCHAR(50) NOT NULL,
	
CONSTRAINT UK_hmsDashboardPreferenceCodeLkup_PreferenceName UNIQUE(PreferenceName),
PRIMARY KEY  CLUSTERED  
(
	[hmsDashboardPreferenceCodeLkupId] ASC
) ON [PRIMARY]
) ON [PRIMARY]


	INSERT INTO [hmsDashboardPreferenceCodeLkup]([PreferenceName]) VALUES ('Clients')
	INSERT INTO [hmsDashboardPreferenceCodeLkup]([PreferenceName]) VALUES ('Funds')
	INSERT INTO [hmsDashboardPreferenceCodeLkup]([PreferenceName]) VALUES ('Counterparties')
	INSERT INTO [hmsDashboardPreferenceCodeLkup]([PreferenceName]) VALUES ('AgreementTypes')
	INSERT INTO [hmsDashboardPreferenceCodeLkup]([PreferenceName]) VALUES ('MessageTypes')
	INSERT INTO [hmsDashboardPreferenceCodeLkup]([PreferenceName]) VALUES ('Currencies')
	INSERT INTO [hmsDashboardPreferenceCodeLkup]([PreferenceName]) VALUES ('Stats')
	INSERT INTO [hmsDashboardPreferenceCodeLkup]([PreferenceName]) VALUES ('Status')
	INSERT INTO [hmsDashboardPreferenceCodeLkup]([PreferenceName]) VALUES ('Reports')

END

IF NOT EXISTS(SELECT * FROM hmsDashboardPreferenceCodeLkup WHERE [PreferenceName] = 'Reports')
BEGIN
	INSERT INTO [hmsDashboardPreferenceCodeLkup]([PreferenceName]) VALUES ('Reports')
END

IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsDashboardTemplates')
BEGIN
CREATE TABLE [dbo].[hmsDashboardTemplates](
	[hmsDashboardTemplateId] BIGINT IDENTITY(1,1) NOT NULL,
	[TemplateName] VARCHAR(50) NOT NULL,
	[IsDeleted] BIT NOT NULL,
	[RecCreatedDt] DATETIME NOT NULL DEFAULT(GETDATE()),
	[RecCreatedById] INT NOT NULL,

	
CONSTRAINT UK_hmsDashboardTemplates_TemplateName UNIQUE([TemplateName]),
PRIMARY KEY  CLUSTERED  
(
	[hmsDashboardTemplateId] ASC
) ON [PRIMARY]
) ON [PRIMARY]

END


IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsDashboardPreferences')
BEGIN
	CREATE TABLE [dbo].[hmsDashboardPreferences](
	[hmsDashboardPreferenceId] bigint IDENTITY(1,1) NOT NULL,
	[hmsDashboardTemplateId] BIGINT NOT NULL,
	[PreferenceCode] INT NOT NULL, 
	[Preferences] VARCHAR(8000) NOT NULL, 
	[RecCreatedDt] DATETIME NOT NULL DEFAULT(GETDATE()),
	[RecCreatedById] INT NOT NULL,
	
CONSTRAINT UK_hmsDashboardPreferences_hmsDashboardTemplateId_PreferenceCode UNIQUE([hmsDashboardTemplateId],[PreferenceCode]),
PRIMARY KEY  CLUSTERED  
(
	[hmsDashboardPreferenceId] ASC
) ON [PRIMARY]
) ON [PRIMARY]


ALTER TABLE [dbo].[hmsDashboardPreferences]  WITH CHECK ADD CONSTRAINT [FK_hmsDashboardPreferences_hmsDashboardPreferenceCodeLkup_PreferenceCode] FOREIGN KEY([PreferenceCode]) REFERENCES [dbo].[hmsDashboardPreferenceCodeLkup] ([hmsDashboardPreferenceCodeLkupId]);
ALTER TABLE [dbo].[hmsDashboardPreferences]  WITH CHECK ADD CONSTRAINT [FK_hmsDashboardPreferences_hmsDashboardTemplates_hmsDashboardTemplateId] FOREIGN KEY([hmsDashboardTemplateId]) REFERENCES [dbo].[hmsDashboardTemplates] ([hmsDashboardTemplateId]);
END

IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsScheduleWorkflowLkup')
BEGIN
	CREATE TABLE [dbo].[hmsScheduleWorkflowLkup](
	[hmsScheduleWorkflowLkupId] INT IDENTITY(1,1) NOT NULL,
	[WorkflowStatusCode] INT NOT NULL,
	[Description] VARCHAR(50) NOT NULL,
	
CONSTRAINT UK_hmsScheduleWorkflowLkup_WorkflowStatusCode UNIQUE(WorkflowStatusCode),
PRIMARY KEY  CLUSTERED  
(
	[hmsScheduleWorkflowLkupId] ASC
) ON [PRIMARY]
) ON [PRIMARY]


INSERT INTO [hmsScheduleWorkflowLkup]([WorkflowStatusCode],[Description]) VALUES (0,'No Action taken')
INSERT INTO [hmsScheduleWorkflowLkup]([WorkflowStatusCode],[Description]) VALUES (1,'Approved')
INSERT INTO [hmsScheduleWorkflowLkup]([WorkflowStatusCode],[Description]) VALUES (2,'Rejected')

END


IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsDashboardScheduleRangeLkup')
BEGIN
	CREATE TABLE [dbo].[hmsDashboardScheduleRangeLkup](
	[hmsDashboardScheduleRangeLkupId] INT IDENTITY(1,1) NOT NULL,
	[ScheduleRange] VARCHAR(50) NOT NULL,
	
CONSTRAINT UK_hmsDashboardScheduleRangeLkup_ScheduleRange UNIQUE(ScheduleRange),
PRIMARY KEY  CLUSTERED  
(
	[hmsDashboardScheduleRangeLkupId] ASC
) ON [PRIMARY]
) ON [PRIMARY]


	INSERT INTO [hmsDashboardScheduleRangeLkup]([ScheduleRange]) VALUES ('Today Only')
	INSERT INTO [hmsDashboardScheduleRangeLkup]([ScheduleRange]) VALUES ('Today and Yesterday')
	INSERT INTO [hmsDashboardScheduleRangeLkup]([ScheduleRange]) VALUES ('Last 7 Days')
	INSERT INTO [hmsDashboardScheduleRangeLkup]([ScheduleRange]) VALUES ('Last 30 Days')
	INSERT INTO [hmsDashboardScheduleRangeLkup]([ScheduleRange]) VALUES ('This Month')
	INSERT INTO [hmsDashboardScheduleRangeLkup]([ScheduleRange]) VALUES ('This Year')
	INSERT INTO [hmsDashboardScheduleRangeLkup]([ScheduleRange]) VALUES ('Last 3 months')

END


IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsSchedules')
BEGIN

CREATE TABLE [dbo].[hmsSchedules](
	[hmsScheduleId] [int] IDENTITY(1,1) NOT NULL,
	[Frequency]  [varchar](30) NOT NULL,
	[ScheduleExpression]  [varchar](30) NOT NULL,
	[TimeZone] [varchar](10) NOT NULL,
	[To] [varchar](500) NOT NULL,
	[CC] [varchar](500) NULL,
	[ExternalTo] [varchar](500) NULL,
	[ExternalToApproved] [varchar](500) NULL,
	[IsActive] [bit] NOT NULL,
	[FileFormat] [varchar](20) NOT NULL,
	[SFTPFolder] [varchar](60) NULL,	
	[InternalFolder][varchar](60) NULL,	
	[ReportFileName] VARCHAR(200) NULL,
	[PreferredFundNameCode] INT NOT NULL DEFAULT(0),
	[CreatedBy] INT NOT NULL,
	[CreatedAt] DATETIME NOT NULL,	
	[LastUpdatedAt] [datetime] NOT NULL,
	[LastModifiedBy] INT NOT NULL,
	[ExternalToWorkflowCode] [INT] NOT NULL DEFAULT(0),
	[ExternalToModifiedBy] INT NULL,
	[ExternalToModifiedAt] DATETIME NULL,
	[ExternalToComments] VARCHAR(500) NULL,
	[SFTPFolderWorkflowCode] [INT] NOT NULL DEFAULT(0),
	[SFTPFolderModifiedBy] INT NULL,
	[SFTPFolderModifiedAt] DATETIME NULL,
	[SFTPFolderComments] VARCHAR(500) NULL,
PRIMARY KEY CLUSTERED 
(
	[hmsScheduleId] ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]



ALTER TABLE [dbo].[hmsSchedules]  WITH CHECK ADD  CONSTRAINT [FK_hmsSchedules_ExternalToWorkflowCode] 
	FOREIGN KEY([ExternalToWorkflowCode]) REFERENCES [dbo].[hmsScheduleWorkflowLkup] ([WorkflowStatusCode])

	
ALTER TABLE [dbo].[hmsSchedules]  WITH CHECK ADD  CONSTRAINT [FK_hmsSchedules_SFTPFolderWorkflowCode] 
	FOREIGN KEY([SFTPFolderWorkflowCode]) REFERENCES [dbo].[hmsScheduleWorkflowLkup] ([WorkflowStatusCode])

END

IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsDashboardSchedules')
BEGIN

	CREATE TABLE [dbo].[hmsDashboardSchedules](
	
		[hmsDashboardScheduleId] [INT] IDENTITY(1,1) NOT NULL,
		[hmsScheduleId] [INT] NOT NULL,
		[hmsDashboardTemplateId] [bigint] NOT NULL,
		[DashboardScheduleRangeLkupId] [INT] NOT NULL,
		[LastUpdatedAt] [DATETIME] NOT NULL,
		[LastModifiedBy] INT NOT NULL,
		[IsDeleted] BIT NOT NULL DEFAULT(0)
		PRIMARY KEY CLUSTERED 
	(
		[hmsDashboardScheduleId] ASC
	) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]


	ALTER TABLE [dbo].[hmsDashboardSchedules]  WITH CHECK ADD  CONSTRAINT [FK_hmsDashboardSchedules_hmsScheduleId] 
		FOREIGN KEY([hmsScheduleId]) REFERENCES [dbo].[hmsSchedules] ([hmsScheduleId])

	ALTER TABLE [dbo].[hmsDashboardSchedules]  WITH CHECK ADD  CONSTRAINT [FK_hmsDashboardSchedules_hmsDashboardTemplateId] 
		FOREIGN KEY([hmsDashboardTemplateId]) REFERENCES [dbo].[hmsDashboardTemplates] (hmsDashboardTemplateId)

	ALTER TABLE [dbo].[hmsDashboardSchedules]  WITH CHECK ADD  CONSTRAINT [FK_hmsDashboardSchedules_DashboardScheduleRangeLkupId] 
		FOREIGN KEY([DashboardScheduleRangeLkupId]) REFERENCES [dbo].[hmsDashboardScheduleRangeLkup] (hmsDashboardScheduleRangeLkupId)

END

IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsScheduleLogs')
BEGIN
													
CREATE TABLE [dbo].[hmsScheduleLogs](
	[hmsScheduleLogId] [int] IDENTITY(1,1) NOT NULL,	
	[hmsScheduleId] INT NOT NULL,
	[ContextDate] DATE NOT NULL,
	[ScheduleStartTime] DATETIME NOT NULL,
	[ScheduleEndTime] DATETIME NULL,
	[TimeOutJobId] VARCHAR(50),
	[RecCreatedAt] DATETIME NOT NULL,
	
PRIMARY KEY CLUSTERED 
(
	[hmsScheduleLogId] ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


ALTER TABLE [dbo].[hmsScheduleLogs]  WITH CHECK ADD  CONSTRAINT [FK_hmsScheduleLogs_hmsScheduleId] 
	FOREIGN KEY([hmsScheduleId]) REFERENCES [dbo].[hmsSchedules] ([hmsScheduleId])
END
GO