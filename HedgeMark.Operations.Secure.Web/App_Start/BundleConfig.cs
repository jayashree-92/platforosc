using System.Web.Optimization;

namespace HMOSecureWeb
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include("~/Scripts/jquery-3.3.1.js"));
            bundles.Add(new ScriptBundle("~/bundles/jquery-ui").Include("~/Scripts/jquery-ui-1.12.1.js"));


            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
                "~/Scripts/angular.js",
                "~/Scripts/angular-animate.js",
                "~/Scripts/angular-aria.js",
                "~/Scripts/angular-cookies.js",
                "~/Scripts/angular-loader.js",
                "~/Scripts/angular-messages.js",
                "~/Scripts/angular-message-format.js",
                "~/Scripts/angular-mocks.js",
                "~/Scripts/angular-parse-ext.js",
                "~/Scripts/angular-resource.js",
                "~/Scripts/angular-sanitize.js",
            //"~/Scripts/angular-route.js",
            //"~/Scripts/angular-sanitize.js",
            //"~/Scripts/angular-scenario.js",
            //"~/Scripts/angular-touch.js"

            //"~/Scripts/detect-element-resize.js",
            //"~/Scripts/tree.jquery.js",
            "~/Scripts/jquery.mark.js"
            //"~/Scripts/jquery.resize.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/datatable").Include(
                "~/Scripts/DataTables/jquery.dataTables.js",
                "~/Scripts/DataTables/dataTables.bootstrap.js",
                "~/Scripts/DataTables/dataTables.fixedColumns.js",
                "~/Scripts/DataTables/dataTables.fixedHeader.js",
                "~/Scripts/DataTables/dataTables.buttons.js",
                "~/Scripts/DataTables/buttons.bootstrap.js",
                "~/Scripts/DataTables/buttons.colVis.js",
                "~/Scripts/DataTables/buttons.print.js",
                "~/Scripts/DataTables/buttons.html5.js",
                "~/Scripts/DataTables/buttons.flash.js",
                "~/Scripts/DataTables/dataTables.scroller.js",
                "~/Scripts/DataTables/dataTables.select.js",
                "~/Scripts/DataTables/dataTables-responsive.js",
                "~/Scripts/DataTables/dataTables.rowReorder.js",
                "~/Scripts/DataTables/dataTables.colReorder.js",
                "~/Scripts/DataTables/dataTables.keyTable.js",
                "~/Scripts/DataTables/datatables.mark.js",
                "~/Scripts/DataTables/dataTables.searchPanes.js",
                //"~/Scripts/DataTables/searchPanes.bootstrap.js",
                "~/Scripts/dataTables-extentions.js"));



            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.js",
                "~/Scripts/bootstrap-notify.js",
                "~/Scripts/bootstrap-datepicker.js",
                "~/Scripts/bootstrap3-editable/js/bootstrap-editable.js",
                "~/Scripts/bootstrap3-typeahead.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/others").Include(
                "~/Scripts/select2.js",
                "~/Scripts/dropzone/dropzone.js",
                "~/Scripts/moment.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap-addon").Include(
                "~/Scripts/bootbox.js",
                "~/Scripts/daterangepicker.js",
                //"~/Scripts/clockpicker-gh-pages/dist/bootstrap-clockpicker.js",
                "~/Scripts/bootstrap-toggle.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/plugins").Include(
                "~/Scripts/data.js",
                "~/Scripts/Plugins/NumericCellEditor.js",
                "~/Scripts/Plugins/ContextMenu.js",
                "~/Scripts/Plugins/SideNavigationMenu.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/layout").Include(
                "~/Scripts/OpsSecure/OpsSecureAngularFactory.js",
                "~/Scripts/OpsSecure/layout.js"
            ));


            bundles.Add(new StyleBundle("~/Content/bundle")
                .Include("~/Content/bootstrap-bny-3.css", new CssRewriteUrlTransform())
                .Include("~/Content/bootstrap-bny-nexen.css", new CssRewriteUrlTransform())
                .Include("~/Content/bootstrap-extensions.css", new CssRewriteUrlTransform())
                .Include("~/Content/css/select2.css", new CssRewriteUrlTransform())
                .Include("~/Content/select2-bootstrap.css", new CssRewriteUrlTransform())
                .Include("~/Content/bootstrap-notify.css", new CssRewriteUrlTransform())
                .Include("~/Scripts/dropzone/dropzone.css", new CssRewriteUrlTransform())
                //   "~/Content/bootstrap-theme.css",
                //.Include("~/Content/inputs-ext/typeaheadjs/lib/typeahead.js-bootstrap.css", new CssRewriteUrlTransform())
                //.Include("~/Content/handsontable/handsontable.full.css", new CssRewriteUrlTransform())
                .Include("~/Content/DataTables/css/buttons.bootstrap.css", new CssRewriteUrlTransform())
                .Include("~/Content/DataTables/css/autoFill.bootstrap.css", new CssRewriteUrlTransform())
                .Include("~/Content/DataTables/css/select.bootstrap.css", new CssRewriteUrlTransform())
                .Include("~/Content/DataTables/css/responsive.bootstrap.css", new CssRewriteUrlTransform())
                      //"~/Content/DataTables/css/scroller.bootstrap.css",
                      .Include("~/Content/DataTables/css/fixedColumns.bootstrap.css", new CssRewriteUrlTransform())
                      .Include("~/Content/DataTables/css/fixedHeader.bootstrap.css", new CssRewriteUrlTransform())
                      .Include("~/Content/DataTables/css/colReorder.bootstrap.css", new CssRewriteUrlTransform())
                      .Include("~/Content/DataTables/css/buttons.bootstrap.css", new CssRewriteUrlTransform())
                      .Include("~/Content/DataTables/css/dataTables.bootstrap.css", new CssRewriteUrlTransform())
                      .Include("~/Content/DataTables/css/keyTable.bootstrap.css", new CssRewriteUrlTransform())
                      .Include("~/Content/DataTables/css/rowReorder.bootstrap.css", new CssRewriteUrlTransform())
                      .Include("~/Content/DataTables/css/colReorder.bootstrap.css", new CssRewriteUrlTransform())
                      .Include("~/Content/DataTables/css/keyTable.bootstrap.css", new CssRewriteUrlTransform())
                      .Include("~/Content/DataTables/css/searchPanes.bootstrap.css", new CssRewriteUrlTransform())
                      .Include("~/Content/DataTables/css/searchPanes.dataTables.min.css", new CssRewriteUrlTransform())
                      .Include("~/Content/DataTables/css/datatables.mark.css", new CssRewriteUrlTransform())
                .Include("~/Content/bootstrap3-editable/css/bootstrap-editable.css", new CssRewriteUrlTransform())
                .Include("~/Content/bootstrap-datepicker.css", new CssRewriteUrlTransform())
                .Include("~/Content/daterangepicker.css", new CssRewriteUrlTransform())
                //.Include("~/Scripts/clockpicker-gh-pages/dist/bootstrap-clockpicker.css", new CssRewriteUrlTransform())
                .Include("~/Content/bootstrap-toggle.less", new CssRewriteUrlTransform())
                .Include("~/Content/bootstrap-milestones.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/base/jquery-ui.css", new CssRewriteUrlTransform())
                .Include("~/Content/site.css", new CssRewriteUrlTransform())
                );
#if Local
            BundleTable.EnableOptimizations = true;
#else
            BundleTable.EnableOptimizations = true;
#endif


        }
    }
}
