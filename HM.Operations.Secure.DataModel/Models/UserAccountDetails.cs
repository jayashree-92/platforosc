namespace HM.Operations.Secure.DataModel.Models
{
    public class HMUser
    {
        public int LoginId { get; set; }
        public string Name { get; set; }
        public string CommitId { get; set; }
    }

    public class UserAccountDetails
    {
        public HMUser User { get; set; }
        public int Id
        {
            get
            {
                return User == null ? 0 : User.LoginId;
            }
        }
        public string Role { get; set; }
        public string Name { get; set; }
    }
}
