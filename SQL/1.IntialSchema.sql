DECLARE @issueNumber VARCHAR(50) = '1.IntialDatabaseSchema'

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'UpdateHistory')
BEGIN
	BEGIN TRY
	    SET XACT_ABORT ON
		SET ANSI_NULLS ON
		SET QUOTED_IDENTIFIER ON
		
		BEGIN TRANSACTION
		
			/****** Object:  Table [dbo].[ApplicationAuthentication]    Script Date: 10/03/2016 3:07:53 p.m. ******/
			CREATE TABLE [dbo].[ApplicationAuthentication](
				[ID] [int] IDENTITY(1,1) NOT NULL,
				[ApplicationID] [int] NOT NULL,
				[WellKnownUrl] [nvarchar](255) NOT NULL,
				[Issuer] [nvarchar](255) NOT NULL,
				[JwksUri] [nvarchar](255) NOT NULL,
				[AuthorizationEndpoint] [nvarchar](255) NOT NULL,
				[TokenEndpoint] [nvarchar](255) NOT NULL,
				[UserInfoEndpoint] [nvarchar](255) NOT NULL,
				[EndSessionEndpoint] [nvarchar](255) NOT NULL,
				[CheckSessionIFrame] [nvarchar](255) NOT NULL,
				[RevocationEndpoint] [nvarchar](255) NOT NULL,
				[CreatedAt] [datetime2](7) NOT NULL,
				[CreatedBy] [int] NOT NULL,
				[UpdatedAt] [datetime2](7) NULL,
				[UpdatedBy] [int] NULL,
			 CONSTRAINT [PK_ApplicationAuthentication] PRIMARY KEY CLUSTERED 
			(
				[ID] ASC
			)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
			) ON [PRIMARY]

			/****** Object:  Table [dbo].[Applications]    Script Date: 10/03/2016 3:07:53 p.m. ******/
			CREATE TABLE [dbo].[Applications](
				[ID] [int] IDENTITY(1,1) NOT NULL,
				[OrganizationID] [int] NOT NULL,
				[Name] [nvarchar](255) NOT NULL,
				[Description] [nvarchar](max) NULL,
				[PublicID] [nvarchar](50) NOT NULL,
				[IsProvider] [bit] NOT NULL,
				[IsActive] [bit] NOT NULL,
				[CreatedAt] [datetime2](7) NOT NULL,
				[CreatedBy] [int] NOT NULL,
				[UpdatedAt] [datetime2](7) NULL,
				[UpdatedBy] [int] NULL,
				[IsIntroducedAsIndustryod] [bit] NOT NULL,
				[IsVerifiedAsIndustryod] [bit] NULL,
			 CONSTRAINT [PK_Applications] PRIMARY KEY CLUSTERED 
			(
				[ID] ASC
			)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
			) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

			/****** Object:  Table [dbo].[ApplicationTokens]    Script Date: 10/03/2016 3:07:53 p.m. ******/
			CREATE TABLE [dbo].[ApplicationTokens](
				[ID] [int] IDENTITY(1,1) NOT NULL,
				[ApplicationID] [int] NOT NULL,
				[OriginHost] [nvarchar](255) NOT NULL,
				[Token] [nvarchar](255) NOT NULL,
				[CreatedAt] [datetime2](7) NOT NULL,
				[CreatedBy] [int] NOT NULL,
				[ExpiredAt] [datetime2](7) NULL,
				[ExpiredBy] [int] NULL,
			 CONSTRAINT [PK_ApplicationTokens] PRIMARY KEY CLUSTERED 
			(
				[ID] ASC
			)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
			) ON [PRIMARY]

			/****** Object:  Table [dbo].[ConsumerLicenseRequests]    Script Date: 10/03/2016 3:07:53 p.m. ******/
			CREATE TABLE [dbo].[ConsumerLicenseRequests](
				[ID] [int] IDENTITY(1,1) NOT NULL,
				[CreatedAt] [datetime2](7) NOT NULL,
				[CreatedBy] [int] NOT NULL,
				[ApprovedAt] [datetime2](7) NULL,
				[ApprovedBy] [int] NULL,
				[ConsumerApplicationID] [int] NOT NULL,
				[ProviderApplicationID] [int] NOT NULL,
				[ProviderEndpointLicenseID] [int] NOT NULL,
				[Status] [int] NOT NULL,
			 CONSTRAINT [PK_ConsumerLicenseRequests] PRIMARY KEY CLUSTERED 
			(
				[ID] ASC
			)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
			) ON [PRIMARY]

			/****** Object:  Table [dbo].[DataSchemas]    Script Date: 10/03/2016 3:07:53 p.m. ******/
			CREATE TABLE [dbo].[DataSchemas](
				[ID] [int] IDENTITY(1,1) NOT NULL,
				[PublicID] [nvarchar](50) NOT NULL,
				[Name] [nvarchar](255) NOT NULL,
				[Description] [nvarchar](max) NULL,
				[CreatedAt] [datetime2](7) NOT NULL,
				[CreatedBy] [int] NOT NULL,
				[UpdatedAt] [datetime2](7) NULL,
				[UpdatedBy] [int] NULL,
				[Version] [int] NOT NULL,
				[PublishedAt] [datetime2](7) NULL,
				[PublishedBy] [int] NULL,
				[RetractedAt] [datetime2](7) NULL,
				[RetractedBy] [int] NULL,
				[Status] [int] NOT NULL,
				[IsIndustryod] [bit] NOT NULL,
			 CONSTRAINT [PK_DataSchemas] PRIMARY KEY CLUSTERED 
			(
				[ID] ASC
			)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
			) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
			
			/****** Object:  Table [dbo].[LicenseClauses]    Script Date: 10/03/2016 3:07:53 p.m. ******/
			CREATE TABLE [dbo].[LicenseClauses](
				[ID] [int] IDENTITY(1,1) NOT NULL,
				[LicenseSectionID] [int] NOT NULL,
				[OrderNumber] [int] NOT NULL,
				[CreatedAt] [datetime2](7) NOT NULL,
				[CreatedBy] [int] NOT NULL,
				[UpdatedAt] [datetime2](7) NULL,
				[UpdatedBy] [int] NULL,
			 CONSTRAINT [PK_LicenseClauses] PRIMARY KEY CLUSTERED 
			(
				[ID] ASC
			)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
			) ON [PRIMARY]

			/****** Object:  Table [dbo].[LicenseClauseTemplates]    Script Date: 10/03/2016 3:07:53 p.m. ******/
			CREATE TABLE [dbo].[LicenseClauseTemplates](
				[ID] [int] IDENTITY(1,1) NOT NULL,
				[LicenseClauseID] [int] NOT NULL,
				[ShortText] [nvarchar](max) NOT NULL,
				[Description] [nvarchar](max) NOT NULL,
				[LegalText] [nvarchar](max) NOT NULL,
				[Version] [int] NOT NULL,
				[Status] [int] NOT NULL,
				[CreatedAt] [datetime2](7) NOT NULL,
				[CreatedBy] [int] NOT NULL,
				[UpdatedAt] [datetime2](7) NULL,
				[UpdatedBy] [int] NULL,
				[ClauseType] [int] NULL,
			 CONSTRAINT [PK_LicenseClauseTemplates] PRIMARY KEY CLUSTERED 
			(
				[ID] ASC
			)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
			) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

			/****** Object:  Table [dbo].[Licenses]    Script Date: 10/03/2016 3:07:53 p.m. ******/
			CREATE TABLE [dbo].[Licenses](
				[ID] [int] IDENTITY(1,1) NOT NULL,
				[CreatedAt] [datetime2](7) NOT NULL,
				[CreatedBy] [int] NOT NULL,
				[UpdatedAt] [datetime2](7) NULL,
				[UpdatedBy] [int] NULL,
			 CONSTRAINT [PK_Licenses] PRIMARY KEY CLUSTERED 
			(
				[ID] ASC
			)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
			) ON [PRIMARY]

			/****** Object:  Table [dbo].[LicenseSections]    Script Date: 10/03/2016 3:07:53 p.m. ******/
			CREATE TABLE [dbo].[LicenseSections](
				[ID] [int] IDENTITY(1,1) NOT NULL,
				[Title] [nvarchar](255) NOT NULL,
				[CreatedAt] [datetime2](7) NOT NULL,
				[CreatedBy] [int] NOT NULL,
				[UpdatedAt] [datetime2](7) NULL,
				[UpdatedBy] [int] NULL,
			 CONSTRAINT [PK_LicenseSections] PRIMARY KEY CLUSTERED 
			(
				[ID] ASC
			)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
			) ON [PRIMARY]

			
			/****** Object:  Table [dbo].[LicenseTemplates]    Script Date: 10/03/2016 3:07:53 p.m. ******/
			CREATE TABLE [dbo].[LicenseTemplates](
				[ID] [int] IDENTITY(1,1) NOT NULL,
				[LicenseID] [int] NOT NULL,
				[Name] [nvarchar](255) NOT NULL,
				[Description] [nvarchar](max) NOT NULL,
				[LicenseText] [nvarchar](max) NOT NULL,
				[Version] [int] NOT NULL,
				[Status] [int] NOT NULL,
				[CreatedAt] [datetime2](7) NOT NULL,
				[CreatedBy] [int] NOT NULL,
				[UpdatedAt] [datetime2](7) NULL,
				[UpdatedBy] [int] NULL,
			 CONSTRAINT [PK_LicenseTemplates] PRIMARY KEY CLUSTERED 
			(
				[ID] ASC
			)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
			) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

			
			/****** Object:  Table [dbo].[Organizations]    Script Date: 10/03/2016 3:07:53 p.m. ******/
			CREATE TABLE [dbo].[Organizations](
				[ID] [int] IDENTITY(1,1) NOT NULL,
				[Name] [nvarchar](255) NOT NULL,
				[IsActive] [bit] NULL,
				[IsAgreeWithTerms] [bit] NULL,
				[Phone] [nvarchar](25) NULL,
				[Address] [nvarchar](50) NULL,
				[AdministrativeContact] [nvarchar](50) NULL,
				[AdministrativePhone] [nvarchar](50) NULL,
				[TermsOfService] [ntext] NULL,
			 CONSTRAINT [PK_Organizations] PRIMARY KEY CLUSTERED 
			(
				[ID] ASC
			)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
			) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
			
			/****** Object:  Table [dbo].[ProviderEndpointLicenseApprovalRequests]    Script Date: 10/03/2016 3:07:53 p.m. ******/
			CREATE TABLE [dbo].[ProviderEndpointLicenseApprovalRequests](
				[ID] [int] IDENTITY(1,1) NOT NULL,
				[ProviderEndpointLicenseID] [int] NOT NULL,
				[SentAt] [datetime2](7) NOT NULL,
				[SentBy] [int] NOT NULL,
				[SentTo] [int] NOT NULL,
				[ExpiresAt] [datetime2](7) NOT NULL,
				[accessToken] [nvarchar](255) NOT NULL,
			 CONSTRAINT [PK_ProviderEndpointLicenseApprovalRequests] PRIMARY KEY CLUSTERED 
			(
				[ID] ASC
			)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
			) ON [PRIMARY]
			
			/****** Object:  Table [dbo].[ProviderEndpointLicenseClauses]    Script Date: 10/03/2016 3:07:53 p.m. ******/
			CREATE TABLE [dbo].[ProviderEndpointLicenseClauses](
				[ID] [int] IDENTITY(1,1) NOT NULL,
				[ProviderEndpointLicenseID] [int] NOT NULL,
				[LicenseClauseID] [int] NOT NULL,
				[ClauseData] [nvarchar](max) NULL,
				[CreatedAt] [datetime2](7) NOT NULL,
				[CreatedBy] [int] NOT NULL,
				[UpdatedAt] [datetime2](7) NULL,
				[UpdatedBy] [int] NULL,
			 CONSTRAINT [PK_ProviderEndpointLicenseClauses] PRIMARY KEY CLUSTERED 
			(
				[ID] ASC
			)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
			) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
			
			/****** Object:  Table [dbo].[ProviderEndpointLicenses]    Script Date: 10/03/2016 3:07:53 p.m. ******/
			CREATE TABLE [dbo].[ProviderEndpointLicenses](
				[ID] [int] IDENTITY(1,1) NOT NULL,
				[ProviderEndpointID] [int] NOT NULL,
				[LicenseTemplateID] [int] NOT NULL,
				[Status] [int] NOT NULL,
				[CreatedAt] [datetime2](7) NOT NULL,
				[CreatedBy] [int] NOT NULL,
				[UpdatedAt] [datetime2](7) NULL,
				[UpdatedBy] [int] NULL,
				[ApprovedAt] [datetime2](7) NULL,
				[ApprovedBy] [int] NULL,
				[PublishedAt] [datetime2](7) NULL,
				[PublishedBy] [int] NULL,
				[SentToApproveAt] [datetime2](7) NULL,
				[SentToApproveBy] [int] NULL,
				[Token] [nvarchar](255) NULL,
				[TokenExpire] [datetime2](7) NULL,
			 CONSTRAINT [PK_ProviderEndpointLicenses] PRIMARY KEY CLUSTERED 
			(
				[ID] ASC
			)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
			) ON [PRIMARY]
			
			/****** Object:  Table [dbo].[ProviderEndpoints]    Script Date: 10/03/2016 3:07:53 p.m. ******/
			CREATE TABLE [dbo].[ProviderEndpoints](
				[ID] [int] IDENTITY(1,1) NOT NULL,
				[ApplicationID] [int] NOT NULL,
				[Name] [nvarchar](255) NOT NULL,
				[Description] [nvarchar](max) NULL,
				[IsIndustryod] [bit] NOT NULL,
				[ServiceUri] [nvarchar](255) NOT NULL,
				[DataSchemaID] [int] NOT NULL,
				[IsActive] [bit] NOT NULL,
				[CreatedAt] [datetime2](7) NOT NULL,
				[CreatedBy] [int] NOT NULL,
				[UpdatedAt] [datetime2](7) NULL,
				[UpdatedBy] [int] NULL,
			 CONSTRAINT [PK_ProviderEndpoint] PRIMARY KEY CLUSTERED 
			(
				[ID] ASC
			)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
			) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
			
			/****** Object:  Table [dbo].[SchemaFiles]    Script Date: 10/03/2016 3:07:53 p.m. ******/
			CREATE TABLE [dbo].[SchemaFiles](
				[ID] [int] IDENTITY(1,1) NOT NULL,
				[DataSchemaID] [int] NOT NULL,
				[SchemaText] [ntext] NOT NULL,
				[IsCurrent] [bit] NOT NULL,
				[CreatedAt] [datetime2](7) NOT NULL,
				[CreatedBy] [int] NOT NULL,
			 CONSTRAINT [PK_SchemaFiles] PRIMARY KEY CLUSTERED 
			(
				[ID] ASC
			)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
			) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
			
			/****** Object:  Table [dbo].[UpdateHistory]    Script Date: 10/03/2016 3:07:53 p.m. ******/
			CREATE TABLE [dbo].[UpdateHistory](
				[Name] [nvarchar](50) NOT NULL,
				[Date] [datetime] NOT NULL
			) ON [PRIMARY]
			
			/****** Object:  Table [dbo].[Users]    Script Date: 10/03/2016 3:07:53 p.m. ******/
			CREATE TABLE [dbo].[Users](
				[ID] [int] IDENTITY(1,1) NOT NULL,
				[Email] [nvarchar](255) NOT NULL,
				[Username] [nvarchar](255) NOT NULL,
				[IsIntroducedAsLegalOfficer] [bit] NOT NULL,
				[OrganizationID] [int] NULL,
				[IsSysAdmin] [bit] NOT NULL,
				[IsActive] [bit] NOT NULL,
				[Name] [nvarchar](50) NULL,
				[Phone] [nvarchar](50) NULL,
				[NewEmail] [nvarchar](50) NULL,
				[Token] [nvarchar](50) NULL,
				[TokenExpire] [datetime2](7) NULL,
				[IsVerifiedAsLegalOfficer] [bit] NULL,
				[UserID] [nvarchar](128) NULL,
			 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
			(
				[ID] ASC
			)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
			) ON [PRIMARY]
			
			ALTER TABLE [dbo].[DataSchemas] ADD  DEFAULT ((0)) FOR [Status]
			
			ALTER TABLE [dbo].[DataSchemas] ADD  DEFAULT ((0)) FOR [IsIndustryod]
			
			ALTER TABLE [dbo].[ApplicationAuthentication]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationAuthentication_Applications] FOREIGN KEY([ApplicationID])
			REFERENCES [dbo].[Applications] ([ID])
			
			ALTER TABLE [dbo].[ApplicationAuthentication] CHECK CONSTRAINT [FK_ApplicationAuthentication_Applications]
			
			ALTER TABLE [dbo].[ApplicationAuthentication]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationAuthentication_Users_CreatedBy] FOREIGN KEY([CreatedBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[ApplicationAuthentication] CHECK CONSTRAINT [FK_ApplicationAuthentication_Users_CreatedBy]
			
			ALTER TABLE [dbo].[ApplicationAuthentication]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationAuthentication_Users_UpdatedBy] FOREIGN KEY([UpdatedBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[ApplicationAuthentication] CHECK CONSTRAINT [FK_ApplicationAuthentication_Users_UpdatedBy]
			
			ALTER TABLE [dbo].[Applications]  WITH CHECK ADD  CONSTRAINT [FK_Applications_Organizations] FOREIGN KEY([OrganizationID])
			REFERENCES [dbo].[Organizations] ([ID])
			
			ALTER TABLE [dbo].[Applications] CHECK CONSTRAINT [FK_Applications_Organizations]
			
			ALTER TABLE [dbo].[Applications]  WITH CHECK ADD  CONSTRAINT [FK_Applications_Users_CreatedBy] FOREIGN KEY([CreatedBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[Applications] CHECK CONSTRAINT [FK_Applications_Users_CreatedBy]
			
			ALTER TABLE [dbo].[Applications]  WITH CHECK ADD  CONSTRAINT [FK_Applications_Users_UpdatedBy] FOREIGN KEY([UpdatedBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[Applications] CHECK CONSTRAINT [FK_Applications_Users_UpdatedBy]
			
			ALTER TABLE [dbo].[ApplicationTokens]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationTokens_Applications] FOREIGN KEY([ApplicationID])
			REFERENCES [dbo].[Applications] ([ID])
			
			ALTER TABLE [dbo].[ApplicationTokens] CHECK CONSTRAINT [FK_ApplicationTokens_Applications]
			
			ALTER TABLE [dbo].[ApplicationTokens]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationTokens_Users_CreatedBy] FOREIGN KEY([CreatedBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[ApplicationTokens] CHECK CONSTRAINT [FK_ApplicationTokens_Users_CreatedBy]
			
			ALTER TABLE [dbo].[ApplicationTokens]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationTokens_Users_ExpiredBy] FOREIGN KEY([ExpiredBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[ApplicationTokens] CHECK CONSTRAINT [FK_ApplicationTokens_Users_ExpiredBy]
			
			ALTER TABLE [dbo].[ConsumerLicenseRequests]  WITH CHECK ADD  CONSTRAINT [FK_ConsumerLicenseRequests_ApprovedBy] FOREIGN KEY([ApprovedBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[ConsumerLicenseRequests] CHECK CONSTRAINT [FK_ConsumerLicenseRequests_ApprovedBy]
			
			ALTER TABLE [dbo].[ConsumerLicenseRequests]  WITH CHECK ADD  CONSTRAINT [FK_ConsumerLicenseRequests_ConsumerApplicationID] FOREIGN KEY([ConsumerApplicationID])
			REFERENCES [dbo].[Applications] ([ID])
			
			ALTER TABLE [dbo].[ConsumerLicenseRequests] CHECK CONSTRAINT [FK_ConsumerLicenseRequests_ConsumerApplicationID]
			
			ALTER TABLE [dbo].[ConsumerLicenseRequests]  WITH CHECK ADD  CONSTRAINT [FK_ConsumerLicenseRequests_CreatedBy] FOREIGN KEY([CreatedBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[ConsumerLicenseRequests] CHECK CONSTRAINT [FK_ConsumerLicenseRequests_CreatedBy]
			
			ALTER TABLE [dbo].[ConsumerLicenseRequests]  WITH CHECK ADD  CONSTRAINT [FK_ConsumerLicenseRequests_ProviderApplicationID] FOREIGN KEY([ProviderApplicationID])
			REFERENCES [dbo].[Applications] ([ID])
			
			ALTER TABLE [dbo].[ConsumerLicenseRequests] CHECK CONSTRAINT [FK_ConsumerLicenseRequests_ProviderApplicationID]
			
			ALTER TABLE [dbo].[ConsumerLicenseRequests]  WITH CHECK ADD  CONSTRAINT [FK_ConsumerLicenseRequests_ProviderEndpointLicenseID] FOREIGN KEY([ProviderEndpointLicenseID])
			REFERENCES [dbo].[ProviderEndpointLicenses] ([ID])
			
			ALTER TABLE [dbo].[ConsumerLicenseRequests] CHECK CONSTRAINT [FK_ConsumerLicenseRequests_ProviderEndpointLicenseID]
			
			ALTER TABLE [dbo].[DataSchemas]  WITH CHECK ADD  CONSTRAINT [FK_DataSchemas_Users_CreatedBy] FOREIGN KEY([CreatedBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[DataSchemas] CHECK CONSTRAINT [FK_DataSchemas_Users_CreatedBy]
			
			ALTER TABLE [dbo].[DataSchemas]  WITH CHECK ADD  CONSTRAINT [FK_DataSchemas_Users_PublishedBy] FOREIGN KEY([PublishedBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[DataSchemas] CHECK CONSTRAINT [FK_DataSchemas_Users_PublishedBy]
			
			ALTER TABLE [dbo].[DataSchemas]  WITH CHECK ADD  CONSTRAINT [FK_DataSchemas_Users_RetractedBy] FOREIGN KEY([RetractedBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[DataSchemas] CHECK CONSTRAINT [FK_DataSchemas_Users_RetractedBy]
			
			ALTER TABLE [dbo].[DataSchemas]  WITH CHECK ADD  CONSTRAINT [FK_DataSchemas_Users_UpdatedBy] FOREIGN KEY([UpdatedBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[DataSchemas] CHECK CONSTRAINT [FK_DataSchemas_Users_UpdatedBy]
			
			ALTER TABLE [dbo].[LicenseClauses]  WITH CHECK ADD  CONSTRAINT [FK_LicenseClauses_CreatedBy] FOREIGN KEY([CreatedBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[LicenseClauses] CHECK CONSTRAINT [FK_LicenseClauses_CreatedBy]
			
			ALTER TABLE [dbo].[LicenseClauses]  WITH CHECK ADD  CONSTRAINT [FK_LicenseClauses_LicenseSectionID] FOREIGN KEY([LicenseSectionID])
			REFERENCES [dbo].[LicenseSections] ([ID])
			
			ALTER TABLE [dbo].[LicenseClauses] CHECK CONSTRAINT [FK_LicenseClauses_LicenseSectionID]
			
			ALTER TABLE [dbo].[LicenseClauses]  WITH CHECK ADD  CONSTRAINT [FK_LicenseClauses_UpdatedBy] FOREIGN KEY([UpdatedBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[LicenseClauses] CHECK CONSTRAINT [FK_LicenseClauses_UpdatedBy]
			
			ALTER TABLE [dbo].[LicenseClauseTemplates]  WITH CHECK ADD  CONSTRAINT [FK_LicenseClauseTemplates_CreatedBy] FOREIGN KEY([CreatedBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[LicenseClauseTemplates] CHECK CONSTRAINT [FK_LicenseClauseTemplates_CreatedBy]
			
			ALTER TABLE [dbo].[LicenseClauseTemplates]  WITH CHECK ADD  CONSTRAINT [FK_LicenseClauseTemplates_LicenseClauseID] FOREIGN KEY([LicenseClauseID])
			REFERENCES [dbo].[LicenseClauses] ([ID])
			
			ALTER TABLE [dbo].[LicenseClauseTemplates] CHECK CONSTRAINT [FK_LicenseClauseTemplates_LicenseClauseID]
			
			ALTER TABLE [dbo].[LicenseClauseTemplates]  WITH CHECK ADD  CONSTRAINT [FK_LicenseClauseTemplates_UpdatedBy] FOREIGN KEY([UpdatedBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[LicenseClauseTemplates] CHECK CONSTRAINT [FK_LicenseClauseTemplates_UpdatedBy]
			
			ALTER TABLE [dbo].[Licenses]  WITH CHECK ADD  CONSTRAINT [FK_Licenses_CreatedBy] FOREIGN KEY([CreatedBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[Licenses] CHECK CONSTRAINT [FK_Licenses_CreatedBy]
			
			ALTER TABLE [dbo].[Licenses]  WITH CHECK ADD  CONSTRAINT [FK_Licenses_UpdatedBy] FOREIGN KEY([UpdatedBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[Licenses] CHECK CONSTRAINT [FK_Licenses_UpdatedBy]
			
			ALTER TABLE [dbo].[LicenseSections]  WITH CHECK ADD  CONSTRAINT [FK_LicenseSections_CreatedBy] FOREIGN KEY([CreatedBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[LicenseSections] CHECK CONSTRAINT [FK_LicenseSections_CreatedBy]
			
			ALTER TABLE [dbo].[LicenseSections]  WITH CHECK ADD  CONSTRAINT [FK_LicenseSections_UpdatedBy] FOREIGN KEY([UpdatedBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[LicenseSections] CHECK CONSTRAINT [FK_LicenseSections_UpdatedBy]
			
			ALTER TABLE [dbo].[LicenseTemplates]  WITH CHECK ADD  CONSTRAINT [FK_LicenseTemplates_CreatedBy] FOREIGN KEY([CreatedBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[LicenseTemplates] CHECK CONSTRAINT [FK_LicenseTemplates_CreatedBy]
			
			ALTER TABLE [dbo].[LicenseTemplates]  WITH CHECK ADD  CONSTRAINT [FK_LicenseTemplates_LicenseID] FOREIGN KEY([LicenseID])
			REFERENCES [dbo].[Licenses] ([ID])
			
			ALTER TABLE [dbo].[LicenseTemplates] CHECK CONSTRAINT [FK_LicenseTemplates_LicenseID]
			
			ALTER TABLE [dbo].[LicenseTemplates]  WITH CHECK ADD  CONSTRAINT [FK_LicenseTemplates_UpdatedBy] FOREIGN KEY([UpdatedBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[LicenseTemplates] CHECK CONSTRAINT [FK_LicenseTemplates_UpdatedBy]
			
			ALTER TABLE [dbo].[ProviderEndpointLicenseApprovalRequests]  WITH CHECK ADD  CONSTRAINT [FK_ProviderEndpointLicenseApprovalRequests_ProviderEndpointLicenseID] FOREIGN KEY([ProviderEndpointLicenseID])
			REFERENCES [dbo].[ProviderEndpointLicenses] ([ID])
			
			ALTER TABLE [dbo].[ProviderEndpointLicenseApprovalRequests] CHECK CONSTRAINT [FK_ProviderEndpointLicenseApprovalRequests_ProviderEndpointLicenseID]
			
			ALTER TABLE [dbo].[ProviderEndpointLicenseApprovalRequests]  WITH CHECK ADD  CONSTRAINT [FK_ProviderEndpointLicenseApprovalRequests_SentBy] FOREIGN KEY([SentBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[ProviderEndpointLicenseApprovalRequests] CHECK CONSTRAINT [FK_ProviderEndpointLicenseApprovalRequests_SentBy]
			
			ALTER TABLE [dbo].[ProviderEndpointLicenseApprovalRequests]  WITH CHECK ADD  CONSTRAINT [FK_ProviderEndpointLicenseApprovalRequests_SentTo] FOREIGN KEY([SentTo])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[ProviderEndpointLicenseApprovalRequests] CHECK CONSTRAINT [FK_ProviderEndpointLicenseApprovalRequests_SentTo]
			
			ALTER TABLE [dbo].[ProviderEndpointLicenseClauses]  WITH CHECK ADD  CONSTRAINT [FK_ProviderEndpointLicenseClauses_CreatedBy] FOREIGN KEY([CreatedBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[ProviderEndpointLicenseClauses] CHECK CONSTRAINT [FK_ProviderEndpointLicenseClauses_CreatedBy]
			
			ALTER TABLE [dbo].[ProviderEndpointLicenseClauses]  WITH CHECK ADD  CONSTRAINT [FK_ProviderEndpointLicenseClauses_LicenseClauseID] FOREIGN KEY([LicenseClauseID])
			REFERENCES [dbo].[LicenseClauses] ([ID])
			
			ALTER TABLE [dbo].[ProviderEndpointLicenseClauses] CHECK CONSTRAINT [FK_ProviderEndpointLicenseClauses_LicenseClauseID]
			
			ALTER TABLE [dbo].[ProviderEndpointLicenseClauses]  WITH CHECK ADD  CONSTRAINT [FK_ProviderEndpointLicenseClauses_ProviderEndpointLicenseID] FOREIGN KEY([ProviderEndpointLicenseID])
			REFERENCES [dbo].[ProviderEndpointLicenses] ([ID])
			
			ALTER TABLE [dbo].[ProviderEndpointLicenseClauses] CHECK CONSTRAINT [FK_ProviderEndpointLicenseClauses_ProviderEndpointLicenseID]
			
			ALTER TABLE [dbo].[ProviderEndpointLicenseClauses]  WITH CHECK ADD  CONSTRAINT [FK_ProviderEndpointLicenseClauses_UpdatedBy] FOREIGN KEY([UpdatedBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[ProviderEndpointLicenseClauses] CHECK CONSTRAINT [FK_ProviderEndpointLicenseClauses_UpdatedBy]
			
			ALTER TABLE [dbo].[ProviderEndpointLicenses]  WITH CHECK ADD  CONSTRAINT [FK_ProviderEndpointLicenses_ApprovedBy] FOREIGN KEY([ApprovedBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[ProviderEndpointLicenses] CHECK CONSTRAINT [FK_ProviderEndpointLicenses_ApprovedBy]
			
			ALTER TABLE [dbo].[ProviderEndpointLicenses]  WITH CHECK ADD  CONSTRAINT [FK_ProviderEndpointLicenses_CreatedBy] FOREIGN KEY([CreatedBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[ProviderEndpointLicenses] CHECK CONSTRAINT [FK_ProviderEndpointLicenses_CreatedBy]
			
			ALTER TABLE [dbo].[ProviderEndpointLicenses]  WITH CHECK ADD  CONSTRAINT [FK_ProviderEndpointLicenses_LicenseTemplateID] FOREIGN KEY([LicenseTemplateID])
			REFERENCES [dbo].[LicenseTemplates] ([ID])
			
			ALTER TABLE [dbo].[ProviderEndpointLicenses] CHECK CONSTRAINT [FK_ProviderEndpointLicenses_LicenseTemplateID]
			
			ALTER TABLE [dbo].[ProviderEndpointLicenses]  WITH CHECK ADD  CONSTRAINT [FK_ProviderEndpointLicenses_ProviderEndpointID] FOREIGN KEY([ProviderEndpointID])
			REFERENCES [dbo].[ProviderEndpoints] ([ID])
			
			ALTER TABLE [dbo].[ProviderEndpointLicenses] CHECK CONSTRAINT [FK_ProviderEndpointLicenses_ProviderEndpointID]
			
			ALTER TABLE [dbo].[ProviderEndpointLicenses]  WITH CHECK ADD  CONSTRAINT [FK_ProviderEndpointLicenses_PublishedBy] FOREIGN KEY([PublishedBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[ProviderEndpointLicenses] CHECK CONSTRAINT [FK_ProviderEndpointLicenses_PublishedBy]
			
			ALTER TABLE [dbo].[ProviderEndpointLicenses]  WITH CHECK ADD  CONSTRAINT [FK_ProviderEndpointLicenses_UpdatedBy] FOREIGN KEY([UpdatedBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[ProviderEndpointLicenses] CHECK CONSTRAINT [FK_ProviderEndpointLicenses_UpdatedBy]
			
			ALTER TABLE [dbo].[ProviderEndpoints]  WITH CHECK ADD  CONSTRAINT [FK_ProviderEndpoint_Applications] FOREIGN KEY([ApplicationID])
			REFERENCES [dbo].[Applications] ([ID])
			
			ALTER TABLE [dbo].[ProviderEndpoints] CHECK CONSTRAINT [FK_ProviderEndpoint_Applications]
			
			ALTER TABLE [dbo].[ProviderEndpoints]  WITH CHECK ADD  CONSTRAINT [FK_ProviderEndpoint_DataSchemas] FOREIGN KEY([DataSchemaID])
			REFERENCES [dbo].[DataSchemas] ([ID])
			
			ALTER TABLE [dbo].[ProviderEndpoints] CHECK CONSTRAINT [FK_ProviderEndpoint_DataSchemas]
			
			ALTER TABLE [dbo].[ProviderEndpoints]  WITH CHECK ADD  CONSTRAINT [FK_ProviderEndpoint_Users_CreatedBy] FOREIGN KEY([CreatedBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[ProviderEndpoints] CHECK CONSTRAINT [FK_ProviderEndpoint_Users_CreatedBy]
			
			ALTER TABLE [dbo].[ProviderEndpoints]  WITH CHECK ADD  CONSTRAINT [FK_ProviderEndpoint_Users_UpdatedBy] FOREIGN KEY([UpdatedBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[ProviderEndpoints] CHECK CONSTRAINT [FK_ProviderEndpoint_Users_UpdatedBy]
			
			ALTER TABLE [dbo].[SchemaFiles]  WITH CHECK ADD  CONSTRAINT [FK_SchemaFiles_DataSchemas] FOREIGN KEY([CreatedBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[SchemaFiles] CHECK CONSTRAINT [FK_SchemaFiles_DataSchemas]
			
			ALTER TABLE [dbo].[SchemaFiles]  WITH CHECK ADD  CONSTRAINT [FK_SchemaFiles_Users_CreatedBy] FOREIGN KEY([CreatedBy])
			REFERENCES [dbo].[Users] ([ID])
			
			ALTER TABLE [dbo].[SchemaFiles] CHECK CONSTRAINT [FK_SchemaFiles_Users_CreatedBy]
			
			ALTER TABLE [dbo].[Users]  WITH CHECK ADD  CONSTRAINT [FK_Users_Organizations] FOREIGN KEY([OrganizationID])
			REFERENCES [dbo].[Organizations] ([ID])
			
			ALTER TABLE [dbo].[Users] CHECK CONSTRAINT [FK_Users_Organizations]
			
		INSERT INTO [dbo].[UpdateHistory] ([Name],[Date]) VALUES (@issueNumber, GETDATE())		
		COMMIT TRANSACTION
	END TRY
	BEGIN CATCH
		SELECT
				ERROR_NUMBER() as ErrorNumber,
				ERROR_MESSAGE() as ErrorMessage
	END CATCH
END	