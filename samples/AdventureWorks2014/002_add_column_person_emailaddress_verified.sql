-- add column with default constraint to pre-populate NOT NULL
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Person.EmailAddress') AND name = 'Verified')
ALTER TABLE Person.EmailAddress ADD Verified BIT NOT NULL CONSTRAINT DF_EmailAddress_Verified DEFAULT(0);

-- drop the added constraint
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[Person].[DF_EmailAddress_Verified]') AND type = 'D')
ALTER TABLE [Person].[EmailAddress] DROP CONSTRAINT [DF_EmailAddress_Verified];