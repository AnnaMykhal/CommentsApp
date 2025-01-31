using CommentsApp.DTO.Users;

namespace CommentsApp.Interfaces;

public interface IUserService
{
    Task<CreateUserResponse> Create(CreateUserRequest req);
    Task<AuthUserResponse> Authenticate(AuthUserRequest req);
    Task<IEnumerable<CreateUserResponse>> GetAll();
    Task<bool> Delete(Guid id);
    Task<string> UploadAvatar(Guid userId, string avatarPath);
    Task<string> GetAvatarUrlAsync(Guid userId);
}
