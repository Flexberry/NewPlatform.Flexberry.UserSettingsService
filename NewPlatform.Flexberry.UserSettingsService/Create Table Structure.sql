CREATE TABLE [UserSetting] (

	 [primaryKey] UNIQUEIDENTIFIER NOT NULL,

	 [AppName] VARCHAR(256) NULL,

	 [UserName] VARCHAR(512) NULL,

	 [UserGuid] UNIQUEIDENTIFIER NULL,

	 [ModuleName] VARCHAR(1024) NULL,

	 [ModuleGuid] UNIQUEIDENTIFIER NULL,

	 [SettName] VARCHAR(256) NULL,

	 [SettGuid] UNIQUEIDENTIFIER NULL,

	 [SettLastAccessTime] DATETIME NULL,

	 [StrVal] VARCHAR(256) NULL,

	 [TxtVal] VARCHAR(MAX) NULL,

	 [IntVal] INT NULL,

	 [BoolVal] BIT NULL,

	 [GuidVal] UNIQUEIDENTIFIER NULL,

	 [DecimalVal] DECIMAL(20,10) NULL,

	 [DateTimeVal] DATETIME NULL,

	 PRIMARY KEY ([primaryKey]))

