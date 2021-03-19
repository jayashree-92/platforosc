using System.Collections.Generic;
using System.Linq;
using HedgeMark.Operations.Secure.DataModel;

namespace HedgeMark.Operations.Secure.Middleware
{
    public class DashboardReportManager
    {
        public static List<hmsDashboardPreference> GetPreferences(long templateId)
        {
            using (var context = new OperationsSecureContext())
            {
                return context.hmsDashboardPreferences.Where(s => s.hmsDashboardTemplateId == templateId).ToList();
            }
        }

        public static hmsDashboardTemplate GetTemplate(long templateId)
        {
            using (var context = new OperationsSecureContext())
            {
                return context.hmsDashboardTemplates.FirstOrDefault(s => s.hmsDashboardTemplateId == templateId && !s.IsDeleted);
            }
        }
    }
}
