namespace HedgeMark.Operations.Secure.DataModel.Models
{
    public class UserAccountDetails
    {
        public hLoginRegistration User { get; set; }
        public int Id
        {
            get
            {
                return User == null ? 0 : User.intLoginID;
            }
        }
        public string Role { get; set; }
        public string Name { get; set; }
    }
}
