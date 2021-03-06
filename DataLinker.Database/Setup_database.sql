USE [Prod]
GO
/****** Object:  Table [dbo].[ApplicationAuthentication]    Script Date: 2/12/2018 11:10:49 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
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
	[RegistrationEndpoint] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_ApplicationAuthentication] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Applications]    Script Date: 2/12/2018 11:10:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Applications](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[OrganizationID] [int] NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[IsProvider] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedBy] [int] NULL,
	[IsIntroducedAsIndustryGood] [bit] NOT NULL,
	[IsVerifiedAsIndustryGood] [bit] NULL,
	[PublicID] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Applications] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ApplicationTokens]    Script Date: 2/12/2018 11:10:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
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
GO
/****** Object:  Table [dbo].[AuditLogMessages]    Script Date: 2/12/2018 11:10:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AuditLogMessages](
	[AuditStream] [nvarchar](40) NOT NULL,
	[Sequence] [bigint] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[Action] [nvarchar](max) NOT NULL,
	[UserDetails] [nvarchar](max) NOT NULL,
	[DataPayload] [nvarchar](max) NOT NULL,
	[Signature] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_AuditLogMessages] PRIMARY KEY NONCLUSTERED 
(
	[AuditStream] ASC,
	[Sequence] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ConsumerRequests]    Script Date: 2/12/2018 11:10:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ConsumerRequests](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ProviderID] [int] NOT NULL,
	[ConsumerID] [int] NOT NULL,
	[DataSchemaID] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[ProcessedAt] [datetime2](7) NULL,
	[ProcessedBy] [int] NULL,
 CONSTRAINT [PK_ConsumerRequests] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DataSchemas]    Script Date: 2/12/2018 11:10:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
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
	[IsIndustryGood] [bit] NOT NULL,
	[IsAggregate] [bit] NOT NULL,
 CONSTRAINT [PK_DataSchemas] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LicenseAgreements]    Script Date: 2/12/2018 11:10:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LicenseAgreements](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ConsumerLicenseID] [int] NOT NULL,
	[ProviderLicenseID] [int] NOT NULL,
	[ProviderOrganizationID] [int] NOT NULL,
	[ConsumerOrganizationID] [int] NOT NULL,
	[DataSchemaID] [int] NOT NULL,
	[SoftwareStatement] [nvarchar](max) NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[ExpiresAt] [datetime2](7) NULL,
 CONSTRAINT [PK_LicenseAgreements] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LicenseApprovalRequests]    Script Date: 2/12/2018 11:10:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LicenseApprovalRequests](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[OrganizationLicenseID] [int] NOT NULL,
	[SentAt] [datetime2](7) NOT NULL,
	[SentBy] [int] NOT NULL,
	[SentTo] [int] NOT NULL,
	[ExpiresAt] [datetime2](7) NOT NULL,
	[accessToken] [nvarchar](255) NOT NULL,
	[Token] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_LicenseApprovalRequests] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LicenseClauses]    Script Date: 2/12/2018 11:10:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
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
GO
/****** Object:  Table [dbo].[LicenseClauseTemplates]    Script Date: 2/12/2018 11:10:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
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
GO
/****** Object:  Table [dbo].[LicenseMatches]    Script Date: 2/12/2018 11:10:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LicenseMatches](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ConsumerLicenseID] [int] NOT NULL,
	[ProviderLicenseID] [int] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[CreatedBy] [int] NOT NULL,
 CONSTRAINT [PK_LicenseMatches] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Licenses]    Script Date: 2/12/2018 11:10:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
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
GO
/****** Object:  Table [dbo].[LicenseSections]    Script Date: 2/12/2018 11:10:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
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
GO
/****** Object:  Table [dbo].[LicenseTemplates]    Script Date: 2/12/2018 11:10:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
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
GO
/****** Object:  Table [dbo].[OrganizationLicenseClauses]    Script Date: 2/12/2018 11:10:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrganizationLicenseClauses](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[OrganizationLicenseID] [int] NOT NULL,
	[LicenseClauseID] [int] NOT NULL,
	[ClauseData] [nvarchar](max) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedBy] [int] NULL,
 CONSTRAINT [PK_OrganizationLicenseClauses] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrganizationLicenses]    Script Date: 2/12/2018 11:10:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrganizationLicenses](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[ApprovedAt] [datetime2](7) NULL,
	[ApprovedBy] [int] NULL,
	[PublishedAt] [datetime2](7) NULL,
	[PublishedBy] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedBy] [int] NULL,
	[ProviderEndpointID] [int] NULL,
	[ApplicationID] [int] NOT NULL,
	[LicenseTemplateID] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[DataSchemaID] [int] NOT NULL,
 CONSTRAINT [PK_OrganizationLicenses] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Organizations]    Script Date: 2/12/2018 11:10:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Organizations](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[IsActive] [bit] NULL,
	[IsAgreeWithTerms] [bit] NULL,
	[Phone] [nvarchar](25) NULL,
	[Address] [nvarchar](255) NULL,
	[AdministrativeContact] [nvarchar](255) NULL,
	[AdministrativePhone] [nvarchar](50) NULL,
	[TermsOfService] [ntext] NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Organizations] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProviderEndpoints]    Script Date: 2/12/2018 11:10:51 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProviderEndpoints](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ApplicationID] [int] NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[IsIndustryGood] [bit] NOT NULL,
	[ServiceUri] [nvarchar](255) NOT NULL,
	[DataSchemaID] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedBy] [int] NULL,
	[NeedPersonalApproval] [bit] NOT NULL,
 CONSTRAINT [PK_ProviderEndpoint] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SchemaFiles]    Script Date: 2/12/2018 11:10:51 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SchemaFiles](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[DataSchemaID] [int] NOT NULL,
	[SchemaText] [ntext] NOT NULL,
	[IsCurrent] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[FileFormat] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_SchemaFiles] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SoftwareStatements]    Script Date: 2/12/2018 11:10:51 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SoftwareStatements](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ApplicationID] [int] NOT NULL,
	[Content] [nvarchar](max) NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[ExpiredAt] [datetime2](7) NULL,
	[ExpiredBy] [int] NULL,
 CONSTRAINT [PK_SoftwareStatements] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UpdateHistory]    Script Date: 2/12/2018 11:10:51 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UpdateHistory](
	[Name] [nvarchar](50) NOT NULL,
	[Date] [datetime] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 2/12/2018 11:10:51 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
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
GO
/****** Object:  Table [dbo].[VersionInformation]    Script Date: 2/12/2018 11:10:51 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VersionInformation](
	[VersionNumber] [int] NOT NULL,
	[UpgradedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_VersionInformation] PRIMARY KEY CLUSTERED 
(
	[VersionNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ApplicationAuthentication] ADD  DEFAULT ('') FOR [RegistrationEndpoint]
GO
ALTER TABLE [dbo].[Applications] ADD  DEFAULT (newid()) FOR [PublicID]
GO
ALTER TABLE [dbo].[DataSchemas] ADD  DEFAULT ((0)) FOR [Status]
GO
ALTER TABLE [dbo].[DataSchemas] ADD  DEFAULT ((0)) FOR [IsIndustryGood]
GO
ALTER TABLE [dbo].[DataSchemas] ADD  DEFAULT ((0)) FOR [IsAggregate]
GO
ALTER TABLE [dbo].[Organizations] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[ProviderEndpoints] ADD  DEFAULT ((0)) FOR [NeedPersonalApproval]
GO
ALTER TABLE [dbo].[VersionInformation] ADD  DEFAULT (getutcdate()) FOR [UpgradedAt]
GO
ALTER TABLE [dbo].[ApplicationAuthentication]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationAuthentication_Applications] FOREIGN KEY([ApplicationID])
REFERENCES [dbo].[Applications] ([ID])
GO
ALTER TABLE [dbo].[ApplicationAuthentication] CHECK CONSTRAINT [FK_ApplicationAuthentication_Applications]
GO
ALTER TABLE [dbo].[ApplicationAuthentication]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationAuthentication_Users_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[ApplicationAuthentication] CHECK CONSTRAINT [FK_ApplicationAuthentication_Users_CreatedBy]
GO
ALTER TABLE [dbo].[ApplicationAuthentication]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationAuthentication_Users_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[ApplicationAuthentication] CHECK CONSTRAINT [FK_ApplicationAuthentication_Users_UpdatedBy]
GO
ALTER TABLE [dbo].[Applications]  WITH CHECK ADD  CONSTRAINT [FK_Applications_Organizations] FOREIGN KEY([OrganizationID])
REFERENCES [dbo].[Organizations] ([ID])
GO
ALTER TABLE [dbo].[Applications] CHECK CONSTRAINT [FK_Applications_Organizations]
GO
ALTER TABLE [dbo].[Applications]  WITH CHECK ADD  CONSTRAINT [FK_Applications_Users_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Applications] CHECK CONSTRAINT [FK_Applications_Users_CreatedBy]
GO
ALTER TABLE [dbo].[Applications]  WITH CHECK ADD  CONSTRAINT [FK_Applications_Users_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Applications] CHECK CONSTRAINT [FK_Applications_Users_UpdatedBy]
GO
ALTER TABLE [dbo].[ApplicationTokens]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationTokens_Applications] FOREIGN KEY([ApplicationID])
REFERENCES [dbo].[Applications] ([ID])
GO
ALTER TABLE [dbo].[ApplicationTokens] CHECK CONSTRAINT [FK_ApplicationTokens_Applications]
GO
ALTER TABLE [dbo].[ApplicationTokens]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationTokens_Users_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[ApplicationTokens] CHECK CONSTRAINT [FK_ApplicationTokens_Users_CreatedBy]
GO
ALTER TABLE [dbo].[ApplicationTokens]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationTokens_Users_ExpiredBy] FOREIGN KEY([ExpiredBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[ApplicationTokens] CHECK CONSTRAINT [FK_ApplicationTokens_Users_ExpiredBy]
GO
ALTER TABLE [dbo].[ConsumerRequests]  WITH CHECK ADD  CONSTRAINT [FK_ConsumerRequests_ConsumerID] FOREIGN KEY([ConsumerID])
REFERENCES [dbo].[Applications] ([ID])
GO
ALTER TABLE [dbo].[ConsumerRequests] CHECK CONSTRAINT [FK_ConsumerRequests_ConsumerID]
GO
ALTER TABLE [dbo].[ConsumerRequests]  WITH CHECK ADD  CONSTRAINT [FK_ConsumerRequests_DataSchemaID] FOREIGN KEY([DataSchemaID])
REFERENCES [dbo].[DataSchemas] ([ID])
GO
ALTER TABLE [dbo].[ConsumerRequests] CHECK CONSTRAINT [FK_ConsumerRequests_DataSchemaID]
GO
ALTER TABLE [dbo].[ConsumerRequests]  WITH CHECK ADD  CONSTRAINT [FK_ConsumerRequests_ProcessedBy] FOREIGN KEY([ProcessedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[ConsumerRequests] CHECK CONSTRAINT [FK_ConsumerRequests_ProcessedBy]
GO
ALTER TABLE [dbo].[ConsumerRequests]  WITH CHECK ADD  CONSTRAINT [FK_ConsumerRequests_ProviderID] FOREIGN KEY([ProviderID])
REFERENCES [dbo].[Applications] ([ID])
GO
ALTER TABLE [dbo].[ConsumerRequests] CHECK CONSTRAINT [FK_ConsumerRequests_ProviderID]
GO
ALTER TABLE [dbo].[DataSchemas]  WITH CHECK ADD  CONSTRAINT [FK_DataSchemas_Users_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[DataSchemas] CHECK CONSTRAINT [FK_DataSchemas_Users_CreatedBy]
GO
ALTER TABLE [dbo].[DataSchemas]  WITH CHECK ADD  CONSTRAINT [FK_DataSchemas_Users_PublishedBy] FOREIGN KEY([PublishedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[DataSchemas] CHECK CONSTRAINT [FK_DataSchemas_Users_PublishedBy]
GO
ALTER TABLE [dbo].[DataSchemas]  WITH CHECK ADD  CONSTRAINT [FK_DataSchemas_Users_RetractedBy] FOREIGN KEY([RetractedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[DataSchemas] CHECK CONSTRAINT [FK_DataSchemas_Users_RetractedBy]
GO
ALTER TABLE [dbo].[DataSchemas]  WITH CHECK ADD  CONSTRAINT [FK_DataSchemas_Users_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[DataSchemas] CHECK CONSTRAINT [FK_DataSchemas_Users_UpdatedBy]
GO
ALTER TABLE [dbo].[LicenseAgreements]  WITH CHECK ADD  CONSTRAINT [FK_LicenseAgreements_ConsumerLicenseID] FOREIGN KEY([ConsumerLicenseID])
REFERENCES [dbo].[OrganizationLicenses] ([ID])
GO
ALTER TABLE [dbo].[LicenseAgreements] CHECK CONSTRAINT [FK_LicenseAgreements_ConsumerLicenseID]
GO
ALTER TABLE [dbo].[LicenseAgreements]  WITH CHECK ADD  CONSTRAINT [FK_LicenseAgreements_ConsumerOrganizationID] FOREIGN KEY([ConsumerOrganizationID])
REFERENCES [dbo].[Organizations] ([ID])
GO
ALTER TABLE [dbo].[LicenseAgreements] CHECK CONSTRAINT [FK_LicenseAgreements_ConsumerOrganizationID]
GO
ALTER TABLE [dbo].[LicenseAgreements]  WITH CHECK ADD  CONSTRAINT [FK_LicenseAgreements_DataSchemaID] FOREIGN KEY([DataSchemaID])
REFERENCES [dbo].[DataSchemas] ([ID])
GO
ALTER TABLE [dbo].[LicenseAgreements] CHECK CONSTRAINT [FK_LicenseAgreements_DataSchemaID]
GO
ALTER TABLE [dbo].[LicenseAgreements]  WITH CHECK ADD  CONSTRAINT [FK_LicenseAgreements_ProviderLicenseID] FOREIGN KEY([ProviderLicenseID])
REFERENCES [dbo].[OrganizationLicenses] ([ID])
GO
ALTER TABLE [dbo].[LicenseAgreements] CHECK CONSTRAINT [FK_LicenseAgreements_ProviderLicenseID]
GO
ALTER TABLE [dbo].[LicenseAgreements]  WITH CHECK ADD  CONSTRAINT [FK_LicenseAgreements_ProviderOrganizationID] FOREIGN KEY([ProviderOrganizationID])
REFERENCES [dbo].[Organizations] ([ID])
GO
ALTER TABLE [dbo].[LicenseAgreements] CHECK CONSTRAINT [FK_LicenseAgreements_ProviderOrganizationID]
GO
ALTER TABLE [dbo].[LicenseApprovalRequests]  WITH CHECK ADD  CONSTRAINT [FK_LicenseApprovalRequests_OrganizationLicenseID] FOREIGN KEY([OrganizationLicenseID])
REFERENCES [dbo].[OrganizationLicenses] ([ID])
GO
ALTER TABLE [dbo].[LicenseApprovalRequests] CHECK CONSTRAINT [FK_LicenseApprovalRequests_OrganizationLicenseID]
GO
ALTER TABLE [dbo].[LicenseApprovalRequests]  WITH CHECK ADD  CONSTRAINT [FK_LicenseApprovalRequests_SentBy] FOREIGN KEY([SentBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[LicenseApprovalRequests] CHECK CONSTRAINT [FK_LicenseApprovalRequests_SentBy]
GO
ALTER TABLE [dbo].[LicenseApprovalRequests]  WITH CHECK ADD  CONSTRAINT [FK_LicenseApprovalRequests_SentTo] FOREIGN KEY([SentTo])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[LicenseApprovalRequests] CHECK CONSTRAINT [FK_LicenseApprovalRequests_SentTo]
GO
ALTER TABLE [dbo].[LicenseClauses]  WITH CHECK ADD  CONSTRAINT [FK_LicenseClauses_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[LicenseClauses] CHECK CONSTRAINT [FK_LicenseClauses_CreatedBy]
GO
ALTER TABLE [dbo].[LicenseClauses]  WITH CHECK ADD  CONSTRAINT [FK_LicenseClauses_LicenseSectionID] FOREIGN KEY([LicenseSectionID])
REFERENCES [dbo].[LicenseSections] ([ID])
GO
ALTER TABLE [dbo].[LicenseClauses] CHECK CONSTRAINT [FK_LicenseClauses_LicenseSectionID]
GO
ALTER TABLE [dbo].[LicenseClauses]  WITH CHECK ADD  CONSTRAINT [FK_LicenseClauses_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[LicenseClauses] CHECK CONSTRAINT [FK_LicenseClauses_UpdatedBy]
GO
ALTER TABLE [dbo].[LicenseClauseTemplates]  WITH CHECK ADD  CONSTRAINT [FK_LicenseClauseTemplates_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[LicenseClauseTemplates] CHECK CONSTRAINT [FK_LicenseClauseTemplates_CreatedBy]
GO
ALTER TABLE [dbo].[LicenseClauseTemplates]  WITH CHECK ADD  CONSTRAINT [FK_LicenseClauseTemplates_LicenseClauseID] FOREIGN KEY([LicenseClauseID])
REFERENCES [dbo].[LicenseClauses] ([ID])
GO
ALTER TABLE [dbo].[LicenseClauseTemplates] CHECK CONSTRAINT [FK_LicenseClauseTemplates_LicenseClauseID]
GO
ALTER TABLE [dbo].[LicenseClauseTemplates]  WITH CHECK ADD  CONSTRAINT [FK_LicenseClauseTemplates_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[LicenseClauseTemplates] CHECK CONSTRAINT [FK_LicenseClauseTemplates_UpdatedBy]
GO
ALTER TABLE [dbo].[LicenseMatches]  WITH CHECK ADD  CONSTRAINT [FK_LicenseMatches_ConsumerLicenseID] FOREIGN KEY([ConsumerLicenseID])
REFERENCES [dbo].[OrganizationLicenses] ([ID])
GO
ALTER TABLE [dbo].[LicenseMatches] CHECK CONSTRAINT [FK_LicenseMatches_ConsumerLicenseID]
GO
ALTER TABLE [dbo].[LicenseMatches]  WITH CHECK ADD  CONSTRAINT [FK_LicenseMatches_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[LicenseMatches] CHECK CONSTRAINT [FK_LicenseMatches_CreatedBy]
GO
ALTER TABLE [dbo].[LicenseMatches]  WITH CHECK ADD  CONSTRAINT [FK_LicenseMatches_ProviderLicenseID] FOREIGN KEY([ProviderLicenseID])
REFERENCES [dbo].[OrganizationLicenses] ([ID])
GO
ALTER TABLE [dbo].[LicenseMatches] CHECK CONSTRAINT [FK_LicenseMatches_ProviderLicenseID]
GO
ALTER TABLE [dbo].[Licenses]  WITH CHECK ADD  CONSTRAINT [FK_Licenses_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Licenses] CHECK CONSTRAINT [FK_Licenses_CreatedBy]
GO
ALTER TABLE [dbo].[Licenses]  WITH CHECK ADD  CONSTRAINT [FK_Licenses_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Licenses] CHECK CONSTRAINT [FK_Licenses_UpdatedBy]
GO
ALTER TABLE [dbo].[LicenseSections]  WITH CHECK ADD  CONSTRAINT [FK_LicenseSections_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[LicenseSections] CHECK CONSTRAINT [FK_LicenseSections_CreatedBy]
GO
ALTER TABLE [dbo].[LicenseSections]  WITH CHECK ADD  CONSTRAINT [FK_LicenseSections_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[LicenseSections] CHECK CONSTRAINT [FK_LicenseSections_UpdatedBy]
GO
ALTER TABLE [dbo].[LicenseTemplates]  WITH CHECK ADD  CONSTRAINT [FK_LicenseTemplates_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[LicenseTemplates] CHECK CONSTRAINT [FK_LicenseTemplates_CreatedBy]
GO
ALTER TABLE [dbo].[LicenseTemplates]  WITH CHECK ADD  CONSTRAINT [FK_LicenseTemplates_LicenseID] FOREIGN KEY([LicenseID])
REFERENCES [dbo].[Licenses] ([ID])
GO
ALTER TABLE [dbo].[LicenseTemplates] CHECK CONSTRAINT [FK_LicenseTemplates_LicenseID]
GO
ALTER TABLE [dbo].[LicenseTemplates]  WITH CHECK ADD  CONSTRAINT [FK_LicenseTemplates_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[LicenseTemplates] CHECK CONSTRAINT [FK_LicenseTemplates_UpdatedBy]
GO
ALTER TABLE [dbo].[OrganizationLicenseClauses]  WITH CHECK ADD  CONSTRAINT [FK_OrganizationLicenseClauses_LicenseClauseID] FOREIGN KEY([LicenseClauseID])
REFERENCES [dbo].[LicenseClauses] ([ID])
GO
ALTER TABLE [dbo].[OrganizationLicenseClauses] CHECK CONSTRAINT [FK_OrganizationLicenseClauses_LicenseClauseID]
GO
ALTER TABLE [dbo].[OrganizationLicenseClauses]  WITH CHECK ADD  CONSTRAINT [FK_OrganizationLicenseClauses_OrganizationLicenseID] FOREIGN KEY([OrganizationLicenseID])
REFERENCES [dbo].[OrganizationLicenses] ([ID])
GO
ALTER TABLE [dbo].[OrganizationLicenseClauses] CHECK CONSTRAINT [FK_OrganizationLicenseClauses_OrganizationLicenseID]
GO
ALTER TABLE [dbo].[OrganizationLicenseClauses]  WITH CHECK ADD  CONSTRAINT [FK_OrganizationLicenseClauses_SentBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[OrganizationLicenseClauses] CHECK CONSTRAINT [FK_OrganizationLicenseClauses_SentBy]
GO
ALTER TABLE [dbo].[OrganizationLicenseClauses]  WITH CHECK ADD  CONSTRAINT [FK_OrganizationLicenseClauses_SentTo] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[OrganizationLicenseClauses] CHECK CONSTRAINT [FK_OrganizationLicenseClauses_SentTo]
GO
ALTER TABLE [dbo].[OrganizationLicenses]  WITH CHECK ADD  CONSTRAINT [FK_OrganizationLicenses_ApplicationID] FOREIGN KEY([ApplicationID])
REFERENCES [dbo].[Applications] ([ID])
GO
ALTER TABLE [dbo].[OrganizationLicenses] CHECK CONSTRAINT [FK_OrganizationLicenses_ApplicationID]
GO
ALTER TABLE [dbo].[OrganizationLicenses]  WITH CHECK ADD  CONSTRAINT [FK_OrganizationLicenses_ApprovedBy] FOREIGN KEY([ApprovedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[OrganizationLicenses] CHECK CONSTRAINT [FK_OrganizationLicenses_ApprovedBy]
GO
ALTER TABLE [dbo].[OrganizationLicenses]  WITH CHECK ADD  CONSTRAINT [FK_OrganizationLicenses_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[OrganizationLicenses] CHECK CONSTRAINT [FK_OrganizationLicenses_CreatedBy]
GO
ALTER TABLE [dbo].[OrganizationLicenses]  WITH CHECK ADD  CONSTRAINT [FK_OrganizationLicenses_DataSchemaID] FOREIGN KEY([DataSchemaID])
REFERENCES [dbo].[DataSchemas] ([ID])
GO
ALTER TABLE [dbo].[OrganizationLicenses] CHECK CONSTRAINT [FK_OrganizationLicenses_DataSchemaID]
GO
ALTER TABLE [dbo].[OrganizationLicenses]  WITH CHECK ADD  CONSTRAINT [FK_OrganizationLicenses_LicenseTemplateID] FOREIGN KEY([LicenseTemplateID])
REFERENCES [dbo].[LicenseTemplates] ([ID])
GO
ALTER TABLE [dbo].[OrganizationLicenses] CHECK CONSTRAINT [FK_OrganizationLicenses_LicenseTemplateID]
GO
ALTER TABLE [dbo].[OrganizationLicenses]  WITH CHECK ADD  CONSTRAINT [FK_OrganizationLicenses_PublishedBy] FOREIGN KEY([PublishedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[OrganizationLicenses] CHECK CONSTRAINT [FK_OrganizationLicenses_PublishedBy]
GO
ALTER TABLE [dbo].[OrganizationLicenses]  WITH CHECK ADD  CONSTRAINT [FK_OrganizationLicenses_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[OrganizationLicenses] CHECK CONSTRAINT [FK_OrganizationLicenses_UpdatedBy]
GO
ALTER TABLE [dbo].[ProviderEndpoints]  WITH CHECK ADD  CONSTRAINT [FK_ProviderEndpoint_Applications] FOREIGN KEY([ApplicationID])
REFERENCES [dbo].[Applications] ([ID])
GO
ALTER TABLE [dbo].[ProviderEndpoints] CHECK CONSTRAINT [FK_ProviderEndpoint_Applications]
GO
ALTER TABLE [dbo].[ProviderEndpoints]  WITH CHECK ADD  CONSTRAINT [FK_ProviderEndpoint_DataSchemas] FOREIGN KEY([DataSchemaID])
REFERENCES [dbo].[DataSchemas] ([ID])
GO
ALTER TABLE [dbo].[ProviderEndpoints] CHECK CONSTRAINT [FK_ProviderEndpoint_DataSchemas]
GO
ALTER TABLE [dbo].[ProviderEndpoints]  WITH CHECK ADD  CONSTRAINT [FK_ProviderEndpoint_Users_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[ProviderEndpoints] CHECK CONSTRAINT [FK_ProviderEndpoint_Users_CreatedBy]
GO
ALTER TABLE [dbo].[ProviderEndpoints]  WITH CHECK ADD  CONSTRAINT [FK_ProviderEndpoint_Users_UpdatedBy] FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[ProviderEndpoints] CHECK CONSTRAINT [FK_ProviderEndpoint_Users_UpdatedBy]
GO
ALTER TABLE [dbo].[SchemaFiles]  WITH CHECK ADD  CONSTRAINT [FK_SchemaFiles_DataSchemas] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[SchemaFiles] CHECK CONSTRAINT [FK_SchemaFiles_DataSchemas]
GO
ALTER TABLE [dbo].[SchemaFiles]  WITH CHECK ADD  CONSTRAINT [FK_SchemaFiles_Users_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[SchemaFiles] CHECK CONSTRAINT [FK_SchemaFiles_Users_CreatedBy]
GO
ALTER TABLE [dbo].[SoftwareStatements]  WITH CHECK ADD  CONSTRAINT [FK_SoftwareStatements_ApplicationID] FOREIGN KEY([ApplicationID])
REFERENCES [dbo].[Applications] ([ID])
GO
ALTER TABLE [dbo].[SoftwareStatements] CHECK CONSTRAINT [FK_SoftwareStatements_ApplicationID]
GO
ALTER TABLE [dbo].[SoftwareStatements]  WITH CHECK ADD  CONSTRAINT [FK_SoftwareStatements_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[SoftwareStatements] CHECK CONSTRAINT [FK_SoftwareStatements_CreatedBy]
GO
ALTER TABLE [dbo].[SoftwareStatements]  WITH CHECK ADD  CONSTRAINT [FK_SoftwareStatements_ExpiredBy] FOREIGN KEY([ExpiredBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[SoftwareStatements] CHECK CONSTRAINT [FK_SoftwareStatements_ExpiredBy]
GO
ALTER TABLE [dbo].[Users]  WITH CHECK ADD  CONSTRAINT [FK_Users_Organizations] FOREIGN KEY([OrganizationID])
REFERENCES [dbo].[Organizations] ([ID])
GO
ALTER TABLE [dbo].[Users] CHECK CONSTRAINT [FK_Users_Organizations]
GO
