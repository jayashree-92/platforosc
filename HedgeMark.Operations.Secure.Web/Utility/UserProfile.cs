using System.Web;
using System.Web.Profile;

namespace HMOSecureWeb.Utility
{
    public class UserProfile : ProfileBase
    {
        public static UserProfile Get()
        {
            return (UserProfile)Create(HttpContext.Current.User.Identity.Name);
        }

        public string Name
        {
            get
            {
                return GetPropertyValue("Name") as string;

            }
            set
            {
                SetPropertyValue("Name", value);
                Save();
            }
        }

        //public Layout Layout
        //{
        //    get
        //    {
        //        return GetPropertyValue("Layout") is Layout ? (Layout)GetPropertyValue("Layout") : Layout.Informative;
        //    }
        //    set
        //    {
        //        SetPropertyValue("Layout", value);
        //        Save();
        //    }
        //}

        //public Theme Theme
        //{
        //    get
        //    {
        //        return GetPropertyValue("Theme") is Theme ? (Theme)GetPropertyValue("Theme") : Theme.Day;
        //    }
        //    set
        //    {
        //        SetPropertyValue("Theme", value);
        //        Save();
        //    }
        //}

        //public string GetCustomColumns(string tableId)
        //{
        //    return GetPropertyValue(tableId + "_CustomColumns") as string;
        //}

        //public void SetCustomColumns(string tableId, string columns)
        //{
        //    SetPropertyValue(tableId + "_CustomColumns", columns);
        //    Save();
        //}

        //public string GetColumnOrder(string tableId)
        //{
        //    return GetPropertyValue(tableId + "_ColOrder") as string;

        //}

        //public void SetColumnOrder(string tableId, string value)
        //{
        //    SetPropertyValue(tableId + "_ColOrder", value);
        //    Save();
        //}

        //public string GetUserPreferredHoldingsColumn()
        //{
        //    return GetPropertyValue("holdingsTable_UserPreferredeColumns") as string;
        //}

        //public void SetUserPreferredHoldingsColumn(string value)
        //{
        //    SetPropertyValue("holdingsTable_UserPreferredeColumns", value);
        //    Save();
        //}

    }

}