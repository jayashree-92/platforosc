USE HM_WIRES;

IF EXISTS (SELECT * FROM SYSOBJECTS WHERE type = 'V' AND name = 'vw_FundAccounts')
BEGIN
DROP VIEW [dbo].[vw_FundAccounts]
END