using Grpc.Core;
using UserService.Protos;

namespace Users.API.GrpcTests
{
    public class UserForRecipeService(ILogger<UserForRecipeService> logger) : UserService.Protos.UserService.UserServiceBase 
    {
        public override Task<UserResponse> GetUserDetails(
            UserRequest request,
            ServerCallContext context)
        {
            logger.LogInformation($"Get request for id: {request.UserId}");

            return Task.FromResult(new UserResponse
            {
                UserId = request.UserId,
                FullName = "Иван Иванов",
                Email = "ivan@example.com",
            });
        }
    }
}
