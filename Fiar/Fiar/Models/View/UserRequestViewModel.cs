namespace Fiar.ViewModels
{
    public class UserRequestViewModel
    {
        public long Id { get; set; }

        public UserRequestType Type { get; set; }

        public UserDataModel User { get; set; }
    }
}
