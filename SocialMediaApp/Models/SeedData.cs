using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Data;
using SocialMediaApp.Data.Migrations;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SocialMediaApp.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(serviceProvider
                .GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                if (context.Roles.Any())
                {
                    return;
                }

                context.Roles.AddRange(

                    new IdentityRole
                    {
                        Id = "239daf37-31bd-42c4-9f11-8320737429cc",
                        Name = "Admin",
                        NormalizedName = "Admin".ToUpper()
                    },

                    new IdentityRole
                    {
                        Id = "38d898da-719a-4b6c-bde8-905712ada7e7",
                        Name = "User",
                        NormalizedName = "User".ToUpper()
                    }

                );

                var hasher = new PasswordHasher<ApplicationUser>();

                context.Users.AddRange(
                    new ApplicationUser
                    {
                        Id = "00974e5c-2d38-4fc2-8745-e88ceba0d3ba",
                        UserName = "admin@test.com",
                        EmailConfirmed = true,
                        NormalizedEmail = "ADMIN@TEST.COM",
                        Email = "admin@test.com",
                        NormalizedUserName = "ADMIN@TEST.COM",
                        PasswordHash = hasher.HashPassword(null, "Admin1!"),

                        FirstName = "Mihai",
                        LastName = "Popa",
                        ProfileVisibility = "public",
                        Description = "Manager al platformei MicroSocial",
                        ProfilePicture = "pfp-admin.png"
                    },

                    new ApplicationUser
                    {
                        Id = "45b7d85f-53f2-4f8c-b4eb-ea70f3c2276e",
                        UserName = "user@test.com",
                        EmailConfirmed = true,
                        NormalizedEmail = "USER@TEST.COM",
                        Email = "user@test.com",
                        NormalizedUserName = "USER@TEST.COM",
                        PasswordHash = hasher.HashPassword(null, "User1!"),

                        FirstName = "Andrei",
                        LastName = "Ion",
                        ProfileVisibility = "public",
                        Description = "Utilizator cu cont public",
                        ProfilePicture = "pfp-user1.jpg"
                    },

                    new ApplicationUser
                    {
                        Id = "9807aab3-397d-41d4-8efb-13fc06ffee5a",
                        UserName = "user2@test.com",
                        EmailConfirmed = true,
                        NormalizedEmail = "USER2@TEST.COM",
                        Email = "user2@test.com",
                        NormalizedUserName = "USER2@TEST.COM",
                        PasswordHash = hasher.HashPassword(null, "User2!"),

                        FirstName = "Anda",
                        LastName = "Ion",
                        ProfileVisibility = "private",
                        Description = "Utilizator cu cont privat",
                        ProfilePicture = "pfp-user2.png"
                    }
                );

                context.UserRoles.AddRange(
                    new IdentityUserRole<string>
                    {
                        UserId = "00974e5c-2d38-4fc2-8745-e88ceba0d3ba",
                        RoleId = "239daf37-31bd-42c4-9f11-8320737429cc"
                    },

                    new IdentityUserRole<string>
                    {
                        UserId = "45b7d85f-53f2-4f8c-b4eb-ea70f3c2276e",
                        RoleId = "38d898da-719a-4b6c-bde8-905712ada7e7"
                    },

                    new IdentityUserRole<string>
                    {
                        UserId = "9807aab3-397d-41d4-8efb-13fc06ffee5a",
                        RoleId = "38d898da-719a-4b6c-bde8-905712ada7e7"
                    }
                );

                context.Groups.AddRange(
                    new Group
                    {
                        Name = "Cinemateca",
                        Description = "Movie discussions"
                    },
                    new Group
                    {
                        Name = "The guitarists",
                        Description = "Tips & tricks for all levels of musicians"
                    },
                    new Group
                    {
                        Name = "Building H5 Entrance 4",
                        Description = "Your neighbourhood's friendly elderly"
                    }
                );

                context.SaveChanges();

                var group1 = context.Groups.First(g => g.Name == "Cinemateca").Id;
                var group2 = context.Groups.First(g => g.Name == "The guitarists").Id;
                var group3 = context.Groups.First(g => g.Name == "Building H5 Entrance 4").Id;

                context.Posts.AddRange(
                    new Post
                    {
                        TextContent = "Salut comunitate!",
                        Date = DateTime.Now.AddDays(-3),
                        UserId = "00974e5c-2d38-4fc2-8745-e88ceba0d3ba" // admin
                    },
                    new Post
                    {
                        TextContent = "Cai verzi pe pereti",
                        Date = DateTime.Now.AddDays(-10),
                        UserId = "45b7d85f-53f2-4f8c-b4eb-ea70f3c2276e" // user1
                    },
                    new Post
                    {
                        TextContent = "Azi incepe vacanta",
                        Date = DateTime.Now.AddDays(-1),
                        UserId = "9807aab3-397d-41d4-8efb-13fc06ffee5a" // user2
                    },
                    new Post
                    {
                        TextContent = "Craciun fericit!",
                        Date = DateTime.Now.AddDays(-7),
                        UserId = "45b7d85f-53f2-4f8c-b4eb-ea70f3c2276e" // user1
                    },
                    new Post
                    {
                        TextContent = "Aceasta este o postare",
                        Date = DateTime.Now.AddDays(-5),
                        UserId = "00974e5c-2d38-4fc2-8745-e88ceba0d3ba" // admin
                    }
                );

                context.GroupUsers.AddRange(

                    //DateTime(2025, 1, 1, 10, 0, 0); // 1 ianuarie 2025, ora 10:00

                    new GroupUser
                    {
                        UserId = "00974e5c-2d38-4fc2-8745-e88ceba0d3ba",//admin in cinemateca
                        GroupId = group1,
                        IsModerator = true,
                        JoinDate = DateTime.Now.AddDays(-10) // s a alaturat acum 10 zile
                    },
                    new GroupUser
                    {
                        UserId = "45b7d85f-53f2-4f8c-b4eb-ea70f3c2276e",//user1 in cinemateca
                        GroupId = group1,
                        IsModerator = false,
                        JoinDate = DateTime.Now.AddDays(-9)
                    },
                    new GroupUser
                    {
                        UserId = "9807aab3-397d-41d4-8efb-13fc06ffee5a",//user2 in cinemateca
                        GroupId = group1,
                        IsModerator = false,
                        JoinDate = DateTime.Now.AddDays(-5)
                    },
                    new GroupUser
                    {
                        UserId = "45b7d85f-53f2-4f8c-b4eb-ea70f3c2276e",//user1 e chitarist
                        GroupId = group2,
                        IsModerator = true,
                        JoinDate = DateTime.Now.AddDays(-100)
                    },
                    new GroupUser
                    {
                        UserId = "9807aab3-397d-41d4-8efb-13fc06ffee5a",//user 2 e in bloc h5
                        GroupId = group3,
                        IsModerator = true,
                        JoinDate = DateTime.Now.AddDays(-30)
                    }
                );

                context.SaveChanges();

                context.GroupMessages.AddRange(
                    new GroupMessage
                    {
                        GroupId = group1,
                        UserId = "00974e5c-2d38-4fc2-8745-e88ceba0d3ba",
                        TextContent = "Welcome to this group, I am the moderator here and a manager of MicroSocial!",
                        CreatedAt = DateTime.Now.AddDays(-10)
                    },
                    new GroupMessage
                    {
                        GroupId = group1,
                        UserId = "45b7d85f-53f2-4f8c-b4eb-ea70f3c2276e",
                        TextContent = "I am glad to be here!",
                        CreatedAt = DateTime.Now.AddDays(-9)
                    },
                    new GroupMessage
                    {
                        GroupId = group1,
                        UserId = "9807aab3-397d-41d4-8efb-13fc06ffee5a",
                        TextContent = "Me too! So what's the last movie that you've seen?",
                        CreatedAt = DateTime.Now.AddDays(-5)
                    },
                    new GroupMessage
                    {
                        GroupId = group2,
                        UserId = "45b7d85f-53f2-4f8c-b4eb-ea70f3c2276e",
                        TextContent = "Welcome to the guitarists' group! As an introduction tell us what's your most listened to song!",
                        CreatedAt = DateTime.Now.AddDays(-100)
                    },
                    new GroupMessage
                    {
                        GroupId = group3,
                        UserId = "9807aab3-397d-41d4-8efb-13fc06ffee5a",
                        TextContent = "Welcome to this community of friendly neighbours!",
                        CreatedAt = DateTime.Now.AddDays(-30)
                    }
                );

                context.Follows.AddRange(
                    new Follows
                    {
                        FollowerId = "45b7d85f-53f2-4f8c-b4eb-ea70f3c2276e",//user1 urmareste admin
                        FollowedId = "00974e5c-2d38-4fc2-8745-e88ceba0d3ba",
                        Accepted = true,
                        Date = DateTime.Now.AddDays(-2)
                    },
                    new Follows
                    {
                        FollowerId = "45b7d85f-53f2-4f8c-b4eb-ea70f3c2276e",//user1 urmareste user2
                        FollowedId = "9807aab3-397d-41d4-8efb-13fc06ffee5a",
                        Accepted = true,
                        Date = DateTime.Now.AddDays(-2)
                    }
                );

                context.SaveChanges();
            }
        }
    }
}
