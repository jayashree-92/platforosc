using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Com.HedgeMark.Commons.Extensions;
using HM.Operations.Secure.DataModel;
using HM.Operations.Secure.Middleware;
using HM.Operations.Secure.Middleware.Models;

namespace HM.Operations.Secure.Web.Controllers
{
    public class DashboardReportController : WireUserBaseController
    {
        public JsonResult GetAllTemplates()
        {
            var favoriteId = GetPreferenceInSession(PreferencesManager.FavoriteDashboardTemplateForWires).ToLong();
            using (var context = new OperationsSecureContext())
            {
                var templates = context.hmsDashboardTemplates.Where(s => !s.IsDeleted)
                    .Select(s => new { id = s.hmsDashboardTemplateId, text = s.TemplateName, selected = favoriteId > 0 && s.hmsDashboardTemplateId == favoriteId }).OrderBy(s => s.text).ToList();

                return Json(new { templates, favoriteId });
            }
        }

        public void SaveFavoriteTemplate(long templateId)
        {
            SavePreferenceInSession(PreferencesManager.FavoriteDashboardTemplateForWires, templateId.ToString());
        }

        public JsonResult GetAllReportPreferences()
        {
            return Json(WiresDashboardController.GetWirePreferences(AuthorizedDMAFundData, AuthorizedSessionData.IsPrivilegedUser));
        }

        public JsonResult GetPreferences(long templateId)
        {
            var preferences = DashboardReportManager.GetPreferences(templateId);
            return Json(preferences.Select(s => new DashboardReport.Preferences()
            {
                Preference = ((DashboardReport.PreferenceCode)s.PreferenceCode).ToString(),
                SelectedIds = s.Preferences.Split(',').ToList()
            }));
        }

        public long SaveTemplateAndPreferences(string templateName, long templateId, Dictionary<string, string> preferences, bool shouldUnApproveAllExternalTo)
        {
            hmsDashboardTemplate template;
            using (var context = new OperationsSecureContext())
            {
                template = templateId > 0
                    ? context.hmsDashboardTemplates.Include(s=>s.hmsDashboardSchedules).Include(s => s.hmsDashboardPreferences).Single(s => s.hmsDashboardTemplateId == templateId)
                    : context.hmsDashboardTemplates.Include(s => s.hmsDashboardPreferences).FirstOrDefault(s => s.TemplateName == templateName);

                if (template == null)
                {
                    template = context.hmsDashboardTemplates.Add(new hmsDashboardTemplate()
                    {
                        TemplateName = templateName,
                        RecCreatedById = UserId,
                        RecCreatedDt = DateTime.Now,
                        IsDeleted = false,
                    });

                    context.SaveChanges();
                }
                else
                {
                    if (template.IsDeleted)
                        template.IsDeleted = false;

                    template.TemplateName = templateName;
                    if (shouldUnApproveAllExternalTo)
                    {
                        foreach (var dashboardSchedule in template.hmsDashboardSchedules)
                        {
                            if (!string.IsNullOrEmpty(dashboardSchedule.hmsSchedule.ExternalToApproved))
                            {
                                dashboardSchedule.hmsSchedule.ExternalTo = $"{dashboardSchedule.hmsSchedule.ExternalTo};{dashboardSchedule.hmsSchedule.ExternalToApproved}";
                                dashboardSchedule.hmsSchedule.ExternalToApproved = "";
                                dashboardSchedule.hmsSchedule.ExternalToWorkflowCode = 0;
                                dashboardSchedule.hmsSchedule.ExternalToModifiedBy = null;
                                dashboardSchedule.hmsSchedule.ExternalToModifiedAt = null;
                                dashboardSchedule.hmsSchedule.LastModifiedBy = UserId;
                                dashboardSchedule.hmsSchedule.LastUpdatedAt = DateTime.Now;
                            }
                        }
                    }
                    context.hmsDashboardPreferences.RemoveRange(template.hmsDashboardPreferences.ToList());
                    context.SaveChanges();
                }


                var allPreferences = (from preference in preferences
                                      let code = (DashboardReport.PreferenceCode)Enum.Parse(typeof(DashboardReport.PreferenceCode), preference.Key)
                                      select new hmsDashboardPreference()
                                      {
                                          RecCreatedDt = DateTime.Now,
                                          PreferenceCode = (int)code,
                                          hmsDashboardTemplateId = template.hmsDashboardTemplateId,
                                          RecCreatedById = UserId,
                                          Preferences = preference.Value
                                      }).ToList();

                context.hmsDashboardPreferences.AddRange(allPreferences);
                context.SaveChanges();

            }
            if (shouldUnApproveAllExternalTo)
                LogUnApproveExternalToAudit(templateName);

            return template.hmsDashboardTemplateId;
        }
        private void LogUnApproveExternalToAudit(string templateName)
        {
            var logData = new AuditLogData
            {
                Action = "Edited",
                ModuleName = "Wire Dashboard",
                TemplateName = templateName,
                changes = new[]
                {
                    new[]
                    {
                        "-", "ExternalToApproved",
                        "",
                        "Approval reverted for schedules with external mail ids",
                    }
},
            };

            AuditManager.LogAudit(logData, UserName);
        }
        public void DeleteTemplate(long templateId)
        {
            using (var context = new OperationsSecureContext())
            {
                var template = context.hmsDashboardTemplates.Include(s => s.hmsDashboardPreferences).Single(s => s.hmsDashboardTemplateId == templateId);
                template.IsDeleted = true;
                context.SaveChanges();
            }
        }
    }
}