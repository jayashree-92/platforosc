using System;
using System.Xml.Linq;

namespace HM.Operations.Secure.DataModel.Models
{
    public interface IEntityMetadataProvider
    {
        XElement GetStorageModel();
        XElement GetMappingsModel();
        XElement GetCodeSpaceModel();
    }

    public class SynchronizedOperationsContext : OperationsSecureContext
    {
        [ThreadStatic]
        private static SynchronizedOperationsContext current;

        public static SynchronizedOperationsContext Current()
        {
            return current ?? (current = new SynchronizedOperationsContext());
        }
    }

    public class OperationsMetadataProvider : IEntityMetadataProvider
    {
        public XElement GetStorageModel()
        {
            return XElement.Load(typeof(OperationsSecureContext).Assembly.GetManifestResourceStream("Models.OperationsSecureModel.ssdl"));
        }

        public XElement GetMappingsModel()
        {
            return XElement.Load(typeof(OperationsSecureContext).Assembly.GetManifestResourceStream("Models.OperationsSecureModel.msl"));
        }

        public XElement GetCodeSpaceModel()
        {
            return XElement.Load(typeof(OperationsSecureContext).Assembly.GetManifestResourceStream("Models.OperationsSecureModel.csdl"));
        }
    }
    public class OperationsSecureSettings
    {
        private static readonly string connectionString;

        static OperationsSecureSettings()
        {
            connectionString = new OperationsSecureContext().Database.Connection.ConnectionString;
        }

        public string ConnectionString
        {
            get { return connectionString; }
        }
    }

    public class SynchronizedAdminContext : AdminContext
    {
        [ThreadStatic]
        private static SynchronizedAdminContext current;

        public static SynchronizedAdminContext Current()
        {
            return current ?? (current = new SynchronizedAdminContext());
        }
    }

    public class AdminMetadataProvider : IEntityMetadataProvider
    {
        public XElement GetStorageModel()
        {
            return XElement.Load(typeof(AdminContext).Assembly.GetManifestResourceStream("Models.AdminModel.ssdl"));
        }

        public XElement GetMappingsModel()
        {
            return XElement.Load(typeof(AdminContext).Assembly.GetManifestResourceStream("Models.AdminModel.msl"));
        }

        public XElement GetCodeSpaceModel()
        {
            return XElement.Load(typeof(AdminContext).Assembly.GetManifestResourceStream("Models.AdminModel.csdl"));
        }
    }

    public class AdminContextSettings
    {
        private static readonly string connectionString;

        static AdminContextSettings()
        {
            connectionString = new AdminContext().Database.Connection.ConnectionString;
        }

        public string ConnectionString
        {
            get { return connectionString; }
        }
    }
}