public class UserRepository : IUserRepository
{
    private List<UserDto> _users = new()
    {
        new UserDto("Andriy", "andriy.plyska322@gmail.com", "123123")
    };
    public UserDto GetUser(UserModel userModel) => 
        _users.FirstOrDefault(user => 
            string.Equals(user.UserName, userModel.UserName) &&
            string.Equals(user.Email, userModel.Email) &&
            string.Equals(user.Password, userModel.Password)) ??
            throw new Exception();
        
}