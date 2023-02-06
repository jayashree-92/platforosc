namespace HM.Operations.Secure.DataModel.Models
{
    public class HMUser
    {
        public int LoginId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public double AllowedWireAmountLimit { get; set; }
        public double TotalYearsOfExperienceInHM { get; set; }
        public double TotalYearsOfExperience { get; set; }
        public bool IsUserVp { get; set; }
    }

    public class UserAccountDetails
    {
        public HMUser User { get; set; }
        public int Id => User?.LoginId ?? 0;
        public string Role { get; set; }
        public string Name { get; set; }
    }
}
