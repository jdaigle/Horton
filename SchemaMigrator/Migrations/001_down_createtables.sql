/****** Object:  Table [dbo].[PersonalPayrollEntries]    Script Date: 07/02/2007 22:49:54 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PersonalPayrollEntries]') AND type in (N'U'))
DROP TABLE [dbo].[PersonalPayrollEntries]