using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using Com.HedgeMark.Commons.Extensions;
using HedgeMark.Operations.Secure.DataModel;

namespace HedgeMark.Operations.Secure.Middleware
{
    public class SSITemplateManager
    {

        public const int BrokerTemplateTypeId = 2;
        public static List<onBoardingSSITemplate> GetAllBrokerSsiTemplates()
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.onBoardingSSITemplates
                    .Include(s => s.Beneficiary)
                    .Include(s => s.Intermediary)
                    .Include(s => s.UltimateBeneficiary).Where(template => template.TemplateTypeId == BrokerTemplateTypeId && !template.IsDeleted).ToList();
            }
        }


        public static Dictionary<long, string> GetAllSsiTemplates()
        {
            using (var context = new OperationsSecureContext())
            {
                return context.onBoardingSSITemplates.Where(f => !f.IsDeleted).ToDictionary(s => s.onBoardingSSITemplateId, s => s.TemplateName);
            }
        }

        public static List<onBoardingSSITemplate> GetAllSsiTemplates(int templateTypeId, long templateEntityId)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.onBoardingSSITemplates.Include(x => x.onBoardingSSITemplateDocuments).Where(template => template.TemplateTypeId == templateTypeId && template.TemplateEntityId == templateEntityId && !template.IsDeleted).ToList();
            }
        }


        public static onBoardingSSITemplate GetSsiTemplate(long templateId)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                var ssiTemplate = context.onBoardingSSITemplates
                    .Include(s => s.Beneficiary)
                    .Include(s => s.Intermediary)
                    .Include(s => s.UltimateBeneficiary)
                    .Include(x => x.onBoardingSSITemplateDocuments)
                    .Include(x => x.onBoardingAccountSSITemplateMaps)
                    .Include(x => x.hmsSSICallbacks)
                    .First(template => template.onBoardingSSITemplateId == templateId);

                return SetSSITemplateDefaults(ssiTemplate);
            }
        }

        public static onBoardingSSITemplate SetSSITemplateDefaults(onBoardingSSITemplate ssiTemplate)
        {
            if (ssiTemplate == null)
                return null;

            if (ssiTemplate.Beneficiary == null)
                ssiTemplate.Beneficiary = new onBoardingAccountBICorABA();
            if (ssiTemplate.Intermediary == null)
                ssiTemplate.Intermediary = new onBoardingAccountBICorABA();
            if (ssiTemplate.UltimateBeneficiary == null)
                ssiTemplate.UltimateBeneficiary = new onBoardingAccountBICorABA();
            if (ssiTemplate.onBoardingSSITemplateDocuments == null)
                ssiTemplate.onBoardingSSITemplateDocuments = new List<onBoardingSSITemplateDocument>();
            if (ssiTemplate.onBoardingAccountSSITemplateMaps == null)
                ssiTemplate.onBoardingAccountSSITemplateMaps = new List<onBoardingAccountSSITemplateMap>();

            //remove circular references
            ssiTemplate.Beneficiary.onBoardingSSITemplates = ssiTemplate.Beneficiary.onBoardingSSITemplates1 = ssiTemplate.Beneficiary.onBoardingSSITemplates2 = null;
            ssiTemplate.Intermediary.onBoardingSSITemplates = ssiTemplate.Intermediary.onBoardingSSITemplates1 = ssiTemplate.Intermediary.onBoardingSSITemplates2 = null;
            ssiTemplate.UltimateBeneficiary.onBoardingSSITemplates = ssiTemplate.UltimateBeneficiary.onBoardingSSITemplates1 = ssiTemplate.UltimateBeneficiary.onBoardingSSITemplates2 = null;
            ssiTemplate.onBoardingSSITemplateDocuments.ForEach(s => s.onBoardingSSITemplate = null);
            ssiTemplate.onBoardingAccountSSITemplateMaps.ForEach(s => { s.onBoardingSSITemplate = null; s.onBoardingAccount = null; });

            ssiTemplate.hmsWires = null;
            ssiTemplate.hmsSSICallbacks.ForEach(s => { s.onBoardingSSITemplate = null; });

            return ssiTemplate;
        }

        public static List<OnBoardingServiceProvider> GetAllServiceProviderList()
        {
            using (var context = new AdminContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.OnBoardingServiceProviders.ToList();
            }
        }

        public static void RemoveSsiTemplateMap(long ssiTemplateMapId)
        {
            using (var context = new OperationsSecureContext())
            {
                var ssiTemplateMap = context.onBoardingAccountSSITemplateMaps.First(x => x.onBoardingAccountSSITemplateMapId == ssiTemplateMapId);
                context.onBoardingAccountSSITemplateMaps.Remove(ssiTemplateMap);
                context.SaveChanges();
            }
        }

        public static List<OnBoardingSSITemplateAccountType> GetAllSsiTemplateAccountTypes(int? agreementTypeId)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.OnBoardingSSITemplateAccountTypes.Where(x => x.dmaAgreementTypeId == agreementTypeId).ToList();
            }
        }

        public static List<OnBoardingServiceProvider> GetAllSsiTemplateServiceProviders(string serviceProviderName)
        {
            using (var context = new AdminContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.OnBoardingServiceProviders.Where(x => x.ServiceProvider == serviceProviderName).ToList();
            }
        }

        public static long AddSsiTemplate(onBoardingSSITemplate ssiTemplate, string userName)
        {
            using (var context = new OperationsSecureContext())
            {
                ssiTemplate.dmaAgreementTypeId = !string.IsNullOrEmpty(ssiTemplate.ServiceProvider) ? 1 : ssiTemplate.dmaAgreementTypeId;

                if (ssiTemplate.onBoardingSSITemplateId == 0)
                {
                    ssiTemplate.CreatedAt = DateTime.Now;
                    ssiTemplate.CreatedBy = userName;
                    ssiTemplate.UpdatedAt = DateTime.Now;
                    ssiTemplate.UpdatedBy = userName;
                    ssiTemplate.SSITemplateStatus = "Saved As Draft";
                    ssiTemplate.LastUsedAt = DateTime.Now;
                }
                else
                {
                    ssiTemplate.UpdatedAt = DateTime.Now;
                    ssiTemplate.UpdatedBy = userName;
                    ssiTemplate.LastUsedAt = DateTime.Now;

                    if (ssiTemplate.onBoardingSSITemplateDocuments != null && ssiTemplate.onBoardingSSITemplateDocuments.Count > 0)
                    {
                        context.onBoardingSSITemplateDocuments.AddRange(ssiTemplate.onBoardingSSITemplateDocuments.Where(s => s.onBoardingSSITemplateDocumentId == 0));
                        //new Repository<onBoardingSSITemplateDocument>().BulkInsert(, dbSchemaName: "HMADMIN.");
                    }
                }
                ssiTemplate.Beneficiary = null;
                ssiTemplate.Intermediary = null;
                ssiTemplate.UltimateBeneficiary = null;
                context.onBoardingSSITemplates.AddOrUpdate(ssiTemplate);
                context.SaveChanges();
            }
            return ssiTemplate.onBoardingSSITemplateId;
        }

        public static void RemoveSsiTemplateDocument(long ssiTemplateId, string fileName)
        {
            using (var context = new OperationsSecureContext())
            {
                var document = context.onBoardingSSITemplateDocuments.FirstOrDefault(x => x.onBoardingSSITemplateId == ssiTemplateId && x.FileName == fileName);
                if (document == null) return;
                context.onBoardingSSITemplateDocuments.Remove(document);
                context.SaveChanges();
            }
        }
        public static hmsSSICallback GetCallbackData(long callbackId)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.hmsSSICallbacks.First(s => s.hmsSSICallbackId == callbackId);
            }
        }

        public static List<hmsSSICallback> GetSSICallbacks(long ssiTemplateId)
        {
            using (var context = new OperationsSecureContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return context.hmsSSICallbacks.Where(s => s.onBoardingSSITemplateId == ssiTemplateId).AsNoTracking().ToList();
            }
        }
        public static void AddOrUpdateCallback(hmsSSICallback callback)
        {
            using (var context = new OperationsSecureContext())
            {
                context.hmsSSICallbacks.AddOrUpdate(callback);
                context.SaveChanges();
            }
        }
    }
}
