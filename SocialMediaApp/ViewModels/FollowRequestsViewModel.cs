namespace SocialMediaApp.ViewModels
{
    public class FollowRequestsViewModel
    {
        public string FollowerId { get; set; }
        public string FollowerFirstName { get; set; }
        public string FollowerLastName { get; set; }
        public string FollowerPfp { get; set; }
        public string FollowedId { get; set; }

        public DateTime FollowDate { get; set; }
    }
}
