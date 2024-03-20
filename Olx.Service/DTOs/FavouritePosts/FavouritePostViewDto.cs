using Olx.Domain.Entities;

namespace Olx.Service.DTOs.FavouritePosts;

public class FavouritePostViewDto
{
    public long UserId { get; set; }
    public User User { get; set; }
    public long PostId { get; set; }
    public Post Post { get; set; }
}