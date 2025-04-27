using Grpc.Core;
using Grpc.Net.Client;
using UserService.Protos;

namespace Recipes.API.GrpcTests
{
    public class UserInfoService(IConfiguration configuration, ILogger<UserInfoService> logger)
    {
        public async Task<UserResponse> GetUserData(string userId)
        {
            logger.LogInformation($"Calling UserService for user {userId}");

            var channel = GrpcChannel.ForAddress(configuration["UserServiceUrl"]);
            var client = new UserService.Protos.UserService.UserServiceClient(channel);

            try
            {
                var response = await client.GetUserDetailsAsync(
                    new UserRequest { UserId = userId });

                return response;
            }
            catch (RpcException ex)
            {
                logger.LogError(ex, "Error calling UserService");
                throw;
            }
        }
    }
}
