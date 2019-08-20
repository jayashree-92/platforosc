---/// vendor id update in invoice


--select i.Vendor, i.VendorId, sp.ServiceProvider, sp.OnBoardingSSITemplateServiceProviderId, * from DMA_HM.dbo.dmaInvoiceReport i -- 122
--join dbo.OnBoardingSSITemplateServiceProvider sp on i.Vendor = sp.ServiceProvider
--where i.Vendor != '' and VendorId = 0

update i set VendorId = sp.OnBoardingSSITemplateServiceProviderId from DMA_HM.dbo.dmaInvoiceReport i -- 122
join dbo.OnBoardingSSITemplateServiceProvider sp on i.Vendor = sp.ServiceProvider
where i.Vendor != '' and VendorId = 0