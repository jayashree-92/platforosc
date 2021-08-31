using System.Linq;
using System.Text;
using System.Web.Routing;

namespace HM.Operations.Secure.Web.Utility
{
    public class Select2List
    {
        private readonly RouteValueDictionary dictionary;

        public Select2List(RouteValueDictionary attributes)
        {
            this.dictionary = attributes;
        }

        public string GetScript()
        {
            var options = dictionary.Keys.Where(x => x != "id").Select(attr => $"{attr}:'{dictionary[attr]}'");
            var optionsMap = $"{{{string.Join(",", options)}}}";
            var scriptBuilder = new StringBuilder();
            scriptBuilder.AppendFormat(@"<script type=""text/javascript"">
                                            $(document).ready(function () {{ $('#{0}').select2({1}); }});
                                        </script>", dictionary["id"], optionsMap);

            return scriptBuilder.ToString();
        }
    }
}