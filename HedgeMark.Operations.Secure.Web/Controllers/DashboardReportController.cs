using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Com.HedgeMark.Commons.Extensions;
using HedgeMark.Operations.Secure.DataModel;
using HedgeMark.Operations.Secure.Middleware;
using HedgeMark.Operations.Secure.Middleware.Models;

namespace HMOSecureWeb.Controllers
{
    public class DashboardReportController : BaseController
    {
        public JsonResult GetAllTemplates()
        {
            var favoriteId = GetPreferenceInSession(PreferencesManager.FavoriteDashboardTemplateForReportId).ToLong();
            using (var context = new OperationsSecureContext())
            {
                var templates = context.hmsDashboardTemplates.Where(s => !s.IsDeleted)
                    .Select(s => new { id = s.hmsDashboardTemplateId, text = s.TemplateName, selected = favoriteId > 0 && s.hmsDashboardTemplateId == favoriteId }).OrderBy(s => s.text).ToList();

                return Json(new { templates, favoriteId });
            }
        }

        public void SaveFavoriteTemplate(long templateId)
        {
            SavePreferenceInSession(PreferencesManager.FavoriteDashboardTemplateForReportId, templateId.ToString());
        }

        public JsonResult GetAllReportPreferences()
        {
            return Json("");
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

        public long SaveTemplateAndPreferences(string templateName, long templateId, Dictionary<string, string> preferences)
        {
            hmsDashboardTemplate template;
            using (var context = new OperationsSecureContext())
            {
                template = templateId > 0
                    ? context.hmsDashboardTemplates.Include(s => s.hmsDashboardPreferences).Single(s => s.hmsDashboardTemplateId == templateId)
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

            return template.hmsDashboardTemplateId;
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