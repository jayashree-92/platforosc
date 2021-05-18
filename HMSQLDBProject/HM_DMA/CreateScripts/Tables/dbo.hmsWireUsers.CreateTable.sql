IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsUsers')
BEGIN
	CREATE TABLE [dbo].[hmsUsers]
	(
		[hmsWireUserId] BIGINT NOT NULL IDENTITY(1,1),
		[hmLoginId] INT NOT NULL,
		[LdapRole] VARCHAR(30) NOT NULL,
		[CreatedAt] DATETIME NOT NULL,
		[CreatedBy] INT NOT NULL,
		[IsApproved] BIT NOT NULL,
		[ApprovedAt] DATETIME NULL,
		[ApprovedBy] INT NULL,
		[AccountStatus] VARCHAR(200) NOT NULL,
		CONSTRAINT PK_hmsUsers PRIMARY KEY NONCLUSTERED ([hmsWireUserId]),
		CONSTRAINT UQ_hmsUsers_hmLoginId_LdapRole UNIQUE ([hmLoginId],[LdapRole])
	);
	
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('1971','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('5308','hm-wire-initiator','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('5382','hm-wire-initiator','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('1964','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('36141','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('3437','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('35860','hm-wire-initiator','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('35360','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('2215','hm-wire-initiator','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('5177','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('5023','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('4581','hm-wire-initiator','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('2333','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('5280','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('1704','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('5281','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('35783','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('4782','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('1703','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('36142','hm-wire-initiator','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('35057','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('35618','hm-wire-initiator','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('36143','hm-wire-initiator','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('35502','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('4670','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('35131','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('4980','hm-wire-initiator','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('35890','hm-wire-initiator','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('35177','hm-wire-initiator','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('35388','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('35910','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('4872','hm-wire-initiator','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('35159','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('1271','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('4545','hm-wire-initiator','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('5182','hm-wire-initiator','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('4798','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('5022','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('36078','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('5128','hm-wire-initiator','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('5062','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('35508','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('5147','hm-wire-initiator','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('1598','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('35019','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('35859','hm-wire-initiator','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('35941','hm-wire-initiator','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('5068','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('4626','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('5181','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('5064','hm-wire-initiator','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('2149','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('4970','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('36096','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('4518','hm-wire-approver','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('5344','hm-wire-initiator','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('5196','hm-wire-initiator','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('35743','hm-wire-initiator','2020-01-01','876','1','2020-01-01','2081','Active')
	INSERT INTO hmsUsers (hmLoginId,LdapRole,CreatedAt,CreatedBy,IsApproved,ApprovedAt,ApprovedBy,AccountStatus)  VALUES ('36123','hm-wire-initiator','2020-01-01','876','1','2020-01-01','2081','Active')


END
GO