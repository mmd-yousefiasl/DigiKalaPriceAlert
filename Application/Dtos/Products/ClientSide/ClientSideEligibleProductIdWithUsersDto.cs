using Application.Dtos.Users.ClientSide;

namespace Application.Dtos.Products.ClientSide
{
    public class ClientSideEligibleProductIdWithUsersDto : CommonDto
    {
        public List<ClientSideUserDto>? Users { get; set; }
    }
}
