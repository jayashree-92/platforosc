using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace HM.Operations.Secure.Web.Utility
{
    public static class HtmlExtensions
    {
        private class SelectItem
        {
            public string OptionValue { get; set; }
            public string OptionText { get; set; }
        }


        public static MvcHtmlString Select2ListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, List<TProperty>>> expression,
                                                                      IEnumerable<TProperty> selectList, Func<TProperty, string> keySelector, Func<TProperty, string> valueSelector, string optionLabel, object htmlAttributes)
        {
            var page = (WebViewPage)htmlHelper.ViewDataContainer;
            var scriptBuilder = page.Context.Items["AdditionalScripts"] as StringBuilder ?? new StringBuilder();

            var select2List = new StringBuilder();
            var htmlAttributesDictionary = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            var dropDownList = htmlHelper.DropDownListFor(expression,
                                                          new SelectList(selectList.Select(item => new SelectItem { OptionText = keySelector(item), OptionValue = valueSelector(item) }),
                                                                         "OptionValue", "OptionText"), optionLabel, htmlAttributesDictionary);
            select2List.Append(dropDownList.ToHtmlString());
            scriptBuilder.AppendLine(new Select2List(htmlAttributesDictionary).GetScript());
            page.Context.Items["AdditionalScripts"] = scriptBuilder;
            return MvcHtmlString.Create(select2List.ToString());
        }

        public static MvcHtmlString InjectAdditionalScripts(this WebViewPage webPage)
        {
            return new MvcHtmlString((webPage.Context.Items["AdditionalScripts"] as StringBuilder ?? new StringBuilder()).ToString());
        }

        public static MvcHtmlString DropDownGroupByList(string name, Dictionary<string, List<SelectListItem>> selectListGrouped, object htmlAttributes)
        {
            return DropDownGroupByList(name, selectListGrouped, new RouteValueDictionary(htmlAttributes));
        }

        public static MvcHtmlString DropDownGroupByList(string name, Dictionary<string, List<SelectListItem>> selectListGrouped, IDictionary<string, object> htmlAttributes)
        {
            var select = new TagBuilder("select");
            select.Attributes.Add("name", name);
            select.MergeAttributes(htmlAttributes);
            var optgroups = new StringBuilder();

            foreach (var group in selectListGrouped.Keys)
            {
                var optgroup = new TagBuilder("optgroup");
                optgroup.Attributes.Add("label", group);

                var options = new StringBuilder();

                foreach (var item in selectListGrouped[group])
                {
                    var option = new TagBuilder("option");

                    option.Attributes.Add("value", item.Value);
                    option.SetInnerText(item.Text);

                    if (item.Selected)
                    {
                        option.Attributes.Add("selected", "selected");
                    }

                    options.Append(option.ToString(TagRenderMode.Normal));
                }

                optgroup.InnerHtml = options.ToString();
                optgroups.Append(optgroup.ToString(TagRenderMode.Normal));
            }

            select.InnerHtml = optgroups.ToString();
            return MvcHtmlString.Create(select.ToString(TagRenderMode.Normal));
        }
    }
}